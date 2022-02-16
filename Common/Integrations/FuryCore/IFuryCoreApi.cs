namespace Common.Integrations.FuryCore;

using System;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     API for Fury Core.
/// </summary>
public interface IFuryCoreApi
{
    /// <summary>
    ///     Event triggered when a menu component is pressed.
    /// </summary>
    public event EventHandler<string> MenuComponentPressed;

    /// <summary>
    ///     Event triggered when a toolbar icon is pressed.
    /// </summary>
    public event EventHandler<string> ToolbarIconPressed;

    /// <summary>
    ///     Adds a context tag to any item that currently meets the predicate.
    /// </summary>
    /// <param name="tag">The tag to add to the item.</param>
    /// <param name="predicate">The predicate to test items that should have the context tag added.</param>
    public void AddCustomTag(string tag, Func<Item, bool> predicate);

    /// <summary>
    ///     Add FuryCoreServices to an instance of IModServices.
    /// </summary>
    /// <param name="services">The mod services to add to.</param>
    public void AddFuryCoreServices(object services);

    /// <summary>
    ///     Adds a menu component to the <see cref="ItemGrabMenu" />.
    /// </summary>
    /// <param name="clickableTextureComponent">The <see cref="ClickableTextureComponent" />.</param>
    /// <param name="area">The area of the screen to orient the component to.</param>
    public void AddMenuComponent(ClickableTextureComponent clickableTextureComponent, string area = "");

    /// <summary>
    ///     Adds a menu component next to the <see cref="Toolbar" />.
    /// </summary>
    /// <param name="clickableTextureComponent">The <see cref="ClickableTextureComponent" />.</param>
    /// <param name="area">The area of the screen to orient the component to.</param>
    public void AddToolbarIcon(ClickableTextureComponent clickableTextureComponent, string area = "");

    /// <summary>
    ///     Sets a search phrase to filter the currently displayed items by.
    /// </summary>
    /// <param name="stringValue">A space-separated list of item context tags.</param>
    public void SetItemFilter(string stringValue);

    /// <summary>
    ///     Set a search phrase to apply highlighting to the currently displayed items by.
    /// </summary>
    /// <param name="stringValue">A space-separated list of item context tags.</param>
    public void SetItemHighlighter(string stringValue);
}