namespace StardewMods.TooManyAnimals.Framework.Models;

using StardewModdingAPI.Utilities;

/// <summary>Controls config data.</summary>
internal sealed class Controls
{
    /// <summary>Gets or sets controls to switch to next page.</summary>
    public KeybindList NextPage { get; set; } = new(SButton.DPadRight);

    /// <summary>Gets or sets controls to switch to previous page.</summary>
    public KeybindList PreviousPage { get; set; } = new(SButton.DPadLeft);
}