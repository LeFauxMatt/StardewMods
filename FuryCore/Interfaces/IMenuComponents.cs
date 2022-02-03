namespace StardewMods.FuryCore.Interfaces;

using System.Collections.Generic;
using StardewMods.FuryCore.Models;
using StardewValley.Menus;

/// <summary>
///     Add custom components to <see cref="ItemGrabMenu" /> with automatic controller support when added to standard
///     screen areas.
/// </summary>
public interface IMenuComponents
{
    /// <summary>
    ///     Gets <see cref="ClickableTextureComponent" /> that are added to the <see cref="ItemGrabMenu" />.
    /// </summary>
    public List<IMenuComponent> Components { get; }

    /// <summary>
    ///     Gets the <see cref="ItemGrabMenu" /> that <see cref="VanillaMenuComponent" /> can be added to.
    /// </summary>
    public ItemGrabMenu Menu { get; }
}