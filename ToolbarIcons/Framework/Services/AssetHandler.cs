namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService<AssetHandler>
{
    /// <summary>Game path to Arrows Texture asset.</summary>
    public const string ArrowsPath = BaseService.ModId + "/Arrows";

    /// <summary>Game path to Icons Texture asset.</summary>
    public const string IconPath = BaseService.ModId + "/Icons";

    /// <summary>Game path to Toolbar Data asset.</summary>
    public const string DataPath = BaseService.ModId + "/Data";

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="theming">Dependency used for swapping palettes.</param>
    public AssetHandler(ILog log, IModEvents modEvents, ITheming theming)
        : base(log)
    {
        // Init
        theming.AddAssets(AssetHandler.IconPath, AssetHandler.ArrowsPath);

        // Events
        modEvents.Content.AssetRequested += AssetHandler.OnAssetRequested;
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(AssetHandler.IconPath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(AssetHandler.ArrowsPath))
        {
            e.LoadFromModFile<Texture2D>("assets/arrows.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(AssetHandler.DataPath))
        {
            e.LoadFrom(() => new Dictionary<string, ToolbarIconData>(), AssetLoadPriority.Exclusive);
        }
    }
}