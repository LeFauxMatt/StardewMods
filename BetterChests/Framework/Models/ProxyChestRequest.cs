namespace StardewMods.BetterChests.Framework.Models;

/// <summary>
/// Makes a temporary request for a Chest to be converted into a proxy chest. This should only be used
/// temporarily, and the request must be confirmed or cancelled before adding items to or removing items from the chest.
/// </summary>
internal sealed class ProxyChestRequest
{
    private readonly Action cancel;
    private readonly Action confirm;

    private bool done;

    /// <summary>Initializes a new instance of the <see cref="ProxyChestRequest" /> class.</summary>
    /// <param name="item">The item that represents a proxy chest.</param>
    /// <param name="confirm">The action to perform when the request has been confirmed.</param>
    /// <param name="cancel">The action to perform when the request has been cancelled.</param>
    public ProxyChestRequest(SObject item, Action confirm, Action cancel)
    {
        this.Item = item;
        this.confirm = confirm;
        this.cancel = cancel;
    }

    /// <summary>Gets the item that represents a proxy chest.</summary>
    public SObject Item { get; }

    /// <summary>Confirm a proxy chest request.</summary>
    public void Confirm()
    {
        if (this.done)
        {
            return;
        }

        this.done = true;
        this.confirm();
    }

    /// <summary>Cancel a proxy chest request.</summary>
    public void Cancel()
    {
        if (this.done)
        {
            return;
        }

        this.done = true;
        this.cancel();
    }
}