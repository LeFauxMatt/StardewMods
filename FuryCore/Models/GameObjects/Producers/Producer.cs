namespace StardewMods.FuryCore.Models.GameObjects.Producers;

using System;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IProducer" />
internal abstract class Producer : GameObject, IProducer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Producer" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="getModData">A get method for the mod data of the object.</param>
    protected Producer(object context, Func<ModDataDictionary> getModData)
        : base(context)
    {
        this.GetModData = getModData;
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.GetModData.Invoke();
    }

    private Func<ModDataDictionary> GetModData { get; }

    /// <inheritdoc />
    public abstract bool TryGetOutput(out Item item);

    /// <inheritdoc />
    public abstract bool TrySetInput(Item item);
}