namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>The event arguments before an item is transferred.</summary>
internal sealed class ItemTransferringEventArgs(IContainer from, IContainer to, Item item) : EventArgs
{
    /// <summary>Gets the source container from which the item was retrieved.</summary>
    public IContainer From { get; } = from;

    /// <summary>Gets the destination container to which the item was sent.</summary>
    public IContainer To { get; } = to;

    /// <summary>Gets the item that was transferred.</summary>
    public Item Item { get; } = item;

    /// <summary>Gets a value indicating whether the the transfer is prevented.</summary>
    public bool IsPrevented { get; private set; }

    /// <summary>Prevent the transfer.</summary>
    public void PreventTransfer() => this.IsPrevented = true;
}