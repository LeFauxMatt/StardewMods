namespace StardewMods.BetterChests.Framework;

/// <summary>
///     Implementation of a Better Chest feature.
/// </summary>
internal interface IFeature
{
    /// <summary>
    ///     Sets whether the feature is currently activated.
    /// </summary>
    /// <param name="value">A value indicating whether the feature is currently enabled.</param>
    public void SetActivated(bool value);
}