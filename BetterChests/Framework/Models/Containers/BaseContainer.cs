namespace StardewMods.BetterChests.Framework.Models.Containers;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Mods;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IContainer{TSource}" />
internal abstract class BaseContainer<TSource> : BaseContainer, IContainer<TSource>
    where TSource : class
{
    /// <summary>Initializes a new instance of the <see cref="BaseContainer{TSource}" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="baseOptions">The type of storage object.</param>
    protected BaseContainer(ItemMatcher itemMatcher, IStorageOptions baseOptions)
        : base(itemMatcher, baseOptions) { }

    /// <inheritdoc />
    public abstract bool IsAlive { get; }

    /// <inheritdoc />
    public abstract WeakReference<TSource> Source { get; }
}

/// <inheritdoc />
internal abstract class BaseContainer : IContainer
{
    private const string LockItemKey = "furyx639.BetterChests/LockItem";
    private readonly IStorageOptions baseOptions;

    private readonly ItemMatcher itemMatcher;
    private readonly Lazy<IStorageOptions> storageOptions;

    /// <summary>Initializes a new instance of the <see cref="BaseContainer" /> class.</summary>
    /// <param name="itemMatcher">The item matcher to use for filters.</param>
    /// <param name="baseOptions">The type of storage object.</param>
    protected BaseContainer(ItemMatcher itemMatcher, IStorageOptions baseOptions)
    {
        this.itemMatcher = itemMatcher;
        this.baseOptions = baseOptions;
        this.storageOptions = new Lazy<IStorageOptions>(
            () => new ChildStorageOptions(baseOptions, new ModDataStorageOptions(this.ModData)));
    }

    /// <inheritdoc />
    public string DisplayName => this.baseOptions.GetDisplayName();

    /// <inheritdoc />
    public string Description => this.baseOptions.GetDescription();

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

    /// <inheritdoc />
    public void OrganizeItems(bool reverse = false)
    {
        if (this.Options is
            {
                OrganizeItemsGroupBy: GroupBy.Default,
                OrganizeItemsSortBy: SortBy.Default,
            })
        {
            ItemGrabMenu.organizeItemsInList(this.Items);
            return;
        }

        var items = this.Items.ToArray();
        Array.Sort(
            items,
            (i1, i2) =>
            {
                if (i2 == null)
                {
                    return -1;
                }

                if (i1 == null)
                {
                    return 1;
                }

                if (i1.Equals(i2))
                {
                    return 0;
                }

                var g1 = this.Options.OrganizeItemsGroupBy switch
                    {
                        GroupBy.Category => i1
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Color => i1
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("color_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Name => i1.DisplayName,
                        _ => null,
                    }
                    ?? string.Empty;

                var g2 = this.Options.OrganizeItemsGroupBy switch
                    {
                        GroupBy.Category => i2
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Color => i2
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("color_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Name => i2.DisplayName,
                        _ => null,
                    }
                    ?? string.Empty;

                if (!g1.Equals(g2, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Compare(g1, g2, StringComparison.OrdinalIgnoreCase);
                }

                var o1 = this.Options.OrganizeItemsSortBy switch
                {
                    SortBy.Type => i1.Category, SortBy.Quality => i1.Quality, SortBy.Quantity => i1.Stack, _ => 0,
                };

                var o2 = this.Options.OrganizeItemsSortBy switch
                {
                    SortBy.Type => i2.Category, SortBy.Quality => i2.Quality, SortBy.Quantity => i2.Stack, _ => 0,
                };

                return o1.CompareTo(o2);
            });

        if (reverse)
        {
            Array.Reverse(items);
        }

        this.Items.OverwriteWith(items);
    }

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
    public virtual bool Transfer(Item item, IContainer containerTo, out Item? remaining)
    {
        if (!this.Items.Contains(item))
        {
            remaining = null;
            return false;
        }

        if (this.Options.LockItem == Option.Enabled && item.modData.ContainsKey(BaseContainer.LockItemKey))
        {
            remaining = null;
            return false;
        }

        if (!containerTo.TryAdd(item, out remaining))
        {
            return false;
        }

        return remaining is null && this.TryRemove(item);
    }

    /// <inheritdoc />
    public bool MatchesFilter(Item item)
    {
        if (this.Options.FilterItems != Option.Enabled)
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

    /// <inheritdoc />
    public abstract bool TryRemove(Item item);
}