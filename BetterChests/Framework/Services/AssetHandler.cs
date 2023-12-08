namespace StardewMods.BetterChests.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler
{
    /// <summary>Game path to Icons asset.</summary>
    internal const string IconPath = "furyx639.BetterChests/Icons";

    /// <summary>Game path to Tab Texture asset.</summary>
    internal const string TabsPath = "furyx639.BetterChests/Tabs/Texture";

    /// <summary>Game path to Hue Bar asset.</summary>
    private const string HueBarPath = "furyx639.BetterChests/HueBar";

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="events">Dependency used for managing access to events.</param>
    public AssetHandler(IModEvents events) => events.Content.AssetRequested += AssetHandler.OnAssetRequested;

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(AssetHandler.HueBarPath))
        {
            e.LoadFromModFile<Texture2D>("assets/hue.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(AssetHandler.IconPath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(AssetHandler.TabsPath))
        {
            e.LoadFromModFile<Texture2D>("assets/tabs.png", AssetLoadPriority.Exclusive);
        }
    }
}
