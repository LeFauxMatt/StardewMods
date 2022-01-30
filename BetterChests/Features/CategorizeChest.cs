namespace StardewMods.BetterChests.Features;

using System;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class CategorizeChest : Feature
{
    private readonly PerScreen<ManagedChest> _managedChest = new();
    private readonly PerScreen<IMenuComponent> _configureButton = new();
    private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
    private readonly PerScreen<ItemSelectionMenu> _itemSelectionMenu = new();
    private readonly Lazy<IMenuComponents> _customMenuComponents;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategorizeChest"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CategorizeChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._customMenuComponents = services.Lazy<IMenuComponents>();
    }

    private ManagedChest ManagedChest
    {
        get => this._managedChest.Value;
        set => this._managedChest.Value = value;
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

    private IMenuComponents MenuComponents
    {
        get => this._customMenuComponents.Value;
    }

    private ItemGrabMenu ReturnMenu
    {
        get => this._returnMenu.Value;
        set => this._returnMenu.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        switch (e.ItemGrabMenu)
        {
            // Enter ItemSelectionMenu
            case ItemSelectionMenu:
                return;

            // Enter an Eligible ItemGrabMenu
            case not null when this.ManagedChests.FindChest(e.Chest, out var managedChest):
                this.MenuComponents.Components.Insert(0, this.ConfigureButton);
                this.ReturnMenu = e.ItemGrabMenu;
                this.ManagedChest = managedChest;
                return;

            // Exit ItemSelectionMenu
            case null when this.ReturnMenu is not null && this.CurrentItemSelectionMenu is not null && this.ManagedChest is not null:
                // Save ItemSelectionMenu to ModData
                this.ManagedChest.FilterItemsList = new(this.ManagedChest.ItemMatcher);
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
        if (this.ManagedChest is null || !ReferenceEquals(this.ConfigureButton, e.Component))
        {
            return;
        }

        this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
        this.CurrentItemSelectionMenu ??= new(this.Helper.Input, this.ManagedChest.ItemMatcher);
        this.CurrentItemSelectionMenu.RegisterEvents(this.Helper.Events.Input);

        Game1.activeClickableMenu = this.CurrentItemSelectionMenu;
        e.SuppressInput();
    }
}