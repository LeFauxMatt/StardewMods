namespace StardewMods.FindAnything.Framework.Models;

using StardewModdingAPI.Utilities;

/// <summary>Controls config data.</summary>
internal sealed class Controls
{
    /// <summary>Gets or sets controls to toggle search bar on or off.</summary>
    public KeybindList ToggleSearch { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.F),
        new Keybind(SButton.RightControl, SButton.F));
}