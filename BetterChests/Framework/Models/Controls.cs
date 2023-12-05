namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using System.Text;
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

    /// <summary>Gets or sets controls to open the first found chest.</summary>
    public KeybindList OpenFoundChest { get; set; } = new(SButton.Enter);

    /// <summary>Gets or sets controls to open the next found chest.</summary>
    public KeybindList OpenNextChest { get; set; } = new(SButton.Tab);

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

    /// <summary>Gets or sets controls to toggle chest info.</summary>
    public KeybindList ToggleInfo { get; set; } = new(SButton.F1);

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"CloseChestFinder: {this.CloseChestFinder}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Configure: {this.Configure}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"FindChest: {this.FindChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"LockSlot: {this.LockSlot}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"NextTab: {this.NextTab}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"OpenCrafting: {this.OpenCrafting}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"OpenFoundChest: {this.OpenFoundChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"OpenNextChest: {this.OpenNextChest}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"PreviousTab: {this.PreviousTab}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"ScrollDown: {this.ScrollDown}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"ScrollPage: {this.ScrollPage}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"ScrollUp: {this.ScrollUp}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"StashItems: {this.StashItems}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"ToggleInfo: {this.ToggleInfo}");
        return sb.ToString();
    }
}
