#nullable disable

namespace StardewMods.EasyAccess;

using System;
using System.Collections.Generic;
using Common.Integrations.EasyAccess;
using StardewModdingAPI;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Services;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
public class EasyAccessApi : IEasyAccessApi
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly Lazy<ModConfigMenu> _modConfigMenu;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EasyAccessApi" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public EasyAccessApi(IModServices services)
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
    public void AddProducerOptions(IManifest manifest, IDictionary<string, string> data)
    {
        this.ModConfigMenu.ProducerConfig(manifest, data);
    }

    /// <inheritdoc />
    public void RegisterModDataKey(string key)
    {
        this.Assets.AddModDataKey(key);
    }

    /// <inheritdoc />
    public bool RegisterProducer(string name)
    {
        return this.Assets.AddProducerData(name, new ProducerData());
    }
}