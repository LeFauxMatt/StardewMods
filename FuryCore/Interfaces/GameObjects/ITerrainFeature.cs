namespace StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     Represents a terrain feature that is interactable.
/// </summary>
public interface ITerrainFeature : IGameObject
{
    /// <summary>
    ///     Attempts to drop an item from this terrain feature.
    /// </summary>
    /// <returns>True if the item was dropped.</returns>
    bool TryDropItem();
}