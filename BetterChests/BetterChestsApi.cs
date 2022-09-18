namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
public sealed class BetterChestsApi : IBetterChestsApi
{
    private readonly IStorageData _default;
    private readonly Dictionary<Func<object, bool>, IStorageData> _storageTypes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BetterChestsApi" /> class.
    /// </summary>
    /// <param name="storageTypes">A dictionary of all registered storage types.</param>
    /// <param name="defaultChest">Default data for any storage.</param>
    public BetterChestsApi(Dictionary<Func<object, bool>, IStorageData> storageTypes, IStorageData defaultChest)
    {
        this._storageTypes = storageTypes;
        this._default = defaultChest;
    }

    /// <inheritdoc />
    public event EventHandler<ICraftingStoragesLoadingEventArgs> CraftingStoragesLoading
    {
        add => BetterCrafting.CraftingStoragesLoading += value;
        remove => BetterCrafting.CraftingStoragesLoading -= value;
    }

    /// <inheritdoc />
    public IEnumerable<IStorageObject> AllStorages => Storages.All;

    /// <inheritdoc />
    public Dictionary<string, IStorageData> StorageTypes => Storages.Types;

    /// <inheritdoc />
    public void AddConfigOptions(IManifest manifest, IStorageData storage)
    {
        Config.SetupSpecificConfig(manifest, storage);
    }

    /// <inheritdoc />
    public IEnumerable<IStorageObject> GetStorages(Farmer farmer)
    {
        return Storages.FromPlayer(farmer);
    }

    /// <inheritdoc />
    public IEnumerable<IStorageObject> GetStorages(GameLocation location)
    {
        return Storages.FromLocation(location);
    }

    /// <inheritdoc />
    public void RegisterChest(Func<object, bool> predicate, IStorageData storage)
    {
        this._storageTypes[predicate] = new StorageNodeData(storage, this._default);
    }

    /// <inheritdoc />
    public void ShowCraftingPage()
    {
        BetterCrafting.ShowCraftingPage();
    }

    /// <inheritdoc />
    public bool TryGetStorage(object context, [NotNullWhen(true)] out IStorageObject? storage)
    {
        return Storages.TryGetOne(context, out storage);
    }
}