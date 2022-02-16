namespace StardewMods.TooManyAnimals.Models;

using StardewModdingAPI;
using StardewMods.FuryCore.Interfaces;
using StardewMods.TooManyAnimals.Interfaces;

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
    public int AnimalShopLimit
    {
        get => this.Data.AnimalShopLimit;
        set => this.Data.AnimalShopLimit = value;
    }

    /// <inheritdoc />
    public ControlScheme ControlScheme
    {
        get => this.Data.ControlScheme;
        set => ((IControlScheme)value).CopyTo(this.Data.ControlScheme);
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
    }
}