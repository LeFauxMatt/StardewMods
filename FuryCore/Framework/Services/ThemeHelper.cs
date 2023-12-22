namespace StardewMods.FuryCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;

/// <inheritdoc />
internal sealed class ThemeHelper : IThemeHelper
{
    private readonly HashSet<string> trackedAssets = [];
    private readonly Dictionary<IAssetName, Texture2D> cachedTextures = new();
    private readonly IGameContentHelper gameContent;
    private readonly Dictionary<Color, Color> paletteSwap = new();

    private readonly Dictionary<Point, Color> vanillaPalette = new()
    {
        { new(17, 369), new(91, 43, 42) },
        { new(18, 370), new(220, 123, 5) },
        { new(19, 371), new(177, 78, 5) },
        { new(20, 372), new(228, 174, 110) },
        { new(21, 373), new(255, 210, 132) },
        { new(104, 471), new(247, 186, 0) },
    };

    private bool initialize;

    /// <summary>Initializes a new instance of the <see cref="ThemeHelper" /> class.</summary>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    public ThemeHelper(IModEvents events, IGameContentHelper gameContent)
    {
        this.gameContent = gameContent;
        events.Content.AssetReady += this.OnAssetReady;
        events.Content.AssetRequested += this.OnAssetRequested;
        events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc/>
    public void AddAssets(params string[] assetNames) => this.trackedAssets.UnionWith(assetNames);

    private void Edit(IAssetData asset)
    {
        var editor = asset.AsImage();

        if (this.cachedTextures.TryGetValue(asset.Name, out var texture))
        {
            editor.PatchImage(texture);
            return;
        }

        if (this.paletteSwap.Any())
        {
            texture = this.SwapPalette(editor.Data);
            editor.PatchImage(texture);
            this.cachedTextures.Add(asset.Name, texture);
        }
    }

    private void InitializePalette()
    {
        var colors = new Color[Game1.mouseCursors.Width * Game1.mouseCursors.Height];
        Game1.mouseCursors.GetData(colors);
        foreach (var (point, color) in this.vanillaPalette)
        {
            if (!color.Equals(colors[point.X + (point.Y * Game1.mouseCursors.Width)]))
            {
                this.paletteSwap[color] = colors[point.X + (point.Y * Game1.mouseCursors.Width)];
            }
        }

        foreach (var assetName in this.trackedAssets)
        {
            this.gameContent.InvalidateCache(assetName);
        }
    }

    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (!this.initialize || !e.Name.IsEquivalentTo("LooseSprites/Cursors"))
        {
            return;
        }

        this.initialize = false;
        this.InitializePalette();
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!this.paletteSwap.Any())
        {
            return;
        }

        if (this.trackedAssets.Any(assetName => e.Name.IsEquivalentTo(assetName)))
        {
            e.Edit(this.Edit);
        }
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo("LooseSprites/Cursors")))
        {
            this.initialize = true;
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) => this.InitializePalette();

    private Texture2D SwapPalette(Texture2D source)
    {
        var colors = new Color[source.Width * source.Height];
        source.GetData(colors);
        for (var index = 0; index < colors.Length; ++index)
        {
            if (this.paletteSwap.TryGetValue(colors[index], out var newColor))
            {
                colors[index] = newColor;
            }
        }

        source.SetData(colors);
        return source;
    }
}
