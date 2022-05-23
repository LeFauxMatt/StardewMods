namespace StardewMods.FuryCore.Services;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IModService" />
internal class AssetHandler : IModService
{
    private readonly PerScreen<IReadOnlyDictionary<string, string[]>> _toolbarData = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssetHandler" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public AssetHandler(IModHelper helper)
    {
        this.Helper = helper;
        this.Helper.Events.Content.AssetRequested += AssetHandler.OnAssetRequested;
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
                        from data in this.Helper.GameContent.Load<IDictionary<string, string>>($"{FuryCore.ModUniqueId}/Toolbar")
                        select (data.Key, info: data.Value.Split('/'))
                    orderby icon.Key
                    select (icon.Key, icon.info))
                .ToDictionary(
                    data => data.Key,
                    data => data.info);
        }
    }

    private IModHelper Helper { get; }

    private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{FuryCore.ModUniqueId}/ConfigTool"))
        {
            e.LoadFromModFile<Texture2D>("assets/ConfigTool.png", AssetLoadPriority.Exclusive);
        }
        else if (e.Name.IsEquivalentTo($"{FuryCore.ModUniqueId}/Toolbar"))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }
}