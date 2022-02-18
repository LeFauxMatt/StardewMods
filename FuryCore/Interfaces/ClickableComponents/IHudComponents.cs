namespace StardewMods.FuryCore.Interfaces.ClickableComponents;

using System.Collections.Generic;
using StardewValley.Menus;

/// <summary>
///     Adds icons above/below the items toolbar.
/// </summary>
public interface IHudComponents
{
    /// <summary>
    ///     Gets <see cref="ClickableTextureComponent" /> that are added to the Toolbar.
    /// </summary>
    public List<IClickableComponent> Components { get; }
}