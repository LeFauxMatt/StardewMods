namespace StardewMods.BetterChests.Framework.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewValley.Menus;

/// <summary>
///     Forces the ShippingBin to use a regular ItemGrabMenu.
/// </summary>
internal sealed class BetterShippingBin : Feature
{
#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private BetterShippingBin(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="BetterShippingBin" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="BetterShippingBin" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return BetterShippingBin.Instance ??= new BetterShippingBin(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this._helper.Events.Display.MenuChanged += BetterShippingBin.OnMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this._helper.Events.Display.MenuChanged -= BetterShippingBin.OnMenuChanged;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Relaunch as regular ItemGrabMenu
        if (e.NewMenu is ItemGrabMenu { context: { } context, shippingBin: true }
         && Storages.TryGetOne(context, out var storage)
         && storage is { Data: ShippingBinStorage storageObject })
        {
            storageObject.ShowMenu();
        }
    }
}