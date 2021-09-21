namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.Extensions;
    using HarmonyLib;
    using Interfaces;
    using Models;
    using Services;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc cref="FeatureWithParam{TParam}" />
    internal class FilterItemsFeature : FeatureWithParam<Dictionary<string, bool>>, IHighlightItemInterface
    {
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly HighlightItemsService _highlightPlayerItemsService;
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<Dictionary<string, bool>?> _filterItems = new();

        /// <summary>Initializes a new instance of the <see cref="FilterItemsFeature"/> class.</summary>
        /// <param name="itemGrabMenuChangedService">Service to handle creation/invocation of ItemGrabMenuChanged event.</param>
        /// <param name="highlightPlayerItemsService">Service to handle creation/invocation of HighlightPlayerItems delegates.</param>
        public FilterItemsFeature(
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            HighlightItemsService highlightPlayerItemsService)
            : base("FilterItems")
        {
            FilterItemsFeature.Instance = this;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._highlightPlayerItemsService = highlightPlayerItemsService;
        }

        /// <summary>Gets the instance of <see cref="FilterItemsFeature"/>.</summary>
        protected internal static FilterItemsFeature Instance { get; private set; } = null!;

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                prefix: new HarmonyMethod(typeof(FilterItemsFeature), nameof(FilterItemsFeature.Chest_addItem_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                patch: AccessTools.Method(typeof(FilterItemsFeature), nameof(FilterItemsFeature.Chest_addItem_prefix)));
        }

        /// <inheritdoc/>
        public bool HighlightMethod(Item item)
        {
            return this._filterItems.Value is null || item.MatchesTagExt(this._filterItems.Value);
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(ref Item __result, Item item)
        {
            if (FilterItemsFeature.Instance._filterItems.Value is null || !item.MatchesTagExt(FilterItemsFeature.Instance._filterItems.Value))
            {
                return true;
            }

            __result = item;
            return false;
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._highlightPlayerItemsService.RemoveHandler(this);
                this._attached.Value = false;
                this._filterItems.Value = null;
                return;
            }

            if (!this._attached.Value)
            {
                this._highlightPlayerItemsService.AddHandler(this);
                this._attached.Value = true;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this._filterItems.Value = this.TryGetValueForItem(e.Chest, out Dictionary<string, bool>? filterItems) ? filterItems : null;
            }
        }
    }
}