namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Storages;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Inventories;
using StardewValley.Mods;

/// <inheritdoc />
internal abstract class BaseContainer<TSource> : IContainer<TSource>
    where TSource : class
{
    private readonly ItemMatcher itemMatcher;
    private readonly Lazy<IStorage> options;

    /// <summary>Initializes a new instance of the <see cref="BaseContainer{TSource}" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    /// <param name="inventory">The storage inventory.</param>
    protected BaseContainer(ItemMatcher itemMatcher, IStorage storageType, IInventory inventory)
    {
        this.itemMatcher = itemMatcher;
        this.StorageType = storageType;
        this.Items = inventory;
        this.options = new Lazy<IStorage>(
            () =>
            {
                var modDataOptions = new ModDataStorage(this.ModData);
                return new ChildStorage(this.StorageType, modDataOptions);
            });
    }

    /// <inheritdoc />
    public IStorage StorageType { get; }

    /// <inheritdoc />
    public IStorage Options => this.options.Value;

    /// <inheritdoc />
    public virtual IInventory Items { get; }

    /// <inheritdoc />
    public abstract GameLocation Location { get; }

    /// <inheritdoc />
    public abstract Vector2 TileLocation { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public abstract bool IsAlive { get; }

    /// <inheritdoc />
    public abstract WeakReference<TSource> Source { get; }

    /// <inheritdoc />
    public bool MatchesFilter(Item item)
    {
        if (this.Options.FilterItems != FeatureOption.Enabled)
        {
            return true;
        }

        if (this.itemMatcher.IsEmpty && this.Options.FilterItemsList.Any())
        {
            this.itemMatcher.SearchText = string.Join(' ', this.Options.FilterItemsList);
        }

        return this.itemMatcher.MatchesFilter(item);
    }

    /// <inheritdoc />
    public abstract bool TryAdd(Item item, out Item? remaining);
}
