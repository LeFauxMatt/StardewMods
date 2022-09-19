namespace StardewMods.ExpandedStorage;

using System;
using System.Collections.Generic;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework;
using StardewMods.ExpandedStorage.Models;
using StardewValley.Objects;

/// <inheritdoc />
public sealed class ExpandedStorage : Mod
{
    private static readonly IDictionary<string, CachedStorage> StorageCache = new Dictionary<string, CachedStorage>();
    private static readonly IDictionary<string, ICustomStorage> Storages = new Dictionary<string, ICustomStorage>();

    private int _wait;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        Extensions.Init(ExpandedStorage.StorageCache);
        Integrations.Init(this.Helper.ModRegistry);
        ModPatches.Init(this.Helper, this.ModManifest, ExpandedStorage.Storages);

        // Events
        this.Helper.Events.Content.AssetRequested += ExpandedStorage.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ExpandedStorageApi(ExpandedStorage.Storages);
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.ExpandedStorage/Storages"))
        {
            e.LoadFrom(() => new Dictionary<string, CustomStorageData>(), AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.ExpandedStorage/Buy"))
        {
            e.LoadFrom(() => new Dictionary<string, ShopEntry>(), AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.ExpandedStorage/Unlock"))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }

    private static void OnStorageTypeRequested(object? sender, IStorageTypeRequestedEventArgs e)
    {
        foreach (var (name, storage) in ExpandedStorage.Storages)
        {
            if (storage.BetterChestsData is null
             || e.Context is not Chest chest
             || !chest.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName)
             || !storageName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            e.Load(storage.BetterChestsData, 1000);
        }
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        // Unlock crafting recipes
        var recipes = this.Helper.GameContent.Load<Dictionary<string, string>>("Data/CraftingRecipes");
        var unlock = this.Helper.GameContent.Load<Dictionary<string, string?>>("furyx639.ExpandedStorage/Unlock");
        foreach (var (name, _) in unlock)
        {
            if (recipes.ContainsKey(name)
             && !Game1.player.craftingRecipes.ContainsKey(name)
             && ExpandedStorage.Storages.ContainsKey(name))
            {
                Game1.player.craftingRecipes.Add(name, 0);
            }
        }

        // Reset cached textures
        foreach (var (_, cachedStorage) in ExpandedStorage.StorageCache)
        {
            cachedStorage.ResetCache();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this._wait = 3;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

        if (Integrations.BetterChests.IsLoaded)
        {
            Integrations.BetterChests.API.StorageTypeRequested += ExpandedStorage.OnStorageTypeRequested;
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (--this._wait > 0)
        {
            return;
        }

        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;

        var api = (IExpandedStorageApi)this.GetApi();
        var storages =
            this.Helper.GameContent.Load<Dictionary<string, CustomStorageData>>("furyx639.ExpandedStorage/Storages");

        foreach (var (name, storage) in storages)
        {
            api.RegisterStorage(name, storage);
        }
    }
}