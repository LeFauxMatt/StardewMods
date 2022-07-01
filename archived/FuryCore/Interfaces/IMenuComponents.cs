#nullable disable

namespace StardewMods.FuryCore.Interfaces;

using System;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley.Menus;

/// <summary>
///     Adds custom components to an IClickableMenu.
/// </summary>
public interface IMenuComponents
{
    /// <summary>
    ///     Triggers when a vanilla or custom <see cref="IClickableComponent" /> is pressed on an <see cref="IClickableMenu" />
    ///     .
    /// </summary>
    public event EventHandler<ClickableComponentPressedEventArgs> MenuComponentPressed;

    /// <summary>
    ///     Triggers when the active menu is changed and components can be added.
    /// </summary>
    public event EventHandler<IMenuComponentsLoadingEventArgs> MenuComponentsLoading;
}