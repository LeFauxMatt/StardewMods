#nullable disable

namespace StardewMods.BetterChests.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Models.Config;
using StardewMods.BetterChests.Models.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

/// <inheritdoc cref="IModService" />
internal class AssetHandler : IModService
{
    private const string CraftablesData = "Data/BigCraftablesInformation";

    private IReadOnlyDictionary<string, IStorageData> _cachedChestData;
    private IReadOnlyDictionary<int, string[]> _cachedCraftables;
    private IReadOnlyDictionary<string, string[]> _cachedTabData;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetHandler" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public AssetHandler(IConfigModel config, IModHelper helper)
    {
        this.Config = config;
        this.Helper = helper;

        this.InitChestData();
        this.InitTabData();

        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        this.Helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
        if (Context.IsMainPlayer)
        {
            this.Helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
        }
    }

    /// <summary>
    ///     Gets the collection of chest data for all known chest types in the game.
    /// </summary>
    public IReadOnlyDictionary<string, IStorageData> ChestData
    {
        get => this._cachedChestData ??= (
                from data in this.Helper.GameContent.Load<IDictionary<string, IDictionary<string, string>>>($"{BetterChests.ModUniqueId}/Chests")
                select (data.Key, Value: new SerializedStorageData(data.Value)))
            .ToDictionary(data => data.Key, data => (IStorageData)data.Value);
    }

    /// <summary>
    ///     Gets the collection of tab data.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> TabData
    {
        get => this._cachedTabData ??= (
                from tab in
                    from data in this.Helper.GameContent.Load<IDictionary<string, string>>($"{BetterChests.ModUniqueId}/Tabs")
                    select (data.Key, info: data.Value.Split('/'))
                orderby int.Parse(tab.info[2]), tab.info[0]
                select (tab.Key, tab.info))
            .ToDictionary(
                data => data.Key,
                data => data.info);
    }

    private IConfigModel Config { get; }

    private IReadOnlyDictionary<int, string[]> Craftables
    {
        get => this._cachedCraftables ??= this.Helper.GameContent.Load<IDictionary<int, string>>(AssetHandler.CraftablesData)
                                              .ToDictionary(
                                                  info => info.Key,
                                                  info => info.Value.Split('/'));
    }

    private IModHelper Helper { get; }

    private IDictionary<string, IDictionary<string, string>> LocalChestData { get; } = new Dictionary<string, IDictionary<string, string>>();

    private IDictionary<string, string> LocalTabData { get; set; }

    private HashSet<string> ModDataKeys { get; } = new();

    /// <summary>
    ///     Adds new Chest Data and saves to assets/chests.json.
    /// </summary>
    /// <param name="id">The qualified item id of the chest.</param>
    /// <param name="data">The chest data to add.</param>
    /// <returns>True if new chest data was added.</returns>
    public bool AddChestData(string id, IStorageData data = default)
    {
        data ??= new StorageData();
        if (this.LocalChestData.ContainsKey(id))
        {
            return false;
        }

        this.LocalChestData.Add(id, SerializedStorageData.GetData(data));
        return true;
    }

    /// <summary>
    ///     Adds a mod data key to source the qualified item id from.
    /// </summary>
    /// <param name="key">The key to source the qualified item id from.</param>
    public void AddModDataKey(string key)
    {
        this.ModDataKeys.Add(key);
    }

    /// <summary>
    ///     Gets the storage name from a Building.
    /// </summary>
    /// <param name="building">The building to get the storage name for.</param>
    /// <returns>The name of the storage.</returns>
    public string GetStorageName(Building building)
    {
        foreach (var key in this.ModDataKeys)
        {
            if (building.modData.TryGetValue(key, out var name))
            {
                return name;
            }
        }

        return building switch
        {
            JunimoHut => "Junimo Hut",
            ShippingBin => "Shipping Bin",
            _ => null,
        };
    }

    /// <summary>
    ///     Gets the storage name from a Game Location.
    /// </summary>
    /// <param name="location">The location to get the storage name for.</param>
    /// <returns>The name of the storage.</returns>
    public string GetStorageName(GameLocation location)
    {
        foreach (var key in this.ModDataKeys)
        {
            if (location.modData.TryGetValue(key, out var name))
            {
                return name;
            }
        }

        return location switch
        {
            FarmHouse or IslandFarmHouse => "Fridge",
            IslandWest => "Shipping Bin",
            _ => null,
        };
    }

    /// <summary>
    ///     Gets the storage name from an Item.
    /// </summary>
    /// <param name="item">The item to get the storage name for.</param>
    /// <returns>The name of the storage.</returns>
    public string GetStorageName(Item item)
    {
        foreach (var key in this.ModDataKeys)
        {
            if (item.modData.TryGetValue(key, out var name))
            {
                return name;
            }
        }

        return this.Craftables.SingleOrDefault(info => info.Key == item.ParentSheetIndex).Value?[0];
    }

    /// <summary>
    ///     Saves the currently cached chest data back to the local chest data.
    /// </summary>
    public void SaveChestData()
    {
        foreach (var (key, data) in this.ChestData)
        {
            this.LocalChestData[key] = SerializedStorageData.GetData(data);
        }

        this.Helper.Data.WriteJsonFile("assets/chests.json", this.LocalChestData);
        this.Helper.Multiplayer.SendMessage(this.LocalChestData, "StorageData", new[] { BetterChests.ModUniqueId });
    }

