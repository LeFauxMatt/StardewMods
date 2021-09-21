namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Extensions;
    using HarmonyLib;
    using Interfaces;
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
    internal class InventoryTabsFeature : FeatureWithParam<HashSet<string>>, IHighlightItemInterface
    {
        private readonly IContentHelper _contentHelper;
        private readonly IInputHelper _inputHelper;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly HighlightItemsService _highlightChestItemsService;
        private readonly RenderingActiveMenuService _renderingActiveMenuService;
        private readonly Func<KeybindList> _getPreviousTab;
        private readonly Func<KeybindList> _getNextTab;
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _tabIndex = new() { Value = -1 };
        private IList<Tab> _tabs = null!;
        private Texture2D _texture = null!;

        /// <summary>Initializes a new instance of the <see cref="InventoryTabsFeature"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="itemGrabMenuChangedService">Service to handle creation/invocation of ItemGrabMenuChanged event.</param>
        /// <param name="highlightChestItemsService">Service to handle creation/invocation of HighlightChestItems delegates.</param>
        /// <param name="renderingActiveMenuService">Service to handle creation/invocation of RenderingActiveMenu event.</param>
        /// <param name="getPreviousTab">Get method for configured previous tab button.</param>
        /// <param name="getNextTab">Get method for configured next tab button.</param>
        public InventoryTabsFeature(
            IContentHelper contentHelper,
            IInputHelper inputHelper,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            HighlightItemsService highlightChestItemsService,
            RenderingActiveMenuService renderingActiveMenuService,
            Func<KeybindList> getPreviousTab,
            Func<KeybindList> getNextTab)
            : base("InventoryTabs")
        {
            this._contentHelper = contentHelper;
            this._inputHelper = inputHelper;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._highlightChestItemsService = highlightChestItemsService;
            this._renderingActiveMenuService = renderingActiveMenuService;
            this._getPreviousTab = getPreviousTab;
            this._getNextTab = getNextTab;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.AddHandler(this.OnRenderingActiveMenu);
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            this._renderingActiveMenuService.RemoveHandler(this.OnRenderingActiveMenu);
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        }

        /// <inheritdoc/>
        public bool HighlightMethod(Item item)
        {
            return this._tabIndex.Value == -1 || item.MatchesTagExt(this._tabs[this._tabIndex.Value].Tags);
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
                this._highlightChestItemsService.RemoveHandler(this);
                this._attached.Value = false;
                return;
            }

            if (!this._attached.Value)
            {
                this._highlightChestItemsService.AddHandler(this);
                this._attached.Value = true;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this._tabIndex.Value = -1;
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
                ClickableTextureComponent? cc = this._tabs[i].Component;
                if (cc is null)
                {
                    continue;
                }

                Color color;
                cc.bounds.X = x;
                if (i == this._tabIndex.Value)
                {
                    cc.bounds.Y = y + (1 * Game1.pixelZoom);
                    color = Color.White;
                }
                else
                {
                    cc.bounds.Y = y;
                    color = Color.Gray;
                }

                cc.draw(e.SpriteBatch, color, 0.86f + (cc.bounds.Y / 20000f));
                x = cc.bounds.Right;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            KeybindList nextTabButton = this._getNextTab();
            if (nextTabButton.JustPressed())
            {
                this._tabIndex.Value++;
                if (this._tabIndex.Value == this._tabs.Count)
                {
                    this._tabIndex.Value = -1;
                }

                this._inputHelper.SuppressActiveKeybinds(nextTabButton);
                return;
            }

            KeybindList previousTabButton = this._getPreviousTab();
            if (previousTabButton.JustPressed())
            {
                this._tabIndex.Value--;
                if (this._tabIndex.Value == -2)
                {
                    this._tabIndex.Value = this._tabs.Count - 1;
                }

                this._inputHelper.SuppressActiveKeybinds(previousTabButton);
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
            Tab? tab = this._tabs.FirstOrDefault(tab => tab.Component is not null && tab.Component.containsPoint(point.X, point.Y));
            if (tab is null)
            {
                return;
            }

            // Toggle if currently active was clicked on.
            int index = this._tabs.IndexOf(tab);
            this._tabIndex.Value = this._tabIndex.Value == index ? -1 : index;
            this._inputHelper.Suppress(e.Button);
        }
    }
}