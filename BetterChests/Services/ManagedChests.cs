namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers;
using Common.Records;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Extensions;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ManagedChests : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<Dictionary<PlacedChest, Lazy<IManagedChest>>> _placedChests = new(() => new());
    private readonly PerScreen<Dictionary<PlayerItem, IManagedChest>> _playerChests = new(() => new());

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedChests" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ManagedChests(IConfigModel config, IModHelper helper, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this._assetHandler = services.Lazy<AssetHandler>();
        PlacedChestExtensions.GetAccessibleLocations = () => this.AccessibleLocations;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
        this.Helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;

        if (Context.IsMainPlayer)
        {
            this.Helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
        }
    }

    /// <summary>
    ///     Gets all placed chests in the world.
    /// </summary>
    public IReadOnlyDictionary<PlacedChest, Lazy<IManagedChest>> PlacedChests
    {
        get => this._placedChests.Value;
    }

    /// <summary>
    ///     Gets all chests in player inventory.
    /// </summary>
    public IReadOnlyDictionary<PlayerItem, IManagedChest> PlayerChests
    {
        get => this._playerChests.Value;
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

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IDictionary<string, IChestData> ChestConfigs { get; } = new Dictionary<string, IChestData>();

    private IConfigModel Config { get; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Attempts to find a <see cref="ManagedChest" /> that matches a <see cref="Chest" /> instance.
    /// </summary>
    /// <param name="chest">The <see cref="Chest" /> to find.</param>
    /// <param name="managedChest">The <see cref="ManagedChest" /> to return if it matches the <see cref="Chest" />.</param>
    /// <returns>Returns true if a matching <see cref="ManagedChest" /> could be found.</returns>
    public bool FindChest(Chest chest, out IManagedChest managedChest)
    {
        if (chest is null)
        {
            managedChest = default;
            return false;
        }

        foreach (var (_, playerChest) in this.PlayerChests)
        {
            if (ReferenceEquals(playerChest.Chest, chest))
            {
                managedChest = playerChest;
                return true;
            }
        }

        foreach (var (placedChest, lazyManagedChest) in this.PlacedChests)
        {
            if (placedChest.ToChest(out var otherChest) && ReferenceEquals(otherChest, chest))
            {
                managedChest = lazyManagedChest.Value;
                return managedChest is not null;
            }
        }

        managedChest = default;
        return false;
    }

    private void AddPlacedChest(string locationName, int x, int y, string chestName)
    {
        this.AddPlacedChest(new(locationName, x, y, chestName));
    }

    private void AddPlacedChest(PlacedChest placedChest)
    {
        Log.Trace($"Placed Chest added to {placedChest.LocationName} at ({placedChest.X.ToString()}, {placedChest.Y.ToString()}).");
        if (!this._placedChests.Value.ContainsKey(placedChest))
        {
            this._placedChests.Value.Add(
                placedChest,
                new(() =>
                {
                    if (!placedChest.ToChest(out var chest))
                    {
                        return null;
                    }

                    var config = this.GetChestData(placedChest.ChestName);
                    return new ManagedChest(chest, config);
                }));
        }
    }

    private void AddPlayerChest(Farmer player, int index)
    {
        string name;
        Chest chest;
        switch (player.Items[index])
        {
            case Chest playerChest when playerChest.IsPlayerChest():
                chest = playerChest;
                name = this.Assets.Craftables.SingleOrDefault(info => info.Key == chest.ParentSheetIndex).Value?[0];
                break;
            case SObject { heldObject.Value: Chest heldChest } heldObj when heldChest.IsPlayerChest():
                chest = heldChest;
                name = this.Assets.Craftables.SingleOrDefault(info => info.Key == heldObj.ParentSheetIndex).Value?[0];
                break;
            default:
                return;
        }

        Log.Trace($"Player Chest {name} added to {player.Name} in slot {index.ToString()}.");
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.Error($"Tried adding an unknown chest {name}");
            return;
        }

        var config = this.GetChestData(name);
        this._playerChests.Value.Add(new(Game1.player, index), new ManagedChest(chest, config));
    }

    private IChestData GetChestData(string name)
    {
        if (!this.ChestConfigs.TryGetValue(name, out var config))
        {
            if (!this.Assets.ChestData.TryGetValue(name, out var chestData))
            {
                chestData = new ChestData();
                this.Assets.AddChestData(name, chestData);
            }

            config = this.ChestConfigs[name] = new ChestModel(chestData, this.Config.DefaultChest);
        }

        return config;
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (!e.IsLocalPlayer)
        {
            return;
        }

        this._playerChests.Value.Clear();
        for (var index = 0; index < e.Player.Items.Count; index++)
        {
            this.AddPlayerChest(e.Player, index);
        }
    }

    private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != BetterChests.ModUniqueId)
        {
            return;
        }

        HashSet<PlacedChest> placedChests;

        switch (e.Type)
        {
            case "InitPlacedChests":
                placedChests = e.ReadAs<HashSet<PlacedChest>>();
                this._placedChests.Value.Clear();
                foreach (var placedChest in placedChests)
                {
                    this.AddPlacedChest(placedChest);
                }

                break;
            case "AddPlacedChests":
                placedChests = e.ReadAs<HashSet<PlacedChest>>();
                foreach (var placedChest in placedChests)
                {
                    this.AddPlacedChest(placedChest);
                }

                break;
            case "RemovePlacedChests":
                placedChests = e.ReadAs<HashSet<PlacedChest>>();
                foreach (var placedChest in placedChests)
                {
                    Log.Trace($"Placed Chest removed from {placedChest.LocationName} at ({placedChest.X.ToString()}, {placedChest.Y.ToString()}).");
                    this._placedChests.Value.Remove(placedChest);
                }

                break;
        }
    }

    private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
    {
        var removedChests = new HashSet<PlacedChest>();
        foreach (var ((x, y), value) in e.Removed)
        {
            if (value is Chest placedChest && placedChest.IsPlayerChest())
            {
                var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == placedChest.ParentSheetIndex).Value?[0];
                removedChests.Add(new(e.Location.NameOrUniqueName, (int)x, (int)y, name));
            }

            if (value is { heldObject.Value: Chest heldChest } && heldChest.IsPlayerChest())
            {
                var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == value.ParentSheetIndex).Value?[0];
                removedChests.Add(new(e.Location.NameOrUniqueName, (int)x, (int)y, name));
            }
        }

        foreach (var placedChest in removedChests)
        {
            Log.Trace($"Placed Chest removed from {placedChest.LocationName} at ({placedChest.X.ToString()}, {placedChest.Y.ToString()}).");
            this._placedChests.Value.Remove(placedChest);
        }

        var addedChests = new HashSet<PlacedChest>();
        foreach (var ((x, y), value) in e.Added)
        {
            if (value is Chest placedChest && placedChest.IsPlayerChest())
            {
                var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == placedChest.ParentSheetIndex).Value?[0];
                addedChests.Add(new(e.Location.NameOrUniqueName, (int)x, (int)y, name));
            }

            if (value is { heldObject.Value: Chest heldChest } && heldChest.IsPlayerChest())
            {
                var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == value.ParentSheetIndex).Value?[0];
                addedChests.Add(new(e.Location.NameOrUniqueName, (int)x, (int)y, name));
            }
        }

        foreach (var placedChest in addedChests)
        {
            this.AddPlacedChest(placedChest);
        }

        if (Context.IsMainPlayer)
        {
            this.Helper.Multiplayer.SendMessage(removedChests, "RemovePlacedChests", new[] { BetterChests.ModUniqueId });
            this.Helper.Multiplayer.SendMessage(addedChests, "AddPlacedChests", new[] { BetterChests.ModUniqueId });
        }
    }

    private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
    {
        if (e.Peer.IsHost)
        {
            return;
        }

        var placedChests = new HashSet<PlacedChest>(this.PlacedChests.Keys);
        this.Helper.Multiplayer.SendMessage(placedChests, "InitPlacedChests", new[] { BetterChests.ModUniqueId });
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        // Initialize Player Items
        for (var index = 0; index < Game1.player.Items.Count; index++)
        {
            this.AddPlayerChest(Game1.player, index);
        }

        // Initialize Placed Objects
        foreach (var location in this.AccessibleLocations)
        {
            foreach (var ((x, y), obj) in location.Objects.Pairs)
            {
                if (obj is Chest chest && chest.IsPlayerChest())
                {
                    var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == chest.ParentSheetIndex).Value?[0];
                    this.AddPlacedChest(location.NameOrUniqueName, (int)x, (int)y, name);
                    continue;
                }

                if (obj is { heldObject.Value: Chest heldChest } && heldChest.IsPlayerChest())
                {
                    var name = this.Assets.Craftables.SingleOrDefault(info => info.Key == obj.ParentSheetIndex).Value?[0];
                    this.AddPlacedChest(location.NameOrUniqueName, (int)x, (int)y, name);
                }
            }

            if (location is FarmHouse farmHouse && farmHouse.fridge.Value is { } && !farmHouse.fridgePosition.ToVector2().Equals(Vector2.Zero))
            {
                this.AddPlacedChest(location.NameOrUniqueName, farmHouse.fridgePosition.X, farmHouse.fridgePosition.Y, "Fridge");
            }
        }

        if (Context.IsMainPlayer)
        {
            this.Helper.Multiplayer.SendMessage(this.PlacedChests.Keys, "InitPlacedChests", new[] { BetterChests.ModUniqueId });
        }
    }
}