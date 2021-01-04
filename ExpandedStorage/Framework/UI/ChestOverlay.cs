using System;
using System.Collections.Generic;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.UI
{
    internal class ChestOverlay : IDisposable
    {
        /// <summary>Currently active ChestOverlay instances.</summary>
        private static readonly PerScreen<ChestOverlay> Instance = new PerScreen<ChestOverlay>();
        public static void DrawArrows(SpriteBatch b)
        {
            if (Instance.Value == null)
                return;
            if (Instance.Value.CanScrollUp)
                Instance.Value._upArrow?.draw(b);
            if (Instance.Value.CanScrollDown)
                Instance.Value._downArrow?.draw(b);
        }
        
        /// <summary>Returns the number of skipped slots from the ManagedInventory instance.</summary>
        public static int Offset(InventoryMenu menu)
        {
            if (Instance.Value == null)
                return 0;
            return Instance.Value._offset <=
                   menu.actualInventory.Count.RoundUp(menu.capacity / menu.rows) - menu.capacity
                ? Instance.Value._offset
                : 0;
        }

        private readonly IModEvents _events;
        private readonly IInputHelper _inputHelper;
        
        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int _screenId;
        
        /// <summary>The last viewport bounds.</summary>
        private Rectangle _lastViewport;

        /// <summary>The number of draw cycles since the menu was initialized.</summary>
        private int _drawCount;

        /// <summary>Returns whether the menu and its components have been initialized.</summary>
        private bool IsInitialized => _drawCount > 1;
        
        /// <summary>The Chest Menu to overlay on to.</summary>
        internal ItemGrabMenu Menu { get; private set; }
        
        /// <summary>The Chest Inventory that is currently being accessed.</summary>
        private readonly IList<Item> _items;
        private readonly int _capacity;
        private readonly int _cols;
        private int _offset;

        /// <summary>Scrolls inventory menu up one row.</summary>
        private ClickableTextureComponent _upArrow;
        
        /// <summary>Scrolls inventory menu down one row.</summary>
        private ClickableTextureComponent _downArrow;

        /// <summary>Unregister Event Handling</summary>
        public void Dispose()
        {
            Instance.Value = null;
            Menu = null;
            _events.GameLoop.UpdateTicked -= OnUpdateTicked;
            _events.Display.Rendered -= OnRendered;
            _events.Input.ButtonPressed -= OnButtonPressed;
            _events.Input.CursorMoved -= OnCursorMoved;
            _events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
        }
        public ChestOverlay(ItemGrabMenu menu, IModEvents events, IInputHelper inputHelper)
        {
            Instance.Value = this;
            Menu = menu;
            _events = events;
            _inputHelper = inputHelper;
            
            _items = menu.ItemsToGrabMenu.actualInventory;
            _capacity = menu.ItemsToGrabMenu.capacity;
            _cols = menu.ItemsToGrabMenu.capacity / menu.ItemsToGrabMenu.rows;
            
            _screenId = Context.ScreenId;
            _lastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
            
            // Events
            events.GameLoop.UpdateTicked += OnUpdateTicked;
            events.Display.Rendered += OnRendered;
            events.Input.ButtonPressed += OnButtonPressed;
            events.Input.CursorMoved += OnCursorMoved;
            events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
        }
        
        /// <summary></summary>
        private void InitComponents()
        {
            var bounds = new Rectangle(Menu.ItemsToGrabMenu.xPositionOnScreen, Menu.ItemsToGrabMenu.yPositionOnScreen, Menu.ItemsToGrabMenu.width, Menu.ItemsToGrabMenu.height);
            
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

        private bool CanScrollUp => _offset > 0;
        private bool CanScrollDown => _offset < _items.Count - _capacity;
        
        /// <summary>Attempts to scroll offset by one row of slots relative to the inventory menu.</summary>
        /// <param name="direction">The direction which to scroll to.</param>
        /// <returns>True if the value of offset changed.</returns>
        private bool Scroll(int direction)
        {
            if (direction > 0 && _offset > 0)
                _offset -= _cols;
            else if (direction < 0 && _offset < _items.Count - _capacity)
                _offset += _cols;
            else
                return false;
            return true;
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

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            if (Context.ScreenId != _screenId)
                return;
            
            if (_drawCount == 0)
                InitComponents();
            _drawCount++;
        }

        /// <summary>Raised after the player pressed a keyboard, mouse, or controller button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return;
            
            var handled = false;
            var x = Game1.getMouseX(Game1.uiMode);
            var y = Game1.getMouseY(Game1.uiMode);
            
            if (e.Button == SButton.MouseLeft || e.Button.IsUseToolButton())
            {
                if (_upArrow.containsPoint(x, y) && Scroll(1))
                {
                    handled = true;
                    Game1.playSound("shwip");
                }
                else if (_downArrow.containsPoint(x, y) && Scroll(-1))
                {
                    handled = true;
                    Game1.playSound("shwip");
                }
            }

            if (handled)
                _inputHelper.Suppress(e.Button);
        }

        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return;
            
            var x = Game1.getMouseX(Game1.uiMode);
            var y = Game1.getMouseY(Game1.uiMode);

            _upArrow.tryHover(x, y, 0.25f);
            _downArrow.tryHover(x, y, 0.25f);
        }

        /// <summary>Raised after the player scrolls the mouse wheel.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return;

            if (!Scroll(e.Delta))
                return;
            
            var cur = Game1.oldMouseState;
            Game1.oldMouseState = new MouseState(
                x: cur.X,
                y: cur.Y,
                scrollWheel: e.NewValue,
                leftButton: cur.LeftButton,
                middleButton: cur.MiddleButton,
                rightButton: cur.RightButton,
                xButton1: cur.XButton1,
                xButton2: cur.XButton2
            );
        }
    }
}