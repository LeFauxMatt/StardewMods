namespace FuryCore.Interfaces;

using System.Collections.Generic;
using FuryCore.Models;
using StardewValley.Menus;

/// <summary>
/// 
/// </summary>
public interface IFuryMenu
{
    /// <summary>
    /// <see cref="ClickableTextureComponent" /> that are arranged to the right of the <see cref="ItemGrabMenu" />.
    /// </summary>
    public List<MenuComponent> SideComponents { get; }

    /// <summary>
    /// <see cref="ClickableTextureComponent" /> that are drawn behind other elements in the <see cref="ItemGrabMenu" />.
    /// </summary>
    public List<MenuComponent> BehindComponents { get; }
}