namespace StardewMods.FuryCore;

using System;
using System.Collections.Generic;
using Common.Integrations.FuryCore;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Helpers;
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
public class FuryCoreApi : IFuryCoreApi
{
    private readonly Lazy<ICustomEvents> _customEvents;
    private readonly Lazy<ICustomTags> _customTags;
    private readonly Lazy<IGameObjects> _gameObjects;
    private readonly PerScreen<ItemMatcher> _itemFilter = new(() => new(true));
    private readonly PerScreen<ItemMatcher> _itemHighlighter = new(() => new(true));
    private EventHandler<(string ComponentName, bool IsSuppressed)> _menuComponentPressed;
    private EventHandler<(string ComponentName, bool IsSuppressed)> _toolbarIconPressed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreApi" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public FuryCoreApi(IModServices services)
    {
        // Services
        this.Services = services;
        this._customTags = services.Lazy<ICustomTags>();
        this._customEvents = services.Lazy<ICustomEvents>();
        this._gameObjects = services.Lazy<IGameObjects>();
        services.Lazy<IMenuItems>();

        // Events
        this.CustomEvents.MenuComponentsLoading += this.OnMenuComponentsLoading;
        this.CustomEvents.MenuItemsChanged += this.OnMenuItemsChanged;
    }

    /// <inheritdoc />
    public event EventHandler<(string ComponentName, bool IsSuppressed)> MenuComponentPressed
    {
        add
        {
            this._menuComponentPressed += value;
            if (this._menuComponentPressed.GetInvocationList().Length == 1)
            {
                this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
            }
        }

        remove
        {
            this._menuComponentPressed -= value;
            if (this._menuComponentPressed.GetInvocationList().Length == 0)
            {
                this.CustomEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
            }
        }
    }

    /// <inheritdoc />
    public event EventHandler<(string ComponentName, bool IsSuppressed)> ToolbarIconPressed
    {
        add
        {
            this._toolbarIconPressed += value;
            if (this._toolbarIconPressed.GetInvocationList().Length == 1)
            {
                this.CustomEvents.HudComponentPressed += this.OnHudComponentPressed;
            }
        }

        remove
        {
            this._toolbarIconPressed -= value;
            if (this._toolbarIconPressed.GetInvocationList().Length == 0)
            {
                this.CustomEvents.HudComponentPressed -= this.OnHudComponentPressed;
            }
        }
    }

    private IList<IClickableComponent> Components { get; } = new List<IClickableComponent>();

    private ICustomEvents CustomEvents
    {
        get => this._customEvents.Value;
    }

    private ICustomTags CustomTags
    {
        get => this._customTags.Value;
    }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IList<IClickableComponent> Icons { get; } = new List<IClickableComponent>();

    private ItemMatcher ItemFilter
    {
        get => this._itemFilter.Value;
    }

    private ItemMatcher ItemHighlighter
    {
        get => this._itemHighlighter.Value;
    }

    private IModServices Services { get; }

    /// <inheritdoc />
    public void AddCustomTag(string tag, Func<Item, bool> predicate)
    {
        this.CustomTags.AddContextTag(tag, predicate);
    }

    /// <inheritdoc />
    public void AddFuryCoreServices(object services)
    {
        if (services is ModServices modServices)
        {
            modServices.Add(new FuryCoreServices(this.Services));
        }
    }

    /// <inheritdoc />
    public void AddInventoryItemsGetter(Func<Farmer, IEnumerable<(int Index, object Context)>> getInventoryItems)
    {
        this.GameObjects.AddInventoryItemsGetter(getInventoryItems);
    }

    /// <inheritdoc />
    public void AddLocationObjectsGetter(Func<GameLocation, IEnumerable<(Vector2 Position, object Context)>> getLocationObjects)
    {
        this.GameObjects.AddLocationObjectsGetter(getLocationObjects);
    }

    /// <inheritdoc />
    public void AddMenuComponent(ClickableTextureComponent clickableTextureComponent, string area = "")
    {
        if (string.IsNullOrWhiteSpace(area) || !Enum.TryParse(area, out ComponentArea componentArea))
        {
            componentArea = ComponentArea.Custom;
        }

        IClickableComponent component = new CustomClickableComponent(clickableTextureComponent, componentArea);
        this.Components.Add(component);
    }

    /// <inheritdoc />
    public void AddToolbarIcon(ClickableTextureComponent clickableTextureComponent, string area = "")
    {
        if (string.IsNullOrWhiteSpace(area) || !Enum.TryParse(area, out ComponentArea componentArea))
        {
            componentArea = ComponentArea.Custom;
        }

        clickableTextureComponent.baseScale = 2f;
        clickableTextureComponent.scale = 2f;
        IClickableComponent component = new CustomClickableComponent(clickableTextureComponent, componentArea);
        this.Icons.Add(component);
    }

    /// <inheritdoc />
    public void SetItemFilter(string stringValue)
    {
        this.ItemFilter.StringValue = stringValue;
    }

    /// <inheritdoc />
    public void SetItemHighlighter(string stringValue)
    {
        this.ItemHighlighter.StringValue = stringValue;
    }

    private void OnHudComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (this.Icons.Contains(e.Component))
        {
            foreach (var handler in this._toolbarIconPressed.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, e.Component.Name);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }

    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (this.Components.Contains(e.Component))
        {
            foreach (var handler in this._menuComponentPressed.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, e.Component.Name);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
    }

    private void OnMenuComponentsLoading(object sender, MenuComponentsLoadingEventArgs e)
    {
        foreach (var component in this.Components)
        {
            e.AddComponent(component);
        }
    }

    private void OnMenuItemsChanged(object sender, MenuItemsChangedEventArgs e)
    {
        e.AddFilter(this.ItemFilter);
        e.AddHighlighter(this.ItemHighlighter);
    }
}