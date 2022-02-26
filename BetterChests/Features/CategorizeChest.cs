namespace StardewMods.BetterChests.Features;

using System;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.BetterChests.UI;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.UI;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class CategorizeChest : Feature
{
    private readonly PerScreen<IClickableComponent> _configureButton = new();
    private readonly PerScreen<IManagedStorage> _currentStorage = new();
    private readonly PerScreen<ItemSelectionMenu> _itemSelectionMenu = new();
    private readonly Lazy<IMenuComponents> _menuComponents;
    private readonly PerScreen<IClickableComponent> _minusButton = new();
    private readonly PerScreen<NumberComponent> _numberComponent = new();
    private readonly PerScreen<IClickableComponent> _plusButton = new();
    private readonly PerScreen<ItemGrabMenu> _returnMenu = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="CategorizeChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CategorizeChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this.Services = services;
        this._menuComponents = services.Lazy<IMenuComponents>();
    }

    private IClickableComponent ConfigureButton
    {
        get => this._configureButton.Value ??= new CustomClickableComponent(
            new(
                new(0, 0, Game1.tileSize, Game1.tileSize),
                this.Helper.Content.Load<Texture2D>($"{BetterChests.ModUniqueId}/Icons", ContentSource.GameContent),
                new(0, 0, 16, 16),
                Game1.pixelZoom)
            {
                name = "Configure",
                hoverText = I18n.Button_Configure_Name(),
            },
            ComponentArea.Right);
    }

    private ItemSelectionMenu CurrentItemSelectionMenu
    {
        get => this._itemSelectionMenu.Value;
        set => this._itemSelectionMenu.Value = value;
    }

    private IManagedStorage CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
    }

    private IMenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    private IClickableComponent MinusButton
    {
        get => this._minusButton.Value ??= new CustomClickableComponent(
            new(new(0, 0, 28, 32), Game1.mouseCursors, new(177, 345, 7, 8), Game1.pixelZoom)
            {
                hoverText = I18n.Button_MinusPriority_Name(),
                name = "Minus",
            });
    }

    private NumberComponent NumberComponent
    {
        get => this._numberComponent.Value ??= new();
    }

    private IClickableComponent PlusButton
    {
        get => this._plusButton.Value ??= new CustomClickableComponent(
            new(new(0, 0, 28, 32), Game1.mouseCursors, new(184, 345, 7, 8), Game1.pixelZoom)
            {
                hoverText = I18n.Button_PlusPriority_Name(),
                name = "Plus",
            });
    }

    private ItemGrabMenu ReturnMenu
    {
        get => this._returnMenu.Value;
        set => this._returnMenu.Value = value;
    }

    private IModServices Services { get; }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
        this.MenuComponents.MenuComponentsLoading += this.OnMenuComponentsLoading;
        this.MenuComponents.MenuComponentPressed += this.OnMenuComponentPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
        this.MenuComponents.MenuComponentsLoading -= this.OnMenuComponentsLoading;
        this.MenuComponents.MenuComponentPressed -= this.OnMenuComponentPressed;
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        switch (e.Menu)
        {
            // Enter ItemSelectionMenu
            case ItemSelectionMenu itemSelectionMenu:
                this.NumberComponent.Value = this.CurrentStorage.StashToChestPriority;
                this.NumberComponent.X = itemSelectionMenu.xPositionOnScreen - Game1.tileSize - 12 * Game1.pixelZoom;
                this.NumberComponent.Y = itemSelectionMenu.yPositionOnScreen;
                this.PlusButton.X = itemSelectionMenu.xPositionOnScreen - Game1.tileSize + 5 * Game1.pixelZoom;
                this.PlusButton.Y = itemSelectionMenu.yPositionOnScreen + 4 * Game1.pixelZoom;
                this.MinusButton.X = itemSelectionMenu.xPositionOnScreen - Game1.tileSize * 2 - 4 * Game1.pixelZoom;
                this.MinusButton.Y = itemSelectionMenu.yPositionOnScreen + 4 * Game1.pixelZoom;
                return;

            // Exit ItemSelectionMenu
            case null when this.ReturnMenu is not null && this.CurrentItemSelectionMenu is not null && this.CurrentStorage is not null:
                // Save ItemSelectionMenu to ModData
                Log.Trace($"Saving FilterItemsList to Chest {this.CurrentStorage.QualifiedItemId}.");
                this.CurrentStorage.FilterItemsList = new(this.CurrentStorage.ItemMatcher);
                this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
                this.CurrentItemSelectionMenu = null;
                Game1.activeClickableMenu = this.ReturnMenu;
                return;

            case not null when this.ReturnMenu is not null:
                break;

            default:
                this.ReturnMenu = null;
                return;
        }
    }

    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (this.CurrentStorage is null || e.Button != SButton.MouseLeft && !e.Button.IsActionButton())
        {
            return;
        }

        if (ReferenceEquals(this.ConfigureButton, e.Component))
        {
            this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
            this.CurrentItemSelectionMenu ??= new(this.Helper.Input, this.Services, this.CurrentStorage.ItemMatcher);
            this.CurrentItemSelectionMenu.RegisterEvents(this.Helper.Events.Input);
            Game1.activeClickableMenu = this.CurrentItemSelectionMenu;
        }
        else if (ReferenceEquals(this.MinusButton, e.Component))
        {
            this.CurrentStorage.StashToChestPriority--;
            this.NumberComponent.Value = this.CurrentStorage.StashToChestPriority;
        }
        else if (ReferenceEquals(this.PlusButton, e.Component))
        {
            this.CurrentStorage.StashToChestPriority++;
            this.NumberComponent.Value = this.CurrentStorage.StashToChestPriority;
        }
        else
        {
            return;
        }

        e.SuppressInput();
    }

    private void OnMenuComponentsLoading(object sender, IMenuComponentsLoadingEventArgs e)
    {
        switch (e.Menu)
        {
            case ItemSelectionMenu:
                e.AddComponent(this.NumberComponent);
                e.AddComponent(this.PlusButton);
                e.AddComponent(this.MinusButton);
                break;
            case ItemGrabMenu itemGrabMenu and not ItemSelectionMenu when e.Context is not null && this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedStorage):
                e.AddComponent(this.ConfigureButton, 0);
                this.ReturnMenu = itemGrabMenu;
                this.CurrentStorage = managedStorage;
                break;
        }
    }
}