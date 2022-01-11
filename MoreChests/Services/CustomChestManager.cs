namespace MoreChests.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Integrations.BetterChests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

internal class CustomChestManager : BaseService
{
    private readonly BetterChestsIntegration _betterChests;
    private readonly IDictionary<string, ChestData> _chestData = new Dictionary<string, ChestData>();
    private AssetHandler _assetHandler;

    private CustomChestManager(ServiceLocator serviceLocator)
        : base("ChestManager")
    {
        // Init
        this._betterChests = new(serviceLocator.Helper.ModRegistry);

        // Dependencies
        this.AddDependency<AssetHandler>(service => this._assetHandler = service as AssetHandler);
    }

    public bool TryGetChestData(string name, out ChestData chestData)
    {
        return this._chestData.TryGetValue(name, out chestData);
    }

    public bool TryCreate(Item item, out Chest chest)
    {
        if (!this._chestData.TryGetValue(item.Name, out var chestData))
        {
            chest = default;
            return false;
        }

        chest = new(true, Vector2.Zero, 232)
        {
            Name = item.Name,
        };

        if (item is Chest oldChest)
        {
            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            if (oldChest.items.Any())
            {
                chest.items.CopyFrom(oldChest.items);
            }
        }

        return true;
    }

    public IEnumerable<string> GetAllChests()
    {
        return this._chestData.Keys;
    }

    public bool Exists(string name)
    {
        return this._chestData.ContainsKey(name);
    }

    public void AddChests(IContentPack contentPack, IEnumerable<KeyValuePair<string, ChestData>> chestData)
    {
        foreach (var data in chestData)
        {
            if (!this.TryAddTexture(contentPack, data))
            {
                continue;
            }

            this._chestData.Add(data.Key, data.Value);
            this.EnableFeatures(data.Key, data.Value);
        }
    }

    private bool TryAddTexture(IContentPack contentPack, KeyValuePair<string, ChestData> data)
    {
        if (!string.IsNullOrWhiteSpace(data.Value.Image) && contentPack.HasFile(data.Value.Image))
        {
            var texture = contentPack.LoadAsset<Texture2D>(data.Value.Image);
            if (texture.Width > 16 || texture.Height > 32)
            {
                if (!this._betterChests.IsLoaded)
                {
                    Log.Warn("Better Chests is required to load bigger chests.", true);
                    return false;
                }

                this._betterChests.API.ConfigureChest(name, "BiggerChest", new Tuple<int, int, int>(texture.Width, texture.Height, data.Value.Depth));
            }

            this._assetHandler.AddAsset(data.Key, texture);
        }

        return true;
    }

    private void EnableFeatures(string name, ChestData data)
    {
        if (!this._betterChests.IsLoaded)
        {
            return;
        }

        this._betterChests.API.RegisterCustomChest(name, $"{ModEntry.ModPrefix}/Name", name);

        if (data.FilterItems.Any())
        {
            this._betterChests.API.ConfigureChest(name, "FilterItems", data.FilterItems);
        }

        if (data.OpenNearby > 0)
        {
            this._betterChests.API.ConfigureChest(name, "OpenNearby", data.OpenNearby);
        }

        if (!data.PlayerColor)
        {
            this._betterChests.API.ConfigureChest(name, "ColorPicker", false);
        }

        foreach (var featureName in data.DisabledFeatures)
        {
            this._betterChests.API.ConfigureChest(name, featureName, false);
        }

        // Skip features overriden by player config
        if (data.PlayerConfig)
        {
            return;
        }

        if (data.Capacity != 0)
        {
            this._betterChests.API.ConfigureChest(name, "ExpandedMenu", true);
            this._betterChests.API.ConfigureChest(name, "Capacity", data.Capacity);
        }

        foreach (var featureName in data.EnabledFeatures.Except(data.DisabledFeatures))
        {
            this._betterChests.API.ConfigureChest(name, featureName, true);
        }
    }
}