namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Menus;

/// <summary>Represents the event arguments for changes in the inventory menu.</summary>
internal sealed class ItemGrabMenuChangedEventArgs : EventArgs
{
    /// <summary>Initializes a new instance of the <see cref="ItemGrabMenuChangedEventArgs" /> class.</summary>
    /// <param name="context">The IContainer object.</param>
    /// <param name="menu">The item grab menu instance.</param>
    public ItemGrabMenuChangedEventArgs(IContainer? context = null, ItemGrabMenu? menu = null)
    {
        this.Context = context;
        this.Menu = menu;
    }

    /// <summary>Gets the container associated with the item grab menu.</summary>
    public IContainer? Context { get; }

    /// <summary>Gets the item grab menu.</summary>
    public ItemGrabMenu? Menu { get; }
}
