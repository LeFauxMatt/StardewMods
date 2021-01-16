using System;
using System.Collections.Generic;
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
        private readonly Action _scrollUp;
        private readonly Action _scrollDown;
        
        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int _screenId;
        
        /// <summary>The last viewport bounds.</summary>
        private Rectangle _lastViewport;

        /// <summary>The number of draw cycles since the menu was initialized.</summary>
        private int _drawCount;

        /// <summary>Returns whether the menu and its components have been initialized.</summary>
        private bool IsInitialized => _drawCount > 1;

        /// <summary>Scrolls inventory menu up one row.</summary>
        private ClickableTextureComponent _upArrow;
        
        /// <summary>Scrolls inventory menu down one row.</summary>
        private ClickableTextureComponent _downArrow;
        
        public MenuOverlay(InventoryMenu menu, IGameLoopEvents gameLoopGameLoopEvents, Func<bool> canScrollUp, Func<bool> canScrollDown, Action scrollUp, Action scrollDown)
        {
            _menu = menu;
            _gameLoopEvents = gameLoopGameLoopEvents;
            _canScrollUp = canScrollUp;
            _canScrollDown = canScrollDown;
            _scrollUp = scrollUp;
            _scrollDown = scrollDown;
            
            _screenId = Context.ScreenId;
            _lastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
            
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
            
            if (Context.ScreenId != _screenId)
                return;
            
            if (Game1.uiViewport.Width == _lastViewport.Width &&
                Game1.uiViewport.Height == _lastViewport.Height)
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
                _upArrow.draw(b);
            if (_canScrollDown.Invoke())
                _downArrow.draw(b);
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
            
            if (_upArrow.containsPoint(x, y))
            {
                _scrollUp.Invoke();
                if (playSound)
                    Game1.playSound("shwip");
                return true;
            }
            
            if (_downArrow.containsPoint(x, y))
            {
                _scrollDown.Invoke();
                if (playSound)
                    Game1.playSound("shwip");
                return true;
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
            
            _upArrow.tryHover(x, y, 0.25f);
            _downArrow.tryHover(x, y, 0.25f);
        }
    }
}