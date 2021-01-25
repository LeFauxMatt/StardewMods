using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.Patches;
using ExpandedStorage.Framework.UI;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ExpandedStorage
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ExpandedStorage : Mod, IAssetEditor
    {
        /// <summary>Dictionary of Expanded Storage object data</summary>
        private static readonly IDictionary<int, string> StorageObjectsById = new Dictionary<int, string>();
        
        /// <summary>Dictionary of Expanded Storage configs</summary>
        private static readonly IDictionary<string, StorageContentData> StorageContent = new Dictionary<string, StorageContentData>();

        /// <summary>Dictionary of Expanded Storage tabs</summary>
        private static readonly IDictionary<string, TabContentData> StorageTabs = new Dictionary<string, TabContentData>();

        /// <summary>List of vanilla storages Display Names</summary>
        private static IDictionary<int, string> _vanillaStorages;

        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        /// <summary>Tracks previously held chest before placing into world.</summary>
        private readonly PerScreen<Chest> _previousHeldChest = new PerScreen<Chest>();

        private ContentLoader _contentLoader;

        /// <summary>Returns ExpandedStorageConfig by item name.</summary>
        public static StorageContentData GetConfig(Item item) =>
            item is Object obj
            && obj.bigCraftable.Value
            && StorageObjectsById.TryGetValue(obj.ParentSheetIndex, out var storageName)
            && StorageContent.TryGetValue(storageName, out var config)
                ? config : null;
        
        /// <summary>Returns true if item is an ExpandedStorage.</summary>
        public static bool HasConfig(Item item) =>
            item is Object obj
            && obj.bigCraftable.Value
            && StorageObjectsById.ContainsKey(item.ParentSheetIndex);
        
        /// <summary>Returns true if item is a Vanilla Storage.</summary>
        public static bool IsVanilla(Item item) =>
            item is Chest
            && _vanillaStorages.ContainsKey(item.ParentSheetIndex);
        
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

            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;

            if (_config.AllowCarryingChests)
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
                helper.Events.Input.ButtonPressed += OnButtonPressed;
            }

            // Harmony Patches
            new Patcher(ModManifest.UniqueID).ApplyAll(
                new FarmerPatches(Monitor, _config),
                new ItemPatch(Monitor, _config),
                new ObjectPatch(Monitor, _config),
                new ChestPatches(Monitor, _config, helper.Reflection),
                new ItemGrabMenuPatch(Monitor, _config, helper.Reflection),
                new InventoryMenuPatch(Monitor, _config),
                new MenuWithInventoryPatch(Monitor, _config),
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
        public void Edit<T>(IAssetData asset) { }
        
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

            if (modConfigApi == null)
                return;
            
            modConfigApi.RegisterModConfig(ModManifest, () => _config.Controls = new ModConfigKeys(), () => Helper.WriteConfig(_config));
            modConfigApi.RegisterLabel(ModManifest, "Controls", "Controller/Keyboard controls");
            modConfigApi.RegisterSimpleOption(ModManifest, "Scroll Up", $"Button for scrolling up",
                () => _config.Controls.ScrollUp.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => _config.Controls.ScrollUp = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(ModManifest, "Scroll Down", $"Button for scrolling down",
                () => _config.Controls.ScrollDown.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => _config.Controls.ScrollDown = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(ModManifest, "Previous Tab", $"Button for switching to the previous tab",
                () => _config.Controls.PreviousTab.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => _config.Controls.PreviousTab = KeybindList.ForSingle(value));
            modConfigApi.RegisterSimpleOption(ModManifest, "Next Tab", $"Button for switching to the next tab",
                () => _config.Controls.NextTab.Keybinds.Single(kb => kb.IsBound).Buttons.First(),
                value => _config.Controls.NextTab = KeybindList.ForSingle(value));
        }
        
        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            var oldChest = _previousHeldChest.Value;
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
            _vanillaStorages = _contentLoader.LoadVanillaStorages(StorageContent, StorageObjectsById);
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            _previousHeldChest.Value = Game1.player.CurrentItem is Chest chest ? chest : null;
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            
            if (e.Button.IsUseToolButton() && _previousHeldChest.Value == null)
            {
                var location = Game1.currentLocation;
                var pos = Game1.player.GetToolLocation() / 64f;
                pos.X = (int) pos.X;
                pos.Y = (int) pos.Y;
                if (!location.objects.TryGetValue(pos, out var obj)
                    || !HasConfig(obj)
                    || !StorageContent[StorageObjectsById[obj.ParentSheetIndex]].CanCarry
                    || !Game1.player.addItemToInventoryBool(obj, true))
                    return;
                obj.TileLocation = Vector2.Zero;
                location.objects.Remove(pos);
                Helper.Input.Suppress(e.Button);
            }
            else if (_config.AllowAccessCarriedChest && _previousHeldChest.Value != null && e.Button.IsActionButton() && _previousHeldChest.Value.Stack == 1)
            {
                _previousHeldChest.Value.GetMutex().RequestLock(delegate
                {
                    _previousHeldChest.Value.ShowMenu();
                });
                Helper.Input.Suppress(e.Button);
            }
        }
    }
}