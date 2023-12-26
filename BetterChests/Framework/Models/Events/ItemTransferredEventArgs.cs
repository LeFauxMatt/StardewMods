namespace StardewMods.BetterChests.Framework.Models.Events;

using StardewMods.BetterChests.Framework.Interfaces;

/// <summary>The event arguments after an item is transferred.</summary>
internal sealed class ItemTransferredEventArgs(IContainer from, IContainer to, Item item, int amount) : EventArgs
{
    /// <summary>Gets the source container from which the item is being retrieved.</summary>
    public IContainer From { get; } = from;

    /// <summary>Gets the destination container to which the item is being sent.</summary>
    public IContainer To { get; } = to;

    /// <summary>Gets the item being transferred.</summary>
    public Item Item { get; } = item;

    /// <summary>Gets the amount of the item that was transferred.</summary>
    public int Amount { get; } = amount;
}