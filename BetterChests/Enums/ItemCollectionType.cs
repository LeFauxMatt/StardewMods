namespace BetterChests.Enums;

using StardewValley;
using SObject = StardewValley.Object;

/// <summary>
/// Denotes the collection that an <see cref="Item" /> belongs to.
/// </summary>
internal enum ItemCollectionType
{
    /// <summary>Placed <see cref="SObject" /> in a <see cref="GameLocation" />.</summary>
    GameLocation = 0,

    /// <summary>Item contained in a <see cref="Farmer" /> inventory.</summary>
    PlayerInventory = 1,

    /// <summary>Item contained in a <see cref="Chest" /> inventory.</summary>
    ChestInventory = 2,
}