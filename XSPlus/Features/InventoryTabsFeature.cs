namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers;
    using Common.Helpers.ItemMatcher;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc cref="FeatureWithParam{TParam}" />
    internal class InventoryTabsFeature : FeatureWithParam<HashSet<string>>
    {
        private readonly ModConfigService _modConfigService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly RenderingActiveMenuService _renderingActiveMenuService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly DisplayedInventoryService _displayedChestInventoryService;
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<ItemGrabMenu> _menu = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<int> _tabIndex = new() { Value = -1 };
        private readonly PerScreen<ItemMatcher> _itemMatcher = new() { Value = new ItemMatcher(string.Empty, true) };
        private readonly PerScreen<string> _hoverText = new();
        private IList<Tab> _tabs = null!;
        private Texture2D _texture = null!;

        private InventoryTabsFeature(
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            DisplayedInventoryService displayedChestInventoryService,
            RenderingActiveMenuService renderingActiveMenuService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("InventoryTabs")
        {
            this._modConfigService = modConfigService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._displayedChestInventoryService = displayedChestInventoryService;
            this._renderingActiveMenuService = renderingActiveMenuService;
            this._renderedActiveMenuService = renderedActiveMenuService;
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="InventoryTabsFeature"/>.
        /// </summary>
        private static InventoryTabsFeature Instance { get; set; }

        /// <inheritdoc/>
        public override void Activate()
        {
            // Events
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.AddHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            this._displayedChestInventoryService.AddHandler(this.FilterMethod);
            Events.GameLoop.GameLaunched += this.OnGameLaunched;
            Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Events.Input.ButtonPressed += this.OnButtonPressed;
            Events.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.RemoveHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenuService.RemoveHandler(this.OnRenderedActiveMenu);
            this._displayedChestInventoryService.RemoveHandler(this.FilterMethod);
            Events.GameLoop.GameLaunched -= this.OnGameLaunched;
            Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Events.Input.ButtonPressed -= this.OnButtonPressed;
            Events.Input.CursorMoved -= this.OnCursorMoved;
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="InventoryTabsFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="InventoryTabsFeature"/> class.</returns>
        public static InventoryTabsFeature GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            var displayedChestInventoryService = serviceManager.RequestService<DisplayedInventoryService>("DisplayedChestInventory");
            var renderingActiveMenuService = serviceManager.RequestService<RenderingActiveMenuService>();
            var renderedActiveMenuService = serviceManager.RequestService<RenderedActiveMenuService>();
            return InventoryTabsFeature.Instance ??= new InventoryTabsFeature(
                modConfigService,
                itemGrabMenuChangedService,
                displayedChestInventoryService,
                renderingActiveMenuService,
                renderedActiveMenuService);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._tabs = Content.FromMod<List<Tab>>("assets/tabs.json");
            this._texture = Content.FromMod<Texture2D>("assets/tabs.png");

            for (int i = 0; i < this._tabs.Count; i++)
            {
                this._tabs[i].Component = new ClickableTextureComponent(
                    bounds: new Rectangle(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    texture: this._texture,
                    sourceRect: new Rectangle(16 * i, 0, 16, 16),
                    scale: Game1.pixelZoom)
                {
                    hoverText = this._tabs[i].Name,
                };
            }
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._screenId.Value = -1;
                return;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this.SetTab(-1);
            }

            this._screenId.Value = Context.ScreenId;
            this._menu.Value = e.ItemGrabMenu;
        }

        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            // Draw tabs between inventory menus along a horizontal axis
            int x = this._menu.Value.ItemsToGrabMenu.xPositionOnScreen;
            int y = this._menu.Value.ItemsToGrabMenu.yPositionOnScreen + this._menu.Value.ItemsToGrabMenu.height + (1 * Game1.pixelZoom);
            for (int i = 0; i < this._tabs.Count; i++)
            {
                Color color;
                this._tabs[i].Component.bounds.X = x;
                if (i == this._tabIndex.Value)
                {
                    this._tabs[i].Component.bounds.Y = y + (1 * Game1.pixelZoom);
                    color = Color.White;
                }
                else
                {
                    this._tabs[i].Component.bounds.Y = y;
                    color = Color.Gray;
                }

                this._tabs[i].Component.draw(e.SpriteBatch, color, 0.86f + (this._tabs[i].Component.bounds.Y / 20000f));
                x = this._tabs[i].Component.bounds.Right;
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(this._menu.Value.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
            {
                this._menu.Value.hoverText = this._hoverText.Value;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            if (this._modConfigService.ModConfig.NextTab.JustPressed())
            {
                this.SetTab(this._tabIndex.Value == this._tabs.Count ? -1 : this._tabIndex.Value + 1);
                this._tabIndex.Value++;
                if (this._tabIndex.Value == this._tabs.Count)
                {
                    this._tabIndex.Value = -1;
                }

                Input.Suppress(this._modConfigService.ModConfig.NextTab);
                return;
            }

            if (this._modConfigService.ModConfig.PreviousTab.JustPressed())
            {
                this.SetTab(this._tabIndex.Value == -1 ? this._tabs.Count - 1 : this._tabIndex.Value - 1);
                Input.Suppress(this._modConfigService.ModConfig.PreviousTab);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            if (e.Button != SButton.MouseLeft && e.IsDown(SButton.MouseLeft))
            {
                return;
            }

            // Check if any tab was clicked on.
            var point = Game1.getMousePosition(true);
            for (int i = 0; i < this._tabs.Count; i++)
            {
                if (this._tabs[i].Component.containsPoint(point.X, point.Y))
                {
                    this.SetTab(this._tabIndex.Value == i ? -1 : i);
                    Input.Suppress(e.Button);
                }
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            // Check if any tab is hovered.
            var point = Game1.getMousePosition(true);
            var tab = this._tabs.SingleOrDefault(tab => tab.Component.containsPoint(point.X, point.Y));
            this._hoverText.Value = tab is not null ? Locale.Get($"tabs.{tab.Name}.name") : string.Empty;
        }

        private void SetTab(int index)
        {
            this._tabIndex.Value = index;
            var tab = this._tabs.ElementAtOrDefault(index);
            if (tab is not null)
            {
                this._itemMatcher.Value.SetSearch(tab.Tags);
            }
            else
            {
                this._itemMatcher.Value.SetSearch(string.Empty);
            }
        }

        private bool FilterMethod(Item item)
        {
            return this._screenId.Value != Context.ScreenId || this._itemMatcher.Value.Matches(item);
        }
    }
}