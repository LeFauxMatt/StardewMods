namespace StardewMods.BetterChests.Models;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;

/// <inheritdoc />
internal class ControlScheme : IControlScheme
{
    /// <inheritdoc />
    public KeybindList LockSlot { get; set; } = new(SButton.A);

    /// <inheritdoc />
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

    /// <inheritdoc />
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <inheritdoc />
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <inheritdoc />
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <inheritdoc />
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <inheritdoc />
    public KeybindList StashItems { get; set; } = new(SButton.Z);
}