namespace StardewMods.BetterChests;

using Common.Enums;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Models;

internal static class Config
{
    /// <summary>
    ///     Gets or sets a value indicating how many chests containing items can be carried at once.
    /// </summary>
    public static int CarryChestLimit
    {
        get => Config.Data!.CarryChestLimit;
        set => Config.Data!.CarryChestLimit = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether carrying a chest containing items will apply a slowness effect.
    /// </summary>
    public static int CarryChestSlow
    {
        get => Config.Data!.CarryChestSlow;
        set => Config.Data!.CarryChestSlow = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether chests can be categorized.
    /// </summary>
    public static bool CategorizeChest
    {
        get => Config.Data!.CategorizeChest;
        set => Config.Data!.CategorizeChest = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether Configurator will be enabled.
    /// </summary>
    public static bool Configurator
    {
        get => Config.Data!.Configurator;
        set => Config.Data!.Configurator = value;
    }

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public static Controls ControlScheme
    {
        get => Config.Data!.ControlScheme;
        set => Config.Data!.ControlScheme = value;
    }

    /// <summary>
    ///     Gets or sets the <see cref="ComponentArea" /> that the <see cref="CustomColorPicker" /> will be aligned to.
    /// </summary>
    public static ComponentArea CustomColorPickerArea
    {
        get => Config.Data!.CustomColorPickerArea;
        set => Config.Data!.CustomColorPickerArea = value;
    }

    public static ModConfig? Data { get; set; }

    /// <summary>
    ///     Gets or sets the default storage configuration.
    /// </summary>
    public static StorageData DefaultChest
    {
        get => Config.Data!.DefaultChest;
        set => Config.Data!.DefaultChest = value;
    }

    /// <summary>
    ///     Gets or sets the symbol used to denote context tags in searches.
    /// </summary>
    public static char SearchTagSymbol
    {
        get => Config.Data!.SearchTagSymbol;
        set => Config.Data!.SearchTagSymbol = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock feature is enabled.
    /// </summary>
    public static bool SlotLock
    {
        get => Config.Data!.SlotLock;
        set => Config.Data!.SlotLock = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the slot lock button needs to be held down.
    /// </summary>
    public static bool SlotLockHold
    {
        get => Config.Data!.SlotLockHold;
        set => Config.Data!.SlotLockHold = value;
    }
}