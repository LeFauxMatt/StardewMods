#nullable disable

namespace Common.Integrations.FuryCore;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
    public event EventHandler<(string ComponentName, bool IsSuppressed)> MenuComponentPressed;

    /// <summary>
    ///     Event triggered when a toolbar icon is pressed.
    /// </summary>
    public event EventHandler<(string ComponentName, bool IsSuppressed)> ToolbarIconPressed;

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
    ///     Adds a custom getter for inventory items.
    /// </summary>
    /// <param name="getInventoryItems">The inventory items getter to add.</param>
    public void AddInventoryItemsGetter(Func<Farmer, IEnumerable<(int Index, object Context)>> getInventoryItems);

    /// <summary>
    ///     Adds a custom getter for location objects.
    /// </summary>
    /// <param name="getLocationObjects">The location objects getter to add.</param>
    public void AddLocationObjectsGetter(Func<GameLocation, IEnumerable<(Vector2 Position, object Context)>> getLocationObjects);

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