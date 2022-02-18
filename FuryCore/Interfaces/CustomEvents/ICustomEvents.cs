namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley.Menus;

/// <summary>
///     Custom Events raised by FuryCore.
/// </summary>
public interface ICustomEvents
{
    /// <summary>
    ///     Similar to SMAPI's own MenuChanged event, except it is invoked for not-yet-active menus in their constructor, and
    ///     has tick monitoring for specific fields that substantially change the menu's functionality.
    /// </summary>
    public event EventHandler<ClickableMenuChangedEventArgs> ClickableMenuChanged;

    /// <summary>
    ///     Triggers when <see cref="IGameObject" /> that are no longer accessible are purged from the cache.
    /// </summary>
    public event EventHandler<GameObjectsRemovedEventArgs> GameObjectsRemoved;

    /// <summary>
    ///     Triggers when a custom <see cref="IClickableComponent" /> is pressed from the <see cref="Toolbar" />.
    /// </summary>
    public event EventHandler<ClickableComponentPressedEventArgs> HudComponentPressed;

    /// <summary>
    ///     Triggers when a vanilla or custom <see cref="IClickableComponent" /> is pressed on an <see cref="IClickableMenu" />
    ///     .
    /// </summary>
    public event EventHandler<ClickableComponentPressedEventArgs> MenuComponentPressed;

    /// <summary>
    ///     Triggers when the active menu is changed and components can be added.
    /// </summary>
    public event EventHandler<MenuComponentsLoadingEventArgs> MenuComponentsLoading;

    /// <summary>
    ///     Triggers when the active menu is changed and items are being displayed on the menu.
    /// </summary>
    public event EventHandler<IMenuItemsChangedEventArgs> MenuItemsChanged;

    /// <summary>
    ///     Similar to SMAPI's own RenderedActiveMenu event, except it ensures that anything drawn to SpriteBatch will be above
    ///     the background fade, but below the actual menu.
    /// </summary>
    public event EventHandler<RenderedActiveMenuEventArgs> RenderedClickableMenu;

    /// <summary>
    ///     Similar to SMAPI's own RenderingActiveMenu event, except it ensures that anything drawn to SpriteBatch will be
    ///     above
    ///     the menu but below the cursor and hover text/items.
    /// </summary>
    public event EventHandler<RenderingActiveMenuEventArgs> RenderingClickableMenu;
}