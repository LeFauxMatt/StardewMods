namespace XSPlus.Features
{
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Models;
    using Common.Services;
    using Common.UI;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
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
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<ClickableTextureComponent> _configButton = new();
        private readonly PerScreen<string> _hoverText = new();
        private readonly ItemGrabMenuSideButtonsService _itemGrabMenuSideButtonsService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
        private readonly PerScreen<int> _screenId = new()
        {
            Value = -1,
        };

        private CategorizeChestFeature(
            ModConfigService modConfigService,
            ItemGrabMenuSideButtonsService itemGrabMenuSideButtonsService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("CategorizeChest", modConfigService)
        {
            this._configButton.Value = new(
                new(0, 0, 64, 64),
                Content.FromMod<Texture2D>("assets/configure.png"),
                Rectangle.Empty,
                Game1.pixelZoom);

            this._itemGrabMenuSideButtonsService = itemGrabMenuSideButtonsService;
            this._renderedActiveMenuService = renderedActiveMenuService;
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="CategorizeChestFeature" />.
        /// </summary>
        private static CategorizeChestFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="CategorizeChestFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="CategorizeChestFeature" /> class.</returns>
        public static async Task<CategorizeChestFeature> Create(ServiceManager serviceManager)
        {
            return CategorizeChestFeature.Instance ??= new(
                await serviceManager.Get<ModConfigService>(),
                await serviceManager.Get<ItemGrabMenuSideButtonsService>(),
                await serviceManager.Get<RenderedActiveMenuService>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this._itemGrabMenuSideButtonsService.AddHandler(this.SetupSideButtons);
            this._renderedActiveMenuService.AddHandler(this.DrawSideButtons);
            Events.Input.ButtonPressed += this.OnButtonPressed;
            Events.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuSideButtonsService.RemoveHandler(this.SetupSideButtons);
            this._renderedActiveMenuService.RemoveHandler(this.DrawSideButtons);
            Events.Input.ButtonPressed -= this.OnButtonPressed;
            Events.Input.CursorMoved -= this.OnCursorMoved;
        }

        private void SetupSideButtons(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._screenId.Value = -1;
                return;
            }

            this._itemGrabMenuSideButtonsService.AddButton(this._configButton.Value);
            this._screenId.Value = Context.ScreenId;
            this._returnMenu.Value = e.ItemGrabMenu;
            this._chest.Value = e.Chest;
        }

        private void DrawSideButtons(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            // Draw config button
            this._configButton.Value.draw(e.SpriteBatch);

            // Draw hover text
            if (string.IsNullOrWhiteSpace(this._returnMenu.Value.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
            {
                this._returnMenu.Value.hoverText = this._hoverText.Value;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId || e.Button != SButton.MouseLeft)
            {
                return;
            }

            var point = Game1.getMousePosition(true);
            if (this._configButton.Value.containsPoint(point.X, point.Y))
            {
                Game1.playSound("drumkit6");

                if (!this._chest.Value.modData.TryGetValue($"{XSPlus.ModPrefix}/FilterItems", out var filterItems))
                {
                    filterItems = string.Empty;
                }

                Game1.activeClickableMenu = new ItemSelectionMenu(
                    string.Empty,
                    this.ReturnToMenu,
                    filterItems,
                    value => this._chest.Value.modData[$"{XSPlus.ModPrefix}/FilterItems"] = value);
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            var point = Game1.getMousePosition(true);
            this._configButton.Value.tryHover(point.X, point.Y, 0.25f);
            this._hoverText.Value = this._configButton.Value.containsPoint(point.X, point.Y) ? Translations.Get("button.Configure.name") : string.Empty;
        }

        private void ReturnToMenu()
        {
            Game1.activeClickableMenu = this._returnMenu.Value;
        }
    }
}