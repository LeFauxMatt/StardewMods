namespace StardewMods.TooManyAnimals.Models;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.TooManyAnimals.Interfaces;

/// <inheritdoc />
internal class ControlScheme : IControlScheme
{
    /// <inheritdoc />
    public KeybindList NextPage { get; set; } = new(SButton.DPadRight);

    /// <inheritdoc />
    public KeybindList PreviousPage { get; set; } = new(SButton.DPadLeft);
}