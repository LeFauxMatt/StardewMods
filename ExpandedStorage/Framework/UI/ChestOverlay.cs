using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.UI
{
    internal class ChestOverlay : IDisposable
    {
        private readonly IModEvents _events;
        private readonly IInputHelper _inputHelper;
        
        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int _screenId;
        
        /// <summary>The last viewport bounds.</summary>
        private Rectangle _lastViewport;

        /// <summary>The number of draw cycles since the menu was initialized.</summary>
        private int _drawCount;
        
        /// <summary>Returns whether the menu and its components have been initialized.</summary>
        protected bool IsInitialized => _drawCount > 1;
        
        /// <summary>The Chest Menu to overlay on to.</summary>
        internal ItemGrabMenu Menu { get; }
        
        /// <summary>Scrolls inventory menu up one row.</summary>
        private ClickableTextureComponent _upArrow;
        
        /// <summary>Scrolls inventory menu down one row.</summary>
        private ClickableTextureComponent _downArrow;
        
        /// <summary>Search field for filtering displayed items by name.</summary>
        private TextBox _searchField;
        
        /// <summary>Unregister Event Handling</summary>
        public void Dispose()
        {
            _events.GameLoop.UpdateTicked -= OnUpdateTicked;
            _events.Display.Rendered -= OnRendered;
            _events.Input.ButtonPressed -= OnButtonPressed;
            _events.Input.CursorMoved -= OnCursorMoved;
            _events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
        }
        
        public ChestOverlay(ItemGrabMenu itemGrabMenu, IModEvents events, IInputHelper inputHelper)
        {
            Menu = itemGrabMenu;
            _events = events;
            _inputHelper = inputHelper;
            _screenId = Context.ScreenId;
            _lastViewport = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
            
            // Events
            events.GameLoop.UpdateTicked += OnUpdateTicked;
            events.Display.Rendered += OnRendered;
            events.Input.ButtonPressed += OnButtonPressed;
            events.Input.CursorMoved += OnCursorMoved;
            events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitComponents()
        {
            var x = Menu.xPositionOnScreen;
            var y = Menu.yPositionOnScreen;
            var right = x + Menu.width;
            var height = Menu.height;
            
            _upArrow = new ClickableTextureComponent(
                new Rectangle(right - 32, y - 64, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
                Game1.mouseCursors,
                new Rectangle(421, 459, 11, 12),
                Game1.pixelZoom);
            
            _downArrow = new ClickableTextureComponent(
                new Rectangle(_upArrow.bounds.X, _upArrow.bounds.Y + height / 2 - 64, _upArrow.bounds.Width, _upArrow.bounds.Height),
                Game1.mouseCursors,
                new Rectangle(421, 472, 11, 12),
                Game1.pixelZoom);
            
        }

        /// <summary>The method invoked when the player presses a button.</summary>
        /// <param name="input">The button that was pressed.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveButtonPress(SButton input)
        {
            if (!IsInitialized)
                return false;
            return true;
        }

        /// <summary>The method invoked when the player uses the mouse scroll wheel.</summary>
        /// <param name="amount">The scroll amount.</param>
        /// <returns>Whether the event has been handled and shouldn't be propagated further.</returns>
        protected virtual bool ReceiveScrollWheelAction(int amount)
        {
            if (!IsInitialized)
                return false;
            return true;
        }

        /// <summary>Draw the mouse cursor.</summary>
        /// <remarks>Derived from <see cref="StardewValley.Menus.IClickableMenu.drawMouse"/>.</remarks>
        protected void DrawCursor()
        {
            if (Game1.options.hardwareCursor)
                return;
            var cursorPos = new Vector2(Game1.getMouseX(), Game1.getMouseY());
            Game1.spriteBatch.Draw(
                Game1.mouseCursors,
                cursorPos,
                Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.SnappyMenus ? 44 : 0, 16, 16),
            Color.White * Game1.mouseCursorTransparency,
                0.0f,
                Vector2.Zero,
            Game1.pixelZoom + Game1.dialogueButtonScale / 150f,
                SpriteEffects.None,
                1f);
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

            _upArrow.draw(Game1.spriteBatch);
            _downArrow.draw(Game1.spriteBatch);
            _searchField.Draw(Game1.spriteBatch);
            
            DrawCursor();
        }

        /// <summary>Raised after the player pressed a keyboard, mouse, or controller button.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return;
            
            var handled = false;
            var nativeZoomLevel = (float)(typeof(Game1).GetProperty("NativeZoomLevel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).GetValue(null));
            var x = (int)(Game1.getMouseX() * Game1.options.zoomLevel / nativeZoomLevel);
            var y = (int)(Game1.getMouseY() * Game1.options.zoomLevel / nativeZoomLevel);
            
            if (e.Button == SButton.MouseLeft || e.Button.IsUseToolButton())
            {
                if (_upArrow.containsPoint(x, y))
                {
                    handled = true;
                }
                else if (_downArrow.containsPoint(x, y))
                {
                    handled = true;
                }
            }
            else
            {
                handled = ReceiveButtonPress(e.Button);
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

            var handled = false;
            var x = Game1.getMouseX(Game1.uiMode);
            var y = Game1.getMouseY(Game1.uiMode);

            _upArrow.tryHover(x, y);
            _downArrow.tryHover(x, y);
            
            if (handled)
                Game1.InvalidateOldMouseMovement();
        }

        /// <summary>Raised after the player scrolls the mouse wheel.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (Context.ScreenId != _screenId || !IsInitialized)
                return;

            var handled = false;
            handled = ReceiveScrollWheelAction(e.Delta);
            
            if (!handled)
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