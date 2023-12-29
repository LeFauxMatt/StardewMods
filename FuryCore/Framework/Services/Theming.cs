namespace StardewMods.FuryCore.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.FuryCore.ITheming" />
internal sealed class Theming : BaseService, ITheming
{
    private readonly Dictionary<IAssetName, Texture2D> cachedTextures = new();
    private readonly IGameContentHelper gameContentHelper;
    private readonly Dictionary<Color, Color> paletteSwap = new();
    private readonly HashSet<string> trackedAssets = [];

    private readonly Dictionary<Point, Color> vanillaPalette = new()
    {
        { new Point(17, 369), new Color(91, 43, 42) },
        { new Point(18, 370), new Color(220, 123, 5) },
        { new Point(19, 371), new Color(177, 78, 5) },
        { new Point(20, 372), new Color(228, 174, 110) },
        { new Point(21, 373), new Color(255, 210, 132) },
        { new Point(104, 471), new Color(247, 186, 0) },
    };

    private bool initialize;

    /// <summary>Initializes a new instance of the <see cref="Theming" /> class.</summary>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public Theming(IGameContentHelper gameContentHelper, ILog log, IManifest manifest, IModEvents modEvents)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        modEvents.Content.AssetReady += this.OnAssetReady;
        modEvents.Content.AssetRequested += this.OnAssetRequested;
        modEvents.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        modEvents.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc />
    public void AddAssets(string[] assetNames) => this.trackedAssets.UnionWith(assetNames);

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
            this.gameContentHelper.InvalidateCache(assetName);
        }
    }

    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (!this.initialize || !e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Cursors"))
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
        if (e.NamesWithoutLocale.Any(name => name.IsEquivalentTo("LooseSprites/Cursors")))
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