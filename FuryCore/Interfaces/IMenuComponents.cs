namespace FuryCore.Interfaces;

using System.Collections.Generic;
using FuryCore.Models;
using StardewValley.Menus;

/// <summary>
///     Add custom components to <see cref="ItemGrabMenu" /> with automatic controller support when added to standard
///     screen areas.
/// </summary>
public interface IMenuComponents
{
    /// <summary>
    ///     The <see cref="ItemGrabMenu" /> that <see cref="MenuComponent" /> can be added to.
    /// </summary>
    public ItemGrabMenu Menu { get; }

    /// <summary>
    ///     Gets <see cref="ClickableTextureComponent" /> that are added to the <see cref="ItemGrabMenu" />.
    /// </summary>
    public List<MenuComponent> Components { get; }
}