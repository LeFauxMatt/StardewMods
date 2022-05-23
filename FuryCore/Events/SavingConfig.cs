#nullable disable

namespace StardewMods.FuryCore.Events;

using System;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
internal class SavingConfig : SortedEventHandler<ISavingConfigEventArgs>
{
    private readonly Lazy<ConfigureGameObject> _configureGameObject;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SavingConfig" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public SavingConfig(IModServices services)
    {
        this._configureGameObject = services.Lazy<ConfigureGameObject>();
    }

    private ConfigureGameObject ConfigureGameObject
    {
        get => this._configureGameObject.Value;
    }

    /// <summary>
    ///     Saves the current configuration data.
    /// </summary>
    public void Save()
    {
        this.InvokeAll(new SavingConfigEventArgs(this.ConfigureGameObject.CurrentObject));
    }
}