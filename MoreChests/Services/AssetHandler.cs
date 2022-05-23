#nullable disable

namespace StardewMods.MoreChests.Services;

using System.Collections.Generic;
using StardewModdingAPI;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IModService" />
internal class AssetHandler : IModService, IAssetLoader
{
    public AssetHandler(IModServices services)
    {
    }

    private IDictionary<string, string> EmptyData = new Dictionary<string, string>();

    /// <inheritdoc />
    public bool CanLoad<T>(IAssetInfo asset)
    {
        return asset.AssetNameEquals($"{MoreChests.ModUniqueId}/Chests");
    }

    /// <inheritdoc />
    public T Load<T>(IAssetInfo asset)
    {
        return (T)this.EmptyData;
    }
}