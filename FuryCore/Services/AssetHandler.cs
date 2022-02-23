namespace StardewMods.FuryCore.Services;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IModService" />
internal class AssetHandler : IModService, IAssetLoader
{
    private readonly PerScreen<IReadOnlyDictionary<string, string[]>> _toolbarData = new();

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
            return this._toolbarData.Value ??= (
                    from icon in
                        from data in this.Helper.Content.Load<IDictionary<string, string>>($"{FuryCore.ModUniqueId}/Toolbar", ContentSource.GameContent)
                        select (data.Key, info: data.Value.Split('/'))
                    orderby icon.Key
                    select (icon.Key, icon.info))
                .ToDictionary(
                    data => data.Key,
                    data => data.info);
        }
    }

    private IModHelper Helper { get; }

    /// <inheritdoc />
    public bool CanLoad<T>(IAssetInfo asset)
    {
        return asset.AssetNameEquals($"{FuryCore.ModUniqueId}/ConfigTool")
               || asset.AssetNameEquals($"{FuryCore.ModUniqueId}/Toolbar");
    }

    /// <inheritdoc />
    public T Load<T>(IAssetInfo asset)
    {
        var segment = PathUtilities.GetSegments(asset.AssetName);
        return segment[1] switch
        {
            "ConfigTool" when segment.Length == 2
                => (T)(object)this.Helper.Content.Load<Texture2D>("assets/ConfigTool.png"),
            "Toolbar" when segment.Length == 2
                => (T)(object)new Dictionary<string, string>(),
            _ => default,
        };
    }
}