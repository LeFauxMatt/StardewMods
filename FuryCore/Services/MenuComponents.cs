namespace FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FuryCore.Attributes;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IMenuComponents" />
[FuryCoreService(true)]
internal class MenuComponents : IMenuComponents, IService
{
    private readonly PerScreen<List<MenuComponent>> _components = new(() => new());
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuComponents"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public MenuComponents(IModHelper helper, ServiceCollection services)
    {
        MenuComponents.Instance = this;
        this.Helper = helper;

        services.Lazy<CustomEvents>(events =>
        {
            events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
            events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged_After;
            events.RenderedItemGrabMenu += this.OnRenderedItemGrabMenu;
            events.RenderingItemGrabMenu += this.OnRenderingItemGrabMenu;
        });

        services.Lazy<HarmonyHelper>(
            harmonyHelper =>
            {
                harmonyHelper.AddPatch(
                    nameof(MenuComponents),
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                    typeof(MenuComponents),
                    nameof(MenuComponents.ItemGrabMenu_RepositionSideButtons_prefix));

                harmonyHelper.ApplyPatches(nameof(MenuComponents));
            });

        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
    }

    /// <inheritdoc/>
    public List<MenuComponent> Components
    {
        get => this._components.Value;
    }

    private static MenuComponents Instance { get; set; }

    private IModHelper Helper { get; }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
    {
        MenuComponents.Instance.RepositionSideButtons(__instance);
        return false;
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var component in this.Components)
        {
            component.TryHover(x, y, 0.25f);

            if (component.Component.containsPoint(x, y))
            {
                this.HoverText = component.HoverText;
            }
        }
    }

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
        if (this.Menu is null)
        {
            return;
        }

        var vanillaComponents = Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>()
                                    .Select(componentType => new MenuComponent(this.Menu, componentType))
                                    .Where(component => component.Component is not null)
                                    .OrderBy(component => component.Component.bounds.Y);

        this.Components.Clear();
        this.Components.AddRange(vanillaComponents);
    }

    [SortedEventPriority(EventPriority.Low)]
    private void OnItemGrabMenuChanged_After(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        foreach (var component in this.Components.Where(component => component.IsCustom))
        {
            this.Menu.allClickableComponents.Add(component.Component);
        }

        this.RepositionSideButtons(this.Menu);
    }

    private void OnRenderedItemGrabMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        foreach (var component in this.Components.Where(component => component.IsCustom && component.Area is not ComponentArea.Bottom))
        {
            component.Draw(e.SpriteBatch);
        }

        if (string.IsNullOrWhiteSpace(this.Menu.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
        {
            this.Menu.hoverText = this._hoverText.Value;
        }
    }

    private void OnRenderingItemGrabMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        foreach (var component in this.Components.Where(component => component.IsCustom && component.Area is ComponentArea.Bottom))
        {
            component.Draw(e.SpriteBatch);
        }
    }

    private void RepositionSideButtons(IClickableMenu menu)
    {
        var sideComponents = this.Components.Where(component => component.Area is ComponentArea.Right).ToList();
        var stepSize = sideComponents.Count >= 4 ? 72 : 80;
        MenuComponent previousComponent = null;
        foreach (var (component, index) in sideComponents.AsEnumerable().Reverse().Select((component, index) => (component, index)))
        {
            if (previousComponent is not null)
            {
                previousComponent.Component.upNeighborID = component.Id;
                component.Component.downNeighborID = previousComponent.Id;
            }

            component.Component.bounds.X = menu.xPositionOnScreen + menu.width;
            component.Component.bounds.Y = menu.yPositionOnScreen + (menu.height / 3) - 64 - (stepSize * index);
            previousComponent = component;
        }
    }
}