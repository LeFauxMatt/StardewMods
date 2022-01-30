namespace StardewMods.FuryCore.Models;

using System;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
public class ItemGrabMenuChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemGrabMenuChangedEventArgs" /> class.
    /// </summary>
    /// <param name="itemGrabMenu">The ItemGrabMenu currently active or null.</param>
    /// <param name="chest">The Chest for the ItemGrabMenu or null.</param>
    /// <param name="screenId">The screen id the menu was opened on.</param>
    /// <param name="isNew">Indicate if the ItemGrabMenu was created.</param>
    public ItemGrabMenuChangedEventArgs(ItemGrabMenu itemGrabMenu, Chest chest, int screenId, bool isNew)
    {
        this.ItemGrabMenu = itemGrabMenu;
        this.Chest = chest;
        this.ScreenId = screenId;
        this.IsNew = isNew;
    }

    /// <summary>
    ///     Gets the ItemGrabMenu if it is the currently active menu.
    /// </summary>
    public ItemGrabMenu ItemGrabMenu { get; }

    /// <summary>
    ///     Gets the Chest for which the ItemGrabMenu was opened.
    /// </summary>
    public Chest Chest { get; }

    /// <summary>
    ///     Gets the screen id that the menu was opened on.
    /// </summary>
    public int ScreenId { get; }

    /// <summary>
    ///     Gets a value indicating whether the ItemGrabMenu is new.
    ///     Returns false when the active menu is changed to an existing ItemGrabMenu.
    /// </summary>
    public bool IsNew { get; }
}