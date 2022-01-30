namespace StardewMods.BetterChestsConfigurator;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <summary>
/// Mod config data.
/// </summary>
internal class ModConfig
{
    /// <summary>
    /// Gets or sets controls to configure the currently held chest.
    /// </summary>
    public KeybindList ConfigureChest { get; set; } = new(SButton.End);
}