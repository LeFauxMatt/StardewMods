namespace StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <summary>The event arguments before an item is transferred into a container.</summary>
public interface IItemTransferring
{
    /// <summary>Gets a value indicating whether the transfer is being forced.</summary>
    public bool IsForced { get; }

    /// <summary>Gets the destination container to which the item was sent.</summary>
    public IStorageContainer Into { get; }

    /// <summary>Gets the item that was transferred.</summary>
    public Item Item { get; }

    /// <summary>Gets a value indicating whether the the transfer is prevented.</summary>
    public bool IsPrevented { get; }

    /// <summary>Prevent the transfer.</summary>
    public void PreventTransfer();
}