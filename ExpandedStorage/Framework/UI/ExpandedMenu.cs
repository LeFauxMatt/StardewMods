using System;
using System.Collections.Generic;
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
    public static class ExpandedMenu
    {
        /// <summary>Returns Y-Offset to lower menu for valid instances.</summary>
        public static int Offset(MenuWithInventory menu) =>
            menu is ItemGrabMenu itemGrabMenu
                ? Offset(itemGrabMenu.context)
                : 0;

        /// <summary>Returns Y-Offset to lower menu for valid contexts.</summary>
        public static int Offset(object context) =>
            context is Chest {SpecialChestType: Chest.SpecialChestTypes.None}
                ? 64 * (Rows(context) - 3)
                : 0;

        /// <summary>Returns Display Capacity of MenuWithInventory.</summary>
        public static int Capacity(object context) =>
            context is Chest {SpecialChestType: Chest.SpecialChestTypes.None}
                ? Rows(context) * 12
                : Chest.capacity;

        /// <summary>Returns Displayed Rows of MenuWithInventory.</summary>
        public static int Rows(object context) =>
            context is Chest {SpecialChestType: Chest.SpecialChestTypes.None} chest
                ? (int) MathHelper.Clamp((float) Math.Ceiling(chest.GetActualCapacity() / 12m), 1, 6)
                : 3;

        /// <summary>Returns the filtered list of items in the InventoryMenu.</summary>
        public static IList<Item> Filtered(InventoryMenu inventoryMenu) =>
            MenuHandler != null && MenuHandler.ContextMatches(inventoryMenu)
                ? MenuHandler.Items
                : inventoryMenu.actualInventory;

        /// <summary>Injected function to draw above chest menu but below tooltips</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        public static void Draw(SpriteBatch b)
        {
            MenuHandler?.Draw(b);
        }
        
        /// <summary>Injected function to draw below chest menu</summary>
        /// <param name="b">The SpriteBatch to draw to</param>
        public static void DrawUnder(SpriteBatch b)
        {
            MenuHandler?.DrawUnder(b);
        }

        public static bool ContextMatches(InventoryMenu inventoryMenu) =>
            MenuHandler?.ContextMatches(inventoryMenu) ?? false;
        
        private static MenuHandler MenuHandler
        {
            get => PerScreenMenuHandler.Value;
            set => PerScreenMenuHandler.Value = value;
        }

        /// <summary>Overlays ItemGrabMenu with UI elements provided by ExpandedStorage.</summary>
        private static readonly PerScreen<MenuHandler> PerScreenMenuHandler = new PerScreen<MenuHandler>();

        private static IModEvents _events;
        private static IInputHelper _inputHelper;
        private static ModConfigControls _controls;

        internal static void Init(IModEvents events, IInputHelper inputHelper, ModConfig config, ModConfigControls controls)
        {
            _events = events;
            _inputHelper = inputHelper;
            _controls = controls;
            
            if (!config.AllowModdedCapacity)
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
                return;
            
            var menuHandler = new MenuHandler(menu, _events, _inputHelper, _controls, MenuHandler);
            MenuHandler?.Dispose();
            MenuHandler = menuHandler;
        }
    }
}