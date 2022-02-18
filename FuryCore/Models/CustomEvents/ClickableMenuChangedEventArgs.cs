namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using StardewValley.Menus;

/// <inheritdoc cref="IClickableMenuChangedEventArgs" />
public class ClickableMenuChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickableMenuChangedEventArgs" /> class.
    /// </summary>
    /// <param name="menu">The currently active menu.</param>
    /// <param name="screenId">The screen id the menu was opened on.</param>
    /// <param name="isNew">Indicate if the menu was constructed.</param>
    internal ClickableMenuChangedEventArgs(IClickableMenu menu, int screenId, bool isNew)
    {
        this.IsNew = isNew;
        this.Menu = menu;
        this.ScreenId = screenId;
    }

    /// <summary>
    ///     Gets a value indicating whether the menu was just constructed.
    ///     Returns false when the active menu is changed to an already created menu.
    /// </summary>
    public bool IsNew { get; }

    /// <summary>
    ///     Gets the IClickableMenu if it is the currently active menu.
    /// </summary>
    public IClickableMenu Menu { get; }

    /// <summary>
    ///     Gets the screen id that the menu was opened on.
    /// </summary>
    public int ScreenId { get; }
}