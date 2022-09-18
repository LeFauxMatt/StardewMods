namespace StardewMods.ExpandedStorage;

using System.Collections.Generic;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Framework;

/// <inheritdoc />
public sealed class ExpandedStorage : Mod
{
    private static readonly IDictionary<string, IManagedStorage> Storages = new Dictionary<string, IManagedStorage>();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        Extensions.GameContent = this.Helper.GameContent;
        Helpers.Init();
        Integrations.Init(this.Helper.ModRegistry);
        ModPatches.Init(this.Helper.GameContent, this.ModManifest, ExpandedStorage.Storages);

        // Events
        this.Helper.Events.Content.AssetRequested += ExpandedStorage.OnAssetRequested;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new ExpandedStorageApi(this.Helper.ContentPacks, ExpandedStorage.Storages);
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (!e.Name.StartsWith("furyx639.ExpandedStorage"))
        {
            return;
        }

        foreach (var (name, storage) in ExpandedStorage.Storages)
        {
            if (e.Name.IsEquivalentTo($"furyx639.ExpandedStorage/Texture/{name}"))
            {
                e.LoadFrom(() => storage.Texture, AssetLoadPriority.Exclusive);
            }
        }
    }
}