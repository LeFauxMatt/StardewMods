namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.Extensions;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class FilterItems : FeatureWithParam<Dictionary<string, bool>>
    {
        private readonly PerScreen<IClickableMenu> Menu = new();
        private readonly PerScreen<Chest> Chest = new();

        /// <summary>Initializes a new instance of the <see cref="FilterItems"/> class.</summary>
        public FilterItems()
            : base("FilterItems")
        {
            FilterItems.Instance = this;
        }

        /// <summary>Gets the instance of <see cref="FilterItems"/>.</summary>
        protected internal static FilterItems Instance { get; private set; }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Display.MenuChanged += this.OnMenuChanged;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(StardewValley.Objects.Chest.addItem)),
                prefix: new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Display.MenuChanged -= this.OnMenuChanged;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(StardewValley.Objects.Chest.addItem)),
                patch: AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));
        }

        /// <summary>Returns true if chest can accept the item.</summary>
        /// <param name="chest">The chest to check if it accepts an item.</param>
        /// <param name="item">The item to check against the chest.</param>
        /// <returns>Returns true if chest accepts item and does not reject it.</returns>
        internal bool TakesItem(Chest chest, Item item)
        {
            return !this.TryGetValueForItem(chest, out var filterItems) || item.MatchesTagExt(filterItems);
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
        {
            if (!FilterItems.Instance.IsEnabledForItem(__instance) || FilterItems.Instance.TakesItem(__instance, item))
            {
                return true;
            }

            __result = item;
            return false;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, this.Menu.Value))
            {
                return;
            }

            this.Menu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } || !this.IsEnabledForItem(chest))
            {
                CommonFeature.HighlightPlayerItems -= this.HighlightMethod;
                this.Chest.Value = null;
                return;
            }

            if (this.Chest.Value is null)
            {
                CommonFeature.HighlightPlayerItems += this.HighlightMethod;
                this.Chest.Value = chest;
            }
        }

        private bool HighlightMethod(Item item)
        {
            return this.Chest.Value is null || this.TakesItem(this.Chest.Value, item);
        }
    }
}