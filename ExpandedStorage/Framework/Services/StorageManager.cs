namespace StardewMods.ExpandedStorage.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ExpandedStorage.Framework.Enums;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <summary>Responsible for managing expanded storage objects.</summary>
internal sealed class StorageManager : BaseService
{
    private const string AssetPath = "Data/BigCraftables";

    private readonly Dictionary<string, StorageData> data = new();

    /// <summary>Initializes a new instance of the <see cref="StorageManager" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public StorageManager(
        ContentPatcherIntegration contentPatcherIntegration,
        ILog log,
        IManifest manifest,
        IModEvents modEvents)
        : base(log, manifest)
    {
        modEvents.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        contentPatcherIntegration.ConditionsApiReady += this.OnConditionsApiReady;
    }

    /// <summary>Tries to retrieve the storage data associated with the specified item.</summary>
    /// <param name="item">The item for which to retrieve the data.</param>
    /// <param name="storageData">
    /// When this method returns, contains the data associated with the specified item, if the
    /// retrieval succeeds; otherwise, null. This parameter is passed uninitialized.
    /// </param>
    /// <returns>true if the data was successfully retrieved; otherwise, false.</returns>
    public bool TryGetData(Item item, [NotNullWhen(true)] out StorageData? storageData)
    {
        // Return from cache
        if (this.data.TryGetValue(item.QualifiedItemId, out storageData))
        {
            return true;
        }

        // Check if enabled
        if (ItemRegistry.GetData(item.QualifiedItemId)?.RawData is not BigCraftableData bigCraftableData
            || !bigCraftableData.CustomFields.TryGetValue(this.ModId + "/Enabled", out var enabled)
            || !bool.TryParse(enabled, out var isEnabled)
            || !isEnabled)
        {
            storageData = null;
            return false;
        }

        // Load storage data
        this.Log.Trace("Loading managed storage: {0}", [item.QualifiedItemId]);
        storageData = new StorageData();
        this.data.Add(item.QualifiedItemId, storageData);

        foreach (var (customFieldKey, customFieldValue) in bigCraftableData.CustomFields)
        {
            var keyParts = customFieldKey.Split('/');
            if (keyParts.Length != 2
                || !keyParts[0].Equals(this.ModId, StringComparison.OrdinalIgnoreCase)
                || !CustomFieldKeysExtensions.TryParse(keyParts[1], out var storageAttribute))
            {
                continue;
            }

            switch (storageAttribute)
            {
                case CustomFieldKeys.CloseNearbySound:
                    storageData.CloseNearbySound = customFieldValue;
                    break;
                case CustomFieldKeys.Frames:
                    storageData.Frames = int.TryParse(customFieldValue, out var frames) ? frames : 1;
                    break;
                case CustomFieldKeys.IsFridge:
                    storageData.IsFridge = bool.TryParse(customFieldValue, out var isFridge) && isFridge;
                    break;
                case CustomFieldKeys.OpenNearby:
                    storageData.OpenNearby = bool.TryParse(customFieldValue, out var openNearby) && openNearby;
                    break;
                case CustomFieldKeys.OpenNearbySound:
                    storageData.OpenNearbySound = customFieldValue;
                    break;
                case CustomFieldKeys.OpenSound:
                    storageData.OpenSound = customFieldValue;
                    break;
                case CustomFieldKeys.PlaceSound:
                    storageData.PlaceSound = customFieldValue;
                    break;
                case CustomFieldKeys.PlayerColor:
                    storageData.PlayerColor = bool.TryParse(customFieldValue, out var playerColor) && playerColor;
                    break;
                default:
                    this.Log.Warn("{0} is not a supported attribute", [keyParts[2]]);
                    break;
            }
        }

        return true;
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(StorageManager.AssetPath)))
        {
            this.data.Clear();
        }
    }

    private void OnConditionsApiReady(object? sender, EventArgs args) => this.data.Clear();
}