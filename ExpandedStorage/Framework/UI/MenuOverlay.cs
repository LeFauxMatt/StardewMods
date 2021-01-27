using System;
using System.Collections.Generic;
using System.Linq;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.UI
{
    internal class MenuOverlay : IDisposable
    {
        private readonly InventoryMenu _menu;
        private readonly IGameLoopEvents _gameLoopEvents;
        private readonly Func<bool> _canScrollUp;
        private readonly Func<bool> _canScrollDown;
        private readonly Func<int, bool> _scroll;
        private readonly Action<TabContentData> _setTab;
        private readonly Action<string> _search;
        
        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int _screenId;
        
        /// <summary>The last viewport bounds.</summary>
        private Rectangle _lastViewport;

        /// <summary>The number of draw cycles since the menu was initialized.</summary>
        private int _drawCount;

        /// <summary>Returns whether the menu and its components have been initialized.</summary>
        private bool IsInitialized => _drawCount > 1;

        /// <summary>Scrolls inventory menu up one row.</summary>
        private readonly ClickableTextureComponent _upArrow;
        
        /// <summary>Scrolls inventory menu down one row.</summary>
        private readonly ClickableTextureComponent _downArrow;

        /// <summary>Input to filter items by name or context tags.</summary>
        private readonly TextBox _searchField;

        /// <summary>Corresponds to the bounds of the searchField.</summary>
        private readonly ClickableComponent _searchArea;

        /// <summary>Icon to display next to search box.</summary>
        private readonly ClickableTextureComponent _searchIcon;

        /// <summary>Chest menu tab components.</summary>
        private readonly IList<ClickableTextureComponent> _tabs = new List<ClickableTextureComponent>();

        /// <summary>Tabs configured for Chest Menu</summary>
        private readonly IList<TabContentData> _tabConfigs;

        /// <summary>Currently selected tab.</summary>
        internal TabContentData CurrentTab { get; set; }

        /// <summary>Current search text.</summary>
        internal string SearchText
        {
            get => _searchField?.Text;
            set
            {
                if (_searchField != null)
                    _searchField.Text = value;
            }
        }
        
        /// <summary>Previous search text.</summary>
        private string _searchText;

        /// <summary>Y-Position for tabs when not selected.</summary>
        private int _tabY;

        /// <summary>Draw hoverText over chest menu.</summary>
        private string _hoverText;

        public bool SearchFocused => _searchField != null && _searchField.Selected;
        
        public MenuOverlay(InventoryMenu menu, IList<TabContentData> tabConfigs, IGameLoopEvents gameLoopGameLoopEvents, ModConfig config, StorageConfig storageConfig,
            Func<bool> canScrollUp,
            Func<bool> canScrollDown,
            Func<int, bool> scroll,
            Action<TabContentData> setTab,
            Action<string> search)
        {
            _menu = menu;
            _tabConfigs = tabConfigs;
            _gameLoopEvents = gameLoopGameLoopEvents;
            _canScrollUp = canScrollUp;
            _canScrollDown = canScrollDown;
            _scroll = scroll;
            _setTab = setTab;
            _search = search;

            _screenId = Context.ScreenId;
            _lastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
            var bounds = new Rectangle(_menu.xPositionOnScreen, _menu.yPositionOnScreen, _menu.width, _menu.height);
            
            if (config.ShowOverlayArrows)
            {
                _upArrow = new ClickableTextureComponent(
                    new Rectangle(bounds.Right + 8, bounds.Y - 40, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
                    Game1.mouseCursors,
                    new Rectangle(421, 459, 11, 12),
                    Game1.pixelZoom);
            
                _downArrow = new ClickableTextureComponent(
                    new Rectangle(_upArrow.bounds.X, bounds.Bottom - 36, _upArrow.bounds.Width, _upArrow.bounds.Height),
                    Game1.mouseCursors,
                    new Rectangle(421, 472, 11, 12),
                    Game1.pixelZoom);
            }

            if (config.ShowSearchBar && storageConfig.ShowSearchBar)
            {
                _searchField = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = bounds.X,
                    Y = bounds.Y - 14 * Game1.pixelZoom,
                    Width = bounds.Width,
                    Text = SearchText,
                    Selected = false
                };

                _searchArea = new ClickableComponent(new Rectangle(_searchField.X, _searchField.Y, _searchField.Width, _searchField.Height), "");

                _searchIcon = new ClickableTextureComponent(
                    new Rectangle(bounds.Right - 38, bounds.Y - 14 * Game1.pixelZoom + 6, 32, 32),
                    Game1.mouseCursors,
                    new Rectangle(80, 0, 13, 13),
                    2.5f);
            }

            if (config.ShowTabs)
            {
                _tabs.Clear();
                var xPosition = bounds.Left;
                _tabY = bounds.Bottom + 1 * Game1.pixelZoom;
                for (var i = 0; i < _tabConfigs.Count; i++)
                {
                    var tab = new ClickableTextureComponent(
                        new Rectangle(xPosition, _tabY, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                        _tabConfigs[i].Texture,
                        Rectangle.Empty,
                        Game1.pixelZoom)
                    {
                        name = i.ToString(),
                        hoverText = _tabConfigs[i].TabName
                    };
                    _tabs.Add(tab);
                    xPosition += tab.bounds.Width;
                }
            }
            
            // Events
            _gameLoopEvents.UpdateTicked += OnUpdateTicked;
        }
        
        /// <summary>Unregister Event Handling</summary>
        public void Dispose()
        {
            _gameLoopEvents.UpdateTicked -= OnUpdateTicked;
        }
        
        /// <summary></summary>
        private void InitComponents()
        {
            var bounds = new Rectangle(_menu.xPositionOnScreen, _menu.yPositionOnScreen, _menu.width, _menu.height);

            if (_upArrow != null)
            {
                _upArrow.bounds.X = bounds.Right + 8;
                _upArrow.bounds.Y = bounds.Y - 40;
            }

            if (_downArrow != null)
            {
                _downArrow.bounds.X = bounds.Right + 8;
                _downArrow.bounds.Y = bounds.Bottom - 36;
            }

            if (_searchField != null)
            {
                _searchField.X = bounds.X;
                _searchField.Y = bounds.Y - 14 * Game1.pixelZoom;
                _searchArea.bounds.X = _searchField.X;
                _searchArea.bounds.Y = _searchField.Y;
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
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.HasScreenId(_screenId) || !(Game1.activeClickableMenu is ItemGrabMenu))
            {
                Dispose();
                return;
            }

            if (_searchField != null && SearchText != _searchText)
            {
                _searchText = SearchText;
                _search.Invoke(_searchText);
            }
            
            if (Context.ScreenId != _screenId)
                return;
            
            if (Game1.uiViewport.Width == _lastViewport.Width && Game1.uiViewport.Height == _lastViewport.Height)
                return;
            
            // Resize Event
            var viewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
            InitComponents();
            _lastViewport = viewport;
        }

        /// <summary>Draws overlay to screen</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        internal void Draw(SpriteBatch b)
        {
            if (Context.ScreenId != _screenId)
                return;
            
            if (_drawCount == 0)
                InitComponents();
            _drawCount++;
            
            if (_canScrollUp.Invoke())
                _upArrow?.draw(b);
            if (_canScrollDown.Invoke())
                _downArrow?.draw(b);
            
            _searchField?.Draw(b, false);
            _searchIcon?.draw(b);

            if (_hoverText != null)
                IClickableMenu.drawHoverText(b, _hoverText, Game1.smallFont);
        }

        /// <summary>Draws overlay to screen</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        internal void DrawUnder(SpriteBatch b)
        {
            if (Context.ScreenId != _screenId)
                return;
            
            if (_drawCount == 0)
                InitComponents();
            _drawCount++;

            for (var i = 0; i < _tabConfigs.Count; i++)
            {
                var isSelectedTab = ReferenceEquals(CurrentTab, _tabConfigs[i]);
                var c = isSelectedTab ? Color.White : Color.Gray;
                _tabs[i].bounds.Y = _tabY + (isSelectedTab ? Game1.pixelZoom : 0);
                _tabs[i].draw(b, c * 1f, 0.86f + _tabs[i].bounds.Y / 20000f);
            }
        }

        /// <summary>Suppress input when search field is selected.</summary>
        /// <param name="button">The button that was pressed</param>
        /// <returns>True when an interaction occurs</returns>
        internal bool ReceiveKeyPress(SButton button)
        {
            if (button == SButton.Escape && _searchField != null &&_searchField.Selected)
                _searchField.Selected = false;
            return _searchField != null && _searchField.Selected;
        }

        /// <summary>Handles Left-Click interaction with overlay elements</summary>
        /// <param name="x">x-coordinate of left-click</param>
        /// <param name="y">Y-Coordinate of left-click</param>
        /// <param name="playSound">Whether sound should be enabled for click</param>
        /// <returns>True when an interaction occurs</returns>
        internal bool LeftClick(int x, int y, bool playSound = true)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return false;

            if (_searchField != null)
            {
                _searchField.Selected = _searchArea.containsPoint(x, y);
                if (_searchField.Selected)
                    return true;
            }

            if (_upArrow != null && _upArrow.containsPoint(x, y))
            {
                _scroll.Invoke(1);
                if (playSound)
                    Game1.playSound("shwip");
                return true;
            }
            
            if (_downArrow != null && _downArrow.containsPoint(x, y))
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
            CurrentTab = ReferenceEquals(CurrentTab, _tabConfigs[i]) ? null : _tabConfigs[i];
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
            if (Context.ScreenId != _screenId || !IsInitialized)
                return false;

            // ReSharper disable once InvertIf
            if (_searchField != null)
            {
                _searchField.Selected = _searchArea.containsPoint(x, y);
                if (_searchField.Selected)
                {
                    _searchField.Text = "";
                    _search.Invoke(_searchField.Text);
                    return true;
                }
            }

            return false;
        }

        /// <summary>Handles Hover interaction with overlay elements</summary>
        /// <param name="x">x-coordinate of mouse</param>
        /// <param name="y">Y-Coordinate of mouse</param>
        internal void Hover(int x, int y)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return;
            
            _upArrow?.tryHover(x, y, 0.25f);
            _downArrow?.tryHover(x, y, 0.25f);
            _searchField?.Hover(x, y);

            var tab = _tabs.FirstOrDefault(t => t.containsPoint(x, y));
            _hoverText = tab?.hoverText;
        }
    }
}