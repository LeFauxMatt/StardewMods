namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler
{
    /// <summary>Game path to Tab Texture asset.</summary>
    public const string ArrowsPath = "furyx639.ToolbarIcons/Arrows";

    /// <summary>Game path to Icons asset.</summary>
    public const string IconPath = "furyx639.ToolbarIcons/Icons";

    /// <summary>Game path to Icons asset.</summary>
    public const string ToolbarPath = "furyx639.ToolbarIcons/Toolbar";

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public AssetHandler(IModEvents modEvents) => modEvents.Content.AssetRequested += AssetHandler.OnAssetRequested;

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

        if (e.Name.IsEquivalentTo(AssetHandler.ToolbarPath))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }
}