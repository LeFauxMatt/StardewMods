namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CommonFeature : BaseFeature
    {
        private static readonly Type[] ItemGrabMenuConstructorParams = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        private static CommonFeature Instance;
        private readonly PerScreen<IClickableMenu> _menu = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<InventoryMenu.highlightThisItem> _vanillaHighlightChestItems = new();
        private readonly PerScreen<InventoryMenu.highlightThisItem> _vanillaHighlightPlayerItems = new();
        private readonly PerScreen<InventoryMenu.highlightThisItem> _expandedHighlightChestItems = new();
        private readonly PerScreen<InventoryMenu.highlightThisItem> _expandedHighlightPlayerItems = new();

        /// <summary>Initializes a new instance of the <see cref="CommonFeature"/> class.</summary>
        internal CommonFeature()
            : base("Common")
        {
            CommonFeature.Instance = this;
        }

        /// <summary>Render below the chest menu, but above the background.</summary>
        public static event EventHandler<RenderingActiveMenuEventArgs> RenderingActiveMenu;

        /// <summary>Render above the chest menu, but below hover items/text.</summary>
        public static event EventHandler<RenderedActiveMenuEventArgs> RenderedActiveMenu;

        /// <summary>Event for ItemGrabMenu Constructor postfix.</summary>
        public static event EventHandler<ItemGrabMenuConstructorEventArgs> ItemGrabMenuConstructor;

        /// <summary>Event for when the active menu switches from/to an ItemGrabMenu with a chest.</summary>
        public static event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged;

        /// <summary>Gets or sets multicast delegate for highlighting items in chest inventory.</summary>
        public static InventoryMenu.highlightThisItem HighlightChestItems
        {
            get => CommonFeature.Instance._vanillaHighlightChestItems.Value + CommonFeature.Instance._expandedHighlightChestItems.Value;
            internal set => CommonFeature.Instance._expandedHighlightChestItems.Value = value;
        }

        /// <summary>Gets or sets multicast delegate for highlighting items in player inventory.</summary>
        public static InventoryMenu.highlightThisItem HighlightPlayerItems
        {
            get => CommonFeature.Instance._vanillaHighlightPlayerItems.Value + CommonFeature.Instance._expandedHighlightPlayerItems.Value;
            internal set => CommonFeature.Instance._expandedHighlightPlayerItems.Value = value;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Display.MenuChanged += this.OnMenuChanged;
            modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
            modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;

            // Patches
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), CommonFeature.ItemGrabMenuConstructorParams),
                postfix: new HarmonyMethod(typeof(CommonFeature), nameof(CommonFeature.ItemGrabMenu_constructor_postfix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Display.MenuChanged -= this.OnMenuChanged;
            modEvents.Display.RenderingActiveMenu -= this.OnRenderingActiveMenu;
            modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), CommonFeature.ItemGrabMenuConstructorParams),
                patch: AccessTools.Method(typeof(CommonFeature), nameof(CommonFeature.ItemGrabMenu_constructor_postfix)));
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.playerChest.Value)
            {
                return;
            }

            if (__instance.inventory.highlightMethod != CommonFeature.OnHighlightPlayerItems)
            {
                CommonFeature.Instance._vanillaHighlightPlayerItems.Value = __instance.inventory.highlightMethod;
                __instance.inventory.highlightMethod = CommonFeature.OnHighlightPlayerItems;
            }

            if (__instance.ItemsToGrabMenu.highlightMethod != CommonFeature.OnHighlightChestItems)
            {
                CommonFeature.Instance._vanillaHighlightChestItems.Value = __instance.ItemsToGrabMenu.highlightMethod;
                __instance.ItemsToGrabMenu.highlightMethod = CommonFeature.OnHighlightChestItems;
            }

            __instance.setBackgroundTransparency(false);

            // Invoke ItemGrabMenu constructor events
            var eventArgs = new ItemGrabMenuConstructorEventArgs
            {
                ItemGrabMenu = __instance,
                Chest = chest,
            };
            foreach (Delegate @delegate in CommonFeature.ItemGrabMenuConstructor.GetInvocationList())
            {
                @delegate.DynamicInvoke(null, eventArgs);
            }
        }

        private static bool OnHighlightChestItems(Item item)
        {
            return CommonFeature.HighlightChestItems.GetInvocationList().All(highlightMethod => (bool)highlightMethod.DynamicInvoke(item));
        }

        private static bool OnHighlightPlayerItems(Item item)
        {
            return CommonFeature.HighlightPlayerItems.GetInvocationList().All(highlightMethod => (bool)highlightMethod.DynamicInvoke(item));
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, this._menu.Value))
            {
                return;
            }

            this._menu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest { playerChest: { Value: true } } chest } itemGrabMenu)
            {
                itemGrabMenu = null;
                chest = null;
                this._attached.Value = false;
                this._screenId.Value = -1;
            }
            else if (!this._attached.Value)
            {
                this._attached.Value = true;
                this._screenId.Value = Context.ScreenId;
                this._chest.Value = chest;
            }

            var eventArgs = new ItemGrabMenuChangedEventArgs
            {
                ItemGrabMenu = itemGrabMenu,
                Chest = chest,
                Attached = this._attached.Value,
                ScreenId = this._screenId.Value,
            };

            // Invoke ItemGrabMenuChanged events
            foreach (Delegate @delegate in CommonFeature.ItemGrabMenuChanged.GetInvocationList())
            {
                @delegate.DynamicInvoke(this, eventArgs);
            }
        }

        [EventPriority(EventPriority.High)]
        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
            {
                return;
            }

            // Draw background
            e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

            // Invoke rendering events
            foreach (Delegate @delegate in CommonFeature.RenderingActiveMenu.GetInvocationList())
            {
                @delegate.DynamicInvoke(this, e);
            }
        }

        [EventPriority(EventPriority.Low)]
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
            {
                return;
            }

            // Invoke rendered events
            foreach (Delegate @delegate in CommonFeature.RenderedActiveMenu.GetInvocationList())
            {
                @delegate.DynamicInvoke(this, e);
            }

            // Draw foreground
            if (itemGrabMenu.hoverText != null && (itemGrabMenu.hoveredItem is null or null || itemGrabMenu.ItemsToGrabMenu == null))
            {
                if (itemGrabMenu.hoverAmount > 0)
                {
                    IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.hoverText, string.Empty, null, heldItem: true, -1, 0, -1, -1, null, itemGrabMenu.hoverAmount);
                }
                else
                {
                    IClickableMenu.drawHoverText(e.SpriteBatch, itemGrabMenu.hoverText, Game1.smallFont);
                }
            }

            if (itemGrabMenu.hoveredItem != null)
            {
                IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.hoveredItem.getDescription(), itemGrabMenu.hoveredItem.DisplayName, itemGrabMenu.hoveredItem, itemGrabMenu.heldItem != null);
            }
            else if (itemGrabMenu.hoveredItem != null && itemGrabMenu.ItemsToGrabMenu != null)
            {
                IClickableMenu.drawToolTip(e.SpriteBatch, itemGrabMenu.ItemsToGrabMenu.descriptionText, itemGrabMenu.ItemsToGrabMenu.descriptionTitle, itemGrabMenu.hoveredItem, itemGrabMenu.heldItem != null);
            }

            itemGrabMenu.heldItem?.drawInMenu(e.SpriteBatch, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);

            itemGrabMenu.drawMouse(e.SpriteBatch);
        }

        /// <inheritdoc />
        internal class ItemGrabMenuConstructorEventArgs : EventArgs
        {
            /// <summary>Gets or sets the ItemGrabMenu being constructed.</summary>
            public ItemGrabMenu ItemGrabMenu { get; internal set; }

            /// <summary>Gets or sets the Chest for which the ItemGrabMenu was opened.</summary>
            public Chest Chest { get; internal set; }
        }

        /// <inheritdoc />
        internal class ItemGrabMenuChangedEventArgs : EventArgs
        {
            /// <summary>Gets or sets the ItemGrabMenu if it is the currently active menu.</summary>
            public ItemGrabMenu ItemGrabMenu { get; internal set; }

            /// <summary>Gets or sets the Chest for which the ItemGrabMenu was opened.</summary>
            public Chest Chest { get; internal set; }

            /// <summary>Gets or sets a value indicating whether true if the ItemGrabMenu is the currently active menu.</summary>
            public bool Attached { get; internal set; }

            /// <summary>Gets or sets the screen Id that the ItemGrabMenu is active on.</summary>
            public int ScreenId { get; internal set; }
        }
    }
}