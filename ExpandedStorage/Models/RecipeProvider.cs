namespace StardewMods.ExpandedStorage.Models;

using System.Collections.Generic;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework;

/// <inheritdoc />
internal sealed class RecipeProvider : IRecipeProvider
{
    private readonly IDictionary<string, CachedStorage> _storageCache;
    private readonly IDictionary<string, ICustomStorage> _storages;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RecipeProvider" /> class.
    /// </summary>
    /// <param name="storages">All custom chests currently loaded in the game.</param>
    /// <param name="storageCache">Cached storage textures and attributes.</param>
    public RecipeProvider(IDictionary<string, ICustomStorage> storages, IDictionary<string, CachedStorage> storageCache)
    {
        this._storages = storages;
        this._storageCache = storageCache;
    }

    /// <inheritdoc />
    public bool CacheAdditionalRecipes => false;

    /// <inheritdoc />
    public int RecipePriority => 1000;

    /// <inheritdoc />
    public IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking)
    {
        return null;
    }

    /// <inheritdoc />
    public IRecipe? GetRecipe(CraftingRecipe recipe)
    {
        var name = recipe.name.EndsWith("Recipe") ? recipe.name[..^6].Trim() : recipe.name;
        if (!this._storages.TryGetValue(name, out var storage))
        {
            return null;
        }

        var cachedStorage = this._storageCache.Get(storage);
        return Integrations.BetterCrafting.API!.RecipeBuilder(recipe)
                           .DisplayName(() => storage.DisplayName)
                           .Description(() => storage.Description)
                           .Texture(() => cachedStorage.Texture)
                           .Source(() => new(0, 0, storage.Width, storage.Height))
                           .Build();
    }
}