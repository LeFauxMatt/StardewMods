namespace StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Implementation of a Better Chest feature.</summary>
internal interface IFeature
{
    /// <summary>Gets the unique id for this feature.</summary>
    public string Id { get; }

    /// <summary>Gets a value indicating whether the feature should be active.</summary>
    public bool ShouldBeActive { get; }

    /// <summary>Sets whether the feature is currently activated.</summary>
    /// <param name="warn">Whether to issue a warning if a mod conflict is detected.</param>
    public void SetActivated(bool warn = false);
}