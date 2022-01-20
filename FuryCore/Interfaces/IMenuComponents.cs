namespace FuryCore.Interfaces;

using System.Collections.Generic;
using FuryCore.Models;
using StardewValley.Menus;

/// <summary>
/// 
/// </summary>
public interface IMenuComponents
{
    /// <summary>
    /// <see cref="ClickableTextureComponent" /> that are added to the <see cref="ItemGrabMenu" />.
    /// </summary>
    public List<MenuComponent> Components { get; }
}