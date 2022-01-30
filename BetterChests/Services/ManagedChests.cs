namespace StardewMods.BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using StardewMods.FuryCore.Interfaces;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

/// <inheritdoc />
internal class ManagedChests : IModService
{
    private const string CraftablesData = "Data/BigCraftablesInformation";
    private readonly PerScreen<IList<ManagedChest>> _placedChests = new(() => null);
    private readonly PerScreen<IList<ManagedChest>> _accessibleChests = new(() => null);
    private Dictionary<int, string[]> _craftables;
    private IList<ManagedChest> _playerChests;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagedChests"/> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public ManagedChests(IConfigModel config, IModHelper helper)
    {
        this.Config = config;
        this.Helper = helper;
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        this.Helper.Events.Player.Warped += this.OnWarped;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
    }

    /// <summary>
    /// Gets all <see cref="Chest" /> that are accessible to the player.
    /// </summary>
    public IEnumerable<ManagedChest> AccessibleChests
    {
        get => this._accessibleChests.Value ??= this.PlayerChests.Concat(this.PlacedChests).ToList();
    }

    private IConfigModel Config { get; }

    private IModHelper Helper { get; }

    private IDictionary<string, IChestModel> ChestConfigs { get; } = new Dictionary<string, IChestModel>();

    private Dictionary<int, string[]> Craftables
    {
        get => this._craftables ??= this.Helper.Content.Load<Dictionary<int, string>>(ManagedChests.CraftablesData, ContentSource.GameContent)
                   .ToDictionary(
                       info => info.Key,
                       info => info.Value.Split('/'));
    }

    private IEnumerable<ManagedChest> PlacedChests
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
                from info in this.Craftables
                where item.Value is Chest chest
                      && chest.playerChest.Value
                      && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin
                      && info.Key == chest.ParentSheetIndex
                select (chest: item.Value as Chest, location, position: item.Key, name: info.Value[0]);

            // Add fridge
            var farmHouses = this.AccessibleLocations.OfType<FarmHouse>().Where(farmHouse => farmHouse.fridge.Value is not null).ToList();
            if (farmHouses.Any(farmHouse => farmHouse.fridge.Value is not null))
            {
                placedChests = placedChests.Concat(
                    from location in farmHouses
                    select (chest: location.fridge.Value, (GameLocation)location, position: Vector2.Zero, name: "Fridge"));
            }

            return this._placedChests.Value = placedChests.Select(
                t =>
                {
                    var (chest, location, position, name) = t;
                    if (!this.ChestConfigs.TryGetValue(name, out var config))
                    {
                        config = this.ChestConfigs[name] = new ChestModel(name, this.Config, this.Helper.Content);
                    }

                    return new ManagedChest(chest, config, location, position);
                }).ToList();
        }
    }

    private IEnumerable<ManagedChest> PlayerChests
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
                from info in this.Craftables
                where item.item is Chest chest
                      && chest.playerChest.Value
                      && chest.SpecialChestType is Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin
                      && chest.Stack == 1
                      && info.Key == chest.ParentSheetIndex
                select (chest: item.item as Chest, player, item.index, name: info.Value[0]);

            return this._playerChests = playerChests.Select(
                t =>
                {
                    var (chest, player, index, name) = t;
                    if (!this.ChestConfigs.TryGetValue(name, out var config))
                    {
                        config = this.ChestConfigs[name] = new ChestModel(name, this.Config, this.Helper.Content);
                    }

                    return new ManagedChest(chest, config, player, index);
                }).ToList();
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

    /// <summary>
    /// Attempts to find a <see cref="ManagedChest" /> that matches a <see cref="Chest" /> instance.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> to find.</param>
    /// <param name="managedChest">The <see cref="ManagedChest" /> to return if it matches the <see cref="Chest" />.</param>
    /// <returns>Returns true if a matching <see cref="ManagedChest" /> could be found.</returns>
    public bool FindChest(Chest chest, out ManagedChest managedChest)
    {
        managedChest = this.AccessibleChests.FirstOrDefault(item => item.MatchesChest(chest));
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
}