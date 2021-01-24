﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public static int Offset(MenuWithInventory menu) =>
            _config.ExpandInventoryMenu
            && menu is ItemGrabMenu itemGrabMenu
                ? 64 * (Rows(itemGrabMenu.context) - 3)
                : 0;

        /// <summary>Returns Padding to top menu for search box.</summary>
        public static int Padding(MenuWithInventory menu) =>
            _config.ShowSearchBar
            && menu is ItemGrabMenu {context: Chest {SpecialChestType: Chest.SpecialChestTypes.None} chest}
            && ExpandedStorage.HasConfig(chest)
                ? 24
                : 0;

        /// <summary>Returns Display Capacity of MenuWithInventory.</summary>
        public static int Capacity(object context) =>
            _config.AllowModdedCapacity
                ? Rows(context) * 12
                : Chest.capacity;

        /// <summary>Returns Displayed Rows of MenuWithInventory.</summary>
        public static int Rows(object context) =>
            _config.ExpandInventoryMenu 
            && context is Chest {SpecialChestType: Chest.SpecialChestTypes.None} chest
            && ExpandedStorage.HasConfig(chest)
                ? (int) MathHelper.Clamp((float) Math.Ceiling(chest.GetActualCapacity() / 12m), 1, 6)
                : 3;

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

        private static IModEvents _events;
        private static IInputHelper _inputHelper;
        private static ModConfig _config;

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
    }
}