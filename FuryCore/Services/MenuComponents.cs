namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Interfaces.MenuComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Models.MenuComponents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IMenuComponents" />
[FuryCoreService(true)]
internal class MenuComponents : IMenuComponents, IModService
{
    private readonly PerScreen<List<IMenuComponent>> _components = new(() => new());
    private readonly Lazy<IGameObjects> _gameObjects;
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<bool> _refreshComponents = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponents" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuComponents(IModHelper helper, IModServices services)
    {
        MenuComponents.Instance = this;
        this.Helper = helper;
        this._gameObjects = services.Lazy<IGameObjects>();

        services.Lazy<CustomEvents>(
            events =>
            {
                events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
                events.RenderedItemGrabMenu += this.OnRenderedItemGrabMenu;
                events.RenderingItemGrabMenu += this.OnRenderingItemGrabMenu;
            });

        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{FuryCore.ModUniqueId}.{nameof(MenuComponents)}";

                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                    typeof(MenuComponents),
                    nameof(MenuComponents.ItemGrabMenu_RepositionSideButtons_prefix));

                harmonyHelper.ApplyPatches(id);
            });

        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
    }

    /// <inheritdoc />
    public List<IMenuComponent> Components
    {
        get => this._components.Value;
    }

    /// <inheritdoc />
    public ItemGrabMenu Menu
    {
        get => this._menu.Value;
        private set => this._menu.Value = value;
    }

    private static MenuComponents Instance { get; set; }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IModHelper Helper { get; }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private bool RefreshComponents
    {
        get => this._refreshComponents.Value;
        set => this._refreshComponents.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
    {
        MenuComponents.Instance.RepositionSideButtons(__instance);
        return false;
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        if (this.Menu is null || !ReferenceEquals(this.Menu, Game1.activeClickableMenu))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var component in this.Components)
        {
            component.TryHover(x, y, 0.25f);

            if (component.Component?.containsPoint(x, y) == true)
            {
                this.HoverText = component.HoverText;
            }
        }
    }

    [SortedEventPriority(EventPriority.High + 1000)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.Context is not null && this.GameObjects.TryGetGameObject(e.Context, out var gameObject) && gameObject is IStorageContainer
            ? e.ItemGrabMenu
            : null;

        this.Components.Clear();
        if (this.Menu is null || e.Context is null)
        {
            return;
        }

        // Add vanilla components
        this.Components.AddRange(
            from component in
                from componentType in Enum.GetValues(typeof(ComponentType)).Cast<ComponentType>()
                where componentType is not ComponentType.Custom
                select new VanillaMenuComponent(this.Menu, componentType)
            where component.Component is not null
            orderby component.Component.bounds.X, component.Component.bounds.Y
            select component);
        this.RefreshComponents = true;
    }

    private void OnRenderedItemGrabMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        foreach (var component in this.Components.Where(component => component.ComponentType is ComponentType.Custom && component.Area is ComponentArea.Left or ComponentArea.Right))
        {
            component.Draw(e.SpriteBatch);
        }

        if (string.IsNullOrWhiteSpace(this.Menu.hoverText) && !string.IsNullOrWhiteSpace(this.HoverText))
        {
            this.Menu.hoverText = this.HoverText;
        }
    }

    private void OnRenderingItemGrabMenu(object sender, RenderingActiveMenuEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        if (this.RefreshComponents)
        {
            foreach (var component in this.Components.Where(component => component.Component is null).ToList())
            {
                this.Components.Remove(component);
            }

            foreach (var component in this.Components.Where(component => component.ComponentType is ComponentType.Custom))
            {
                this.Menu.allClickableComponents.Add(component.Component);
            }

            this.RepositionSideButtons(this.Menu);
            this.RefreshComponents = false;
        }

        foreach (var component in this.Components.Where(component => component.ComponentType is ComponentType.Custom && component.Area is ComponentArea.Top or ComponentArea.Bottom))
        {
            component.Draw(e.SpriteBatch);
        }
    }

    private void RepositionSideButtons(ItemGrabMenu menu)
    {
        if (!ReferenceEquals(this.Menu, menu))
        {
            return;
        }

        foreach (var componentArea in Enum.GetValues<ComponentArea>().Where(componentType => componentType is not ComponentArea.Custom))
        {
            var components = this.Components.Where(component => component.Area == componentArea && component.Component is not null).ToList();
            if (!components.Any())
            {
                continue;
            }

            if (componentArea is ComponentArea.Left or ComponentArea.Right)
            {
                components.Reverse();
            }

            IMenuComponent previousComponent = null;
            var stepSize = componentArea switch
            {
                ComponentArea.Right or ComponentArea.Left => components.Count >= 4 ? 72 : 80,
                _ => Game1.tileSize,
            };

            var topMenu = menu.ItemsToGrabMenu;
            var bottomMenu = menu.inventory;
            var slot = topMenu.capacity - topMenu.capacity / topMenu.rows;

            foreach (var (component, index) in components.Select((component, index) => (component, index)))
            {
                switch (componentArea)
                {
                    case ComponentArea.Top or ComponentArea.Bottom:
                        component.X = topMenu.inventory[0].bounds.X + stepSize * index;
                        break;
                    case ComponentArea.Left or ComponentArea.Right:
                        component.Y = menu.yPositionOnScreen + menu.height / 3 - Game1.tileSize - stepSize * index;
                        break;
                }

                switch (componentArea)
                {
                    case ComponentArea.Top:
                        component.Y = menu.yPositionOnScreen - Game1.tileSize;
                        if (topMenu.inventory.Count > index)
                        {
                            component.Component.downNeighborID = topMenu.inventory[index].myID;
                        }

                        break;
                    case ComponentArea.Bottom:
                        component.Y = topMenu.yPositionOnScreen + topMenu.height + Game1.pixelZoom;
                        if (topMenu.inventory.Count > slot + index)
                        {
                            component.Component.upNeighborID = topMenu.inventory[slot + index].myID;
                            topMenu.inventory[slot + index].downNeighborID = component.Id;
                        }

                        if (bottomMenu.inventory.Count > index)
                        {
                            component.Component.downNeighborID = bottomMenu.inventory[index].myID;
                            bottomMenu.inventory[index].upNeighborID = component.Id;
                        }

                        break;
                    case ComponentArea.Left:
                        component.X = menu.xPositionOnScreen - Game1.tileSize;
                        break;
                    case ComponentArea.Right:
                        component.X = menu.xPositionOnScreen + menu.width;
                        break;
                }

                if (previousComponent is null)
                {
                    previousComponent = component;
                    continue;
                }

                switch (componentArea)
                {
                    case ComponentArea.Top or ComponentArea.Bottom:
                        previousComponent.Component.rightNeighborID = component.Id;
                        component.Component.leftNeighborID = previousComponent.Id;
                        break;
                    case ComponentArea.Left or ComponentArea.Right:
                        previousComponent.Component.upNeighborID = component.Id;
                        component.Component.downNeighborID = previousComponent.Id;
                        break;
                }

                previousComponent = component;
            }
        }
    }
}