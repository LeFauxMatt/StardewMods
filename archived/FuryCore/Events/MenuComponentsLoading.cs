#nullable disable

namespace StardewMods.FuryCore.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class MenuComponentsLoading : SortedEventHandler<IMenuComponentsLoadingEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();
    private readonly Lazy<MenuComponents> _menuComponents;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponentsLoading" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuComponentsLoading(IModServices services)
    {
        this._menuComponents = services.Lazy<MenuComponents>();
        services.Lazy<ICustomEvents>(customEvents => customEvents.ClickableMenuChanged += this.OnClickableMenuChanged);
    }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private MenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        if (!ReferenceEquals(this.Menu, e.Menu))
        {
            this.Menu = e.Menu;
            this.MenuComponents.Components.Clear();
        }

        if (this.Menu is null || this.HandlerCount == 0 || !ReferenceEquals(this.Menu, Game1.activeClickableMenu))
        {
            return;
        }

        var vanillaComponents = (
            from componentType in Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>()
            where componentType is not ComponentType.Custom
            select new VanillaClickableComponent(this.Menu, componentType)
            into component
            where component.Component is not null
            orderby component.Component.bounds.X, component.Component.bounds.Y
            select component).ToList();
        var components = new List<IClickableComponent>();
        components.AddRange(vanillaComponents);
        this.InvokeAll(new MenuComponentsLoadingEventArgs(e.Menu, e.Context as IStorageContainer, components));
        this.MenuComponents.Components.AddRange(components);
        this.Menu.populateClickableComponentList();
        foreach (var component in components.Where(component => !this.Menu.allClickableComponents.Contains(component.Component)))
        {
            this.Menu.allClickableComponents.Add(component.Component);
        }
    }
}