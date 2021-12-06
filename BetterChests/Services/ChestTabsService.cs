namespace BetterChests.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers.ItemMatcher;
    using Common.Services;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;

    internal class ChestTabsService : BaseService, IFeatureService
    {
        private readonly PerScreen<string> _hoverText = new();
        private readonly PerScreen<ItemMatcher> _itemMatcher = new(() => new(string.Empty, true));
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private readonly PerScreen<int> _tabIndex = new(() => -1);
        private readonly IModHelper _helper;
        private DisplayedInventoryService _displayedInventory;
        private ItemGrabMenuChangedService _itemGrabMenuChanged;
        private RenderedActiveMenuService _renderedActiveMenu;
        private RenderingActiveMenuService _renderingActiveMenu;
        private ModConfig _modConfig;
        private Texture2D _texture;

        private ChestTabsService(ServiceManager serviceManager)
            : base("ChestTabs")
        {
            // Init
            this._helper = serviceManager.Helper;

            // Dependencies
            this.AddDependency<ModConfigService>(service => this._modConfig = (service as ModConfigService)?.ModConfig);
            this.AddDependency<ItemGrabMenuChangedService>(service => this._itemGrabMenuChanged = service as ItemGrabMenuChangedService);
            this.AddDependency<RenderingActiveMenuService>(service => this._renderingActiveMenu = service as RenderingActiveMenuService);
            this.AddDependency<RenderedActiveMenuService>(service => this._renderedActiveMenu = service as RenderedActiveMenuService);
            this.AddDependency<DisplayedInventoryService>(service => this._displayedInventory = service as DisplayedInventoryService);
        }

        internal IList<Tab> Tabs { get; private set; }

        /// <inheritdoc />
        public void Activate()
        {
            // Events
            this._itemGrabMenuChanged.AddHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenu.AddHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenu.AddHandler(this.OnRenderedActiveMenu);
            this._displayedInventory.AddHandler(this.FilterMethod);
            this._helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this._helper.Events.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <inheritdoc />
        public void Deactivate()
        {
            // Events
            this._itemGrabMenuChanged.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenu.RemoveHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenu.RemoveHandler(this.OnRenderedActiveMenu);
            this._displayedInventory.RemoveHandler(this.FilterMethod);
            this._helper.Events.GameLoop.GameLaunched -= this.OnGameLaunched;
            this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this._helper.Events.Input.CursorMoved -= this.OnCursorMoved;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Tabs = this._helper.Content.Load<List<Tab>>("assets/tabs.json");
            this._texture = this._helper.Content.Load<Texture2D>("assets/tabs.png");

            for (var i = 0; i < this.Tabs.Count; i++)
            {
                this.Tabs[i].Component = new(
                    new(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    this._texture,
                    new(16 * i, 0, 16, 16),
                    Game1.pixelZoom)
                {
                    hoverText = this.Tabs[i].Name,
                };
            }
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null)
            {
                this._menu.Value = null;
                return;
            }

            if (this._menu.Value is null || !ReferenceEquals(e.Chest, this._menu.Value.Chest))
            {
                this._menu.Value = e;
                this.SetTab(-1);
            }
            else
            {
                this._menu.Value = e;
            }
        }

        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Draw tabs between inventory menus along a horizontal axis
            var x = this._menu.Value.ItemGrabMenu.ItemsToGrabMenu.xPositionOnScreen;
            var y = this._menu.Value.ItemGrabMenu.ItemsToGrabMenu.yPositionOnScreen + this._menu.Value.ItemGrabMenu.ItemsToGrabMenu.height + 1 * Game1.pixelZoom;
            for (var i = 0; i < this.Tabs.Count; i++)
            {
                Color color;
                this.Tabs[i].Component.bounds.X = x;
                if (i == this._tabIndex.Value)
                {
                    this.Tabs[i].Component.bounds.Y = y + 1 * Game1.pixelZoom;
                    color = Color.White;
                }
                else
                {
                    this.Tabs[i].Component.bounds.Y = y;
                    color = Color.Gray;
                }

                this.Tabs[i].Component.draw(e.SpriteBatch, color, 0.86f + this.Tabs[i].Component.bounds.Y / 20000f);
                x = this.Tabs[i].Component.bounds.Right;
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(this._menu.Value.ItemGrabMenu.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
            {
                this._menu.Value.ItemGrabMenu.hoverText = this._hoverText.Value;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            if (this._modConfig.NextTab.JustPressed())
            {
                this.SetTab(this._tabIndex.Value == this.Tabs.Count ? -1 : this._tabIndex.Value + 1);
                this._helper.Input.SuppressActiveKeybinds(this._modConfig.NextTab);
                return;
            }

            if (this._modConfig.PreviousTab.JustPressed())
            {
                this.SetTab(this._tabIndex.Value == -1 ? this.Tabs.Count - 1 : this._tabIndex.Value - 1);
                this._helper.Input.SuppressActiveKeybinds(this._modConfig.PreviousTab);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            if (e.Button != SButton.MouseLeft && !e.IsDown(SButton.MouseLeft))
            {
                return;
            }

            // Check if any tab was clicked on.
            var point = Game1.getMousePosition(true);
            for (var i = 0; i < this.Tabs.Count; i++)
            {
                if (this.Tabs[i].Component.containsPoint(point.X, point.Y))
                {
                    this.SetTab(this._tabIndex.Value == i ? -1 : i);
                    this._helper.Input.Suppress(e.Button);
                }
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Check if any tab is hovered.
            var (x, y) = Game1.getMousePosition(true);
            var tab = this.Tabs.SingleOrDefault(tab => tab.Component.containsPoint(x, y));
            this._hoverText.Value = tab is not null ? this._helper.Translation.Get($"tabs.{tab.Name}.name") : string.Empty;
        }

        private void SetTab(int index)
        {
            this._tabIndex.Value = index;
            var tab = this.Tabs.ElementAtOrDefault(index);
            if (tab is not null)
            {
                this._itemMatcher.Value.SetSearch(tab.Tags);
            }
            else
            {
                this._itemMatcher.Value.SetSearch(string.Empty);
            }

            this._displayedInventory.ReSyncInventory(this._menu.Value.ItemGrabMenu.ItemsToGrabMenu, true);
        }

        private bool FilterMethod(Item item)
        {
            return this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || this._itemMatcher.Value.Matches(item);
        }
    }
}