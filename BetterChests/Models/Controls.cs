namespace StardewMods.BetterChests.Models;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <summary>
///     Controls config data.
/// </summary>
internal class Controls
{
    /// <summary>
    ///     Gets or sets controls to lock an item slot.
    /// </summary>
    public SButton LockSlot { get; set; } = SButton.LeftAlt;

    /// <summary>
    ///     Gets or sets controls to switch to next tab.
    /// </summary>
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

    /// <summary>
    ///     Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.
    /// </summary>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <summary>
    ///     Gets or sets controls to switch to previous tab.
    /// </summary>
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <summary>
    ///     Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.
    /// </summary>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <summary>
    ///     Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.
    /// </summary>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <summary>
    ///     Gets or sets controls to stash player items into storages.
    /// </summary>
    public KeybindList StashItems { get; set; } = new(SButton.Z);
}