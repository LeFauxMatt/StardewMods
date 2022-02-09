namespace StardewMods.BetterChests.Features;

using System;
using Common.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.UI;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class CategorizeChest : Feature
{
    private readonly PerScreen<IMenuComponent> _configureButton = new();
    private readonly PerScreen<IManagedStorage> _currentStorage = new();
    private readonly PerScreen<ItemSelectionMenu> _itemSelectionMenu = new();
    private readonly Lazy<IMenuComponents> _menuComponents;
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

    private IMenuComponent ConfigureButton
    {
        get => this._configureButton.Value ??= new CustomMenuComponent(
            new(
                new(0, 0, Game1.tileSize, Game1.tileSize),
                this.Helper.Content.Load<Texture2D>("assets/configure.png"),
                Rectangle.Empty,
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

    private ItemGrabMenu ReturnMenu
    {
        get => this._returnMenu.Value;
        set => this._returnMenu.Value = value;
    }

    private IModServices Services { get; }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.CustomEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        switch (e.ItemGrabMenu)
        {
            // Enter ItemSelectionMenu
            case ItemSelectionMenu:
                return;

            // Enter an Eligible ItemGrabMenu
            case not null when e.Context is not null && this.ManagedStorages.FindStorage(e.Context, out var managedStorage):
                this.MenuComponents.Components.Insert(0, this.ConfigureButton);
                this.ReturnMenu = e.ItemGrabMenu;
                this.CurrentStorage = managedStorage;
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

            default:
                this.ReturnMenu = e.ItemGrabMenu;
                return;
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (this.CurrentStorage is null || !ReferenceEquals(this.ConfigureButton, e.Component))
        {
            return;
        }

        this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
        this.CurrentItemSelectionMenu ??= new(this.Helper.Input, this.Services, this.CurrentStorage.ItemMatcher);
        this.CurrentItemSelectionMenu.RegisterEvents(this.Helper.Events.Input);

        Game1.activeClickableMenu = this.CurrentItemSelectionMenu;
        e.SuppressInput();
    }
}