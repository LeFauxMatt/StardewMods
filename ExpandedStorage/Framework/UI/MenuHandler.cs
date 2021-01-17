using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using ExpandedStorage.Framework.Models;
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
        private readonly IModEvents _events;
        private readonly IInputHelper _inputHelper;
        private readonly ModConfigControls _controls;

        private readonly object _context;
        private readonly IList<Item> _items;
        private readonly List<Item> _filteredItems = new List<Item>();
        private readonly int _capacity;
        private readonly int _cols;
        private int _skipped;
        private ExpandedStorageTab _currentTab;
        
        public IList<Item> Items
        {
            get
            {
                if (_currentTab == null)
                    return _items.Skip(_skipped).ToList();
                _filteredItems.Clear();
                _filteredItems.AddRange(_items.Where(item => Allowed(item.Category) && !Blocked(item.Category)));
                _skipped = (int) MathHelper.Clamp(_skipped, 0, _filteredItems.Count().RoundUp(_cols) - _capacity);
                return _filteredItems.Skip(_skipped).ToList();
            }
        }

        private bool Allowed(int category) =>
            !_currentTab.AllowList.Any() || _currentTab.AllowList.Contains(category);
        private bool Blocked(int category) =>
            _currentTab.BlockList.Any() && _currentTab.AllowList.Contains(category);
        private bool ContextMatches(MenuHandler handler) =>
            ReferenceEquals(_context, handler._context);
        public bool ContextMatches(InventoryMenu inventoryMenu) =>
            ReferenceEquals(_items, inventoryMenu.actualInventory);
        
        internal MenuHandler(ItemGrabMenu menu, IModEvents events, IInputHelper inputHelper, ModConfigControls controls, MenuHandler menuHandler = null)
        {
            var inventoryMenu = menu.ItemsToGrabMenu;
            var config = menu.context is Item item ? ExpandedStorage.GetConfig(item) : null;
            var tabs = config != null
                ? config.Tabs.Select(t => ExpandedStorage.GetTab($"{config.ModUniqueId}/{t}")).ToList()
                : new List<ExpandedStorageTab>();
            
            _events = events;
            _inputHelper = inputHelper;
            _controls = controls;
            
            _context = menu.context;
            _items = inventoryMenu.actualInventory;
            _capacity = inventoryMenu.capacity;
            _cols = inventoryMenu.capacity / inventoryMenu.rows;
            
            if (menuHandler != null && ContextMatches(menuHandler))
            {
                _skipped = menuHandler._skipped;
                _currentTab = menuHandler._currentTab;
            }
            
            _overlay = new MenuOverlay(inventoryMenu, tabs, events.GameLoop,
                () => _skipped > 0,
                () => _skipped < Items.Count - _capacity,
                Scroll,
                SetTab,
                _currentTab?.TabName);

            // Events
            _events.Input.ButtonPressed += OnButtonPressed;
            _events.Input.CursorMoved += OnCursorMoved;
            _events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
        }

        public void Dispose()
        {
            _overlay.Dispose();
            UnregisterEvents();
        }

        public void UnregisterEvents()
        {
            _events.Input.ButtonPressed -= OnButtonPressed;
            _events.Input.CursorMoved -= OnCursorMoved;
            _events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
        }

        internal void Draw(SpriteBatch b)
        {
            _overlay.Draw(b);
        }
        
        internal void DrawUnder(SpriteBatch b)
        {
            _overlay.DrawUnder(b);
        }
        
        /// <summary>Attempts to scroll offset by one row of slots relative to the inventory menu.</summary>
        /// <param name="direction">The direction which to scroll to.</param>
        /// <returns>True if the value of offset changed.</returns>
        private bool Scroll(int direction)
        {
            if (direction > 0 && _skipped > 0)
                _skipped -= _cols;
            else if (direction < 0 && _skipped < Items.Count - _capacity)
                _skipped += _cols;
            else
                return false;
            return true;
        }

        private void SetTab(ExpandedStorageTab tab)
        {
            _currentTab = tab;
            _skipped = 0;
        }

        /// <summary>Track if configured control buttons are pressed or pass input to overlay.</summary>
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