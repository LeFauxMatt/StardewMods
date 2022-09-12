namespace StardewMods.BetterChests.Models;

using System.Text;
using StardewModdingAPI.Utilities;

/// <summary>
///     Controls config data.
/// </summary>
internal sealed class Controls
{
    /// <summary>
    ///     Gets or sets controls to close the chest finder.
    /// </summary>
    public KeybindList CloseChestFinder { get; set; } = new(SButton.Escape);

    /// <summary>
    ///     Gets or sets controls to configure currently held object.
    /// </summary>
    public KeybindList Configure { get; set; } = new(SButton.End);

    /// <summary>
    ///     Gets or sets controls to find a chest.
    /// </summary>
    public KeybindList FindChest { get; set; } = new(
        new Keybind(SButton.LeftControl, SButton.F),
        new Keybind(SButton.RightControl, SButton.F));

    /// <summary>
    ///     Gets or sets controls to lock an item slot.
    /// </summary>
    public KeybindList LockSlot { get; set; } = new(SButton.LeftAlt);

    /// <summary>
    ///     Gets or sets controls to switch to next tab.
    /// </summary>
    public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

    /// <summary>
    ///     Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.
    /// </summary>
    public KeybindList OpenCrafting { get; set; } = new(SButton.K);

    /// <summary>
    ///     Gets or sets controls to open the first found chest.
    /// </summary>
    public KeybindList OpenFoundChest { get; set; } = new(SButton.Enter);

    /// <summary>
    ///     Gets or sets controls to open the next found chest.
    /// </summary>
    public KeybindList OpenNextChest { get; set; } = new(SButton.Tab);

    /// <summary>
    ///     Gets or sets controls to switch to previous tab.
    /// </summary>
    public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

    /// <summary>
    ///     Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.
    /// </summary>
    public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

    /// <summary>
    ///     Gets or sets controls to scroll by page instead of row.
    /// </summary>
    public KeybindList ScrollPage { get; set; } = new(new Keybind(SButton.LeftShift), new Keybind(SButton.RightShift));

    /// <summary>
    ///     Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.
    /// </summary>
    public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

    /// <summary>
    ///     Gets or sets controls to stash player items into storages.
    /// </summary>
    public KeybindList StashItems { get; set; } = new(SButton.Z);

    /// <summary>
    ///     Gets or sets controls to toggle chest info.
    /// </summary>
    public KeybindList ToggleInfo { get; set; } = new(SButton.F1);

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CloseChestFinder: {this.CloseChestFinder}");
        sb.AppendLine($"Configure: {this.Configure}");
        sb.AppendLine($"FindChest: {this.FindChest}");
        sb.AppendLine($"LockSlot: {this.LockSlot}");
        sb.AppendLine($"NextTab: {this.NextTab}");
        sb.AppendLine($"OpenCrafting: {this.OpenCrafting}");
        sb.AppendLine($"OpenFoundChest: {this.OpenFoundChest}");
        sb.AppendLine($"OpenNextChest: {this.OpenNextChest}");
        sb.AppendLine($"PreviousTab: {this.PreviousTab}");
        sb.AppendLine($"ScrollDown: {this.ScrollDown}");
        sb.AppendLine($"ScrollPage: {this.ScrollPage}");
        sb.AppendLine($"ScrollUp: {this.ScrollUp}");
        sb.AppendLine($"StashItems: {this.StashItems}");
        sb.AppendLine($"ToggleInfo: {this.ToggleInfo}");
        return sb.ToString();
    }
}