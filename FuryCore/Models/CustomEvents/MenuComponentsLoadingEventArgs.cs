namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.CustomEvents.IMenuComponentsLoadingEventArgs" />
internal class MenuComponentsLoadingEventArgs : EventArgs, IMenuComponentsLoadingEventArgs
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

    /// <inheritdoc />
    public IClickableMenu Menu { get; }

    private List<IClickableComponent> Components { get; }

    /// <inheritdoc />
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