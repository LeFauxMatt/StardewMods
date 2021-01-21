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
    internal class ExpandedStorage : Mod
    {
        /// <summary>Dictionary of Expanded Storage object data</summary>
        private static readonly IDictionary<int, string> StorageObjects = new Dictionary<int, string>();
        
        /// <summary>Dictionary of Expanded Storage configs</summary>
        private static readonly IDictionary<string, StorageContentData> StorageContent = new Dictionary<string, StorageContentData>();

        /// <summary>Dictionary of Expanded Storage tabs</summary>
        private static readonly IDictionary<string, TabContentData> StorageTabs = new Dictionary<string, TabContentData>();

        /// <summary>List of vanilla storages Display Names</summary>
        private static readonly IDictionary<int, string> VanillaStorages = new Dictionary<int, string>
        {
            { 130, "Chest" },
            { 232, "Stone Chest" },
            { 216, "Mini-Fridge" }
        };
        
        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        /// <summary>Control scheme.</summary>
        private ModConfigControls _controls;

        /// <summary>Tracks previously held chest before placing into world.</summary>
        private readonly PerScreen<Chest> _previousHeldChest = new PerScreen<Chest>();

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
            _controls = new ModConfigControls(_config.Controls);

            if (helper.ModRegistry.IsLoaded("spacechase0.CarryChest"))
            {
                Monitor.Log("Expanded Storage should not be run alongside Carry Chest", LogLevel.Warn);
                _config.AllowCarryingChests = false;
            }
            
            var isAutomateLoaded = helper.ModRegistry.IsLoaded("Pathoschild.Automate");

            ExpandedMenu.Init(helper.Events, helper.Input, _config, _controls);

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
        
        /// <summary>Load content packs.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            LoadContentPacks();
            
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
        private void LoadContentPacks()
        {
            var modConfigApi = Helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            var contentLoader = new ContentLoader(Monitor, Helper.Content, Helper.ContentPacks.GetOwned());
            contentLoader.LoadAll(modConfigApi, StorageContent, StorageTabs);
            foreach (var item in VanillaStorages.Where(item => StorageContent.ContainsKey(item.Value)))
            {
                StorageObjects.Add(item.Key, item.Value);
            }
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

            var handled = false;
            
            if (e.Button == SButton.MouseLeft || e.Button.IsUseToolButton())
            {
                var location = Game1.currentLocation;
                var pos = e.Cursor.Tile;
                if (!location.objects.TryGetValue(pos, out var obj)
                    || !HasConfig(obj)
                    || !StorageContent[StorageObjects[obj.ParentSheetIndex]].CanCarry
                    || !Game1.player.addItemToInventoryBool(obj, true))
                    return;
                location.objects.Remove(pos);
                handled = true;
            }
            else if (_config.AllowAccessCarriedChest && _previousHeldChest.Value != null && (e.Button == SButton.MouseRight || e.Button.IsActionButton()))
            {
                _previousHeldChest.Value.ShowMenu();
                handled = true;
            }

            if (handled)
                Helper.Input.Suppress(e.Button);
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
            if (obj == null)
                return;

            // Convert Chest to Expanded Storage
            if (!(obj is Chest chest))
            {
                chest = new Chest(true, obj.TileLocation, obj.ParentSheetIndex)
                {
                    name = obj.name
                };
            }

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