namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers.ItemMatcher;
    using HarmonyLib;
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
        private readonly IContentHelper _contentHelper;
        private readonly IInputHelper _inputHelper;
        private readonly ITranslationHelper _translationHelper;
        private readonly ModConfigService _modConfigService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly DisplayedInventoryService _displayedChestInventoryService;
        private readonly RenderingActiveMenuService _renderingActiveMenuService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _tabIndex = new() { Value = -1 };
        private readonly PerScreen<ItemMatcher> _itemMatcher = new() { Value = new ItemMatcher(string.Empty, true) };
        private readonly PerScreen<string> _hoverText = new();
        private IList<Tab> _tabs = null!;
        private Texture2D _texture = null!;

        /// <summary>Initializes a new instance of the <see cref="InventoryTabsFeature"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="translationHelper">Provides translations stored in the mod's i18n folder.</param>
        /// <param name="modConfigService">Service to handle read/write to ModConfig.</param>
        /// <param name="itemGrabMenuChangedService">Service to handle creation/invocation of ItemGrabMenuChanged event.</param>
        /// <param name="displayedChestInventoryService">Service for manipulating the displayed items in an inventory menu.</param>
        /// <param name="renderingActiveMenuService">Service to handle creation/invocation of RenderingActiveMenu event.</param>
        /// <param name="renderedActiveMenuService">Service to handle creation/invocation of RenderedActiveMenu event.</param>
        public InventoryTabsFeature(
            IContentHelper contentHelper,
            IInputHelper inputHelper,
            ITranslationHelper translationHelper,
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            DisplayedInventoryService displayedChestInventoryService,
            RenderingActiveMenuService renderingActiveMenuService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("InventoryTabs")
        {
            this._contentHelper = contentHelper;
            this._inputHelper = inputHelper;
            this._translationHelper = translationHelper;
            this._modConfigService = modConfigService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._displayedChestInventoryService = displayedChestInventoryService;
            this._renderingActiveMenuService = renderingActiveMenuService;
            this._renderedActiveMenuService = renderedActiveMenuService;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.AddHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;
            modEvents.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.RemoveHandler(this.OnRenderingActiveMenu);
            this._renderedActiveMenuService.RemoveHandler(this.OnRenderedActiveMenu);
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;
            modEvents.Input.CursorMoved -= this.OnCursorMoved;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._tabs = this._contentHelper.Load<List<Tab>>("assets/tabs.json");
            this._texture = this._contentHelper.Load<Texture2D>("assets/tabs.png");

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
                this._displayedChestInventoryService.RemoveHandler(this.FilterMethod);
                this._attached.Value = false;
                return;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this.SetTab(-1);
            }

            if (!this._attached.Value)
            {
                this._displayedChestInventoryService.AddHandler(this.FilterMethod);
                this._attached.Value = true;
            }
        }

        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (!this._attached.Value || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
            {
                return;
            }

            // Draw tabs between inventory menus along a horizontal axis
            int x = itemGrabMenu.ItemsToGrabMenu.xPositionOnScreen;
            int y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen + itemGrabMenu.ItemsToGrabMenu.height + (1 * Game1.pixelZoom);
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
            if (!this._attached.Value || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(itemGrabMenu.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
            {
                itemGrabMenu.hoverText = this._hoverText.Value;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!this._attached.Value)
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

                this._inputHelper.SuppressActiveKeybinds(this._modConfigService.ModConfig.NextTab);
                return;
            }

            if (this._modConfigService.ModConfig.PreviousTab.JustPressed())
            {
                this.SetTab(this._tabIndex.Value == -1 ? this._tabs.Count - 1 : this._tabIndex.Value - 1);
                this._inputHelper.SuppressActiveKeybinds(this._modConfigService.ModConfig.PreviousTab);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!this._attached.Value || e.Button != SButton.MouseLeft)
            {
                return;
            }

            // Check if any tab was clicked on.
            Point point = Game1.getMousePosition(true);
            for (int i = 0; i < this._tabs.Count; i++)
            {
                if (this._tabs[i].Component.containsPoint(point.X, point.Y))
                {
                    this.SetTab(this._tabIndex.Value == i ? -1 : i);
                    this._inputHelper.Suppress(e.Button);
                }
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            // Check if any tab is hovered.
            Point point = Game1.getMousePosition(true);
            Tab? tab = this._tabs.SingleOrDefault(tab => tab.Component.containsPoint(point.X, point.Y));
            this._hoverText.Value = tab is not null ? this._translationHelper.Get($"tabs.{tab.Name}.name") : string.Empty;
        }

        private void SetTab(int index)
        {
            this._tabIndex.Value = index;
            Tab? tab = this._tabs.ElementAtOrDefault(index);
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
            return this._itemMatcher.Value.Matches(item);
        }
    }
}