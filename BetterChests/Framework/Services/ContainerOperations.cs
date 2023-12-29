namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Extensions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Responsible for handling operations between containers.</summary>
internal sealed class ContainerOperations : BaseService
{
    private EventHandler<ItemTransferredEventArgs>? itemTransferred;
    private EventHandler<ItemTransferringEventArgs>? itemTransferring;

    /// <summary>Initializes a new instance of the <see cref="ContainerOperations" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ContainerOperations(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Represents an event that is raised after an item is transferred.</summary>
    public event EventHandler<ItemTransferredEventArgs> ItemTransferred
    {
        add => this.itemTransferred += value;
        remove => this.itemTransferred -= value;
    }

    /// <summary>Represents an event that is raised before an item is transferred.</summary>
    public event EventHandler<ItemTransferringEventArgs> ItemTransferring
    {
        add => this.itemTransferring += value;
        remove => this.itemTransferring -= value;
    }

    /// <summary>Arranges items in container according to group by and sort by options.</summary>
    /// <param name="container">The container to organize.</param>
    /// <param name="reverse">Whether to sort the items in reverse order.</param>
    public void OrganizeItems(IContainer container, bool reverse = false)
    {
        if (container.Options is
            {
                OrganizeItemsGroupBy: GroupBy.Default,
                OrganizeItemsSortBy: SortBy.Default,
            })
        {
            ItemGrabMenu.organizeItemsInList(container.Items);
            return;
        }

        var items = container.Items.ToArray();
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

                var g1 = container.Options.OrganizeItemsGroupBy switch
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

                var g2 = container.Options.OrganizeItemsGroupBy switch
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

                var o1 = container.Options.OrganizeItemsSortBy switch
                {
                    SortBy.Type => i1.Category, SortBy.Quality => i1.Quality, SortBy.Quantity => i1.Stack, _ => 0,
                };

                var o2 = container.Options.OrganizeItemsSortBy switch
                {
                    SortBy.Type => i2.Category, SortBy.Quality => i2.Quality, SortBy.Quantity => i2.Stack, _ => 0,
                };

                return o1.CompareTo(o2);
            });

        if (reverse)
        {
            Array.Reverse(items);
        }

        container.Items.OverwriteWith(items);
    }

    /// <summary>Transfers items from one container to another.</summary>
    /// <param name="from">The container to transfer items from.</param>
    /// <param name="to">The container to transfer items to.</param>
    /// <param name="amounts">Output parameter that contains the transferred item amounts.</param>
    /// <returns>True if the transfer was successful and at least one item was transferred, otherwise False.</returns>
    public bool Transfer(IContainer from, IContainer to, [NotNullWhen(true)] out Dictionary<string, int>? amounts)
    {
        var items = new Dictionary<string, int>();
        from.ForEachItem(
            item =>
            {
                // Stop iterating if destination container is already at capacity
                if (to.Items.CountItemStacks() >= to.Capacity)
                {
                    return false;
                }

                var itemTransferringEventArgs = new ItemTransferringEventArgs(from, to, item);
                this.itemTransferring?.InvokeAll(this, itemTransferringEventArgs);
                if (itemTransferringEventArgs.IsPrevented)
                {
                    return true;
                }

                var stack = item.Stack;
                items.TryAdd(item.Name, 0);
                if (!to.TryAdd(item, out var remaining) || !from.TryRemove(item))
                {
                    return true;
                }

                var amount = stack - (remaining?.Stack ?? 0);
                items[item.Name] += amount;
                var itemTransferredEventArgs = new ItemTransferredEventArgs(from, to, item, amount);
                this.itemTransferred?.InvokeAll(this, itemTransferredEventArgs);
                return true;
            });

        if (items.Any())
        {
            amounts = items;
            return true;
        }

        amounts = null;
        return false;
    }
}