    private void InitChestData()
    {
        IDictionary<string, IDictionary<string, string>> storageData;

        // Load existing storage data
        try
        {
            storageData = this.Helper.Data.ReadJsonFile<IDictionary<string, IDictionary<string, string>>>("assets/chests.json");
            this.LoadStorageData(storageData);
        }
        catch (Exception)
        {
            // ignored
        }

        // Load new storage data
        var chestsDir = Path.Combine(this.Helper.DirectoryPath, "chests");
        Directory.CreateDirectory(chestsDir);
        foreach (var path in Directory.GetFiles(chestsDir, "*.json"))
        {
            try
            {
                var chestPath = Path.GetRelativePath(this.Helper.DirectoryPath, path);
                storageData = this.Helper.Data.ReadJsonFile<IDictionary<string, IDictionary<string, string>>>(chestPath);
                this.LoadStorageData(storageData);
            }
            catch (Exception e)
            {
                Log.Warn($"Failed loading chest data from {path}.\nError: {e.Message}");
            }
        }

        // Load vanilla special storages
        IStorageData defaultStorage = new StorageData();
        var specialStorages = new Dictionary<string, IStorageData>
        {
            { "Fridge", defaultStorage },
            { "Shipping Bin", defaultStorage },
            { "Junimo Hut", defaultStorage },
        };
        this.LoadStorageData(specialStorages);

        // Load default vanilla storage data
        var vanillaStorages = this.Craftables
                                  .Where(craftable => Enum.IsDefined(typeof(VanillaStorageObjects), craftable.Key))
                                  .ToDictionary(craftable => craftable.Value[0], _ => defaultStorage);
        this.LoadStorageData(vanillaStorages);

        // Save to chests.json
        this.Helper.Data.WriteJsonFile("assets/chests.json", this.LocalChestData);
    }

    private void InitTabData()
    {
        // Load Tab Data
        try
        {
            this.LocalTabData = this.Helper.ModContent.Load<Dictionary<string, string>>("assets/tabs.json");
        }
        catch (Exception)
        {
            // ignored
        }

        // Initialize Tab Data
        if (this.LocalTabData is null)
        {
            this.LocalTabData = new Dictionary<string, string>
            {
                {
                    "Clothing",
                    "Clothing/furyx639.BetterChests\\Tabs\\Texture/0/category_clothing category_boots category_hat"
                },
                {
                    "Cooking",
                    "Cooking/furyx639.BetterChests\\Tabs\\Texture/1/category_syrup category_artisan_goods category_ingredients category_sell_at_pierres_and_marnies category_sell_at_pierres category_meat category_cooking category_milk category_egg"
                },
                {
                    "Crops",
                    "Crops/furyx639.BetterChests\\Tabs\\Texture/2/category_greens category_flowers category_fruits category_vegetable"
                },
                {
                    "Equipment",
                    "/furyx639.BetterChests\\Tabs\\Texture/3/category_equipment category_ring category_tool category_weapon"
                },
                {
                    "Fishing",
                    "/furyx639.BetterChests\\Tabs\\Texture/4/category_bait category_fish category_tackle category_sell_at_fish_shop"
                },
                {
                    "Materials",
                    "/furyx639.BetterChests\\Tabs\\Texture/5/category_monster_loot category_metal_resources category_building_resources category_minerals category_crafting category_gem"
                },
                {
                    "Misc",
                    "/furyx639.BetterChests\\Tabs\\Texture/6/category_big_craftable category_furniture category_junk"
                },
                {
                    "Seeds",
                    "/furyx639.BetterChests\\Tabs\\Texture/7/category_seeds category_fertilizer"
                },
            };
            this.Helper.Data.WriteJsonFile("assets/tabs.json", this.LocalTabData);
        }
    }

    private void LoadStorageData(IDictionary<string, IStorageData> storageData)
    {
        this.LoadStorageData(storageData.ToDictionary(
            data => data.Key,
            data => SerializedStorageData.GetData(data.Value)));
    }

    private void LoadStorageData(IDictionary<string, IDictionary<string, string>> chestData)
    {
        if (chestData is null)
        {
            return;
        }

        foreach (var (key, value) in chestData)
        {
            if (!this.LocalChestData.ContainsKey(key))
            {
                this.LocalChestData.Add(key, value);
            }
        }
    }

    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{BetterChests.ModUniqueId}/Chests"))
        {
            e.LoadFrom(() => this.LocalChestData, AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo($"{BetterChests.ModUniqueId}/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo($"{BetterChests.ModUniqueId}/Tabs"))
        {
            e.LoadFrom(
                () => this.LocalTabData.ToDictionary(
                    data => data.Key,
                    data =>
                    {
                        var (key, value) = data;
                        var info = value.Split('/');
                        info[0] = this.Helper.Translation.Get($"tabs.{key}.name");
                        return string.Join('/', info);
                    }),
                AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo($"{BetterChests.ModUniqueId}/Tabs/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnDayEnding(object sender, DayEndingEventArgs e)
    {
        this._cachedCraftables = null;
        this._cachedChestData = null;
        this._cachedTabData = null;
    }

    private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != BetterChests.ModUniqueId)
        {
            return;
        }

        switch (e.Type)
        {
            case "StorageData":
                Log.Trace("Loading StorageData from Host");
                var storageData = e.ReadAs<IDictionary<string, IDictionary<string, string>>>();
                this.LocalChestData.Clear();
                this.LoadStorageData(storageData);
                break;
            case "DefaultStorage":
                Log.Trace("Loading DefaultStorage Config from Host");
                var defaultStorage = e.ReadAs<StorageData>();
                ((IStorageData)defaultStorage).CopyTo(this.Config.DefaultChest);
                break;
        }
    }

    private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
    {
        if (e.Peer.IsHost)
        {
            return;
        }

        this.Helper.Multiplayer.SendMessage(this.LocalChestData, "StorageData", new[] { BetterChests.ModUniqueId });
        this.Helper.Multiplayer.SendMessage(this.Config.DefaultChest, "DefaultStorage", new[] { BetterChests.ModUniqueId });
    }
}