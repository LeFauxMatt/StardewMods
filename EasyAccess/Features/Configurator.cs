#nullable disable

namespace StardewMods.EasyAccess.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Interfaces.ManagedObjects;
using StardewMods.EasyAccess.Services;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;

/// <inheritdoc />
internal class Configurator : Feature
{
    private readonly Lazy<IConfigureGameObject> _configureGameObject;
    private readonly PerScreen<IDictionary<string, string>> _currentData = new();
    private readonly PerScreen<IManagedObject> _currentObject = new();
    private readonly Lazy<ModConfigMenu> _modConfigMenu;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Configurator" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public Configurator(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._configureGameObject = services.Lazy<IConfigureGameObject>();
        this._modConfigMenu = services.Lazy<ModConfigMenu>();
    }

    private IConfigureGameObject ConfigureGameObject
    {
        get => this._configureGameObject.Value;
    }

    private IDictionary<string, string> CurrentData
    {
        get => this._currentData.Value;
        set => this._currentData.Value = value;
    }

    private IManagedObject CurrentObject
    {
        get => this._currentObject.Value;
        set => this._currentObject.Value = value;
    }

    private ModConfigMenu ModConfigMenu
    {
        get => this._modConfigMenu.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.ConfigureGameObject.ConfiguringGameObject += this.OnConfiguringGameObject;
        this.ConfigureGameObject.ResettingConfig += this.OnResettingConfig;
        this.ConfigureGameObject.SavingConfig += this.OnSavingConfig;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.ConfigureGameObject.ConfiguringGameObject -= this.OnConfiguringGameObject;
        this.ConfigureGameObject.ResettingConfig -= this.OnResettingConfig;
        this.ConfigureGameObject.SavingConfig -= this.OnSavingConfig;
    }

    private void OnConfiguringGameObject(object sender, IConfiguringGameObjectEventArgs e)
    {
        if (!this.ManagedObjects.TryGetManagedProducer(e.GameObject, out var managedStorage))
        {
            this.CurrentObject = null;
            this.CurrentData = null;
            return;
        }

        this.CurrentObject = managedStorage;
        this.CurrentData = this.CurrentObject.ModData.Pairs
                               .Where(modData => modData.Key.StartsWith($"{EasyAccess.ModUniqueId}"))
                               .ToDictionary(
                                   modData => modData.Key[(EasyAccess.ModUniqueId.Length + 1)..],
                                   modData => modData.Value);
        this.ModConfigMenu.ProducerConfig(e.ModManifest, this.CurrentData, this.CurrentObject.QualifiedItemId);
    }

    private void OnResettingConfig(object sender, IResettingConfigEventArgs e)
    {
        if (this.CurrentObject is null || this.CurrentData is null)
        {
            return;
        }

        foreach (var (key, _) in this.CurrentData)
        {
            this.CurrentData[key] = string.Empty;
        }
    }

    private void OnSavingConfig(object sender, ISavingConfigEventArgs e)
    {
        if (this.CurrentObject is null || this.CurrentData is null)
        {
            return;
        }

        foreach (var (key, value) in this.CurrentData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.CurrentObject.ModData.Remove($"{EasyAccess.ModUniqueId}/{key}");
                continue;
            }

            this.CurrentObject.ModData[$"{EasyAccess.ModUniqueId}/{key}"] = value;
        }
    }
}