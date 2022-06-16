namespace StardewMods.FuryCore;

using System;
using System.Collections.Generic;
using Common.Enums;
using Common.Integrations.FuryCore;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
public class FuryCoreApi : IFuryCoreApi
{
    private readonly PerScreen<ItemMatcher> _itemFilter = new(() => new(true));
    private readonly PerScreen<ItemMatcher> _itemHighlighter = new(() => new(true));

    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreApi" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public FuryCoreApi(IModHelper helper, IModServices services)
    {
        // Services
        this.Helper = helper;
        this.Services = services;
        this.CustomTags = services.FindService<ICustomTags>();
        this.GameObjects = services.FindService<IGameObjects>();
        this.MenuComponents = services.FindService<IMenuComponents>();
        this.MenuItems = services.FindService<IMenuItems>();

        // Events
        this.MenuItems.MenuItemsChanged += this.OnMenuItemsChanged;
    }

    private IList<IClickableComponent> Components { get; } = new List<IClickableComponent>();

    private ICustomTags CustomTags { get; }

    private IGameObjects GameObjects { get; }

    private IModHelper Helper { get; }

    private IList<IClickableComponent> Icons { get; } = new List<IClickableComponent>();

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
    public void SetItemFilter(string stringValue)
    {
        this.ItemFilter.StringValue = stringValue;
    }

    /// <inheritdoc />
    public void SetItemHighlighter(string stringValue)
    {
        this.ItemHighlighter.StringValue = stringValue;
    }

    [SortedEventPriority(EventPriority.Low)]
    private void OnMenuItemsChanged(object sender, IMenuItemsChangedEventArgs e)
    {
        e.AddFilter(this.ItemFilter);
        e.AddHighlighter(this.ItemHighlighter);
    }
}