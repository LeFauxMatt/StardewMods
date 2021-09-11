using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Extensions;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class FilterItems : FeatureWithParam<Dictionary<string, bool>>
    {
        private static FilterItems _feature;
        private readonly PerScreen<IClickableMenu> _oldMenu = new();
        private readonly PerScreen<Chest> _context = new();
        public FilterItems(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Events
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                prefix: new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix))
            );
        }
        protected override void DisableFeature()
        {
            // Events
            Helper.Events.Display.MenuChanged -= OnMenuChanged;
            
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                patch: AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix))
            );
        }
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, _oldMenu.Value))
                return;
            _oldMenu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu || !IsEnabled(chest))
            {
                CommonHelper.HighlightMethods_inventory -= HighlightMethod;
                _context.Value = null;
            }
            else if (_context.Value is null)
            {
                CommonHelper.HighlightMethods_inventory += HighlightMethod;
                _context.Value = chest;
            }
        }
        /// <summary>Returns true if chest can accept the item</summary>
        internal bool TakesItem(Chest chest, Item item)
        {
            return !TryGetValue(chest, out var filterItems) || item.MatchesTagExt(filterItems);
        }
        private bool HighlightMethod(Item item) => _context.Value is null || TakesItem(_context.Value, item);
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
        {
            if (!_feature.IsEnabled(__instance) || _feature.TakesItem(__instance, item))
                return true;
            __result = item;
            return false;
        }
    }
}