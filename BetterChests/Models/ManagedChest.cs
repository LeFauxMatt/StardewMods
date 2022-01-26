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
    /// <param name="config">The <see cref="IChestData" /> associated with this type of <see cref="Chest" />.</param>
    /// <param name="location">The <see cref="GameLocation" /> where the Chest is placed.</param>
    /// <param name="position">The coordinates where the Chest is placed.</param>
    public ManagedChest(Chest chest, IChestModel config, GameLocation location, Vector2 position)
        : this(chest, config)
    {
        this.CollectionType = ItemCollectionType.GameLocation;
        this.Location = location;
        this.Position = position;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedChest"/> class.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> managed by this mod.</param>
    /// <param name="config">The <see cref="IChestData" /> associated with this type of <see cref="Chest" />.</param>
    /// <param name="player">The <see cref="Farmer" /> whose inventory contains the Chest.</param>
    /// <param name="index">The item slot where the Chest is being stored.</param>
    public ManagedChest(Chest chest, IChestModel config, Farmer player, int index)
        : this(chest, config)
    {
        this.CollectionType = ItemCollectionType.PlayerInventory;
        this.Player = player;
        this.Index = index;
    }

    private ManagedChest(Chest chest, IChestModel config)
    {
        this.Chest = chest;
        this.Config = config;
        if (this.Chest.modData.TryGetValue("FilterItems", out var filterItems) && !string.IsNullOrWhiteSpace(filterItems))
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
        get => this.Config.ItemMatcherByType;
    }

    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get => this.Config.CarryChest;
        set => this.Config.CarryChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CategorizeChest
    {
        get => this.Config.CategorizeChest;
        set => this.Config.CategorizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get => this.Config.ChestMenuTabs;
        set => this.Config.ChestMenuTabs = value;
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get => this.Config.CollectItems;
        set => this.Config.CollectItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get => this.Config.CraftFromChest;
        set => this.Config.CraftFromChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get => this.Config.CustomColorPicker;
        set => this.Config.CustomColorPicker = value;
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get => this.Config.FilterItems;
        set => this.Config.FilterItems = value;
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get => this.Config.OpenHeldChest;
        set => this.Config.OpenHeldChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get => this.Config.ResizeChest;
        set => this.Config.ResizeChest = value;
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get => this.Config.ResizeChestMenu;
        set => this.Config.ResizeChestMenu = value;
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get => this.Config.SearchItems;
        set => this.Config.SearchItems = value;
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get => this.Config.StashToChest;
        set => this.Config.StashToChest = value;
    }

    // ****************************************************************************************
    // Feature Options

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get => this.Config.CraftFromChestDistance;
        set => this.Config.CraftFromChestDistance = value;
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.Config.FilterItemsList;
        set => this.Config.FilterItemsList = value;
    }

    /// <inheritdoc/>
    public bool FillStacks
    {
        get => this.Config.FillStacks;
        set => this.Config.FillStacks = value;
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get => this.Config.ResizeChestCapacity;
        set => this.Config.ResizeChestCapacity = value;
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get => this.Config.ResizeChestMenuRows;
        set => this.Config.ResizeChestMenuRows = value;
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get => this.Config.StashToChestDistance;
        set => this.Config.StashToChestDistance = value;
    }

    private IChestModel Config { get; }

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
    public Item StashItem(Item item, bool fillStacks = false)
    {
        var stack = item.Stack;

        if ((this.Config.ItemMatcherByType.Any() || this.ItemMatcherByType.Any()) && this.Config.ItemMatcherByType.Matches(item) && this.ItemMatcherByType.Matches(item))
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

        if (fillStacks)
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