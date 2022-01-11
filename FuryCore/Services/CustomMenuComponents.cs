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

/// <inheritdoc cref="FuryCore.Interfaces.IFuryMenu" />
[FuryCoreService(true)]
internal class CustomMenuComponents : IFuryMenu, IService
{
    private readonly PerScreen<List<MenuComponent>> _behindComponents = new(() => new());
    private readonly PerScreen<List<MenuComponent>> _sideComponents = new(() => new());
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomMenuComponents"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public CustomMenuComponents(IModHelper helper, ServiceCollection services)
    {
        CustomMenuComponents.Instance = this;
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
                    nameof(CustomMenuComponents),
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                    typeof(CustomMenuComponents),
                    nameof(CustomMenuComponents.ItemGrabMenu_RepositionSideButtons_prefix));

                harmonyHelper.ApplyPatches(nameof(CustomMenuComponents));
            });

        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
    }

    /// <inheritdoc/>
    public List<MenuComponent> SideComponents
    {
        get => this._sideComponents.Value;
    }

    /// <inheritdoc/>
    public List<MenuComponent> BehindComponents
    {
        get => this._behindComponents.Value;
    }

    private static CustomMenuComponents Instance { get; set; }

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
        CustomMenuComponents.Instance.RepositionSideButtons(__instance);
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
        foreach (var component in this.SideComponents.Concat(this.BehindComponents))
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
                              .Select(componentType => this.GetVanillaComponent(componentType, out var component) ? component : null)
                              .Where(component => component is not null)
                              .OrderBy(component => component.Component.bounds.Y);

        this.SideComponents.Clear();
        this.SideComponents.AddRange(vanillaComponents);

        this.BehindComponents.Clear();
    }

    [SortedEventPriority(EventPriority.Low)]
    private void OnItemGrabMenuChanged_After(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        this.RepositionSideButtons(this.Menu);
        this.Menu.SetupBorderNeighbors();
    }

    private void OnRenderedItemGrabMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        foreach (var component in this.SideComponents.Where(this.IsCustomComponent))
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
        foreach (var component in this.BehindComponents.Where(this.IsCustomComponent))
        {
            component.Draw(e.SpriteBatch);
        }
    }

    private void RepositionSideButtons(IClickableMenu menu)
    {
        var stepSize = this.SideComponents.Count >= 4 ? 72 : 80;
        ClickableTextureComponent previousComponent = null;
        foreach (var (component, index) in this.SideComponents.AsEnumerable().Reverse().Select((component, index) => (component, index)))
        {
            if (previousComponent is not null)
            {
                previousComponent.downNeighborID = component.Component.myID;
                component.Component.upNeighborID = previousComponent.myID;
            }

            component.Component.bounds.X = menu.xPositionOnScreen + menu.width;
            component.Component.bounds.Y = menu.yPositionOnScreen + (menu.height / 3) - 64 - (stepSize * index);
            previousComponent = component.Component;
        }
    }

    private bool GetVanillaComponent(ComponentType componentType, out MenuComponent menuComponent)
    {
        menuComponent = null;
        if (this.Menu is null)
        {
            return false;
        }

        var component = componentType switch
        {
            ComponentType.OrganizeButton => this.Menu.organizeButton,
            ComponentType.FillStacksButton => this.Menu.fillStacksButton,
            ComponentType.ColorPickerToggleButton => this.Menu.colorPickerToggleButton,
            ComponentType.SpecialButton => this.Menu.specialButton,
            ComponentType.JunimoNoteIcon => this.Menu.junimoNoteIcon,
            _ => null,
        };

        if (component is not null)
        {
            menuComponent = new(component);
            return true;
        }

        return false;
    }

    private bool IsCustomComponent(MenuComponent component)
    {
        return !this.IsVanillaComponent(component);
    }

    private bool IsVanillaComponent(MenuComponent component)
    {
        return Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>()
                   .Any(componentType => this.GetVanillaComponent(componentType, out var vanillaComponent) && ReferenceEquals(component, vanillaComponent));
    }
}