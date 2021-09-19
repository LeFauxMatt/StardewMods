namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.Extensions;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class FilterItems : FeatureWithParam<Dictionary<string, bool>>
    {
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<Dictionary<string, bool>> _filterItems = new();

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
            CommonFeature.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                prefix: new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                patch: AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(ref Item __result, Item item)
        {
            if (FilterItems.Instance._filterItems.Value is null || !item.MatchesTagExt(FilterItems.Instance._filterItems.Value))
            {
                return true;
            }

            __result = item;
            return false;
        }

        private void OnItemGrabMenuChanged(object sender, CommonFeature.ItemGrabMenuChangedEventArgs e)
        {
            if (!e.Attached || !this.IsEnabledForItem(e.Chest))
            {
                CommonFeature.HighlightPlayerItems -= this.HighlightMethod;
                this._attached.Value = false;
                this._chest.Value = null;
                this._filterItems.Value = null;
                return;
            }

            if (!this._attached.Value)
            {
                CommonFeature.HighlightChestItems += this.HighlightMethod;
                this._attached.Value = true;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this._filterItems.Value = this.TryGetValueForItem(e.Chest, out Dictionary<string, bool> filterItems) ? filterItems : null;
            }
        }

        private bool HighlightMethod(Item item)
        {
            return this._filterItems.Value is null || item.MatchesTagExt(this._filterItems.Value);
        }
    }
}