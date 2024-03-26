namespace StardewMods.GarbageDay.Framework.Services;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.GameData.BigCraftables;
using xTile;
using xTile.Dimensions;

/// <summary>Handles modification and manipulation of assets in the game.</summary>
internal sealed class AssetHandler : BaseService
{
    private readonly Definitions definitions;
    private readonly GarbageCanManager garbageCanManager;

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="definitions">Dependency used for defining common variables.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="garbageCanManager">Dependency used for managing garbage cans.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public AssetHandler(
        Definitions definitions,
        IEventSubscriber eventSubscriber,
        GarbageCanManager garbageCanManager,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        // Init
        this.definitions = definitions;
        this.garbageCanManager = garbageCanManager;

        // Events
        eventSubscriber.Subscribe<AssetRequestedEventArgs>(this.OnAssetRequested);
    }

    private void OnAssetRequested(AssetRequestedEventArgs e)
    {
        // Load Garbage Can Texture
        if (e.NameWithoutLocale.IsEquivalentTo(this.definitions.TexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/GarbageCan.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo(this.definitions.IconTexturePath))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        // Load Garbage Can Object
        if (e.NameWithoutLocale.IsEquivalentTo(Definitions.BigCraftablePath))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;
                    var bigCraftableData = new BigCraftableData
                    {
                        Name = "Garbage Can",
                        DisplayName = I18n.GarbageCan_Name(),
                        Description = I18n.GarbageCan_Description(),
                        Fragility = 2,
                        IsLamp = false,
                        Texture = this.definitions.TexturePath,
                        CustomFields = new Dictionary<string, string>
                        {
                            { "furyx639.ExpandedStorage/Enabled", "true" },
                            { "furyx639.ExpandedStorage/Frames", "3" },
                            { "furyx639.ExpandedStorage/CloseNearbySound", "trashcanlid" },
                            { "furyx639.ExpandedStorage/OpenNearby", "true" },
                            { "furyx639.ExpandedStorage/OpenNearbySound", "trashcanlid" },
                            { "furyx639.ExpandedStorage/OpenSound", "trashcan" },
                            { "furyx639.ExpandedStorage/PlayerColor", "true" },
                        },
                    };

                    data.Add(this.definitions.ItemId, bigCraftableData);
                });

            return;
        }

        if (e.DataType != typeof(Map))
        {
            return;
        }

        e.Edit(
            asset =>
            {
                var map = asset.AsMap().Data;
                for (var x = 0; x < map.Layers[0].LayerWidth; ++x)
                {
                    for (var y = 0; y < map.Layers[0].LayerHeight; ++y)
                    {
                        var layer = map.GetLayer("Buildings");
                        var tile = layer.PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile is null
                            || !tile.Properties.TryGetValue("Action", out var property)
                            || string.IsNullOrWhiteSpace(property))
                        {
                            continue;
                        }

                        var parts = ArgUtility.SplitBySpace(property);
                        if (parts.Length < 2
                            || !parts[0].Equals("Garbage", StringComparison.OrdinalIgnoreCase)
                            || string.IsNullOrWhiteSpace(parts[1])
                            || !this.garbageCanManager.TryAddFound(parts[1], asset.NameWithoutLocale, x, y))
                        {
                            continue;
                        }

                        // Remove base tile
                        layer.Tiles[x, y] = null;

                        // Remove Lid tile
                        layer = map.GetLayer("Front");
                        layer.Tiles[x, y - 1] = null;

                        // Add NoPath to tile
                        map
                            .GetLayer("Back")
                            .PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size)
                            ?.Properties.Add("NoPath", string.Empty);
                    }
                }
            },
            AssetEditPriority.Late);
    }
}