using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.Extensions;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.Patches;
using ExpandedStorage.Framework.UI;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ExpandedStorage
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ExpandedStorage : Mod, IAssetEditor
    {
        private const string AdvancedLootKey = "aedenthorn.AdvancedLootFramework/IsAdvancedLootFrameworkChest";
        
        /// <summary>Tracks previously held chest before placing into world.</summary>
        internal static readonly PerScreen<Chest> HeldChest = new PerScreen<Chest>();
        
        /// <summary>Dictionary of Expanded Storage object data</summary>
        private static readonly IDictionary<int, string> StorageObjectsById = new Dictionary<int, string>();
        
        /// <summary>Dictionary of Expanded Storage configs</summary>
        private static readonly IDictionary<string, StorageContentData> StorageContent = new Dictionary<string, StorageContentData>();

        /// <summary>Dictionary of Expanded Storage tabs</summary>
        private static readonly IDictionary<string, TabContentData> StorageTabs = new Dictionary<string, TabContentData>();

        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        /// <summary>Tracks previously held chest lid frame.</summary>
        private readonly PerScreen<int> _currentLidFrame = new PerScreen<int>();

        /// <summary>Reflected currentLidFrame for previousHeldChest.</summary>
        private readonly PerScreen<IReflectedField<int>> _currentLidFrameReflected =
            new PerScreen<IReflectedField<int>>();

        private ContentLoader _contentLoader;

        /// <summary>Returns ExpandedStorageConfig by item name.</summary>
        public static StorageContentData GetConfig(object context) =>
            context switch
            {
                GameLocation when StorageContent.TryGetValue("Mini-Shipping Bin", out var shippingBinConfig)
                        => shippingBinConfig,
                JunimoHut when StorageContent.TryGetValue("Junimo Hut", out var junimoHutConfig)
                        => junimoHutConfig,
                Chest chest when chest.fridge.Value
                    && StorageContent.TryGetValue("Mini-Fridge", out var fridgeConfig)
                        => fridgeConfig,
                Object obj when obj.heldObject.Value is Chest
                    && StorageContent.TryGetValue("Auto-Grabber", out var autoGrabberConfig)
                        => autoGrabberConfig,
                Object obj when obj.bigCraftable.Value
                    && !obj.modData.ContainsKey(AdvancedLootKey)
                    && StorageObjectsById.TryGetValue(obj.ParentSheetIndex, out var storageName)
                    && StorageContent.TryGetValue(storageName, out var config)
                        => config,
                _ => null
            };

        /// <summary>Returns true if item is an ExpandedStorage.</summary>
        public static bool HasConfig(object context) =>
            context switch
            {
                GameLocation
                    => StorageContent.ContainsKey("Mini-Shipping Bin"),
                JunimoHut
                    => StorageContent.ContainsKey("Junimo Hut"),
                Chest chest when chest.fridge.Value
                    => StorageContent.ContainsKey("Mini-Fridge"),
                Object obj when obj.heldObject.Value is Chest
                    => StorageContent.ContainsKey("Auto-Grabber"),
                Object obj when obj.bigCraftable.Value
                    && !obj.modData.ContainsKey(AdvancedLootKey)
                    => StorageObjectsById.ContainsKey(obj.ParentSheetIndex),
                _ => false
            };

        /// <summary>Returns ExpandedStorageTab by tab name.</summary>
        public static TabContentData GetTab(string tabName) =>
            StorageTabs.TryGetValue(tabName, out var tab) ? tab : null;
        
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();

            if (helper.ModRegistry.IsLoaded("spacechase0.CarryChest"))
            {
                Monitor.Log("Expanded Storage should not be run alongside Carry Chest", LogLevel.Warn);
                _config.AllowCarryingChests = false;
            }
            
            var isAutomateLoaded = helper.ModRegistry.IsLoaded("Pathoschild.Automate");

            ExpandedMenu.Init(helper.Events, helper.Input, _config);
            ChestExtensions.Init(helper.Reflection);

            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;

            if (_config.AllowCarryingChests)
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
                helper.Events.Input.ButtonPressed += OnButtonPressed;
            }
            
            // Harmony Patches
            new Patcher(ModManifest.UniqueID).ApplyAll(
                new FarmerPatch(Monitor, _config),
                new ItemPatch(Monitor, _config),
                new ObjectPatch(Monitor, _config, helper.Reflection),
                new ChestPatches(Monitor, _config),
                new ItemGrabMenuPatch(Monitor, _config, helper.Reflection),
                new InventoryMenuPatch(Monitor, _config),
                new MenuWithInventoryPatch(Monitor, _config),
                new DebrisPatch(Monitor, _config),
                new AutomatePatch(Monitor, _config, helper.Reflection, isAutomateLoaded));
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            // Load bigCraftable on next tick for vanilla storages
            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
                Helper.Events.GameLoop.UpdateTicked += LoadContentPacks;
            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public void Edit<T>(IAssetData asset) {}
        
        /// <summary>Load content packs.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var modConfigApi = Helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            _contentLoader = new ContentLoader(Monitor, Helper.Content, Helper.ContentPacks.GetOwned());
            _contentLoader.LoadOwnedStorages(modConfigApi, StorageContent, StorageTabs);
            
            var jsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (jsonAssetsApi == null)
                return;
            
            jsonAssetsApi.IdsAssigned += delegate
            {
                foreach (var jsonAssetsId in jsonAssetsApi.GetAllBigCraftableIds()
                    .Where(obj => StorageContent.ContainsKey(obj.Key)))
                {
                    StorageObjectsById.Add(jsonAssetsId.Value, jsonAssetsId.Key);
                }
            };

            if (modConfigApi != null)
            {
                modConfigApi.RegisterModConfig(ModManifest,
                    () => _config = new ModConfig(),
                    () => Helper.WriteConfig(_config));
                ModConfig.RegisterModConfig(ModManifest, modConfigApi, _config);
            }
        }
        
        /// <summary>Clear out Object Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            var removeNames = StorageContent
                .Where(c => !c.Value.IsVanilla)
                .Select(c => c.Key)
                .ToList();
            var removeIds = StorageObjectsById
                .Where(i => removeNames.Contains(i.Value))
                .Select(i => i.Key)
                .ToList();
            foreach (var id in removeIds)
            {
                StorageObjectsById.Remove(id);
            }
        }
        
        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            var oldChest = HeldChest.Value;
            var chest = e.Added
                .Select(p => p.Value)
                .OfType<Chest>()
                .LastOrDefault(HasConfig);

            if (oldChest == null || chest == null || chest.items.Any() || !ReferenceEquals(e.Location, Game1.currentLocation))
                return;

            // Backup method for restoring carried Chest items
            chest.name = oldChest.name;
            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            if (oldChest.items.Any())
                chest.items.CopyFrom(oldChest.items);
            foreach (var modData in oldChest.modData)
                chest.modData.CopyFrom(modData);
        }
        
        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LoadContentPacks(object sender, UpdateTickedEventArgs e)
        {
            if (!_contentLoader.IsOwnedLoaded)
                return;
            Helper.Events.GameLoop.UpdateTicked -= LoadContentPacks;
            if (!_contentLoader.IsVanillaLoaded)
                _contentLoader.LoadVanillaStorages(StorageContent, StorageObjectsById);
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            HeldChest.Value = Game1.player.CurrentItem is Chest chest ? chest : null;
            if (HeldChest.Value == null)
                return;
            
            if (HeldChest.Value.frameCounter.Value <= -1
                || _currentLidFrame.Value > HeldChest.Value.getLastLidFrame())
                return;
            
            HeldChest.Value.frameCounter.Value--;
            if (HeldChest.Value.frameCounter.Value > 0
                || !HeldChest.Value.GetMutex().IsLockHeld())
                return;
            
            if (_currentLidFrame.Value == HeldChest.Value.getLastLidFrame())
            {
                HeldChest.Value.frameCounter.Value = -1;
                _currentLidFrame.Value = HeldChest.Value.startingLidFrame.Value;
                _currentLidFrameReflected.Value.SetValue(_currentLidFrame.Value);
                HeldChest.Value.ShowMenu();
            }
            else
            {
                HeldChest.Value.frameCounter.Value = 5;
                _currentLidFrame.Value++;
                _currentLidFrameReflected.Value.SetValue(_currentLidFrame.Value);
            }
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            
            var location = Game1.currentLocation;
            var pos = Game1.player.GetToolLocation() / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;
            
            if (HeldChest.Value == null
                && _config.AllowCarryingChests
                && e.Button.IsUseToolButton()
                && location.CarryChest(pos))
            {
                Helper.Input.Suppress(e.Button);
            }
            else if (HeldChest.Value != null
                     && _config.AllowAccessCarriedChest
                     && e.Button.IsActionButton()
                     && HeldChest.Value.Stack <= 1)
            {
                if (location.objects.TryGetValue(pos, out var obj) && HasConfig(obj))
                    return;
                var config = GetConfig(HeldChest.Value);
                HeldChest.Value.GetMutex().RequestLock(delegate
                {
                    HeldChest.Value.fixLidFrame();
                    HeldChest.Value.performOpenChest();
                    _currentLidFrameReflected.Value = Helper.Reflection.GetField<int>(HeldChest.Value, "currentLidFrame");
                    _currentLidFrame.Value = HeldChest.Value.startingLidFrame.Value;
                    Game1.playSound(config.OpenSound);
                    Game1.player.Halt();
                    Game1.player.freezePause = 1000;
                });
                Helper.Input.Suppress(e.Button);
            }
        }
    }
}