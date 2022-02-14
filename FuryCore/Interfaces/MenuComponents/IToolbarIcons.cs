namespace StardewMods.FuryCore.Interfaces.MenuComponents;

using System.Collections.Generic;
using StardewValley.Menus;

/// <summary>
///     Adds icons above/below the items toolbar.
/// </summary>
public interface IToolbarIcons
{
    /// <summary>
    ///     Gets <see cref="ClickableTextureComponent" /> that are added to the Toolbar.
    /// </summary>
    public List<IMenuComponent> Icons { get; }
}