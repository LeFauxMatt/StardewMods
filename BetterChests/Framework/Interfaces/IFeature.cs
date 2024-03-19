namespace StardewMods.BetterChests.Framework.Interfaces;

/// <summary>Implementation of a Better Chest feature.</summary>
internal interface IFeature
{
    /// <summary>Gets the unique id for this feature.</summary>
    public string Id { get; }

    /// <summary>Gets a value indicating whether the feature should be active.</summary>
    public bool ShouldBeActive { get; }
}