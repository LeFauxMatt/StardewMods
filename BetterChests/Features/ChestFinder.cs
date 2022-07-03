namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;

/// <summary>
///     Search for which chests have the item you're looking for.
/// </summary>
public class ChestFinder : IFeature
{
    private ChestFinder(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static ChestFinder? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    /// <summary>
    ///     Initializes <see cref="ChestFinder" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="ChestFinder" /> class.</returns>
    public static ChestFinder Init(IModHelper helper)
    {
        return ChestFinder.Instance ??= new(helper);
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