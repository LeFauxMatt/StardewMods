namespace XSPlus.Features
{
    using Common.Helpers;
    using Common.UI;
    using Models;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CategorizeChestFeature : BaseFeature
    {
        private readonly ModConfigService _modConfigService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
        private readonly PerScreen<Chest> _chest = new();

        private CategorizeChestFeature(
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("CategorizeChest")
        {
            this._modConfigService = modConfigService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._renderedActiveMenuService = renderedActiveMenuService;
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="CategorizeChestFeature"/>.
        /// </summary>
        private static CategorizeChestFeature Instance { get; set; }

        /// <inheritdoc/>
        public override void Activate()
        {
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
            this._renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChanged);
            this._renderedActiveMenuService.RemoveHandler(this.OnRenderedActiveMenu);
            Events.Input.ButtonPressed -= this.OnButtonPressed;
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="CategorizeChestFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="CategorizeChestFeature"/> class.</returns>
        public static CategorizeChestFeature GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            var renderedActiveMenuService = serviceManager.RequestService<RenderedActiveMenuService>();
            return CategorizeChestFeature.Instance ??= new CategorizeChestFeature(modConfigService, itemGrabMenuChangedService, renderedActiveMenuService);
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null/* || !this.IsEnabledForItem(e.Chest)*/)
            {
                this._screenId.Value = -1;
                return;
            }

            this._screenId.Value = Context.ScreenId;
            this._returnMenu.Value = e.ItemGrabMenu;
            this._chest.Value = e.Chest;
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            // Draw config button
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            if (e.Button == SButton.F9)
            {
                if (!this._chest.Value.modData.TryGetValue($"{XSPlus.ModPrefix}/FilterItems", out var filterItems))
                {
                    filterItems = string.Empty;
                }

                Game1.activeClickableMenu = new ItemSelectionMenu(
                    this._modConfigService.ModConfig.SearchTagSymbol,
                    this.ReturnToMenu,
                    filterItems,
                    value => this._chest.Value.modData[$"{XSPlus.ModPrefix}/FilterItems"] = value);
            }
        }

        private void ReturnToMenu()
        {
            Game1.activeClickableMenu = this._returnMenu.Value;
        }
    }
}