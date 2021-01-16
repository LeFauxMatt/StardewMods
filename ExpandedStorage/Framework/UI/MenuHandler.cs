using System;
using System.Collections.Generic;
using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.UI
{
    internal class MenuHandler : IDisposable
    {
        private readonly MenuOverlay _overlay;
        private readonly IInputEvents _inputEvents;
        private readonly IInputHelper _inputHelper;
        private readonly ModConfigControls _controls;

        private readonly object _context;
        private readonly IList<Item> _items;
        private readonly int _capacity;
        private readonly int _cols;
        private int _skipped;

        public int Skipped
        {
            get => (int) MathHelper.Clamp(_skipped, 0, _items.Count.RoundUp(_cols) - _capacity);
            set => _skipped = value;
        }
        private bool ContextMatches(MenuHandler handler) =>
            ReferenceEquals(_context, handler._context);
        public bool ContextMatches(InventoryMenu inventoryMenu) =>
            ReferenceEquals(_items, inventoryMenu.actualInventory);
        
        internal MenuHandler(ItemGrabMenu menu, IModEvents events, IInputHelper inputHelper, ModConfigControls controls, MenuHandler menuHandler = null)
        {
            var inventoryMenu = menu.ItemsToGrabMenu;
            
            _overlay = new MenuOverlay(inventoryMenu, events.GameLoop,
                () => _skipped > 0,
                () => _skipped < _items.Count - _capacity,
                () => Scroll(1),
                () => Scroll(-1));
            _inputEvents = events.Input;
            _inputHelper = inputHelper;
            _controls = controls;

            _context = menu.context;
            _items = inventoryMenu.actualInventory;
            _capacity = inventoryMenu.capacity;
            _cols = inventoryMenu.capacity / inventoryMenu.rows;

            if (menuHandler != null && ContextMatches(menuHandler))
                _skipped = menuHandler.Skipped;
            
            // Events
            _inputEvents.ButtonPressed += OnButtonPressed;
            _inputEvents.CursorMoved += OnCursorMoved;
            _inputEvents.MouseWheelScrolled += OnMouseWheelScrolled;
        }

        public void Dispose()
        {
            _inputEvents.ButtonPressed -= OnButtonPressed;
            _inputEvents.CursorMoved -= OnCursorMoved;
            _inputEvents.MouseWheelScrolled -= OnMouseWheelScrolled;
        }

        internal void Draw(SpriteBatch b)
        {
            _overlay.Draw(b);
        }
        
        /// <summary>Attempts to scroll offset by one row of slots relative to the inventory menu.</summary>
        /// <param name="direction">The direction which to scroll to.</param>
        /// <returns>True if the value of offset changed.</returns>
        internal bool Scroll(int direction)
        {
            if (direction > 0 && Skipped > 0)
                Skipped -= _cols;
            else if (direction < 0 && Skipped < _items.Count - _capacity)
                Skipped += _cols;
            else
                return false;
            return true;
        }
        
        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var handled = false;
            var x = Game1.getMouseX(Game1.uiMode);
            var y = Game1.getMouseY(Game1.uiMode);

            if (e.Button == _controls.ScrollDown && Scroll(-1))
                handled = true;
            else if (e.Button == _controls.ScrollUp && Scroll(1))
                handled = true;
            else if (e.Button == SButton.MouseLeft || e.Button.IsUseToolButton())
                handled = _overlay.LeftClick(x, y);
            
            if (handled)
                _inputHelper.Suppress(e.Button);
        }
        
        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            var x = Game1.getMouseX(Game1.uiMode);
            var y = Game1.getMouseY(Game1.uiMode);
            
            _overlay.Hover(x, y);
        }
        
        /// <summary>Raised after the player scrolls the mouse wheel.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
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