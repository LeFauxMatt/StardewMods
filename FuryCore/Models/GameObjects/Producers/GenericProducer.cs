#nullable disable

namespace StardewMods.FuryCore.Models.GameObjects.Producers;

using StardewValley;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class GenericProducer : BaseProducer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GenericProducer" /> class.
    /// </summary>
    /// <param name="obj">The source object.</param>
    public GenericProducer(SObject obj)
        : base(obj)
    {
        this.SourceObject = obj;
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.SourceObject.modData;
    }

    /// <inheritdoc />
    public override Item OutputItem
    {
        get => this.SourceObject.heldObject.Value;
        protected set => this.SourceObject.heldObject.Value = value as SObject;
    }

    /// <summary>
    ///     Gets the source object.
    /// </summary>
    private SObject SourceObject { get; }

    /// <inheritdoc />
    public override bool TryGetOutput(out Item item)
    {
        item = this.OutputItem;
        if (item is null || !this.SourceObject.checkForAction(Game1.player))
        {
            item = null;
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool TrySetInput(Item item)
    {
        if (this.OutputItem is not null)
        {
            return false;
        }

        var probeTest = this.SourceObject.performObjectDropInAction(item, true, Game1.player);
        this.OutputItem = null;
        if (!probeTest || !this.SourceObject.performObjectDropInAction(item, false, Game1.player))
        {
            return false;
        }

        if (--item.Stack <= 0)
        {
            Game1.player.removeItemFromInventory(item);
        }

        return true;
    }
}