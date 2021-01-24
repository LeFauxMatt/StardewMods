using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.UI
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal class MenuHandler : IDisposable
    {
        private readonly MenuOverlay _overlay;
        private readonly IModEvents _events;
        private readonly IInputHelper _inputHelper;
        private readonly ModConfigKeys _keys;

        private readonly InventoryMenu _menu;
        private readonly object _context;
        private readonly IList<Item> _items;
        private IList<Item> _filteredItems;
        private readonly int _capacity;
        private readonly int _cols;

        private int _skipped;
        private readonly IList<TabContentData> _tabConfigs;
        private string _searchText;

        public IList<Item> Items => _filteredItems ?? _items;

        public bool SearchFocused => _overlay != null && _overlay.SearchFocused;
        
        internal MenuHandler(ItemGrabMenu menu, IModEvents events, IInputHelper inputHelper, ModConfig config, MenuHandler menuHandler = null)
        {
            _menu = menu.ItemsToGrabMenu;
            _events = events;
            _inputHelper = inputHelper;
            _keys = config.Keys;
            
            _context = menu.context;
            _items = _menu.actualInventory;
            _capacity = _menu.capacity;
            _cols = _menu.capacity / _menu.rows;
            
            var storageConfig = menu.context is Item item ? ExpandedStorage.GetConfig(item) : null;
            
            if (storageConfig == null)
                return;

            _tabConfigs = storageConfig.Tabs
                    .Select(t => ExpandedStorage.GetTab($"{storageConfig.ModUniqueId}/{t}"))
                    .Where(t => t != null)
                    .ToList();
            
            _overlay = new MenuOverlay(_menu, _tabConfigs, events.GameLoop, config,
                () => CanScrollUp,
                () => CanScrollDown,
                Scroll,
                SetTab,
                SetSearch);
            
            if (menuHandler != null && ContextMatches(menuHandler))
            {
                _skipped = menuHandler._skipped;
                _overlay.CurrentTab = menuHandler._overlay.CurrentTab;
            }
            
            RefreshList();
            
            // Events
            _events.Input.ButtonsChanged += OnButtonsChanged;
            _events.Input.ButtonPressed += OnButtonPressed;
            _events.Input.CursorMoved += OnCursorMoved;
            _events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
            
            switch (_context)
            {
                case Chest chest:
                    chest.items.OnElementChanged += ItemsOnElementChanged;
                    break;
                case JunimoHut junimoHut:
                    junimoHut.output.Value.items.OnElementChanged += ItemsOnElementChanged;
                    break;
                case GameLocation location:
                    var farm = location as Farm ?? Game1.getFarm();
                    var shippingBin = farm.getShippingBin(Game1.player);
                    shippingBin.OnValueAdded += ShippingBinOnValueChanged;
                    shippingBin.OnValueRemoved += ShippingBinOnValueChanged;
                    break;
            }
        }

        public void Dispose()
        {
            _overlay?.Dispose();
            UnregisterEvents();
            switch (_context)
            {
                case Chest chest:
                    chest.items.OnElementChanged -= ItemsOnElementChanged;
                    break;
                case GameLocation location:
                    var farm = location as Farm ?? Game1.getFarm();
                    var shippingBin = farm.getShippingBin(Game1.player);
                    shippingBin.OnValueAdded -= ShippingBinOnValueChanged;
                    shippingBin.OnValueRemoved -= ShippingBinOnValueChanged;
                    break;
                case JunimoHut junimoHut:
                    junimoHut.output.Value.items.OnElementChanged -= ItemsOnElementChanged;
                    break;
            }
        }
        
        public void UnregisterEvents()
        {
            _events.Input.ButtonsChanged -= OnButtonsChanged;
            _events.Input.ButtonPressed -= OnButtonPressed;
            _events.Input.CursorMoved -= OnCursorMoved;
            _events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
        }

        internal void Draw(SpriteBatch b) =>
            _overlay?.Draw(b);
        
        internal void DrawUnder(SpriteBatch b) =>
            _overlay?.DrawUnder(b);

        /// <summary>Attempts to scroll offset by one row of slots relative to the inventory menu.</summary>
        /// <param name="direction">The direction which to scroll to.</param>
        /// <returns>True if the value of offset changed.</returns>
        private bool Scroll(int direction)
        {
            switch (direction)
            {
                case > 0 when CanScrollUp:
                    _skipped -= _cols;
                    break;
                case < 0 when CanScrollDown:
                    _skipped += _cols;
                    break;
                default:
                    return false;
            }
            RefreshList();
            return true;
        }

        private void SetTab(TabContentData tabConfig)
        {
            _overlay.CurrentTab = tabConfig;
            _skipped = 0;
            RefreshList();
        }

        private bool SetTab(int direction = 0)
        {
            var index = _tabConfigs.IndexOf(_overlay.CurrentTab);
            switch (direction)
            {
                case > 0 when index + 1 == _tabConfigs.Count:
                    index = -1;
                    break;
                case < 0 when index == 0:
                    index = -1;
                    break;
                case < 0 when index == -1:
                    index = _tabConfigs.Count - 1;
                    break;
                default:
                    index += direction;
                    break;
            }
            var tabConfig = _tabConfigs.ElementAtOrDefault(index);
            SetTab(tabConfig);
            return true;
        }

        private void SetSearch(string searchText)
        {
            if (_searchText == searchText)
                return;
            _searchText = searchText;
            RefreshList();
        }
        
        /// <summary>Track if configured control buttons are pressed or pass input to overlay.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (_overlay == null)
                return;
            if (_keys.ScrollDown.JustPressed() && Scroll(-1))
                _inputHelper.SuppressActiveKeybinds(_keys.ScrollDown);
            else if (_keys.ScrollUp.JustPressed() && Scroll(1))
                _inputHelper.SuppressActiveKeybinds(_keys.ScrollUp);
            else if (_keys.PreviousTab.JustPressed() && SetTab(-1))
                _inputHelper.SuppressActiveKeybinds(_keys.PreviousTab);
            else if (_keys.NextTab.JustPressed() && SetTab(1))
                _inputHelper.SuppressActiveKeybinds(_keys.NextTab);
        }

        /// <summary>Track if configured control buttons are pressed or pass input to overlay.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (_overlay == null)
                return;
            
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            
            if ((e.Button == SButton.MouseLeft || e.Button.IsUseToolButton()) && _overlay.LeftClick(x, y))
                _inputHelper.Suppress(e.Button);
            else if ((e.Button == SButton.MouseRight || e.Button.IsActionButton()) && _overlay.RightClick(x, y))
                _inputHelper.Suppress(e.Button);
            else if (_overlay.ReceiveKeyPress(e.Button))
                _inputHelper.Suppress(e.Button);
        }
        
        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            _overlay?.Hover(x, y);
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
                cur.X,
                cur.Y,
                e.NewValue,
                cur.LeftButton,
                cur.MiddleButton,
                cur.RightButton,
                cur.XButton1,
                cur.XButton2
            );
        }
        private bool ContextMatches(MenuHandler handler) =>
            ReferenceEquals(_context, handler._context);
        public bool ContextMatches(InventoryMenu inventoryMenu) =>
            ReferenceEquals(_items, inventoryMenu.actualInventory);
        
        private void RefreshList()
        {
            var filteredItems = _items.Where(item => item != null);
            
            if (_overlay.CurrentTab != null)
                filteredItems = filteredItems.Where(item => _overlay.CurrentTab.IsAllowed(item) && !_overlay.CurrentTab.IsBlocked(item));

            if (!string.IsNullOrWhiteSpace(_searchText))
                filteredItems = filteredItems.Where(SearchMatches);
            var filteredItemsList = filteredItems.ToList();
            
            _skipped = _skipped <= 0
                ? 0
                : Math.Min(_skipped, filteredItemsList.Count.RoundUp(_cols) - _capacity);
            _filteredItems = filteredItemsList.Skip(_skipped).Take(_capacity + _cols).ToList();
            
            for (var i = 0; i < _menu.inventory.Count; i++)
            {
                var item = _filteredItems.ElementAtOrDefault(i);
                _menu.inventory[i].name = item != null
                    ? _menu.actualInventory.IndexOf(item).ToString()
                    : _menu.actualInventory.Count.ToString();
            }
        }

        private bool SearchMatches(Item item)
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return true;
            var searchParts = _searchText.Split(' ');
            HashSet<string> tags = null;
            foreach (var searchPart in searchParts)
            {
                if (searchPart.StartsWith("#"))
                {
                    tags ??= item.GetContextTags(); 
                    if (!tags.Any(tag => tag.IndexOf(searchPart.Substring(1), StringComparison.InvariantCultureIgnoreCase) >= 0))
                        return false;
                }
                else
                {
                    if (item.Name.IndexOf(searchPart, StringComparison.InvariantCultureIgnoreCase) == -1 &&
                        item.DisplayName.IndexOf(searchPart, StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;
                }
            }
            return true;
        }
        private void ItemsOnElementChanged(NetList<Item, NetRef<Item>> list, int index, Item oldvalue, Item newvalue) =>
            RefreshList();
        private void ShippingBinOnValueChanged(Item value) =>
            RefreshList();
        private bool CanScrollUp => _skipped > 0;
        private bool CanScrollDown => _cols <= _filteredItems.Count.RoundUp(_cols) - _capacity;
    }
}