namespace StardewMods.FuryCore.Models.GameObjects.Producers;

using System;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class CrabPotProducer : Producer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CrabPotProducer" /> class.
    /// </summary>
    /// <param name="crabPot">The source crab pot.</param>
    /// <param name="getModData">A get method for the mod data of the object.</param>
    public CrabPotProducer(CrabPot crabPot, Func<ModDataDictionary> getModData)
        : base(crabPot, getModData)
    {
        this.CrabPot = crabPot;
    }

    private CrabPot CrabPot { get; }

    /// <inheritdoc />
    public override bool TryGetOutput(out Item item)
    {
        if (!this.CrabPot.readyForHarvest.Value)
        {
            item = null;
            return false;
        }

        item = this.CrabPot.heldObject.Value;
        if (this.CrabPot.checkForAction(Game1.player))
        {
            return true;
        }

        item = null;
        return false;
    }

    /// <inheritdoc />
    public override bool TrySetInput(Item item)
    {
        if (item is not SObject obj || !this.CrabPot.performObjectDropInAction(item, true, Game1.player))
        {
            return false;
        }

        if (this.CrabPot.performObjectDropInAction(item, false, Game1.player))
        {
            Game1.player.removeItemsFromInventory(obj.ParentSheetIndex, 1);
        }

        return true;
    }
}