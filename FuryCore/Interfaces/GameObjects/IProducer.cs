namespace StardewMods.FuryCore.Interfaces.GameObjects;

using StardewValley;

/// <inheritdoc />
public interface IProducer : IGameObject
{
    /// <summary>
    ///     Gets the current output item for this producer.
    /// </summary>
    Item OutputItem { get; }

    /// <summary>
    ///     Attempts to get the output item for this producer.
    /// </summary>
    /// <param name="item">The item to take from the producer.</param>
    /// <returns>True if output item could be taken.</returns>
    bool TryGetOutput(out Item item);

    /// <summary>
    ///     Attempts to set the input item for this producer.
    /// </summary>
    /// <param name="item">The item to set input to.</param>
    /// <returns>True if input item was accepted.</returns>
    bool TrySetInput(Item item);
}