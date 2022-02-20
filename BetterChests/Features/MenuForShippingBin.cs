namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewValley.Menus;

/// <inheritdoc />
internal class MenuForShippingBin : Feature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuForShippingBin" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuForShippingBin(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
    }

    [SortedEventPriority(EventPriority.High)]
    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        // Relaunch as regular ItemGrabMenu
        if (e.Menu is ItemGrabMenu { context: { } context, shippingBin: true } && this.ManagedObjects.TryGetManagedStorage(context, out var managedStorage))
        {
            managedStorage.ShowMenu();
        }
    }
}