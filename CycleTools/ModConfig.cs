namespace StardewMods.CycleTools;

using StardewModdingAPI.Utilities;

/// <summary>
///     Mod config data for Cycle Tools.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets the key to hold to cycle through tools.
    /// </summary>
    public KeybindList ModifierKey { get; set; } = new(SButton.LeftShift);
}