using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.UI
{
    internal class MenuView : IDisposable
    {
        private static readonly PerScreen<MenuView> Instance = new();

        private readonly InventoryMenu _menu;

        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int _screenId;

        private readonly Action<int> _scroll;
        private readonly Action<string> _search;

        /// <summary>Corresponds to the bounds of the searchField.</summary>
        private readonly ClickableComponent _searchArea;

        /// <summary>Icon to display next to search box.</summary>
        private readonly ClickableTextureComponent _searchIcon;

        private readonly Action<int> _setTab;

        /// <summary>Chest menu tab components.</summary>
        private readonly IList<ClickableTextureComponent> _tabs = new List<ClickableTextureComponent>();

        /// <summary>Scrolls inventory menu down one row.</summary>
        internal readonly ClickableTextureComponent DownArrow;

        /// <summary>Input to filter items by name or context tags.</summary>
        internal readonly TextBox SearchField;

        /// <summary>Scrolls inventory menu up one row.</summary>
        internal readonly ClickableTextureComponent UpArrow;

        /// <summary>The number of draw cycles since the menu was initialized.</summary>
        private int _drawCount;

        /// <summary>Draw hoverText over chest menu.</summary>
        private string _hoverText;

        /// <summary>The last viewport bounds.</summary>
        private Rectangle _lastViewport;

        /// <summary>Y-Position for tabs when not selected.</summary>
        private int _tabY;

        /// <summary>Currently selected tab.</summary>
        internal int CurrentTab;

        protected internal MenuView(
            InventoryMenu menu,
            MenuViewOptions config,
            Action<int> scroll,
            Action<int> setTab,
            Action<string> search)
        {
            Instance.Value = this;
            _screenId = Context.ScreenId;

            _menu = menu;
            _scroll = scroll;
            _setTab = setTab;
            _search = search;

            var bounds = new Rectangle(_menu.xPositionOnScreen, _menu.yPositionOnScreen, _menu.width, _menu.height);
            _tabY = bounds.Bottom + 1 * Game1.pixelZoom;

            if (config.ShowArrows)
            {
                UpArrow = new ClickableTextureComponent(
                    new Rectangle(bounds.Right + 8, bounds.Y - 40, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
                    Game1.mouseCursors,
                    new Rectangle(421, 459, 11, 12),
                    Game1.pixelZoom);

                DownArrow = new ClickableTextureComponent(
                    new Rectangle(UpArrow.bounds.X, bounds.Bottom - 36, UpArrow.bounds.Width, UpArrow.bounds.Height),
                    Game1.mouseCursors,
                    new Rectangle(421, 472, 11, 12),
                    Game1.pixelZoom);
            }

            if (config.ShowSearch)
            {
                SearchField = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = bounds.X,
                    Y = bounds.Y - 14 * Game1.pixelZoom,
                    Width = bounds.Width,
                    Selected = false
                };

                _searchArea = new ClickableComponent(new Rectangle(SearchField.X, SearchField.Y, SearchField.Width, SearchField.Height), "");

                _searchIcon = new ClickableTextureComponent(
                    new Rectangle(bounds.Right - 38, bounds.Y - 14 * Game1.pixelZoom + 6, 32, 32),
                    Game1.mouseCursors,
                    new Rectangle(80, 0, 13, 13),
                    2.5f);
            }

            CurrentTab = -1;
        }

        /// <summary>Returns whether the menu and its components have been initialized.</summary>
        private bool IsInitialized => _drawCount > 1;

        /// <summary>Unregister Event Handling</summary>
        public void Dispose()
        {
            Instance.Value = null;
        }

        public void AddTab(Texture2D texture, string name)
        {
            var lastTab = _tabs.LastOrDefault();
            var i = _tabs.Count;
            var x = lastTab?.bounds.Right ?? _menu.xPositionOnScreen;
            var tab = new ClickableTextureComponent(
                new Rectangle(x, _tabY, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                texture,
                Rectangle.Empty,
                Game1.pixelZoom)
            {
                name = i.ToString(),
                hoverText = name
            };
            _tabs.Add(tab);
        }

        /// <summary></summary>
        private void InitComponents()
        {
            var bounds = new Rectangle(_menu.xPositionOnScreen, _menu.yPositionOnScreen, _menu.width, _menu.height);

            if (UpArrow != null)
            {
                UpArrow.bounds.X = bounds.Right + 8;
                UpArrow.bounds.Y = bounds.Y - 40;
            }

            if (DownArrow != null)
            {
                DownArrow.bounds.X = bounds.Right + 8;
                DownArrow.bounds.Y = bounds.Bottom - 36;
            }

            if (SearchField != null)
            {
                SearchField.X = bounds.X;
                SearchField.Y = bounds.Y - 14 * Game1.pixelZoom;
                _searchArea.bounds.X = SearchField.X;
                _searchArea.bounds.Y = SearchField.Y;
                _searchIcon.bounds.X = bounds.Right - 38;
                _searchIcon.bounds.Y = bounds.Y - 14 * Game1.pixelZoom + 6;
            }

            _tabY = bounds.Bottom + 1 * Game1.pixelZoom;
            var xPosition = bounds.Left;
            foreach (var tab in _tabs)
            {
                tab.bounds.X = xPosition;
                tab.bounds.Y = _tabY;
                xPosition += tab.bounds.Width;
            }

            _lastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
        }

        /// <summary>Draws overlay to screen (above menu but below tooltips/hover items)</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        public static void DrawOverlay(SpriteBatch b)
        {
            if (Instance.Value == null || Instance.Value._screenId != Context.ScreenId)
                return;

            if (Instance.Value._drawCount == 0
                || Game1.uiViewport.Width != Instance.Value._lastViewport.Width
                || Game1.uiViewport.Height != Instance.Value._lastViewport.Height)
                Instance.Value.InitComponents();

            Instance.Value._drawCount++;

            Instance.Value.UpArrow?.draw(b);
            Instance.Value.DownArrow?.draw(b);
            Instance.Value.SearchField?.Draw(b, false);
            Instance.Value._searchIcon?.draw(b);

            if (Instance.Value._hoverText != null)
                IClickableMenu.drawHoverText(b, Instance.Value._hoverText, Game1.smallFont);
        }

        /// <summary>Draws underlay to screen (below menu)</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        public static void DrawUnderlay(SpriteBatch b)
        {
            if (Instance.Value == null || Instance.Value._screenId != Context.ScreenId)
                return;

            if (Instance.Value._drawCount == 0
                || Game1.uiViewport.Width != Instance.Value._lastViewport.Width
                || Game1.uiViewport.Height != Instance.Value._lastViewport.Height)
                Instance.Value.InitComponents();

            Instance.Value._drawCount++;

            for (var i = 0; i < Instance.Value._tabs.Count; i++)
            {
                Instance.Value._tabs[i].bounds.Y = Instance.Value._tabY + (Instance.Value.CurrentTab == i ? 1 * Game1.pixelZoom : 0);
                Instance.Value._tabs[i].draw(b,
                    Instance.Value.CurrentTab == i ? Color.White : Color.Gray,
                    0.86f + Instance.Value._tabs[i].bounds.Y / 20000f);
            }
        }

        /// <summary>Suppress input when search field is selected.</summary>
        /// <param name="button">The button that was pressed</param>
        /// <returns>True when an interaction occurs</returns>
        internal bool ReceiveKeyPress(SButton button)
        {
            if (!IsInitialized || Context.ScreenId != _screenId)
                return false;

            if (button != SButton.Escape)
                return SearchField != null && SearchField.Selected;

            Game1.playSound("bigDeSelect");
            Game1.activeClickableMenu = null;
            return true;
        }

        /// <summary>Handles Left-Click interaction with overlay elements</summary>
        /// <param name="x">x-coordinate of left-click</param>
        /// <param name="y">Y-Coordinate of left-click</param>
        /// <param name="playSound">Whether sound should be enabled for click</param>
        /// <returns>True when an interaction occurs</returns>
        internal bool LeftClick(int x, int y, bool playSound = true)
        {
            if (!IsInitialized || Context.ScreenId != _screenId)
                return false;

            if (SearchField != null)
            {
                SearchField.Selected = _searchArea.containsPoint(x, y);
                if (SearchField.Selected)
                    return true;
            }

            if (UpArrow != null && UpArrow.containsPoint(x, y))
            {
                _scroll.Invoke(1);
                if (playSound)
                    Game1.playSound("shwip");
                return true;
            }

            if (DownArrow != null && DownArrow.containsPoint(x, y))
            {
                _scroll.Invoke(-1);
                if (playSound)
                    Game1.playSound("shwip");
                return true;
            }

            var tab = _tabs.FirstOrDefault(t => t.containsPoint(x, y));
            if (tab == null)
                return false;

            var i = Convert.ToInt32(tab.name);
            CurrentTab = CurrentTab == i ? -1 : i;
            _setTab.Invoke(CurrentTab);
            if (playSound)
                Game1.playSound("smallSelect");
            return true;
        }

        /// <summary>Handles Right-Click interaction with overlay elements</summary>
        /// <param name="x">x-coordinate of left-click</param>
        /// <param name="y">Y-Coordinate of left-click</param>
        /// <param name="playSound">Whether sound should be enabled for click</param>
        /// <returns>True when an interaction occurs</returns>
        // ReSharper disable once UnusedParameter.Global
        internal bool RightClick(int x, int y, bool playSound = true)
        {
            if (!IsInitialized || Context.ScreenId != _screenId)
                return false;

            if (SearchField == null)
                return false;

            SearchField.Selected = _searchArea.containsPoint(x, y);
            if (!SearchField.Selected)
                return false;

            SearchField.Text = "";
            _search.Invoke(SearchField.Text);
            return true;
        }

        /// <summary>Handles Hover interaction with overlay elements</summary>
        /// <param name="x">x-coordinate of mouse</param>
        /// <param name="y">Y-Coordinate of mouse</param>
        internal void Hover(int x, int y)
        {
            if (!IsInitialized || Context.ScreenId != _screenId)
                return;

            UpArrow?.tryHover(x, y, 0.25f);
            DownArrow?.tryHover(x, y, 0.25f);
            SearchField?.Hover(x, y);

            var tab = _tabs.FirstOrDefault(t => t.containsPoint(x, y));
            _hoverText = tab?.hoverText;
        }
    }
}