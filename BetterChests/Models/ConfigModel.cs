namespace StardewMods.BetterChests.Models;

using StardewModdingAPI;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;

/// <summary>
///     Encapsulates a <see cref="ConfigData" /> wrapper class.
/// </summary>
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
    public int CarryChestLimit
    {
        get => this.Data.CarryChestLimit;
        set => this.Data.CarryChestLimit = value;
    }

    /// <inheritdoc />
    public bool CarryChestSlow
    {
        get => this.Data.CarryChestSlow;
        set => this.Data.CarryChestSlow = value;
    }

    /// <inheritdoc />
    public bool CategorizeChest
    {
        get => this.Data.CategorizeChest;
        set => this.Data.CategorizeChest = value;
    }

    /// <inheritdoc />
    public ControlScheme ControlScheme
    {
        get => this.Data.ControlScheme;
        set => ((IControlScheme)value).CopyTo(this.Data.ControlScheme);
    }

    /// <inheritdoc />
    public ComponentArea CustomColorPickerArea
    {
        get => this.Data.CustomColorPickerArea;
        set => this.Data.CustomColorPickerArea = value;
    }

    /// <inheritdoc />
    public ChestData DefaultChest
    {
        get => this.Data.DefaultChest;
        set => ((IChestData)value).CopyTo(this.Data.DefaultChest);
    }

    /// <inheritdoc />
    public char SearchTagSymbol
    {
        get => this.Data.SearchTagSymbol;
        set => this.Data.SearchTagSymbol = value;
    }

    /// <inheritdoc />
    public bool SlotLock
    {
        get => this.Data.SlotLock;
        set => this.Data.SlotLock = value;
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