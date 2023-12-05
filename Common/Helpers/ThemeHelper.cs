namespace StardewMods.Common.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

/// <summary>Handles palette swaps for theme compatibility.</summary>
internal sealed class ThemeHelper
{
    private ThemeHelper(IModHelper helper, string[] assetNames)
    {
        this.Helper = helper;
        this.AssetNames = new(assetNames);
        this.Helper.Events.Content.AssetReady += this.OnAssetReady;
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    private static ThemeHelper? Instance { get; set; }

    private HashSet<string> AssetNames { get; }

    private Dictionary<IAssetName, Texture2D> CachedTextures { get; } = new();

    private IModHelper Helper { get; }

    private bool Initialize { get; set; }

    private Dictionary<Color, Color> PaletteSwap { get; } = new();

    private Dictionary<Point, Color> VanillaPalette { get; } = new()
    {
        { new(17, 369), new(91, 43, 42) },
        { new(18, 370), new(220, 123, 5) },
        { new(19, 371), new(177, 78, 5) },
        { new(20, 372), new(228, 174, 110) },
        { new(21, 373), new(255, 210, 132) },
        { new(104, 471), new(247, 186, 0) },
    };

    /// <summary>Initializes <see cref="ThemeHelper" />.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="assetNames">The asset names to edit.</param>
    /// <returns>Returns an instance of the <see cref="ThemeHelper" /> class.</returns>
    public static ThemeHelper Init(IModHelper helper, params string[] assetNames) =>
        ThemeHelper.Instance ??= new(helper, assetNames);

    private void Edit(IAssetData asset)
    {
        var editor = asset.AsImage();

        if (this.CachedTextures.TryGetValue(asset.Name, out var texture))
        {
            editor.PatchImage(texture);
            return;
        }

        if (this.PaletteSwap.Any())
        {
            texture = this.SwapPalette(editor.Data);
            editor.PatchImage(texture);
            this.CachedTextures.Add(asset.Name, texture);
        }
    }

    private void InitializePalette()
    {
        var colors = new Color[Game1.mouseCursors.Width * Game1.mouseCursors.Height];
        Game1.mouseCursors.GetData(colors);
        foreach (var (point, color) in this.VanillaPalette)
        {
            if (!color.Equals(colors[point.X + (point.Y * Game1.mouseCursors.Width)]))
            {
                this.PaletteSwap[color] = colors[point.X + (point.Y * Game1.mouseCursors.Width)];
            }
        }

        foreach (var assetName in this.AssetNames)
        {
            this.Helper.GameContent.InvalidateCache(assetName);
        }
    }

    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (!this.Initialize || !e.Name.IsEquivalentTo("LooseSprites/Cursors"))
        {
            return;
        }

        this.Initialize = false;
        this.InitializePalette();
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!this.PaletteSwap.Any())
        {
            return;
        }

        if (this.AssetNames.Any(assetName => e.Name.IsEquivalentTo(assetName)))
        {
            e.Edit(this.Edit);
        }
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(name => name.IsEquivalentTo("LooseSprites/Cursors")))
        {
            this.Initialize = true;
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) => this.InitializePalette();

    private Texture2D SwapPalette(Texture2D source)
    {
        var colors = new Color[source.Width * source.Height];
        source.GetData(colors);
        for (var index = 0; index < colors.Length; ++index)
        {
            if (this.PaletteSwap.TryGetValue(colors[index], out var newColor))
            {
                colors[index] = newColor;
            }
        }

        source.SetData(colors);
        return source;
    }
}
