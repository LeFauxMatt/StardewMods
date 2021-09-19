namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Extensions;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class InventoryTabs : FeatureWithParam<HashSet<string>>
    {
        private readonly IContentHelper _contentHelper;
        private readonly IInputHelper _inputHelper;
        private readonly Func<KeybindList> _getPreviousTab;
        private readonly Func<KeybindList> _getNextTab;
        private readonly PerScreen<IClickableMenu> _menu = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<int> _tabIndex = new() { Value = -1 };
        private IList<Tab> _tabs;
        private Texture2D _texture;

        /// <summary>Initializes a new instance of the <see cref="InventoryTabs"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="getPreviousTab">Get method for configured previous tab button.</param>
        /// <param name="getNextTab">Get method for configured next tab button.</param>
        public InventoryTabs(IContentHelper contentHelper, IInputHelper inputHelper, Func<KeybindList> getPreviousTab, Func<KeybindList> getNextTab)
            : base("InventoryTabs")
        {
            this._contentHelper = contentHelper;
            this._inputHelper = inputHelper;
            this._getPreviousTab = getPreviousTab;
            this._getNextTab = getNextTab;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
            CommonFeature.RenderingActiveMenu += this.OnRenderingActiveMenu;
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
            CommonFeature.RenderingActiveMenu -= this.OnRenderingActiveMenu;
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;
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
                    scale: Game1.pixelZoom);
            }
        }

        private void OnItemGrabMenuChanged(object sender, CommonFeature.ItemGrabMenuChangedEventArgs e)
        {
            if (!e.Attached || !this.IsEnabledForItem(e.Chest))
            {
                CommonFeature.HighlightChestItems -= this.HighlightMethod;
                this._attached.Value = false;
                this._screenId.Value = -1;
                return;
            }

            if (!this._attached.Value)
            {
                CommonFeature.HighlightChestItems += this.HighlightMethod;
                this._attached.Value = true;
                this._screenId.Value = e.ScreenId;
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
                ClickableTextureComponent cc = this._tabs[i].Component;
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
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || e.Button != SButton.MouseLeft)
            {
                return;
            }

            // Check if any tab was clicked on.
            Point point = Game1.getMousePosition(true);
            Tab tab = this._tabs.FirstOrDefault(tab => tab.Component.containsPoint(point.X, point.Y));
            if (tab is null)
            {
                return;
            }

            // Toggle if currently active was clicked on.
            int index = this._tabs.IndexOf(tab);
            this._tabIndex.Value = this._tabIndex.Value == index ? -1 : index;
            this._inputHelper.Suppress(e.Button);
        }

        private bool HighlightMethod(Item item)
        {
            return this._tabIndex.Value == -1 || item.MatchesTagExt(this._tabs[this._tabIndex.Value].Tags);
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Record is instantiated by ContentHelper.")]
        private record Tab
        {
            public string Name { get; set; }

            public string[] Tags { get; set; }

            public ClickableTextureComponent Component { get; set; }
        }
    }
}