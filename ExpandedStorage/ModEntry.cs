namespace StardewMods.ExpandedStorage;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework;
using StardewMods.ExpandedStorage.Models;
using StardewValley.Objects;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private static readonly IDictionary<string, LegacyAsset> LegacyAssets = new Dictionary<string, LegacyAsset>();
    private static readonly IDictionary<string, CachedStorage> StorageCache = new Dictionary<string, CachedStorage>();
    private static readonly IDictionary<string, ICustomStorage> Storages = new Dictionary<string, ICustomStorage>();

    private bool _wait;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        Extensions.Init(ModEntry.StorageCache);
        Integrations.Init(this.Helper.ModRegistry);
        Config.Init(this.Helper, this.ModManifest);
        Commands.Init(this.Helper, ModEntry.Storages);
        ModPatches.Init(this.Helper, this.ModManifest, ModEntry.Storages, ModEntry.StorageCache);

        // Events
        this.Helper.Events.Content.AssetRequested += ModEntry.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new Api(this.Helper, ModEntry.Storages, ModEntry.StorageCache, ModEntry.LegacyAssets);
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
            return;
        }

        if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
        {
            var craftingRecipes = new Dictionary<string, string>();
            foreach (var (_, legacyAsset) in ModEntry.LegacyAssets)
            {
                var (id, recipe) = legacyAsset.CraftingRecipe;
                if (!string.IsNullOrWhiteSpace(recipe))
                {
                    craftingRecipes.Add(id, recipe);
                }
            }

            if (craftingRecipes.Any())
            {
                e.Edit(
                    asset =>
                    {
                        var data = asset.AsDictionary<string, string>().Data;
                        foreach (var (key, craftingRecipe) in craftingRecipes)
                        {
                            data.Add(key, craftingRecipe);
                        }
                    });
            }

            return;
        }

        if (!e.Name.IsDirectlyUnderPath("ExpandedStorage/SpriteSheets"))
        {
            return;
        }

        foreach (var (key, legacyAsset) in ModEntry.LegacyAssets)
        {
            if (!e.Name.IsEquivalentTo($"ExpandedStorage/SpriteSheets/{key}"))
            {
                continue;
            }

            e.LoadFrom(() => legacyAsset.Texture, AssetLoadPriority.Exclusive);
            return;
        }
    }

    private static void OnStorageTypeRequested(object? sender, IStorageTypeRequestedEventArgs e)
    {
        foreach (var (name, storage) in ModEntry.Storages)
        {
            if (storage.BetterChestsData is null
             || e.Context is not Chest chest
             || !chest.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var storageName)
             || !storageName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var config = Config.GetConfig(name);
            e.Load(config?.BetterChestsData ?? storage.BetterChestsData, 1000);
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
             && ModEntry.Storages.ContainsKey(name))
            {
                Game1.player.craftingRecipes.Add(name, 0);
            }
        }

        // Reset cached textures
        foreach (var (_, cachedStorage) in ModEntry.StorageCache)
        {
            cachedStorage.ResetCache();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this._wait = true;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

        if (Integrations.BetterChests.IsLoaded)
        {
            Integrations.BetterChests.Api.StorageTypeRequested += ModEntry.OnStorageTypeRequested;
        }

        if (Integrations.BetterCrafting.IsLoaded)
        {
            Integrations.BetterCrafting.Api.AddRecipeProvider(
                new RecipeProvider(ModEntry.Storages, ModEntry.StorageCache));
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (this._wait)
        {
            this._wait = false;
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

        Config.SetupConfig(ModEntry.Storages);
    }
}