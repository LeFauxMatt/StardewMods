#nullable disable

namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley.Menus;

/// <inheritdoc cref="IClickableMenuChangedEventArgs" />
internal class ClickableMenuChangedEventArgs : EventArgs, IClickableMenuChangedEventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickableMenuChangedEventArgs" /> class.
    /// </summary>
    /// <param name="menu">The currently active menu.</param>
    /// <param name="screenId">The screen id the menu was opened on.</param>
    /// <param name="isNew">Indicate if the menu was constructed.</param>
    /// <param name="context">The game object context if applicable.</param>
    internal ClickableMenuChangedEventArgs(IClickableMenu menu, int screenId, bool isNew, IGameObject context)
    {
        this.Context = context;
        this.IsNew = isNew;
        this.Menu = menu;
        this.ScreenId = screenId;
    }

    /// <inheritdoc />
    public IGameObject Context { get; }

    /// <inheritdoc />
    public bool IsNew { get; }

    /// <inheritdoc />
    public IClickableMenu Menu { get; }

    /// <inheritdoc />
    public int ScreenId { get; }
}