namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Inventories;
using StardewValley.Mods;

/// <inheritdoc />
internal class ChildContainer : IContainer
{
    private readonly IContainer child;
    private readonly IContainer parent;

    /// <summary>Initializes a new instance of the <see cref="ChildContainer" /> class.</summary>
    /// <param name="parent">The parent container.</param>
    /// <param name="child">The child container.</param>
    public ChildContainer(IContainer parent, IContainer child)
    {
        this.parent = parent;
        this.child = child;
    }

    /// <summary>Gets the top-most parent storage.</summary>
    public IContainer Parent => this.parent switch { ChildContainer childStorage => childStorage.parent, _ => this.parent };

    /// <summary>Gets the bottom-most child storage.</summary>
    public IContainer Child => this.child switch { ChildContainer childStorage => childStorage.Child, _ => this.child };

    /// <inheritdoc />
    public IStorage StorageType => this.child.StorageType;

    /// <inheritdoc />
    public IStorage Options => this.child.Options;

    /// <inheritdoc />
    public IInventory Items => this.child.Items;

    /// <inheritdoc />
    public GameLocation Location => this.parent.Location;

    /// <inheritdoc />
    public Vector2 TileLocation => this.parent.TileLocation;

    /// <inheritdoc />
    public ModDataDictionary ModData => this.child.ModData;

    /// <inheritdoc />
    public bool MatchesFilter(Item item) => this.child.MatchesFilter(item);

    /// <inheritdoc />
    public bool TryAdd(Item item, out Item? remaining) => this.child.TryAdd(item, out remaining);
}