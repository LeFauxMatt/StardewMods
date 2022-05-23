#nullable disable

namespace StardewMods.EasyAccess.Models.Config;

using StardewModdingAPI;
using StardewMods.EasyAccess.Features;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class ConfigModel : IConfigModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigModel" /> class.
    /// </summary>
    /// <param name="configData">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ConfigModel(IConfigData configData, IModHelper helper, IModServices services)
    {
        this.Data = configData;
        this.Helper = helper;
        this.Services = services;
    }

    /// <inheritdoc />
    public bool Configurator
    {
        get => this.Data.Configurator;
        set => this.Data.Configurator = value;
    }

    /// <inheritdoc />
    public ControlScheme ControlScheme
    {
        get => this.Data.ControlScheme;
        set => ((IControlScheme)value).CopyTo(this.Data.ControlScheme);
    }

    /// <inheritdoc />
    public ProducerData DefaultProducer
    {
        get => this.Data.DefaultProducer;
        set => ((IProducerData)value).CopyTo(this.Data.DefaultProducer);
    }

    private IConfigData Data { get; }

    private IModHelper Helper { get; }

    private IModServices Services { get; }

    /// <inheritdoc />
    public void Reset()
    {
        ((IConfigData)new ConfigData()).CopyTo(this.Data);
    }

    /// <inheritdoc />
    public void Save()
    {
        this.Helper.WriteConfig((ConfigData)this.Data);
        foreach (var feature in this.Services.FindServices<Feature>())
        {
            feature.Toggle();
        }
    }
}