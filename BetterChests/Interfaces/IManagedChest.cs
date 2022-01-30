namespace StardewMods.BetterChests.Interfaces;

using FuryCore.Helpers;
using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Enums;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal interface IManagedChest : IChestModel
{
    /// <summary>
    /// Gets the actual instance of the <see cref="Chest" /> being managed.
    /// </summary>
    public Chest Chest { get; }

    /// <summary>
    /// Gets the type of collection that a <see cref="Chest" /> belongs to.
    /// </summary>
    public ItemCollectionType CollectionType { get; }

    /// <summary>
    /// Gets the game location of the placed <see cref="Chest" />.
    /// </summary>
    public GameLocation Location { get; }

    /// <summary>
    /// Gets the player of the <see cref="Chest" /> being held in inventory.
    /// </summary>
    public Farmer Player { get; }

    /// <summary>
    /// Gets the coordinates of the placed <see cref="Chest" />.
    /// </summary>
    public Vector2 Position { get; }

    /// <summary>
    /// Gets the item slot in player inventory of the <see cref="Chest" /> being held in inventory.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> that is uniquely assigned to each type of chest.
    /// </summary>
    public ItemMatcher ItemMatcher { get; }

    /// <summary>
    /// Determines if the placed or player <see cref="Chest" /> refers to another <see cref="Chest" /> instance.
    /// </summary>
    /// <param name="other">The instance of the other <see cref="Chest" />.</param>
    /// <returns>True if the <see cref="Chest" /> matches this one.</returns>
    public bool MatchesChest(Chest other);

    /// <summary>
    /// Attempt to stash an item into the chest based on categorization and existing items.
    /// </summary>
    /// <param name="item">The item to stash.</param>
    /// <returns>Returns the item if it could not be stashed completely, or null if it could.</returns>
    public Item StashItem(Item item);
}