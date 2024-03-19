namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler : BaseService
{
    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public AssetHandler(IEventSubscriber eventSubscriber, ILog log, IManifest manifest, IThemeHelper themeHelper)
        : base(log, manifest)
    {
        // Init
        this.IconTexturePath = this.ModId + "/Icons";
        themeHelper.AddAssets([this.IconTexturePath]);

        // Events
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    /// <summary>Gets the game path to the icon texture.</summary>
    public string IconTexturePath { get; }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(this.IconTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
    }
}