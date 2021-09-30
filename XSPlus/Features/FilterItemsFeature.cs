namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.Helpers.ItemMatcher;
    using Common.Models;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc cref="FeatureWithParam{TParam}" />
    internal class FilterItemsFeature : FeatureWithParam<Dictionary<string, bool>>
    {
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly HighlightItemsService _highlightPlayerItemsService;
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<Dictionary<string, bool>> _filterItems = new();
        private readonly PerScreen<ItemMatcher> _itemMatcher = new() { Value = new ItemMatcher(string.Empty, true) };
        private MixInfo _addItemPatch;

        private FilterItemsFeature(
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            HighlightItemsService highlightPlayerItemsService)
            : base("FilterItems", modConfigService)
        {
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._highlightPlayerItemsService = highlightPlayerItemsService;
        }

        /// <summary>
        /// Gets the instance of <see cref="FilterItemsFeature"/>.
        /// </summary>
        protected internal static FilterItemsFeature Instance { get; private set; }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="FilterItemsFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="FilterItemsFeature"/> class.</returns>
        public static FilterItemsFeature GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            var highlightPlayerItemsService = serviceManager.RequestService<HighlightItemsService>("HighlightPlayerItems");
            return FilterItemsFeature.Instance ??= new FilterItemsFeature(
                modConfigService,
                itemGrabMenuChangedService,
                highlightPlayerItemsService);
        }

        /// <inheritdoc/>
        public override void Activate()
        {
            // Events
            this._highlightPlayerItemsService.AddHandler(this.HighlightMethod);
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);

            // Patches
            this._addItemPatch = Mixin.Prefix(
                AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                typeof(FilterItemsFeature),
                nameof(FilterItemsFeature.Chest_addItem_prefix));
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._highlightPlayerItemsService.RemoveHandler(this.HighlightMethod);

            // Patches
            Mixin.Unpatch(this._addItemPatch);
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
        {
            if (!FilterItemsFeature.Instance.TryGetValueForItem(__instance, out Dictionary<string, bool> filterItems))
            {
                return true;
            }

            var itemMatcher = new ItemMatcher(string.Empty, true);
            itemMatcher.SetSearch(filterItems);
            if (itemMatcher.Matches(item))
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
                this._attached.Value = false;
                this._filterItems.Value = null;
                this._itemMatcher.Value.SetSearch(string.Empty);
                return;
            }

            if (!this._attached.Value)
            {
                this._attached.Value = true;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                if (this.TryGetValueForItem(e.Chest, out Dictionary<string, bool> filterItems))
                {
                    this._itemMatcher.Value.SetSearch(filterItems);
                }
                else
                {
                    this._itemMatcher.Value.SetSearch(string.Empty);
                }
            }
        }

        private bool HighlightMethod(Item item)
        {
            return !this._attached.Value || this._itemMatcher.Value.Matches(item);
        }
    }
}