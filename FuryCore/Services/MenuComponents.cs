#nullable disable

namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.Enums;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IMenuComponents" />
[FuryCoreService(true)]
internal class MenuComponents : IMenuComponents, IModService
{
    private readonly PerScreen<List<IClickableComponent>> _components = new(() => new());
    private readonly MenuComponentPressed _menuComponentPressed;
    private readonly MenuComponentsLoading _menuComponentsLoading;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponents" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    /// <param name="harmony">Helper to apply/reverse harmony patches.</param>
    public MenuComponents(IModHelper helper, IModServices services, HarmonyHelper harmony)
    {
        MenuComponents.Instance = this;
        this.Helper = helper;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this._menuComponentsLoading = new(services);
        this._menuComponentPressed = new(helper, services);

        services.Lazy<CustomEvents>(
            events =>
            {
                events.ClickableMenuChanged += this.OnClickableMenuChanged;
                events.RenderedClickableMenu += this.OnRenderedClickableMenu;
                events.RenderingClickableMenu += this.OnRenderingClickableMenu;
            });

        var id = $"{FuryCore.ModUniqueId}.{nameof(MenuComponents)}";
        harmony.AddPatches(
            id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                    typeof(MenuComponents),
                    nameof(MenuComponents.ItemGrabMenu_RepositionSideButtons_prefix),
                    PatchType.Prefix),
            });
        harmony.ApplyPatches(id);
    }

    /// <inheritdoc />
    public event EventHandler<ClickableComponentPressedEventArgs> MenuComponentPressed
    {
        add => this._menuComponentPressed.Add(value);
        remove => this._menuComponentPressed.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<IMenuComponentsLoadingEventArgs> MenuComponentsLoading
    {
        add => this._menuComponentsLoading.Add(value);
        remove => this._menuComponentsLoading.Remove(value);
    }

    /// <summary>
    ///     Gets <see cref="ClickableTextureComponent" /> that are added to the <see cref="ItemGrabMenu" />.
    /// </summary>
    public List<IClickableComponent> Components
    {
        get => this._components.Value;
    }

    private static MenuComponents Instance { get; set; }

    private IModHelper Helper { get; }

    private string HoverText { get; set; }

    /// <summary>
    ///     Reorients components to the current menu.
    /// </summary>
    /// <param name="menu">The menu to orient components to.</param>
    public void ReorientComponents(IClickableMenu menu)
    {
        foreach (var area in Enum.GetValues<ComponentArea>().Where(componentType => componentType is not ComponentArea.Custom))
        {
            this.ReorientComponents(menu, area);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
    {
        MenuComponents.Instance.ReorientComponents(__instance);
        return false;
    }

    [SortedEventPriority(EventPriority.Low - 1000)]
    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        this.ReorientComponents(e.Menu);
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        this.HoverText = string.Empty;
        if (!this.Components.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        foreach (var component in this.Components)
        {
            component.TryHover(x, y, 0.25f);
            switch (Game1.activeClickableMenu)
            {
                case not null when component.Component?.containsPoint(x, y) != true:
                    break;
                default:
                    this.HoverText = component.HoverText;
                    break;
            }
        }
    }

    private void OnRenderedClickableMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        foreach (var component in this.Components.Where(component => component is { ComponentType: ComponentType.Custom, Layer: ComponentLayer.Above }))
        {
            component.Draw(e.SpriteBatch);
        }

        switch (Game1.activeClickableMenu)
        {
            case ItemGrabMenu itemGrabMenu when string.IsNullOrWhiteSpace(itemGrabMenu.hoverText) && !string.IsNullOrWhiteSpace(this.HoverText):
                itemGrabMenu.hoverText = this.HoverText;
                break;
        }
    }

    private void OnRenderingClickableMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        foreach (var component in this.Components.Where(component => component is { ComponentType: ComponentType.Custom, Layer: ComponentLayer.Below }))
        {
            component.Draw(e.SpriteBatch);
        }
    }

    private void ReorientComponents(IClickableMenu menu, ComponentArea area)
    {
        var components = this.Components.Where(component => component.Area == area && component.Component is not null).ToList();
        if (!components.Any())
        {
            return;
        }

        int x, y;
        switch (area)
        {
            case ComponentArea.Top or ComponentArea.Bottom:
                x = menu switch
                {
                    ItemGrabMenu { ItemsToGrabMenu: { } topMenu } => topMenu.inventory[0].bounds.X,
                    _ => 0,
                };

                y = menu switch
                {
                    ItemGrabMenu { ItemsToGrabMenu: { } topMenu } => topMenu.yPositionOnScreen,
                    _ => 0,
                };

                if (area is ComponentArea.Bottom && menu is ItemGrabMenu itemGrabMenu)
                {
                    y += Game1.tileSize * itemGrabMenu.ItemsToGrabMenu.rows + IClickableMenu.borderWidth;
                }

                this.ReorientComponentsAlongX(components, x, y, area is ComponentArea.Bottom);
                break;

            case ComponentArea.Left or ComponentArea.Right:
                x = menu switch
                {
                    ItemGrabMenu { ItemsToGrabMenu: { } } => menu.xPositionOnScreen,
                    _ => 0,
                };

                y = menu switch
                {
                    ItemGrabMenu { ItemsToGrabMenu: { } topMenu } => topMenu.inventory[0].bounds.Y,
                    _ => 0,
                };

                var height = menu switch
                {
                    ItemGrabMenu { ItemsToGrabMenu: { } topMenu } => Game1.tileSize * topMenu.rows,
                    not null => menu.height,
                    _ => 0,
                };

                if (area is ComponentArea.Right && menu is not null)
                {
                    x += menu.width;
                }

                this.ReorientComponentsAlongY(components, x, y, height, area is ComponentArea.Right);
                break;
        }
    }

    private void ReorientComponentsAlongX(List<IClickableComponent> components, int x, int y, bool originTop)
    {
        IClickableComponent previousComponent = null;
        foreach (var component in components)
        {
            component.X = x;
            component.Y = y - (originTop ? 0 : component.Component.bounds.Height);
            x += component.Component.bounds.Width;
            if (previousComponent is not null)
            {
                previousComponent.Component.rightNeighborID = component.Id;
                component.Component.leftNeighborID = previousComponent.Id;
            }

            previousComponent = component;
        }
    }

    private void ReorientComponentsAlongY(List<IClickableComponent> components, int x, int y, int height, bool originLeft)
    {
        var usedHeight = components.Sum(component => component.Component.bounds.Height);
        var gap = (height - usedHeight) / (components.Count + 1);
        IClickableComponent previousComponent = null;
        foreach (var component in components)
        {
            y += gap;
            component.X = x - (originLeft ? 0 : component.Component.bounds.Width);
            component.Y = y;
            y += component.Component.bounds.Height;
            if (previousComponent is not null)
            {
                previousComponent.Component.downNeighborID = component.Id;
                component.Component.upNeighborID = previousComponent.Id;
            }

            previousComponent = component;
        }
    }
}