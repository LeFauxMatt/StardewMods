namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Storages;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Inventories;
using StardewValley.Mods;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IContainer{TSource}" />
internal abstract class BaseContainer<TSource> : BaseContainer, IContainer<TSource>
    where TSource : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseContainer{TSource}" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    protected BaseContainer(ItemMatcher itemMatcher, IStorage storageType)
        : base(itemMatcher, storageType) { }

    /// <inheritdoc />
    public abstract bool IsAlive { get; }

    /// <inheritdoc />
    public abstract WeakReference<TSource> Source { get; }
}

/// <inheritdoc />
internal abstract class BaseContainer : IContainer
{
    private const string LockedSlotKey = "furyx639.BetterChests/LockedSlot";

    private readonly ItemMatcher itemMatcher;
    private readonly Lazy<IStorage> options;
    private readonly IStorage storageType;

    /// <summary>Initializes a new instance of the <see cref="BaseContainer" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="storageType">The type of storage object.</param>
    protected BaseContainer(ItemMatcher itemMatcher, IStorage storageType)
    {
        this.itemMatcher = itemMatcher;
        this.storageType = storageType;
        this.options = new Lazy<IStorage>(() => new ChildStorage(storageType, new ModDataStorage(this.ModData)));
    }

    /// <inheritdoc />
    public string DisplayName => this.storageType.GetDisplayName();

    /// <inheritdoc />
    public string Description => this.storageType.GetDescription();

    /// <inheritdoc />
    public IStorage Options => this.options.Value;

    /// <inheritdoc />
    public abstract IInventory Items { get; }

    /// <inheritdoc />
    public abstract GameLocation Location { get; }

    /// <inheritdoc />
    public abstract Vector2 TileLocation { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }

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
    public bool Transfer(Item item, IContainer containerTo, out Item? remaining)
    {
        if (!this.Items.Contains(item))
        {
            remaining = null;
            return false;
        }

        if (this.Options.SlotLock == FeatureOption.Enabled && item.modData.ContainsKey(BaseContainer.LockedSlotKey))
        {
            remaining = null;
            return false;
        }

        if (!containerTo.TryAdd(item, out remaining))
        {
            return false;
        }

        if (remaining is null)
        {
            this.Items.Remove(item);
        }

        return true;
    }

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