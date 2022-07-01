namespace StardewMods.TooManyAnimals;

/// <summary>
///     Mod config data.
/// </summary>
internal class ModConfig
{
    /// <summary>
    ///     Gets or sets a value indicating how many animals will be shown in the Animal Purchase menu at once.
    /// </summary>
    public int AnimalShopLimit { get; set; } = 30;

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; } = new();
}