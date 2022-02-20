namespace StardewMods.BetterChests.Features;

using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
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
        this.CustomEvents.MenuComponentsLoading += this.OnMenuComponentsLoading;
        this.CustomEvents.MenuComponentPressed += this.OnMenuComponentPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
        this.CustomEvents.MenuComponentsLoading -= this.OnMenuComponentsLoading;
        this.CustomEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        switch (e.Menu)
        {
            // Enter ItemSelectionMenu
            case ItemSelectionMenu:
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
        if (this.CurrentStorage is null || !ReferenceEquals(this.ConfigureButton, e.Component) || e.Button != SButton.MouseLeft && !e.Button.IsActionButton())
        {
            return;
        }

        this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
        this.CurrentItemSelectionMenu ??= new(this.Helper.Input, this.Services, this.CurrentStorage.ItemMatcher);
        this.CurrentItemSelectionMenu.RegisterEvents(this.Helper.Events.Input);
        Game1.activeClickableMenu = this.CurrentItemSelectionMenu;
        e.SuppressInput();
    }

    private void OnMenuComponentsLoading(object sender, MenuComponentsLoadingEventArgs e)
    {
        if (e.Menu is ItemGrabMenu { context: { } context } itemGrabMenu and not ItemSelectionMenu && this.ManagedObjects.TryGetManagedStorage(context, out var managedStorage))
        {
            e.AddComponent(this.ConfigureButton, 0);
            this.ReturnMenu = itemGrabMenu;
            this.CurrentStorage = managedStorage;
        }
    }
}