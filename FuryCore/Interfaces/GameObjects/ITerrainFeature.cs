namespace StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     Represents a terrain feature that is interactable.
/// </summary>
public interface ITerrainFeature : IGameObject
{
    /// <summary>
    ///     Checks if the terrain feature is ready for harvesting.
    /// </summary>
    /// <returns>True if the terrain can be harvested.</returns>
    bool CanHarvest();

    /// <summary>
    ///     Attempts to drop an item from this terrain feature.
    /// </summary>
    /// <returns>True if the item was dropped.</returns>
    bool TryHarvest();
}