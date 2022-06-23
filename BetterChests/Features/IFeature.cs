namespace StardewMods.BetterChests.Features;

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