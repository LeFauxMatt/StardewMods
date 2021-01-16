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

namespace ExpandedStorage
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ExpandedStorage : Mod
    {
        /// <summary>Dictionary list of objects which are Expanded Storage</summary>
        private static readonly IDictionary<string, ExpandedStorageConfig> ExpandedStorageConfigs = new Dictionary<string, ExpandedStorageConfig>();

        /// <summary>List of vanilla storages Display Names</summary>
        private static readonly IList<string> VanillaStorages = new List<string>()
        {
            "Chest",
            "Stone Chest",
            "Mini-Fridge"
        };
        
        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        /// <summary>Control scheme.</summary>
        private ModConfigControls _controls;

        /// <summary>Tracks previously held chest before placing into world.</summary>
        private readonly PerScreen<Chest> _previousHeldChest = new PerScreen<Chest>();

        /// <summary>Returns ExpandedStorageConfig by item name.</summary>
        public static ExpandedStorageConfig GetConfig(Item item) =>
            ExpandedStorageConfigs.TryGetValue(item.Name, out var config) ? config : null;

        /// <summary>Returns true if item is an ExpandedStorage.</summary>
        public static bool HasConfig(Item item) =>
            item is Object && ExpandedStorageConfigs.ContainsKey(item.Name);
        
        /// <summary>Returns true if item is a Vanilla Storage.</summary>
        public static bool IsVanilla(Item item) =>
            item is Chest && VanillaStorages.Contains(item.Name);
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

            ExpandedMenu.Init(helper.Events, helper.Input, _config, _controls);

            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;

            if (_config.AllowCarryingChests)
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
                helper.Events.Input.ButtonPressed += OnButtonPressed;
            }
            
            helper.Events.GameLoop.SaveLoaded += delegate
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTickingOnce;
            };

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
        /// Load Expanded Storage content packs
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
        
        /// <summary>Fix any placed objects that require a name correction.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTickingOnce(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            Helper.Events.GameLoop.UpdateTicking -= OnUpdateTickingOnce;
            var chestNames = new Dictionary<int, string>();
            Utility.ForAllLocations(location =>
            {
                var chests = location.Objects.Pairs
                    .Where(c => c.Value is Chest);

                foreach (var chest in chests)
                {
                    var parentSheetIndex = chest.Value.ParentSheetIndex;
                    if (!chestNames.TryGetValue(parentSheetIndex, out var chestName))
                    {
                        Game1.bigCraftablesInformation.TryGetValue(parentSheetIndex, out var chestInfo);
                        if (!string.IsNullOrEmpty(chestInfo))
                        {
                            chestName = chestInfo.Split('/')[0];
                            chestNames.Add(parentSheetIndex, chestName);
                        }
                    }

                    if (string.IsNullOrEmpty(chestName) || chest.Value.name == chestName)
                        continue;
                    
                    Monitor.Log($"Updating storage in {location.Name} at {chest.Key.X},{chest.Key.Y} to {chestName}");
                    chest.Value.name = chestName;
                }
            });
        }
        
        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || e.Button != SButton.MouseLeft && e.Button != _controls.CarryChest || Game1.player.CurrentItem != null)
                return;
            
            var location = Game1.currentLocation;
            var pos = e.Cursor.Tile;
            if (!location.objects.TryGetValue(pos, out var obj) ||
                !ExpandedStorageConfigs.TryGetValue(obj.name, out var data) ||
                !data.CanCarry ||
                !Game1.player.addItemToInventoryBool(obj, true))
                return;
            location.objects.Remove(pos);
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