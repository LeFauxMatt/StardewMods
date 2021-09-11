using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class StashToChest : FeatureWithParam<string>
    {
        private List<Chest> EnabledChests
        {
            get
            {
                if (_updated.Value)
                    return _enabledChests.Value.ToList();
                _carriedChests.Value ??= Game1.player.Items.OfType<Chest>().Where(IsEnabled);
                _locationChests.Value ??= CommonHelper.GetChests(CommonHelper.GetAccessibleLocations(Helper.Multiplayer.GetActiveLocations)).Where(IsEnabled);
                _enabledChests.Value = _carriedChests.Value.Union(_locationChests.Value).Distinct();
                _updated.Value = true;
                return _enabledChests.Value.ToList();
            }
        }
        private readonly PerScreen<IEnumerable<Chest>> _enabledChests = new();
        private readonly PerScreen<IEnumerable<Chest>> _carriedChests = new();
        private readonly PerScreen<IEnumerable<Chest>> _locationChests = new();
        private readonly PerScreen<bool> _updated = new();
        private FilterItems _filterItems;
        public StashToChest(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
        }
        protected override void EnableFeature()
        {
            // Events
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Player.InventoryChanged += OnInventoryChanged;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        }
        protected override void DisableFeature()
        {
            // Events
            Helper.Events.GameLoop.GameLaunched -= OnGameLaunched;
            Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
            Helper.Events.Player.Warped -= OnWarped;
            Helper.Events.Input.ButtonsChanged -= OnButtonsChanged;
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _filterItems = XSPlus.Features["FilterItems"] as FilterItems;
        }
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;
            _carriedChests.Value = null;
            _updated.Value = false;
        }
        private void OnWarped(object sender, WarpedEventArgs e)
        {
            _locationChests.Value = null;
            _updated.Value = false;
        }
        /// <summary>Stash inventory items into all supported chests</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree || !XSPlus.Config.StashItems.JustPressed() || !EnabledChests.Any())
                return;
            for (var i = 0; i < Game1.player.Items.Count; i++)
            {
                var item = Game1.player.Items[i];
                if (item is null)
                    continue;
                var stack = (uint) item.Stack;
                foreach (var chest in EnabledChests)
                {
                    var allowList = _filterItems.IsEnabled(chest);
                    chest.GetModDataList("Favorites", out var favorites);
                    // Skip chest if it has favorites and none are matched
                    if (favorites.Count > 0 && !favorites.Any(search => item.SearchTag(search, XSPlus.Config.SearchTagSymbol)))
                        continue;
                    
                    // Skip chest if no favorites and no built-in filter
                    if (favorites.Count == 0 && !allowList)
                        continue;
                    
                    // Attempt to add item into chest
                    var tmp = chest.addItem(item);
                    if (tmp == null)
                    {
                        Game1.player.Items[i] = null;
                        break;
                    }
                    if (tmp.Stack != stack)
                        item.Stack = tmp.Stack;
                }
            }
            Game1.playSound("Ship");
            Helper.Input.SuppressActiveKeybinds(XSPlus.Config.StashItems);
        }
        public override bool IsEnabled(Item item)
        {
            if (!base.IsEnabled(item) || item is not Chest)
                return false;
            if (!TryGetValue(item, out var range))
                range = XSPlus.Config.CraftingRange;
            if (!_filterItems.IsEnabled(item) && (!item.GetModDataList("Favorites", out var favorites) || favorites.Count == 0))
                return false;
            return range switch
            {
                "Inventory" => Game1.player.Items.IndexOf(item) != -1,
                "Location" => Game1.currentLocation.Objects.Values.Contains(item),
                "World" => true,
                _ => false
            };
        }
    }
}