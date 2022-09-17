namespace StardewMods.BetterChests.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.StorageHandlers;
using StardewValley.Menus;

/// <summary>
///     Forces the ShippingBin to use a regular ItemGrabMenu.
/// </summary>
internal sealed class BetterShippingBin : IFeature
{
#nullable disable
    private static IFeature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private bool _isActivated;

    private BetterShippingBin(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="BetterShippingBin" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="BetterShippingBin" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return BetterShippingBin.Instance ??= new BetterShippingBin(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Display.MenuChanged += BetterShippingBin.OnMenuChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Display.MenuChanged -= BetterShippingBin.OnMenuChanged;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Relaunch as regular ItemGrabMenu
        if (e.NewMenu is ItemGrabMenu { context: { } context, shippingBin: true }
         && Storages.TryGetOne(context, out var storage)
         && storage is ShippingBinStorage)
        {
            storage.ShowMenu();
        }
    }
}