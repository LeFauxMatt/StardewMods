namespace StardewMods.BetterChests.Features;

/// <summary>
///     Implementation of a Better Chest feature.
/// </summary>
internal interface IFeature
{
    /// <summary>
    ///     Subscribe to events and apply any Harmony patches.
    /// </summary>
    public void Activate();

    /// <summary>
    ///     Unsubscribe from events, and reverse any Harmony patches.
    /// </summary>
    public void Deactivate();
}