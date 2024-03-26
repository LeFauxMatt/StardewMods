namespace StardewMods.ExpandedStorage.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.ExpandedStorage;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.ExpandedStorage.Framework.Enums;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <summary>Responsible for managing expanded storage objects.</summary>
internal sealed class StorageManager : BaseService
{
    private const string AssetPath = "Data/BigCraftables";

    private readonly Dictionary<string, IStorageData> data = new();

    /// <summary>Initializes a new instance of the <see cref="StorageManager" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public StorageManager(IEventSubscriber eventSubscriber, ILog log, IManifest manifest)
        : base(log, manifest)
    {
        eventSubscriber.Subscribe<AssetsInvalidatedEventArgs>(this.OnAssetsInvalidated);
        eventSubscriber.Subscribe<ConditionsApiReadyEventArgs>(this.OnConditionsApiReady);
    }

    /// <summary>Tries to retrieve the storage data associated with the specified item.</summary>
    /// <param name="item">The item for which to retrieve the data.</param>
    /// <param name="storageData">
    /// When this method returns, contains the data associated with the specified item, if the
    /// retrieval succeeds; otherwise, null. This parameter is passed uninitialized.
    /// </param>
    /// <returns>true if the data was successfully retrieved; otherwise, false.</returns>
    public bool TryGetData(Item item, [NotNullWhen(true)] out IStorageData? storageData)
    {
        // Return from cache
        if (this.data.TryGetValue(item.QualifiedItemId, out storageData))
        {
            return true;
        }

        // Check if enabled
        if (ItemRegistry.GetData(item.QualifiedItemId)?.RawData is not BigCraftableData bigCraftableData
            || bigCraftableData.CustomFields?.GetBool(this.ModId + "/Enabled") != true)
        {
            storageData = null;
            return false;
        }

        // Load storage data
        this.Log.Trace("Loading managed storage: {0}", item.QualifiedItemId);
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
                    storageData.Frames = customFieldValue.GetInt(1);
                    break;
                case CustomFieldKeys.IsFridge:
                    storageData.IsFridge = customFieldValue.GetBool();
                    break;
                case CustomFieldKeys.OpenNearby:
                    storageData.OpenNearby = customFieldValue.GetBool();
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
                    storageData.PlayerColor = customFieldValue.GetBool();
                    break;
                default:
                    this.Log.Warn("{0} is not a supported attribute", keyParts[2]);
                    break;
            }
        }

        return true;
    }

    private void OnAssetsInvalidated(AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(StorageManager.AssetPath)))
        {
            this.data.Clear();
        }
    }

    private void OnConditionsApiReady(ConditionsApiReadyEventArgs args) => this.data.Clear();
}