using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class CraftFromChest : FeatureWithParam<string>
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
        public CraftFromChest(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
        }
        protected override void EnableFeature()
        {
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                postfix: new HarmonyMethod(typeof(CraftFromChest), nameof(CraftFromChest_getContainerContents_postfix))
            );
            
            // Events
            Helper.Events.Player.InventoryChanged += OnInventoryChanged;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        }
        protected override void DisableFeature()
        {
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                patch: AccessTools.Method(typeof(CraftFromChest), nameof(CraftFromChest_getContainerContents_postfix))
            );
            
            // Events
            Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
            Helper.Events.Player.Warped -= OnWarped;
            Helper.Events.Input.ButtonsChanged -= OnButtonsChanged;
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
        /// <summary>Open crafting menu for all chests in inventory</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree || !XSPlus.Config.OpenCrafting.JustPressed() || !EnabledChests.Any())
                return;
            var mutexes = EnabledChests.Select(chest => chest.mutex).ToList();
            MultipleMutexRequest multipleMutexRequest = null;
            multipleMutexRequest = new MultipleMutexRequest( 
                mutexes: mutexes,
                success_callback: delegate
                {
                    var width = 800 + IClickableMenu.borderWidth * 2;
                    var height = 600 + IClickableMenu.borderWidth * 2;
                    var pos = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
                    Game1.activeClickableMenu = new CraftingPage((int) pos.X, (int) pos.Y, width, height, false, true, EnabledChests)
                    {
                        exitFunction = delegate { multipleMutexRequest.ReleaseLocks(); }
                    };
                },
                failure_callback: delegate
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
                });
            Helper.Input.SuppressActiveKeybinds(XSPlus.Config.OpenCrafting);
        }
        public override bool IsEnabled(Item item)
        {
            if (!base.IsEnabled(item) || item is not Chest)
                return false;
            if (!TryGetValue(item, out var range))
                range = XSPlus.Config.CraftingRange;
            return range switch
            {
                "Inventory" => Game1.player.Items.IndexOf(item) != -1,
                "Location" => Game1.currentLocation.Objects.Values.Contains(item),
                "World" => true,
                _ => false
            };
        }
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void CraftFromChest_getContainerContents_postfix(CraftingPage __instance, ref IList<Item> __result)
        {
            if (__instance._materialContainers == null)
                return;
            __result.Clear();
            var items = new List<Item>();
            foreach (var chest in __instance._materialContainers)
            {
                items.AddRange(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
            }
            __result = items;
        }
    }
}