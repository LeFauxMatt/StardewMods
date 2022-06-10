#nullable disable

namespace StardewMods.TooManyAnimals.Models;

using StardewModdingAPI;
using StardewMods.TooManyAnimals.Interfaces;

/// <inheritdoc />
internal class ConfigModel : IConfigModel
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigModel" /> class.
    /// </summary>
    /// <param name="configData">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public ConfigModel(IConfigData configData, IModHelper helper)
    {
        this.Data = configData;
        this.Helper = helper;
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