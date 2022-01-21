namespace BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using BetterChests.Enums;
using BetterChests.Interfaces;
using FuryCore.Helpers;
using FuryCore.Interfaces;
using BetterChests.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedChests : IService
{
    private readonly PerScreen<IDictionary<ManagedChestId, ManagedChest>> _placedChests = new();
    private readonly PerScreen<IDictionary<ManagedChestId, ManagedChest>> _accessibleChests = new();
    private IDictionary<ManagedChestId, ManagedChest> _playerChests;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedChests"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    public ManagedChests(ModConfig config, IModHelper helper)
    {
        this.Config = config;
        this.Helper = helper;
        this.ChestConfigs = config.ChestConfigs.Keys.ToDictionary(key => key, key => (IChestConfigExtended)new ManagedChestConfig(this.Config, key));
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        this.Helper.Events.Player.Warped += this.OnWarped;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
    }

    public IDictionary<ManagedChestId, ManagedChest> AccessibleChests
    {
        get => this._accessibleChests.Value ??= this.PlayerChests.Concat(this.PlacedChests).ToDictionary(item => item.Key, item => item.Value);
    }

    public IDictionary<string, IChestConfigExtended> ChestConfigs { get; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private IDictionary<ManagedChestId, ManagedChest> PlacedChests
    {
        get
        {
            if (this._placedChests.Value is not null)
            {
                return this._placedChests.Value;
            }

            var placedChests =
                from location in this.AccessibleLocations
                from item in location.Objects.Pairs
                where item.Value is Chest chest
                      && chest.playerChest.Value
                      && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin
                      && Game1.bigCraftablesInformation.ContainsKey(chest.ParentSheetIndex)
                select (chest: item.Value as Chest, id: new ManagedChestId(location, item.Key));

            return this._placedChests.Value = placedChests.ToDictionary(
                t => t.id,
                t =>
                {
                    var name = Game1.bigCraftablesInformation[t.chest.ParentSheetIndex].Split('/')[0];
                    if (!this.ChestConfigs.TryGetValue(name, out var config))
                    {
                        config = new ManagedChestConfig(this.Config, name);
                    }

                    return new ManagedChest(t.chest, config);
                });
        }
    }

    private IDictionary<ManagedChestId, ManagedChest> PlayerChests
    {
        get
        {
            if (this._playerChests is not null)
            {
                return this._playerChests;
            }

            var playerChests =
                from player in Game1.getOnlineFarmers()
                from item in player.Items.Select((item, index) => (item, index))
                where item.item is Chest chest
                      && chest.playerChest.Value
                      && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin
                      && chest.Stack == 1
                      && Game1.bigCraftablesInformation.ContainsKey(chest.ParentSheetIndex)
                select (chest: item.item as Chest, id: new ManagedChestId(player, item.index));

            return this._playerChests = playerChests.ToDictionary(
                t => t.id,
                t =>
                {
                    var name = Game1.bigCraftablesInformation[t.chest.ParentSheetIndex].Split('/')[0];
                    if (!this.ChestConfigs.TryGetValue(name, out var config))
                    {
                        config = new ManagedChestConfig(this.Config, name);
                    }

                    return new ManagedChest(t.chest, config);
                });
        }
    }

    private IEnumerable<GameLocation> AccessibleLocations
    {
        get => Context.IsMainPlayer
            ? Game1.locations.Concat(
                from location in Game1.locations.OfType<BuildableGameLocation>()
                from building in location.buildings
                where building.indoors.Value is not null
                select building.indoors.Value)
            : this.Helper.Multiplayer.GetActiveLocations();
    }

    public bool FindChest(Chest chest, out ManagedChest managedChest)
    {
        managedChest = this.AccessibleChests.SingleOrDefault(item => item.Key.Equals(chest)).Value;
        return managedChest is not null;
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (e.Added.OfType<Chest>().Any() || e.Removed.OfType<Chest>().Any() || e.QuantityChanged.Any(itemStackChange => itemStackChange.Item is Chest))
        {
            this._playerChests = null;
            this._accessibleChests.Value = null;
        }
    }

    private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
    {
        this._placedChests.Value = null;
        this._accessibleChests.Value = null;
    }

    private void OnWarped(object sender, WarpedEventArgs e)
    {
        this._placedChests.Value = null;
        this._accessibleChests.Value = null;
    }

    /// <inheritdoc cref="BetterChests.Interfaces.IChestConfig" />
    private class ManagedChestConfig : IChestConfigExtended
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagedChestConfig"/> class.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        public ManagedChestConfig(ModConfig config, string name)
        {
            if (!config.ChestConfigs.TryGetValue(name, out var chestConfig))
            {
                chestConfig = new();
            }

            this.ChestConfig = chestConfig;
            if (ManagedChestConfig.Default is null && string.IsNullOrWhiteSpace(name))
            {
                this.IsDefault = true;
                ManagedChestConfig.Default = this;
            }
        }

        /// <inheritdoc />
        public int Capacity
        {
            get
            {
                if (this.ChestConfig.Capacity != 0)
                {
                    return this.ChestConfig.Capacity;
                }

                return this.IsDefault ? 60 : ManagedChestConfig.Default.Capacity;
            }
            set => this.ChestConfig.Capacity = value;
        }

        /// <inheritdoc />
        public FeatureOption CollectItems
        {
            get
            {
                if (this.ChestConfig.CollectItems != FeatureOption.Default)
                {
                    return this.ChestConfig.CollectItems;
                }

                return this.IsDefault ? FeatureOption.Enabled : ManagedChestConfig.Default.CollectItems;
            }
            set => this.ChestConfig.CollectItems = value;
        }

        /// <inheritdoc />
        public FeatureOptionRange CraftingRange
        {
            get
            {
                if (this.ChestConfig.CraftingRange != FeatureOptionRange.Default)
                {
                    return this.ChestConfig.CraftingRange;
                }

                return this.IsDefault ? FeatureOptionRange.Location : ManagedChestConfig.Default.CraftingRange;
            }
            set => this.ChestConfig.CraftingRange = value;
        }

        /// <inheritdoc />
        public FeatureOptionRange StashingRange
        {
            get
            {
                if (this.ChestConfig.StashingRange != FeatureOptionRange.Default)
                {
                    return this.ChestConfig.StashingRange;
                }

                return this.IsDefault ? FeatureOptionRange.Location : ManagedChestConfig.Default.StashingRange;
            }
            set => this.ChestConfig.StashingRange = value;
        }

        /// <inheritdoc />
        public HashSet<string> FilterItems
        {
            get
            {
                if (this.ChestConfig.FilterItems.Any())
                {
                    return this.ChestConfig.FilterItems;
                }

                return ManagedChestConfig.Default.FilterItems ??= new();
            }

            set
            {
                this.ChestConfig.FilterItems = value;
                this.ItemMatcher.Clear();
                foreach (var filterItem in this.FilterItems)
                {
                    this.ItemMatcher.Add(filterItem);
                }
            }
        }

        /// <inheritdoc/>
        public ItemMatcher ItemMatcher { get; } = new(true);

        private static IChestConfig Default { get; set; }

        private ChestConfig ChestConfig { get; }

        private bool IsDefault { get; }
    }
}