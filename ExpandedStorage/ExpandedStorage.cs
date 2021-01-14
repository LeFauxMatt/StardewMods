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
using StardewValley.Menus;
using StardewValley.Objects;
using SDVObject = StardewValley.Object;

namespace ExpandedStorage
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ExpandedStorage : Mod
    {
        /// <summary>Dictionary list of objects which are Expanded Storage, </summary>
        private static readonly IDictionary<string, ExpandedStorageConfig> ExpandedStorageConfigs = new Dictionary<string, ExpandedStorageConfig>();
        
        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        /// <summary>Control scheme.</summary>
        private ModConfigControls _controls;

        /// <summary>Overlays ItemGrabMenu with UI elements provided by ExpandedStorage.</summary>
        private readonly PerScreen<ChestOverlay> _chestOverlay = new PerScreen<ChestOverlay>();
        
        /// <summary>Tracks previously held chest before placing into world.</summary>
        private readonly PerScreen<Chest> _previousHeldChest = new PerScreen<Chest>();
        
        /// <summary>Returns ExpandedStorageConfig by item name.</summary>
        public static ExpandedStorageConfig GetConfig(string storageName) =>
            ExpandedStorageConfigs.TryGetValue(storageName, out var config)
                ? config
                : null;
        
        /// <summary>Returns true if item is an ExpandedStorage.</summary>
        public static bool HasConfig(string storageName) =>
            ExpandedStorageConfigs.ContainsKey(storageName);

        /// <summary>Returns Y-Offset to lower menu for valid contexts.</summary>
        public static int Offset(object context) =>
            context is Chest {SpecialChestType: Chest.SpecialChestTypes.None}
                ? 192
                : 0;
        
        /// <summary>Returns Y-Offset to lower menu for valid instances.</summary>
        public static int Offset(MenuWithInventory menu) =>
            menu is ItemGrabMenu {context: Chest {SpecialChestType: Chest.SpecialChestTypes.None}}
                ? 192
                : 0;
        
        /// <summary>Returns Display Capacity of MenuWithInventory.</summary>
        public static int Capacity(MenuWithInventory menu) =>
            menu is ItemGrabMenu {context: Chest {SpecialChestType: Chest.SpecialChestTypes.None}}
                ? 72
                : Chest.capacity;
        
        /// <summary>Returns Display Rows of MenuWithInventory.</summary>
        public static int Rows(MenuWithInventory menu) =>
            menu is ItemGrabMenu {context: Chest {SpecialChestType: Chest.SpecialChestTypes.None}}
                ? 6
                : 3;
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _controls = new ModConfigControls(_config.ControlsRaw);
#if !DEBUG
            // Disable unready features in release
            _config.ShowSearchBar = false;
#endif
            if (helper.ModRegistry.IsLoaded("spacechase0.CarryChest"))
            {
                Monitor.Log("Expanded Storage should not be run alongside Carry Chest", LogLevel.Warn);
                _config.AllowCarryingChests = false;
            }

            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            
            if (_config.AllowCarryingChests)
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
            }
            
            if (_config.AllowModdedCapacity)
                helper.Events.Display.MenuChanged += OnMenuChanged;

            // Harmony Patches
            new Patcher(ModManifest.UniqueID).ApplyAll(
                new ItemPatch(Monitor, _config),
                new ObjectPatch(Monitor, _config),
                new ChestPatches(Monitor, _config, helper.Reflection),
                new ItemGrabMenuPatch(Monitor, _config),
                new InventoryMenuPatch(Monitor, _config),
                new MenuWithInventoryPatch(Monitor, _config));
        }

        /// <summary>
        /// Load Json Assets Api and wait for IDs to be assigned.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Monitor.Log($"Loading Expanded Storage Content", LogLevel.Info);
            ExpandedStorageConfigs.Clear();
            foreach (var contentPack in Helper.ContentPacks.GetOwned())
            {
                if (!contentPack.HasFile("expandedStorage.json"))
                {
                    Monitor.Log($"Cannot load {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                    continue;
                }
                
                Monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
                var contentData = contentPack.ReadJsonFile<ContentData>("expandedStorage.json");
                foreach (var expandedStorage in contentData.ExpandedStorage
                    .Where(s => !string.IsNullOrWhiteSpace(s.StorageName)))
                {
                    if (ExpandedStorageConfigs.ContainsKey(expandedStorage.StorageName))
                    {
                        Monitor.Log(
                            $"Cannot load {expandedStorage.StorageName} from {contentPack.Manifest.Name} {contentPack.Manifest.Version}: a storage with that name is already loaded",
                            LogLevel.Warn);
                    }
                    else
                    {
                        expandedStorage.ModUniqueId = contentPack.Manifest.UniqueID;
                        ExpandedStorageConfigs.Add(expandedStorage.StorageName, expandedStorage);
                    }
                }
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
            // Button Controls
            if (_chestOverlay.Value != null)
            {
                if (e.Button == _controls.ScrollDown)
                {
                    _chestOverlay.Value.Scroll(-1);
                    Helper.Input.Suppress(e.Button);
                }
                else if (e.Button == _controls.ScrollUp)
                {
                    _chestOverlay.Value.Scroll(1);
                    Helper.Input.Suppress(e.Button);
                }
            }
            
            if (!Context.IsPlayerFree)
                return;
            
            // Carry Chests
            if (_config.AllowCarryingChests && (e.Button == SButton.MouseLeft || e.Button == _controls.CarryChest) && Game1.player.CurrentItem == null)
            {
                var location = Game1.currentLocation;
                var pos = e.Cursor.Tile;
                if (!location.objects.TryGetValue(pos, out var obj) ||
                    !(obj is Chest && (!ExpandedStorageConfigs.TryGetValue(obj.name, out var data) || data.CanCarry)) ||
                    !Game1.player.addItemToInventoryBool(obj, true))
                    return;
                location.objects.Remove(pos);
                Helper.Input.Suppress(e.Button);
            }
        }
        
        /// <summary>
        /// Resets scrolling/overlay when chest menu exits or context changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Menu is exited or context has changed
            if (e.NewMenu is null ||
                e.NewMenu is ItemGrabMenu newMenu &&
                _chestOverlay.Value != null &&
                !ReferenceEquals(newMenu.context, _chestOverlay.Value.Menu?.context))
            {
                _chestOverlay.Value = null;
            }
            
            // Add new overlay
            if (e.NewMenu is ItemGrabMenu menu)
                _chestOverlay.Value = new ChestOverlay(menu, Helper.Events, Helper.Input);
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
                    p.Value is Chest || p.Value.bigCraftable.Value && ExpandedStorageConfigs.ContainsKey(p.Value.DisplayName));
            
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