namespace StardewMods.BetterChests.Interfaces.Config;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <summary>
///     Controls config data.
/// </summary>
internal interface IControlScheme
{
    /// <summary>
    ///     Gets or sets controls to lock an item slot.
    /// </summary>
    public SButton LockSlot { get; set; }

    /// <summary>
    ///     Gets or sets controls to switch to next tab.
    /// </summary>
    public KeybindList NextTab { get; set; }

    /// <summary>
    ///     Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage" />.
    /// </summary>
    public KeybindList OpenCrafting { get; set; }

    /// <summary>
    ///     Gets or sets controls to switch to previous tab.
    /// </summary>
    public KeybindList PreviousTab { get; set; }

    /// <summary>
    ///     Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> down.
    /// </summary>
    public KeybindList ScrollDown { get; set; }

    /// <summary>
    ///     Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu" /> up.
    /// </summary>
    public KeybindList ScrollUp { get; set; }

    /// <summary>
    ///     Gets or sets controls to stash player items into storages.
    /// </summary>
    public KeybindList StashItems { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IControlScheme
    {
        other.OpenCrafting = this.OpenCrafting;
        other.StashItems = this.StashItems;
        other.ScrollUp = this.ScrollUp;
        other.ScrollDown = this.ScrollDown;
        other.PreviousTab = this.PreviousTab;
        other.NextTab = this.NextTab;
    }
}