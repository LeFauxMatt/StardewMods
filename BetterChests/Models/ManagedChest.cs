namespace BetterChests.Models;

using System.Collections.Generic;
using System.Linq;
using BetterChests.Enums;
using BetterChests.Interfaces;
using FuryCore.Helpers;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

/// <inheritdoc />
internal class ManagedChest : IManagedChest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedChest"/> class.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> managed by this mod.</param>
    /// <param name="data">The <see cref="IChestData" /> associated with this type of <see cref="Chest" />.</param>
    /// <param name="location">The <see cref="GameLocation" /> where the Chest is placed.</param>
    /// <param name="position">The coordinates where the Chest is placed.</param>
    public ManagedChest(Chest chest, IChestModel data, GameLocation location, Vector2 position)
        : this(chest, data)
    {
        this.CollectionType = ItemCollectionType.GameLocation;
        this.Location = location;
        this.Position = position;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedChest"/> class.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> managed by this mod.</param>
    /// <param name="data">The <see cref="IChestData" /> associated with this type of <see cref="Chest" />.</param>
    /// <param name="player">The <see cref="Farmer" /> whose inventory contains the Chest.</param>
    /// <param name="index">The item slot where the Chest is being stored.</param>
    public ManagedChest(Chest chest, IChestModel data, Farmer player, int index)
        : this(chest, data)
    {
        this.CollectionType = ItemCollectionType.PlayerInventory;
        this.Player = player;
        this.Index = index;
    }

    private ManagedChest(Chest chest, IChestModel data)
    {
        this.Chest = chest;
        this.Data = data;

        if (this.Chest.modData.TryGetValue("FilterItems", out var filterItems))
        {
            // Migrate Legacy Keys
            this.Chest.modData[$"{ModEntry.ModUniqueId}/CategorizeChest"] = filterItems;
            this.Chest.modData.Remove("FilterItems");
            this.ItemMatcherByChest.StringValue = filterItems;
        }
        else if (this.Chest.modData.TryGetValue($"{ModEntry.ModUniqueId}/CategorizeChest", out filterItems) && !string.IsNullOrWhiteSpace(filterItems))
        {
            this.ItemMatcherByChest.StringValue = filterItems;
        }
    }

    // ****************************************************************************************
    // General

    /// <inheritdoc/>
    public Chest Chest { get; }

    /// <inheritdoc/>
    public ItemCollectionType CollectionType { get; }

    /// <inheritdoc/>
    public GameLocation Location { get; }

    /// <inheritdoc/>
    public Farmer Player { get; }

    /// <inheritdoc/>
    public Vector2 Position { get; }

    /// <inheritdoc/>
    public int Index { get; }

    /// <inheritdoc/>
    public ItemMatcher ItemMatcherByChest { get; } = new(true);

    /// <inheritdoc/>
    public ItemMatcher ItemMatcherByType
    {
        get => this.Data.ItemMatcherByType;
    }

    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get => this.Data.CarryChest;
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CategorizeChest
    {
        get => this.Data.CategorizeChest;
        set => this.Data.CategorizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get => this.Data.ChestMenuTabs;
        set => this.Data.ChestMenuTabs = value;
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get => this.Data.CollectItems;
        set => this.Data.CollectItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get => this.Data.CraftFromChest;
        set => this.Data.CraftFromChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get => this.Data.CustomColorPicker;
        set => this.Data.CustomColorPicker = value;
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get => this.Data.FilterItems;
        set => this.Data.FilterItems = value;
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get => this.Data.OpenHeldChest;
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get => this.Data.ResizeChest;
        set => this.Data.ResizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get => this.Data.ResizeChestMenu;
        set => this.Data.ResizeChestMenu = value;
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get => this.Data.SearchItems;
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get => this.Data.StashToChest;
        set => this.Data.StashToChest = value;
    }

    // ****************************************************************************************
    // Feature Options

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get => this.Data.CraftFromChestDistance;
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.Data.FilterItemsList;
        set => this.Data.FilterItemsList = value;
    }

    /// <inheritdoc/>
    public bool StashToChestStacks
    {
        get => this.Data.StashToChestStacks;
        set => this.Data.StashToChestStacks = value;
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get => this.Data.ResizeChestCapacity;
        set => this.Data.ResizeChestCapacity = value;
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get => this.Data.ResizeChestMenuRows;
        set => this.Data.ResizeChestMenuRows = value;
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get => this.Data.StashToChestDistance;
        set => this.Data.StashToChestDistance = value;
    }

    private IChestModel Data { get; }

    /// <inheritdoc/>
    public bool MatchesChest(Chest other)
    {
        var chest = (this.Location, this.Position, this.Player, this.Index) switch
        {
            (FarmHouse farmHouse, var pos, _, _) when pos == Vector2.Zero => farmHouse.fridge.Value,
            (not null, _, _, _) when this.Location.Objects.TryGetValue(this.Position, out var obj) => obj as Chest,
            (_, _, not null, > -1) => this.Player.Items.ElementAtOrDefault(this.Index) as Chest,
            _ => null,
        };
        return chest is not null && ReferenceEquals(chest, other);
    }

    /// <inheritdoc/>
    public Item StashItem(Item item)
    {
        var stack = item.Stack;

        if ((this.Data.ItemMatcherByType.Any() || this.ItemMatcherByType.Any()) && this.Data.ItemMatcherByType.Matches(item) && this.ItemMatcherByType.Matches(item))
        {
            var tmp = this.Chest.addItem(item);
            if (tmp is null || tmp.Stack <= 0)
            {
                return null;
            }

            if (tmp.Stack != stack)
            {
                item.Stack = tmp.Stack;
            }
        }

        if (this.Data.StashToChestStacks)
        {
            foreach (var chestItem in this.Chest.items.Where(chestItem => chestItem.maximumStackSize() > 1 && chestItem.canStackWith(item)))
            {
                if (chestItem.getRemainingStackSpace() > 0)
                {
                    item.Stack = chestItem.addToStack(item);
                }

                if (item.Stack <= 0)
                {
                    return null;
                }
            }
        }

        return item;
    }
}