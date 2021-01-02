using System.Linq;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.UI;
using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using SDVObject = StardewValley.Object;

namespace ExpandedStorage
{
    internal class ModEntry : Mod
    {
        private readonly PerScreen<ChestOverlay> _chestOverlay = new PerScreen<ChestOverlay>();
        private DataLoader _dataLoader;
        private ModConfig _config;

        public override void Entry(IModHelper helper)
        {
            _dataLoader = new DataLoader(helper, Monitor);
            _config = helper.ReadConfig<ModConfig>();

            // Events
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            if (_config.AllowModdedCapacity)
                helper.Events.Display.MenuChanged += OnMenuChanged;

            // Patches
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);
            
            ChestPatches.PatchAll(_config, Monitor, harmony);
            ItemGrabMenuPatches.PatchAll(_config, Monitor, harmony);
            InventoryMenuPatches.PatchAll(_config, Monitor, harmony);
        }

        /// <summary>
        /// Converts vanilla chests to expanded, if necessary.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Utility.ForAllLocations(delegate(GameLocation location)
            {
                foreach (var itemPosition in location.Objects.Pairs
                    .Where(c =>
                        c.Value is Chest &&
                        c.Value.ShouldBeExpandedStorage() &&
                        !c.Value.IsExpandedStorage()))
                {
                    var pos = itemPosition.Key;
                    var obj = itemPosition.Value;
                    location.Objects[pos] = obj.ToExpandedStorage();
                }
            });
        }
        /// <summary>
        /// Resets scrolling/overlay when chest menu exits or context changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Remove overlay when menu is exited.
            if (e.OldMenu is ItemGrabMenu && e.NewMenu is null)
                _chestOverlay.Value?.Dispose();
            
            // Menu is not a relevant context or context is unchanged.
            if (!(e.NewMenu is ItemGrabMenu menu) || menu.context == _chestOverlay.Value?.Menu.context)
                return;
            
            // Remove old overlay
            if (_chestOverlay.Value != null)
            {
                _chestOverlay.Value?.Dispose();
                _chestOverlay.Value = null;
            }
            
            // Add new overlay
            _chestOverlay.Value = new ChestOverlay(menu, Helper.Events, Helper.Input);
        }
        
        /// <summary>
        /// Converts objects to modded storage when placed in the world.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (e.Added.Count() != 1)
                return;

            var itemPosition = e.Added.Single();
            var pos = itemPosition.Key;
            var obj = itemPosition.Value;

            if (!obj.ShouldBeExpandedStorage() || obj.IsExpandedStorage())
                return;
            
            Monitor.VerboseLog($"OnObjectListChanged: Converting to Expanded Storage Chest");
            e.Location.objects[pos] = obj.ToExpandedStorage();
        }
    }
}