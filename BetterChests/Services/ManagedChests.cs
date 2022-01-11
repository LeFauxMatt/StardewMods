namespace BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using FuryCore.Interfaces;
using Models;
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
    private static readonly string[] VanillaChests = { "Chest", "Stone Chest", "Junimo Chest", "Mini-Fridge", "Mini-Shipping Bin", "Auto-Grabber" };
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
        this.ChestTypes = config.ChestConfigs.Keys.ToDictionary(key => key, key => new ChestType(this.Config, key));

        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        this.Helper.Events.Player.Warped += this.OnWarped;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
    }

    public IDictionary<ManagedChestId, ManagedChest> AccessibleChests
    {
        get => this._accessibleChests.Value ??= this.PlayerChests.Concat(this.PlacedChests).ToDictionary(item => item.Key, item => item.Value);
    }

    public IDictionary<string, ChestType> ChestTypes { get; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private IDictionary<ManagedChestId, ManagedChest> PlacedChests
    {
        get => this._placedChests.Value ??= (
            from location in this.AccessibleLocations
            from item in location.Objects.Pairs
            where item.Value is Chest chest
                  && chest.playerChest.Value
                  && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin
            select (chest: item.Value as Chest, location, position: item.Key)
        ).ToDictionary(
            t => new ManagedChestId(t.location, t.position),
            t =>
            {
                if (!this.ChestTypes.TryGetValue(t.chest.Name, out var chestType))
                {
                    chestType = new(this.Config, t.chest.Name);
                }

                return new ManagedChest(t.chest, chestType);
            });
    }

    private IDictionary<ManagedChestId, ManagedChest> PlayerChests
    {
        get => this._playerChests ??= (
            from player in Game1.getOnlineFarmers()
            from item in player.Items.Select((item, index) => (item, index))
            where item.item is Chest chest
                && chest.playerChest.Value
                && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin
                && chest.Stack == 1
            select (chest: item.item as Chest, player, item.index)
        ).ToDictionary(
            t => new ManagedChestId(t.player, t.index),
            t =>
            {
                var (chest, _, _) = t;
                if (!this.ChestTypes.TryGetValue(chest.Name, out var chestType))
                {
                    chestType = new(this.Config, chest.Name);
                }

                return new ManagedChest(chest, chestType);
            });
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

    private void OnWarped(object sender, WarpedEventArgs e)
    {
        this._placedChests.Value = null;
        this._accessibleChests.Value = null;
    }

    private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
    {
        this._placedChests.Value = null;
        this._accessibleChests.Value = null;
    }
}