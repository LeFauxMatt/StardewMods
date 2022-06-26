namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;

/// <summary>
///     Forces the ShippingBin to use a regular ItemGrabMenu.
/// </summary>
internal class BetterShippingBin : IFeature
{
    private BetterShippingBin(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static BetterShippingBin? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    /// <summary>
    ///     Initializes <see cref="BetterShippingBin" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="BetterShippingBin" /> class.</returns>
    public static BetterShippingBin Init(IModHelper helper)
    {
        return BetterShippingBin.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Display.MenuChanged += BetterShippingBin.OnMenuChanged;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Display.MenuChanged -= BetterShippingBin.OnMenuChanged;
        }
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Relaunch as regular ItemGrabMenu
        if (e.NewMenu is ItemGrabMenu { context: ShippingBin shippingBin, shippingBin: true } itemGrabMenu
            && StorageHelper.TryGetOne(shippingBin, out var storage))
        {
            var lastSnappedComponent = itemGrabMenu.currentlySnappedComponent;
            var heldItem = itemGrabMenu.heldItem;
            itemGrabMenu.heldItem = null;
            storage.ShowMenu();
            if (lastSnappedComponent is not null)
            {
                Game1.activeClickableMenu.setCurrentlySnappedComponentTo(lastSnappedComponent.myID);
                if (Game1.options.SnappyMenus)
                {
                    itemGrabMenu.snapCursorToCurrentSnappedComponent();
                }
            }

            ((ItemGrabMenu)Game1.activeClickableMenu).heldItem = heldItem;
        }
    }
}