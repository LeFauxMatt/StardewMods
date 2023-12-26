namespace StardewMods.ExpandedStorage.Framework.Services;

using System.ComponentModel;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ExpandedStorage.Framework.Enums;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <summary>Handles the objects which should be managed by Expanded Storages.</summary>
internal sealed class ManagedStorages
{
    private const string AssetPath = "Data/BigCraftables";
    private const string CustomFieldPrefix = "furyx639.ExpandedStorage";

    private readonly Dictionary<string, StorageData> data = new();
    private readonly IGameContentHelper gameContentHelper;
    private readonly ILog log;

    /// <summary>Initializes a new instance of the <see cref="ManagedStorages" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    public ManagedStorages(
        ILog log,
        ContentPatcherIntegration contentPatcherIntegration,
        IModEvents modEvents,
        IGameContentHelper gameContentHelper)
    {
        // Init
        this.log = log;
        this.gameContentHelper = gameContentHelper;

        // Events
        modEvents.Content.AssetReady += this.OnAssetReady;
        contentPatcherIntegration.ConditionsApiReady += this.OnConditionsApiReady;
    }

    /// <summary>Gets the Storage Data for Expanded Storage objects.</summary>
    public IReadOnlyDictionary<string, StorageData> Data => this.data;

    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (!e.Name.IsEquivalentTo(ManagedStorages.AssetPath))
        {
            return;
        }

        this.data.Clear();
        foreach (var (itemId, bigCraftableData) in Game1.bigCraftableData)
        {
            if (!bigCraftableData.CustomFields.TryGetValue(
                    $"{ManagedStorages.CustomFieldPrefix}/Enabled",
                    out var enabled)
                || !bool.TryParse(enabled, out var isEnabled)
                || !isEnabled)
            {
                continue;
            }

            this.log.Trace("Found managed storage: {0}", itemId);
            if (!this.data.TryGetValue(itemId, out var storage))
            {
                storage = new StorageData();
                this.data.Add(itemId, storage);
            }

            foreach (var (customFieldKey, customFieldValue) in bigCraftableData.CustomFields)
            {
                var keyParts = customFieldKey.Split('/');
                if (keyParts.Length != 2
                    || !keyParts[0].Equals(ManagedStorages.CustomFieldPrefix, StringComparison.OrdinalIgnoreCase)
                    || !CustomFieldKeysExtensions.TryParse(keyParts[1], out var storageAttribute))
                {
                    continue;
                }

                switch (storageAttribute)
                {
                    case CustomFieldKeys.CloseNearbySound:
                        storage.CloseNearbySound = customFieldValue;
                        break;
                    case CustomFieldKeys.Frames:
                        storage.Frames = int.TryParse(customFieldValue, out var frames) ? frames : 1;
                        break;
                    case CustomFieldKeys.IsFridge:
                        storage.IsFridge = bool.TryParse(customFieldValue, out var isFridge) && isFridge;
                        break;
                    case CustomFieldKeys.OpenNearby:
                        storage.OpenNearby = bool.TryParse(customFieldValue, out var openNearby) && openNearby;
                        break;
                    case CustomFieldKeys.OpenNearbySound:
                        storage.OpenNearbySound = customFieldValue;
                        break;
                    case CustomFieldKeys.OpenSound:
                        storage.OpenSound = customFieldValue;
                        break;
                    case CustomFieldKeys.PlaceSound:
                        storage.PlaceSound = customFieldValue;
                        break;
                    case CustomFieldKeys.PlayerColor:
                        storage.PlayerColor = bool.TryParse(customFieldValue, out var playerColor) && playerColor;
                        break;
                    default: throw new InvalidEnumArgumentException($"{keyParts[2]} is not a supported attribute");
                }
            }
        }
    }

    private void OnConditionsApiReady(object? sender, EventArgs args) =>
        _ = this.gameContentHelper.Load<Dictionary<string, BigCraftableData>>(ManagedStorages.AssetPath);
}