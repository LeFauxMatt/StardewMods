namespace StardewMods.ItemIconOverlays.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ItemIconOverlays.Framework.Interfaces;
using StardewMods.ItemIconOverlays.Framework.Models;
using StardewValley.GameData.Objects;

/// <summary>Manages icon data for items.</summary>
internal sealed class IconManager : BaseService
{
    private const string AssetPath = "Data/Objects";

    private readonly Dictionary<string, List<IIconData>> data = new();
    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="IconManager" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public IconManager(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        eventSubscriber.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventSubscriber.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    /// <summary>Retrieves icon data for a given item.</summary>
    /// <param name="item">The item to retrieve the icon data for.</param>
    /// <returns>An enumerable collection of icon data for the specified item.</returns>
    public IEnumerable<IIconData> GetData(Item item)
    {
        // Check if item is supported
        if (ItemRegistry.GetData(item.QualifiedItemId)?.RawData is not ObjectData objectData)
        {
            return Enumerable.Empty<IIconData>();
        }

        // Return from cache
        if (this.data.TryGetValue(item.QualifiedItemId, out var list))
        {
            return list;
        }

        list = new List<IIconData>();
        this.data[item.QualifiedItemId] = list;

        // Load icon data
        foreach (var (key, value) in objectData.CustomFields)
        {
            if (!key.StartsWith(this.ModId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            this.Log.TraceOnce("Loading icon data for: {0}", item.QualifiedItemId);

            var keyParts = PathUtilities.GetSegments(key, 3);
            if (keyParts.Length != 3)
            {
                this.Log.WarnOnce(
                    "Failed to load icon data for {0}: invalid custom field key '{1}'.",
                    item.QualifiedItemId,
                    key);

                continue;
            }

            var valueParts = value.Split(',');
            if (valueParts.Length != 2)
            {
                this.Log.WarnOnce(
                    "Failed to load icon data for {0}: invalid custom field value '{1}'.",
                    item.QualifiedItemId,
                    value);

                continue;
            }

            if (!int.TryParse(valueParts[1], out var index))
            {
                this.Log.WarnOnce(
                    "Failed to load icon data for {0}: invalid custom field value '{1}'.",
                    item.QualifiedItemId,
                    value);

                continue;
            }

            Texture2D? texture;
            try
            {
                texture = this.gameContentHelper.Load<Texture2D>(valueParts[0].Trim());
            }
            catch (Exception)
            {
                this.Log.WarnOnce(
                    "Failed to load icon data for {0}: texture '{1}' not found.",
                    item.QualifiedItemId,
                    valueParts[0].Trim());

                continue;
            }

            var columns = texture.Width / 16;
            var iconData = new IconData(
                keyParts[1].Trim(),
                keyParts[2].Trim(),
                valueParts[0].Trim(),
                new Rectangle(index % columns * 16, index / columns * 16, 16, 16));

            list.Add(iconData);
        }

        return list;
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(IconManager.AssetPath)))
        {
            this.data.Clear();
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs e) => this.data.Clear();
}