namespace StardewMods.FuryCore.Models.GameObjects.Producers;

using StardewValley;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class GenericProducer : Producer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GenericProducer" /> class.
    /// </summary>
    /// <param name="obj">The source object.</param>
    public GenericProducer(SObject obj)
        : base(obj, () => obj.modData)
    {
        this.SourceObject = obj;
    }

    /// <inheritdoc />
    public override Item OutputItem
    {
        get => this.SourceObject.heldObject.Value;
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
        if (this.OutputItem is null && this.SourceObject.performObjectDropInAction(item, false, Game1.player))
        {
            Game1.player.removeItemsFromInventory(item.ParentSheetIndex, 1);
            return true;
        }

        return false;
    }
}