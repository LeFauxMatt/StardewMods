namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;

/// <summary>
///     Lock your owned chests so they cannot be accessed by other players.
/// </summary>
internal class LockedChests : IFeature
{
    private LockedChests(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static LockedChests? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    /// <summary>
    ///     Initializes <see cref="LockedChests" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="LockedChests" /> class.</returns>
    public static LockedChests Init(IModHelper helper)
    {
        return LockedChests.Instance ??= new(helper);
    }

    /// <inheritdoc/>
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
        }
    }

    /// <inheritdoc/>
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
        }
    }
}