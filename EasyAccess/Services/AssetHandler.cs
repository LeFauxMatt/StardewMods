namespace StardewMods.EasyAccess.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Models.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IModService" />
internal class AssetHandler : IModService
{
    private const string CraftablesData = "Data/BigCraftablesInformation";
    private IReadOnlyDictionary<int, string[]> _cachedCraftables;

    private IReadOnlyDictionary<string, IProducerData> _cachedProducerData;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetHandler" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public AssetHandler(IConfigModel config, IModHelper helper)
    {
        this.Config = config;
        this.Helper = helper;

        this.InitProducerData();

        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        this.Helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
        if (Context.IsMainPlayer)
        {
            this.Helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
        }
    }

    /// <summary>
    ///     Gets the collection of producer data for all known producer types in the game.
    /// </summary>
    public IReadOnlyDictionary<string, IProducerData> ProducerData
    {
        get => this._cachedProducerData ??= (
                from data in this.Helper.GameContent.Load<IDictionary<string, IDictionary<string, string>>>($"{EasyAccess.ModUniqueId}/Producers")
                select (data.Key, Value: new SerializedProducerData(data.Value)))
            .ToDictionary(data => data.Key, data => (IProducerData)data.Value);
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

    private IDictionary<string, IDictionary<string, string>> LocalProducerData { get; } = new Dictionary<string, IDictionary<string, string>>();

    private HashSet<string> ModDataKeys { get; } = new();

    /// <summary>
    ///     Adds a mod data key to source the qualified item id from.
    /// </summary>
    /// <param name="key">The key to source the qualified item id from.</param>
    public void AddModDataKey(string key)
    {
        this.ModDataKeys.Add(key);
    }

    /// <summary>
    ///     Adds new producer data and saves to assets/producers.json.
    /// </summary>
    /// <param name="id">The qualified item id of the producer.</param>
    /// <param name="data">The producer data to add.</param>
    /// <returns>True if new producer data was added.</returns>
    public bool AddProducerData(string id, IProducerData data = default)
    {
        data ??= new ProducerData();
        if (this.LocalProducerData.ContainsKey(id))
        {
            return false;
        }

        this.LocalProducerData.Add(id, SerializedProducerData.GetData(data));
        return true;
    }

    /// <summary>
    ///     Gets the producer name from an Item.
    /// </summary>
    /// <param name="item">The item to get the producer name for.</param>
    /// <returns>The name of the producer.</returns>
    public string GetProducerName(Item item)
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
    ///     Saves the currently cached producer data back to the local producer data.
    /// </summary>
    public void SaveProducerData()
    {
        foreach (var (key, data) in this.ProducerData)
        {
            this.LocalProducerData[key] = SerializedProducerData.GetData(data);
        }

        this.Helper.Data.WriteJsonFile("assets/producers.json", this.LocalProducerData);
        this.Helper.Multiplayer.SendMessage(this.LocalProducerData, "ProducerData", new[] { EasyAccess.ModUniqueId });
    }

    private void InitProducerData()
    {
        IDictionary<string, IDictionary<string, string>> producerData;

        // Load existing producer data
        try
        {
            producerData = this.Helper.Data.ReadJsonFile<IDictionary<string, IDictionary<string, string>>>("assets/producers.json");
            this.LoadProducerData(producerData);
        }
        catch (Exception)
        {
            // ignored
        }

        // Load new producer data
        var producersDir = Path.Combine(this.Helper.DirectoryPath, "producers");
        Directory.CreateDirectory(producersDir);
        foreach (var path in Directory.GetFiles(producersDir, "*.json"))
        {
            try
            {
                var producerPath = Path.GetRelativePath(this.Helper.DirectoryPath, path);
                producerData = this.Helper.Data.ReadJsonFile<IDictionary<string, IDictionary<string, string>>>(producerPath);
                this.LoadProducerData(producerData);
            }
            catch (Exception e)
            {
                Log.Warn($"Failed loading producer data from {path}.\nError: {e.Message}");
            }
        }

        // Load vanilla special producers
        var specialProducers = new Dictionary<string, IProducerData>
        {
            {
                "Cask", new ProducerData
                {
                    CollectOutputItems = new(new[] { "quality_iridium" }),
                    DispenseInputItems = new(new[] { "category_artisan_goods", "!quality_iridium" }),
                }
            },
            {
                "Crab Pot", new ProducerData
                {
                    DispenseInputItems = new(new[] { "item_bait" }),
                }
            },
        };
        this.LoadProducerData(specialProducers);

        // Load default vanilla producer data
        IProducerData defaultProducer = new ProducerData();
        var vanillaProducers = this.Craftables
                                   .Where(craftable => Enum.IsDefined(typeof(VanillaProducerObjects), craftable.Key))
                                   .ToDictionary(craftable => craftable.Value[0], _ => defaultProducer);
        this.LoadProducerData(vanillaProducers);

        // Save to producers.json
        this.Helper.Data.WriteJsonFile("assets/producers.json", this.LocalProducerData);
    }

    private void LoadProducerData(IDictionary<string, IProducerData> producerData)
    {
        this.LoadProducerData(producerData.ToDictionary(
            data => data.Key,
            data => SerializedProducerData.GetData(data.Value)));
    }

    private void LoadProducerData(IDictionary<string, IDictionary<string, string>> producerData)
    {
        if (producerData is null)
        {
            return;
        }

        foreach (var (key, value) in producerData)
        {
            if (!this.LocalProducerData.ContainsKey(key))
            {
                this.LocalProducerData.Add(key, value);
            }
        }
    }

    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{EasyAccess.ModUniqueId}/Producers"))
        {
            e.LoadFrom(() => this.LocalProducerData,  AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo($"{EasyAccess.ModUniqueId}/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnDayEnding(object sender, DayEndingEventArgs e)
    {
        this._cachedCraftables = null;
        this._cachedProducerData = null;
    }

    private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
    {
        if (e.FromModID != EasyAccess.ModUniqueId)
        {
            return;
        }

        switch (e.Type)
        {
            case "ProducerData":
                Log.Trace("Loading ProducerData from Host");
                var producerData = e.ReadAs<IDictionary<string, IDictionary<string, string>>>();
                this.LocalProducerData.Clear();
                this.LoadProducerData(producerData);
                break;
            case "DefaultProducer":
                Log.Trace("Loading DefaultProducer Config from Host");
                var defaultProducer = e.ReadAs<ProducerData>();
                ((IProducerData)defaultProducer).CopyTo(this.Config.DefaultProducer);
                break;
        }
    }

    private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
    {
        if (e.Peer.IsHost)
        {
            return;
        }

        this.Helper.Multiplayer.SendMessage(this.LocalProducerData, "ProducerData", new[] { EasyAccess.ModUniqueId });
        this.Helper.Multiplayer.SendMessage(this.Config.DefaultProducer, "DefaultProducer", new[] { EasyAccess.ModUniqueId });
    }
}