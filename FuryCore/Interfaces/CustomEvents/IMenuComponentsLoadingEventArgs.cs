namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewValley.Menus;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="MenuComponentsLoading" /> event.
/// </summary>
public interface IMenuComponentsLoadingEventArgs
{
    /// <summary>
    ///     Gets the Menu to add components to.
    /// </summary>
    public IClickableMenu Menu { get; }

    /// <summary>
    ///     Adds a component to the menu.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <param name="index">Where to insert the component.</param>
    public void AddComponent(IClickableComponent component, int index = -1);
}