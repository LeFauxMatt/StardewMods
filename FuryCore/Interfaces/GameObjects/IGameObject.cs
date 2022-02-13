namespace StardewMods.FuryCore.Interfaces.GameObjects;

using StardewValley;

/// <summary>
///     Represents any object in the game.
/// </summary>
public interface IGameObject
{
    /// <summary>
    ///     Gets the context object.
    /// </summary>
    object Context { get; }

    /// <summary>
    ///     Gets the ModData associated with the context object.
    /// </summary>
    ModDataDictionary ModData { get; }
}