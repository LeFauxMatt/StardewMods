namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>The event arguments before an item is transferred into a container.</summary>
internal sealed class ItemTransferringEventArgs(IContainer into, Item item, bool force) : EventArgs
{
    /// <summary>Gets a value indicating whether the transfer is being forced.</summary>
    public bool IsForced { get; } = force;

    /// <summary>Gets the destination container to which the item was sent.</summary>
    public IContainer Into { get; } = into;

    /// <summary>Gets the item that was transferred.</summary>
    public Item Item { get; } = item;

    /// <summary>Gets a value indicating whether the the transfer is prevented.</summary>
    public bool IsPrevented { get; private set; }

    /// <summary>Prevent the transfer.</summary>
    public void PreventTransfer() => this.IsPrevented = true;
}