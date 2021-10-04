namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Helpers.ItemMatcher;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;

    /// <inheritdoc cref="FeatureWithParam{TParam}" />
    internal class InventoryTabsFeature : FeatureWithParam<HashSet<string>>
    {
        private readonly DisplayedInventoryService _displayedInventoryService;
        private readonly PerScreen<string> _hoverText = new();
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly PerScreen<ItemMatcher> _itemMatcher = new()
        {
            Value = new(string.Empty, true),
        };
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private readonly ModConfigService _modConfigService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly RenderingActiveMenuService _renderingActiveMenuService;
        private readonly PerScreen<int> _tabIndex = new()
        {
            Value = -1,
        };
        private IList<Tab> _tabs = null!;
        private Texture2D _texture = null!;

        private InventoryTabsFeature(
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            RenderingActiveMenuService renderingActiveMenuService,
            RenderedActiveMenuService renderedActiveMenuService,
            DisplayedInventoryService displayedInventoryService)
            : base("InventoryTabs", modConfigService)
        {
            this._modConfigService = modConfigService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._renderingActiveMenuService = renderingActiveMenuService;
            this._renderedActiveMenuService = renderedActiveMenuService;
            this._displayedInventoryService = displayedInventoryService;
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="InventoryTabsFeature" />.
        /// </summary>
        private static InventoryTabsFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="InventoryTabsFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="InventoryTabsFeature" /> class.</returns>
        public static async Task<InventoryTabsFeature> Create(ServiceManager serviceManager)
        {
            return InventoryTabsFeature.Instance ??= new(
                await serviceManager.Get<ModConfigService>(),
                await serviceManager.Get<ItemGrabMenuChangedService>(),
                await serviceManager.Get<RenderingActiveMenuService>(),
                await serviceManager.Get<RenderedActiveMenuService>(),
                await serviceManager.Get<DisplayedInventoryService>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.AddHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            this._displayedInventoryService.AddHandler(this.FilterMethod);
            Events.GameLoop.GameLaunched += this.OnGameLaunched;
            Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Events.Input.ButtonPressed += this.OnButtonPressed;
            Events.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.RemoveHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenuService.RemoveHandler(this.OnRenderedActiveMenu);
            this._displayedInventoryService.RemoveHandler(this.FilterMethod);
            Events.GameLoop.GameLaunched -= this.OnGameLaunched;
            Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Events.Input.ButtonPressed -= this.OnButtonPressed;
            Events.Input.CursorMoved -= this.OnCursorMoved;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._tabs = Content.FromMod<List<Tab>>("assets/tabs.json");
            this._texture = Content.FromMod<Texture2D>("assets/tabs.png");

            for (var i = 0; i < this._tabs.Count; i++)
            {
                this._tabs[i].Component = new(
                    new(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    this._texture,
                    new(16 * i, 0, 16, 16),
                    Game1.pixelZoom)
                {
                    hoverText = this._tabs[i].Name,
                };
            }
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
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
            for (var i = 0; i < this._tabs.Count; i++)
            {
                Color color;
                this._tabs[i].Component.bounds.X = x;
                if (i == this._tabIndex.Value)
                {
                    this._tabs[i].Component.bounds.Y = y + 1 * Game1.pixelZoom;
                    color = Color.White;
                }
                else
                {
                    this._tabs[i].Component.bounds.Y = y;
                    color = Color.Gray;
                }

                this._tabs[i].Component.draw(e.SpriteBatch, color, 0.86f + this._tabs[i].Component.bounds.Y / 20000f);
                x = this._tabs[i].Component.bounds.Right;
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
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            if (e.Button != SButton.MouseLeft && e.IsDown(SButton.MouseLeft))
            {
                return;
            }

            // Check if any tab was clicked on.
            var point = Game1.getMousePosition(true);
            for (var i = 0; i < this._tabs.Count; i++)
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
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Check if any tab is hovered.
            var point = Game1.getMousePosition(true);
            var tab = this._tabs.SingleOrDefault(tab => tab.Component.containsPoint(point.X, point.Y));
            this._hoverText.Value = tab is not null ? Translations.Get($"tabs.{tab.Name}.name") : string.Empty;
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

            this._displayedInventoryService.ReSyncInventory(this._menu.Value.ItemGrabMenu.ItemsToGrabMenu, true);
        }

        private bool FilterMethod(Item item)
        {
            return this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || this._itemMatcher.Value.Matches(item);
        }
    }
}