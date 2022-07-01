#nullable disable

namespace StardewMods.FuryCore.Models.GameObjects.Producers;

using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IProducer" />
internal abstract class BaseProducer : GameObject, IProducer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseProducer" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    protected BaseProducer(object context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public abstract override ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public abstract Item OutputItem { get; protected set; }

    /// <inheritdoc />
    public abstract bool TryGetOutput(out Item item);

    /// <inheritdoc />
    public abstract bool TrySetInput(Item item);
}