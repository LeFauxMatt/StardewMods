namespace StardewMods.FuryCore;

using System;
using System.Collections.Generic;
using Common.Integrations.FuryCore;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.MenuComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Models.MenuComponents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
public class FuryCoreApi : IFuryCoreApi
{
    private readonly PerScreen<ItemMatcher> _itemFilter = new(() => new(true));
    private readonly PerScreen<ItemMatcher> _itemHighlighter = new(() => new(true));

    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreApi" /> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public FuryCoreApi(IModServices services)
    {
        // Services
        this.Services = services;
        this.CustomEvents = this.Services.FindService<ICustomEvents>();
        this.CustomTags = this.Services.FindService<ICustomTags>();
        this.MenuComponents = this.Services.FindService<IMenuComponents>();
        this.MenuItems = this.Services.FindService<IMenuItems>();

        // Events
        this.CustomEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.CustomEvents.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <inheritdoc />
    public event EventHandler<string> MenuComponentPressed;

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed;

    private IList<IMenuComponent> Components { get; } = new List<IMenuComponent>();

    private ICustomEvents CustomEvents { get; }

    private ICustomTags CustomTags { get; }

    private IList<IMenuComponent> Icons { get; } = new List<IMenuComponent>();

    private ItemMatcher ItemFilter
    {
        get => this._itemFilter.Value;
    }

    private ItemMatcher ItemHighlighter
    {
        get => this._itemHighlighter.Value;
    }

    private IMenuComponents MenuComponents { get; }

    private IMenuItems MenuItems { get; }

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
    public void AddMenuComponent(ClickableTextureComponent clickableTextureComponent, string area = "")
    {
        if (string.IsNullOrWhiteSpace(area) || !Enum.TryParse(area, out ComponentArea componentArea))
        {
            componentArea = ComponentArea.Custom;
        }

        IMenuComponent component = new CustomMenuComponent(clickableTextureComponent, componentArea);
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
        IMenuComponent component = new CustomMenuComponent(clickableTextureComponent, componentArea);
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

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.ItemGrabMenu is not null && e.Context is Chest)
        {
            this.MenuComponents.Components.AddRange(this.Components);
            this.MenuItems.AddFilter(this.ItemFilter);
            this.MenuItems.AddHighlighter(this.ItemHighlighter);
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (this.Components.Contains(e.Component))
        {
            this.MenuComponentPressed?.Invoke(this, e.Component.Name);
        }
    }

    private void OnToolbarIconPressed(object sender, ToolbarIconPressedEventArgs e)
    {
        if (this.Icons.Contains(e.Component))
        {
            this.ToolbarIconPressed?.Invoke(this, e.Component.Name);
        }
    }
}