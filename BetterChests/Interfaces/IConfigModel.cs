namespace BetterChests.Interfaces;

/// <inheritdoc />
internal interface IConfigModel : IConfigData
{
    /// <summary>
    /// Gets or sets a string representation of the CraftFromChest field.
    /// </summary>
    public string CraftFromChestString { get; set; }

    /// <summary>
    /// Gets or sets a string representation of the CustomColorPickerArea field.
    /// </summary>
    public string CustomColorPickerAreaString { get; set; }

    /// <summary>
    /// Gets or sets a string representation of the StashToChest field.
    /// </summary>
    public string StashToChestString { get; set; }

    /// <summary>
    /// Gets or sets a string representation of the SearchTagSymbol field.
    /// </summary>
    public string SearchTagSymbolString { get; set; }

    /// <summary>
    /// Resets all config options back to their default value.
    /// </summary>
    public void Reset();

    /// <summary>
    /// Saves current config options to the config.json file.
    /// </summary>
    public void Save();
}