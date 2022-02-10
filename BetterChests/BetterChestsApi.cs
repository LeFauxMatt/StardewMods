namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using Common.Integrations.BetterChests;
using StardewModdingAPI;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Services;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
public class BetterChestsApi : IBetterChestsApi
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly Lazy<ModConfigMenu> _modConfigMenu;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BetterChestsApi" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public BetterChestsApi(IModServices services)
    {
        this._assetHandler = services.Lazy<AssetHandler>();
        this._modConfigMenu = services.Lazy<ModConfigMenu>();
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private ModConfigMenu ModConfigMenu
    {
        get => this._modConfigMenu.Value;
    }

    /// <inheritdoc />
    public void AddChestOptions(IManifest manifest, IDictionary<string, string> data)
    {
        this.ModConfigMenu.ChestConfig(manifest, data);
    }

    /// <inheritdoc />
    public bool RegisterChest(string name)
    {
        return this.Assets.AddChestData(name, new StorageData());
    }

    /// <inheritdoc />
    public void RegisterModDataKey(string key)
    {
        this.Assets.AddModDataKey(key);
    }
}