namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
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
        this.IconTexturePath = this.ModId + "/Icons";
        theming.AddAssets([this.IconTexturePath]);

        // Events
        modEvents.Content.AssetRequested += this.OnAssetRequested;
    }

    /// <summary>Gets the game path to the icon texture.</summary>
    public string IconTexturePath { get; }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.IconTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
    }
}