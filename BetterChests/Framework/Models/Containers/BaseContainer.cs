namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewValley.Inventories;
using StardewValley.Mods;
using StardewValley.Network;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IContainer{TSource}" />
internal abstract class BaseContainer<TSource> : BaseContainer, IContainer<TSource>
    where TSource : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseContainer{TSource}" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    protected BaseContainer(IStorageOptions baseOptions)
        : base(baseOptions) { }

    /// <inheritdoc />
    public abstract bool IsAlive { get; }

    /// <inheritdoc />
    public abstract WeakReference<TSource> Source { get; }
}

/// <inheritdoc />
internal abstract class BaseContainer : IContainer
{
    private readonly IStorageOptions baseOptions;
    private readonly Lazy<IStorageOptions> storageOptions;

    /// <summary>Initializes a new instance of the <see cref="BaseContainer" /> class.</summary>
    /// <param name="baseOptions">The type of storage object.</param>
    protected BaseContainer(IStorageOptions baseOptions)
    {
        this.baseOptions = baseOptions;
        this.storageOptions = new Lazy<IStorageOptions>(
            () => new ChildStorageOptions(baseOptions, new ModDataStorageOptions(this.ModData)));
    }

    /// <inheritdoc />
    public string DisplayName => this.baseOptions.GetDisplayName();

    /// <inheritdoc />
    public string Description => this.baseOptions.GetDescription();

    /// <inheritdoc />
    public abstract int Capacity { get; }

    /// <inheritdoc />
    public IStorageOptions Options => this.storageOptions.Value;

    /// <inheritdoc />
    public abstract IInventory Items { get; }

    /// <inheritdoc />
    public abstract GameLocation Location { get; }

    /// <inheritdoc />
    public abstract Vector2 TileLocation { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

    /// <inheritdoc/>
    public abstract NetMutex? Mutex { get; }

    /// <inheritdoc />
    public void ForEachItem(Func<Item, bool> action)
    {
        for (var index = this.Items.Count - 1; index >= 0; --index)
        {
            if (this.Items[index] is null)
            {
                continue;
            }

            if (!action(this.Items[index]))
            {
                break;
            }
        }
    }

    /// <inheritdoc />
    public virtual void ShowMenu() { }

    /// <inheritdoc />
    public abstract bool TryAdd(Item item, out Item? remaining);

    /// <inheritdoc />
    public abstract bool TryRemove(Item item);

    /// <inheritdoc />
    public override string ToString() =>
        $"{this.DisplayName} at {this.Location.DisplayName} ({this.TileLocation.X:n0}, {this.TileLocation.Y:n0})";
}