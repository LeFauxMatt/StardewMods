using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.UI
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class ExpandedMenu
    {
        /// <summary>Returns Offset to lower menu for expanded menus.</summary>
        public static int Offset(MenuWithInventory menu)
        {
            UpdateReference(menu);
            return _offset;
        }

        /// <summary>Returns Padding to top menu for search box.</summary>
        public static int Padding(MenuWithInventory menu)
        {
            UpdateReference(menu);
            return _padding;
        }

        /// <summary>Returns Display Capacity of MenuWithInventory.</summary>
        public static int Capacity(object context)
        {
            UpdateReference(context);
            return _capacity;
        }

        /// <summary>Returns Displayed Rows of MenuWithInventory.</summary>
        public static int Rows(object context)
        {
            UpdateReference(context);
            return _rows;
        }

        /// <summary>Returns the filtered list of items in the InventoryMenu.</summary>
        public static IList<Item> Filtered(InventoryMenu inventoryMenu) =>
            MenuHandler?.ContextMatches(inventoryMenu) ?? false
                ? MenuHandler.Items
                : inventoryMenu.actualInventory;

        /// <summary>Injected function to draw above chest menu but below tooltips</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        public static void Draw(SpriteBatch b) =>
            MenuHandler?.Draw(b);
        
        /// <summary>Injected function to draw below chest menu</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        public static void DrawUnder(SpriteBatch b) =>
            MenuHandler?.DrawUnder(b);

        public static bool SearchFocused(ItemGrabMenu menu) =>
            MenuHandler != null && MenuHandler.ContextMatches(menu.ItemsToGrabMenu) && MenuHandler.SearchFocused;

        private static MenuHandler MenuHandler
        {
            get => PerScreenMenuHandler.Value;
            set => PerScreenMenuHandler.Value = value;
        }

        /// <summary>Overlays ItemGrabMenu with UI elements provided by ExpandedStorage.</summary>
        private static readonly PerScreen<MenuHandler> PerScreenMenuHandler = new PerScreen<MenuHandler>();

        private static int _offset;
        private static int _padding;
        private static int _capacity;
        private static int _rows;
        
        private static IModEvents _events;
        private static IInputHelper _inputHelper;
        private static ModConfig _config;
        private static MenuWithInventory _menu;
        private static object _context;

        internal static void Init(IModEvents events, IInputHelper inputHelper, ModConfig config)
        {
            _events = events;
            _inputHelper = inputHelper;
            _config = config;
            
            if (!_config.AllowModdedCapacity)
                return;

            // Events
            _events.Display.MenuChanged += OnMenuChanged;
        }

        /// <summary>
        /// Resets scrolling/overlay when chest menu exits or context changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (!(e.NewMenu is ItemGrabMenu menu))
            {
                MenuHandler?.UnregisterEvents();
                return;
            }
            var menuHandler = new MenuHandler(menu, _events, _inputHelper, _config, MenuHandler);
            MenuHandler?.Dispose();
            MenuHandler = menuHandler;
        }
        
        private static void UpdateReference(MenuWithInventory menu)
        {
            if (ReferenceEquals(menu, _menu))
                return;
            _menu = menu;

            StorageContentData config = null;
            if (menu is ItemGrabMenu itemGrabMenu)
            {
                UpdateReference(itemGrabMenu.context);
                config = ExpandedStorage.GetConfig(itemGrabMenu.context);
            }

            _offset = _config.ExpandInventoryMenu && config != null
                ? 64 * (_rows - 3)
                : 0;
            
            _padding = _config.ShowSearchBar
                       && config != null
                       && config.ShowSearchBar
                ? 24
                : 0;
        }

        private static void UpdateReference(object context)
        {
            if (context != null && ReferenceEquals(context, _context))
                return;
            _context = context;
            var config = ExpandedStorage.GetConfig(context);
            var capacity = (config?.Capacity ?? 0) switch
            {
                -1 => int.MaxValue,
                0 when context is Chest chest => chest.GetActualCapacity(),
                _ when config != null => config.Capacity,
                _ => Chest.capacity
            };
            
            _rows = _config.ExpandInventoryMenu
                ? (int) MathHelper.Clamp((float) Math.Ceiling(capacity / 12m), 1, 6)
                : 3;
            
            _capacity = _config.AllowModdedCapacity
                ? _rows * 12
                : Chest.capacity;
        }
    }
}