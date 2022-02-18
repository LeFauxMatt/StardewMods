namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewValley.Menus;

/// <inheritdoc />
public class MenuComponentsLoadingEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponentsLoadingEventArgs" /> class.
    /// </summary>
    /// <param name="menu">The menu to add components to.</param>
    /// <param name="components">The list of components for this menu.</param>
    public MenuComponentsLoadingEventArgs(IClickableMenu menu, List<IClickableComponent> components)
    {
        this.Menu = menu;
        this.Components = components;
    }

    /// <summary>
    ///     Gets the Menu to add components to.
    /// </summary>
    public IClickableMenu Menu { get; }

    private List<IClickableComponent> Components { get; }

    /// <summary>
    ///     Adds a component to the menu.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <param name="index">Where to insert the component.</param>
    public void AddComponent(IClickableComponent component, int index = -1)
    {
        if (index == -1)
        {
            this.Components.Add(component);
            return;
        }

        this.Components.Insert(index, component);
    }
}