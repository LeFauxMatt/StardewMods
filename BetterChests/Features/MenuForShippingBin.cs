namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley.Menus;

/// <inheritdoc />
internal class MenuForShippingBin : Feature
{
    private readonly PerScreen<ItemGrabMenu> _menu = new();

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

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    [SortedEventPriority(EventPriority.High + 1001)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
        if (this.Menu is { context: not null, shippingBin: true } && this.ManagedObjects.FindManagedStorage(e.Context, out var managedStorage))
        {
            // Relaunch as ItemGrabMenu
            managedStorage.ShowMenu();
        }
    }
}