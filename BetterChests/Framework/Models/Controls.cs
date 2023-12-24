namespace StardewMods.BetterChests.Framework.Models;

using StardewModdingAPI.Utilities;

/// <summary>Controls config data.</summary>
internal sealed class Controls
{
    /// <summary>Gets or sets controls to close the chest finder.</summary>
    public KeybindList CloseChestFinder { get; set; } = new(SButton.Escape);

    /// <summary>Gets or sets controls to configure currently held object.</summary>
    public KeybindList Configure { get; set; } = new(SButton.End);

    /// <summary>Gets or sets controls to find a chest.</summary>
    public KeybindList FindChest { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.F),
        new Keybind(SButton.RightControl, SButton.F));

    /// <summary>Gets or sets controls to lock an item slot.</summary>
    public KeybindList LockSlot { get; set; } = new(SButton.LeftAlt);

    /// <summary>Gets or sets controls to switch to next tab.</summary>
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

    /// <summary>Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.</summary>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <summary>Gets or sets controls to open the found chest.</summary>
    public KeybindList OpenFoundChest { get; set; } = new(
        new Keybind(SButton.LeftShift, SButton.Enter),
        new Keybind(SButton.RightShift, SButton.Enter));

    /// <summary>Gets or sets controls to switch to previous tab.</summary>
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.</summary>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <summary>Gets or sets controls to scroll by page instead of row.</summary>
    public KeybindList ScrollPage { get; set; } = new(new Keybind(SButton.LeftShift), new Keybind(SButton.RightShift));

    /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.</summary>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <summary>Gets or sets controls to stash player items into storages.</summary>
    public KeybindList StashItems { get; set; } = new(SButton.Z);

    /// <summary>Gets or sets controls to toggle item collection on or off.</summary>
    public KeybindList ToggleCollectItems { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.Space),
        new Keybind(SButton.RightControl, SButton.Space));

    /// <summary>Gets or sets controls to toggle chest info on or off.</summary>
    public KeybindList ToggleInfo { get; set; } = new(
        new Keybind(SButton.LeftShift, SButton.OemQuestion),
        new Keybind(SButton.RightShift, SButton.OemQuestion));

    /// <summary>Gets or sets controls to toggle search bar on or off.</summary>
    public KeybindList ToggleSearch { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.F),
        new Keybind(SButton.RightControl, SButton.F));
}