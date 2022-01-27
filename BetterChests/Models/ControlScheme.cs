namespace BetterChests.Models;

using BetterChests.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <inheritdoc />
internal class ControlScheme : IControlScheme
{
    /// <inheritdoc/>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <inheritdoc/>
    public KeybindList StashItems { get; set; } = new(SButton.Z);

    /// <inheritdoc/>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <inheritdoc/>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <inheritdoc/>
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <inheritdoc/>
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

    /// <inheritdoc/>
    public KeybindList LockSlot { get; set; } = new(SButton.A);
}