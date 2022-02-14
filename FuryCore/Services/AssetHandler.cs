namespace StardewMods.FuryCore.Services;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IModService" />
internal class AssetHandler : IModService, IAssetLoader
{
    private IReadOnlyDictionary<string, string[]> _toolbarData;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetHandler" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public AssetHandler(IModHelper helper)
    {
        this.Helper = helper;
        this.Helper.Content.AssetLoaders.Add(this);
    }

    /// <summary>
    ///     Gets the collection of toolbar data.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> ToolbarData
    {
        get
        {
            this._toolbarData ??= (
                    from icon in
                        from data in this.Helper.Content.Load<IDictionary<string, string>>($"{FuryCore.ModUniqueId}/Toolbar", ContentSource.GameContent)
                        select (data.Key, info: data.Value.Split('/'))
                    orderby int.Parse(icon.info[2]), icon.info[0]
                    select (icon.Key, icon.info))
                .ToDictionary(
                    data => data.Key,
                    data => data.info);
            return this._toolbarData;
        }
    }

    private IModHelper Helper { get; }

    /// <inheritdoc />
    public bool CanLoad<T>(IAssetInfo asset)
    {
        return asset.AssetNameEquals($"{FuryCore.ModUniqueId}/Toolbar");
    }

    /// <inheritdoc />
    public T Load<T>(IAssetInfo asset)
    {
        var segment = PathUtilities.GetSegments(asset.AssetName);
        return segment[1] switch
        {
            "Toolbar" when segment.Length == 2
                => (T)(object)new Dictionary<string, string>(),
            _ => default,
        };
    }
}