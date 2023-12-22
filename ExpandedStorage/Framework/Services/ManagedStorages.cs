namespace StardewMods.ExpandedStorage.Framework.Services;

using System.ComponentModel;
using StardewModdingAPI.Events;
using StardewMods.Common.Integrations.ContentPatcher;
using StardewMods.Common.Services;
using StardewMods.ExpandedStorage.Framework.Enums;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.GameData.BigCraftables;

/// <summary>Handles the objects which should be managed by Expanded Storages.</summary>
internal sealed class ManagedStorages
{
    private const string AssetPath = "Data/BigCraftables";
    private const string CustomFieldPrefix = "furyx639.ExpandedStorage";
    private readonly ContentPatcherIntegration contentPatcher;

    private readonly Dictionary<string, StorageData> data = new();
    private readonly IModEvents events;
    private readonly IGameContentHelper gameContent;
    private readonly Logging logging;

    private bool nextTick;

    /// <summary>Initializes a new instance of the <see cref="ManagedStorages" /> class.</summary>
    /// <param name="contentPatcher">Dependency for Content Patcher integration.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    public ManagedStorages(
        ContentPatcherIntegration contentPatcher,
        IModEvents events,
        IGameContentHelper gameContent,
        Logging logging)
    {
        // Init
        this.contentPatcher = contentPatcher;
        this.events = events;
        this.gameContent = gameContent;
        this.logging = logging;
        this.nextTick = true;

        // Events
        this.events.Content.AssetReady += this.OnAssetReady;
        this.events.GameLoop.UpdateTicked += this.OnUpdateTicked;
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

            this.logging.Trace("Found managed storage: {0}", itemId);
            if (!this.data.TryGetValue(itemId, out var storage))
            {
                storage = new();
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
                    default:
                        throw new InvalidEnumArgumentException($"{keyParts[2]} is not a supported attribute");
                }
            }
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (this.contentPatcher.IsLoaded && !this.contentPatcher.Api.IsConditionsApiReady)
        {
            return;
        }

        if (this.nextTick)
        {
            this.nextTick = false;
            return;
        }

        this.events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        _ = this.gameContent.Load<Dictionary<string, BigCraftableData>>(ManagedStorages.AssetPath);
    }
}
