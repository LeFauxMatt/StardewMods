#nullable disable

namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley.Menus;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="ClickableMenuChanged" /> event.
/// </summary>
public interface IClickableMenuChangedEventArgs
{
    /// <summary>
    ///     Gets a value indicating a game object context for the menu if applicable.
    /// </summary>
    public IGameObject Context { get; }

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