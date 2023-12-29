namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService<AssetHandler>
{
    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="theming">Dependency used for swapping palettes.</param>
    public AssetHandler(ILog log, IManifest manifest, IModEvents modEvents, ITheming theming)
        : base(log, manifest)
    {
        // Init
        this.ArrowsPath = this.ModId + "/Arrows";
        this.IconPath = this.ModId + "/Icons";
        this.DataPath = this.ModId + "/Data";
        theming.AddAssets([this.IconPath, this.ArrowsPath]);

        // Events
        modEvents.Content.AssetRequested += this.OnAssetRequested;
    }

    /// <summary>Gets the game path to Arrows Texture asset.</summary>
    public string ArrowsPath { get; }

    /// <summary>Gets the game path to Icons Texture asset.</summary>
    public string IconPath { get; }

    /// <summary>Getst he game path to Toolbar Data asset.</summary>
    public string DataPath { get; }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.IconPath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.ArrowsPath))
        {
            e.LoadFromModFile<Texture2D>("assets/arrows.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.DataPath))
        {
            e.LoadFrom(() => new Dictionary<string, ToolbarIconData>(), AssetLoadPriority.Exclusive);
        }
    }
}