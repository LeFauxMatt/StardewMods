#nullable disable

namespace StardewMods.EasyAccess.Models.Config;

using StardewModdingAPI;
using StardewMods.EasyAccess.Interfaces.Config;

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
    public int CollectOutputDistance
    {
        get => this.Data.CollectOutputDistance;
        set => this.Data.CollectOutputDistance = value;
    }

    /// <inheritdoc />
    public ControlScheme ControlScheme
    {
        get => this.Data.ControlScheme;
        set => ((IControlScheme)value).CopyTo(this.Data.ControlScheme);
    }

    /// <inheritdoc />
    public int DispenseInputDistance
    {
        get => this.Data.DispenseInputDistance;
        set => this.Data.DispenseInputDistance = value;
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