namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Helpers.ItemMatcher;
    using CommonHarmony;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc cref="FeatureWithParam{TParam}" />
    internal class FilterItemsFeature : FeatureWithParam<Dictionary<string, bool>>
    {
        private readonly ItemMatcher _addItemMatcher = new(string.Empty, true);
        private readonly HighlightItemsService _highlightItemsService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly PerScreen<ItemMatcher> _itemMatcher = new()
        {
            Value = new(string.Empty, true),
        };
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private MixInfo _addItemPatch;
        private MixInfo _automatePatch;

        private FilterItemsFeature(
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            HighlightItemsService highlightItemsService)
            : base("FilterItems", modConfigService)
        {
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._highlightItemsService = highlightItemsService;
        }

        /// <summary>
        ///     Gets the instance of <see cref="FilterItemsFeature" />.
        /// </summary>
        private static FilterItemsFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="FilterItemsFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="FilterItemsFeature" /> class.</returns>
        public static async Task<FilterItemsFeature> Create(ServiceManager serviceManager)
        {
            return FilterItemsFeature.Instance ??= new(
                await serviceManager.Get<ModConfigService>(),
                await serviceManager.Get<ItemGrabMenuChangedService>(),
                await serviceManager.Get<HighlightItemsService>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this._highlightItemsService.AddHandler(this.HighlightMethod);
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);

            // Patches
            this._addItemPatch = Mixin.Prefix(
                AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                typeof(FilterItemsFeature),
                nameof(FilterItemsFeature.Chest_addItem_prefix));

            if (ModRegistry.IsLoaded("Pathochild.Automate"))
            {
                this._automatePatch = Mixin.Prefix(
                    new AssemblyPatch("Automate").Method("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer", "Store"),
                    typeof(FilterItemsFeature),
                    nameof(FilterItemsFeature.Automate_Store_prefix));
            }
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._highlightItemsService.RemoveHandler(this.HighlightMethod);

            // Patches
            Mixin.Unpatch(this._addItemPatch);
            if (this._automatePatch is not null)
            {
                Mixin.Unpatch(this._automatePatch);
            }
        }

        /// <inheritdoc />
        protected override bool IsEnabledForItem(Item item)
        {
            return base.IsEnabledForItem(item) || item is Chest chest && chest.playerChest.Value && chest.modData.TryGetValue($"{XSPlus.ModPrefix}/FilterItems", out var filterItems) && !string.IsNullOrWhiteSpace(filterItems);
        }

        public bool HasFilterItems(Chest chest)
        {
            return this.IsEnabledForItem(chest);
        }

        public bool Matches(Chest chest, Item item, ItemMatcher itemMatcher = null)
        {
            if (!this.IsEnabledForItem(chest))
            {
                return true;
            }

            itemMatcher ??= this._addItemMatcher;

            // Mod configured filter
            if (this.TryGetValueForItem(chest, out var modFilterItems))
            {
                itemMatcher.SetSearch(modFilterItems);
            }
            else
            {
                itemMatcher.SetSearch(string.Empty);
            }

            // Player configured filter
            itemMatcher.AddSearch(chest.GetFilterItems());

            return itemMatcher.Matches(item);
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
        {
            if (FilterItemsFeature.Instance.Matches(__instance, item))
            {
                return true;
            }

            __result = item;
            return false;
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool Automate_Store_prefix(Chest ___Chest, object stack)
        {
            return FilterItemsFeature.Instance.Matches(
                ___Chest,
                Reflection.Property<Item>(stack, "Sample").GetValue());
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._menu.Value = null;
                return;
            }

            this._menu.Value = e;
        }

        private bool HighlightMethod(Item item)
        {
            return this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || this.Matches(this._menu.Value.Chest, item, this._itemMatcher.Value);
        }
    }
}