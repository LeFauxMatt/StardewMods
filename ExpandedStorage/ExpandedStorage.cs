using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.Patches;
using ExpandedStorage.Framework.UI;
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
        private static readonly IDictionary<int, string> StorageObjects = new Dictionary<int, string>();
        
        /// <summary>Dictionary of Expanded Storage configs</summary>
        private static readonly IDictionary<string, StorageContentData> StorageContent = new Dictionary<string, StorageContentData>();

        /// <summary>Dictionary of Expanded Storage tabs</summary>
        private static readonly IDictionary<string, TabContentData> StorageTabs = new Dictionary<string, TabContentData>();

        /// <summary>List of vanilla storages Display Names</summary>
        private static IDictionary<int, string> VanillaStorages;

        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        /// <summary>Tracks previously held chest before placing into world.</summary>
        private readonly PerScreen<Chest> _previousHeldChest = new PerScreen<Chest>();

        private ContentLoader _contentLoader;

        /// <summary>Returns ExpandedStorageConfig by item name.</summary>
        public static StorageContentData GetConfig(Item item) =>
            item is Object obj
            && obj.bigCraftable.Value
            && StorageObjects.TryGetValue(obj.ParentSheetIndex, out var storageName)
            && StorageContent.TryGetValue(storageName, out var config)
                ? config : null;
        
        /// <summary>Returns true if item is an ExpandedStorage.</summary>
        public static bool HasConfig(Item item) =>
            item is Object obj
            && obj.bigCraftable.Value
            && StorageObjects.ContainsKey(item.ParentSheetIndex);
        
        /// <summary>Returns true if item is a Vanilla Storage.</summary>
        public static bool IsVanilla(Item item) =>
            item is Chest
            && VanillaStorages.ContainsKey(item.ParentSheetIndex);
        
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
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            if (_config.AllowCarryingChests)
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
                helper.Events.Input.ButtonPressed += OnButtonPressed;
            }

            // Harmony Patches
            new Patcher(ModManifest.UniqueID).ApplyAll(
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
            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
            {
                Helper.Events.GameLoop.UpdateTicked += LoadContentPacks;
            }
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
                    StorageObjects.Add(jsonAssetsId.Value, jsonAssetsId.Key);
                }
            };
        }
        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void LoadContentPacks(object sender, UpdateTickedEventArgs e)
        {
            if (!_contentLoader.IsOwnedLoaded)
                return;
            Helper.Events.GameLoop.UpdateTicked -= LoadContentPacks;
            VanillaStorages = _contentLoader.LoadVanillaStorages(StorageContent, StorageObjects);
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
                    || !StorageContent[StorageObjects[obj.ParentSheetIndex]].CanCarry
                    || !Game1.player.addItemToInventoryBool(obj, true))
                    return;
                location.objects.Remove(pos);
                Helper.Input.Suppress(e.Button);
            }
            else if (_config.AllowAccessCarriedChest && _previousHeldChest.Value != null && e.Button.IsActionButton())
            {
                _previousHeldChest.Value.ShowMenu();
                Helper.Input.Suppress(e.Button);
            }
        }

        /// <summary>
        /// Converts objects to modded storage when placed in the world.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            var itemPos = e.Added
                .LastOrDefault(p =>
                    p.Value is Chest || p.Value.bigCraftable.Value && HasConfig(p.Value));
            
            var obj = itemPos.Value;
            var pos = itemPos.Key;
            if (obj is not Chest chest)
                return;

            // Copy properties from previously held chest
            var previousHeldChest = _previousHeldChest.Value;
            if (previousHeldChest != null && ReferenceEquals(e.Location, Game1.currentLocation))
            {
                chest.Name = previousHeldChest.Name;
                chest.playerChoiceColor.Value = previousHeldChest.playerChoiceColor.Value;
                if (previousHeldChest.items.Any())
                    chest.items.CopyFrom(previousHeldChest.items);
                // Copy modData
                foreach (var chestModData in previousHeldChest.modData)
                    chest.modData.CopyFrom(chestModData);
            }
            
            // Replace object if necessary
            if (!ReferenceEquals(chest, obj))
                e.Location.objects[pos] = chest;
        }
    }
}