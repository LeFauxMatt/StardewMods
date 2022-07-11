namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.CommonHarmony.Helpers;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <summary>
///     Configure storages individually.
/// </summary>
internal class Configurator : IFeature
{
    private const string Id = "furyx639.BetterChests/Configurator";

    private readonly PerScreen<ClickableTextureComponent?> _configureButton = new();
    private readonly PerScreen<ItemGrabMenu?> _currentMenu = new();

    private Configurator(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static Configurator? Instance { get; set; }

    private ModConfig Config { get; }

    private ClickableTextureComponent ConfigureButton
    {
        get => this._configureButton.Value ??= new(
            new(0, 0, Game1.tileSize, Game1.tileSize),
            this.Helper.GameContent.Load<Texture2D>("furyx639.BetterChests/Icons"),
            new(0, 0, 16, 16),
            Game1.pixelZoom)
        {
            name = "Configure",
            hoverText = I18n.Button_Configure_Name(),
        };
    }

    private ItemGrabMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private bool IsActive { get; set; }

    /// <summary>
    ///     Initializes <see cref="Configurator" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="Configurator" /> class.</returns>
    public static Configurator Init(IModHelper helper, ModConfig config)
    {
        return Configurator.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            HarmonyHelper.ApplyPatches(Configurator.Id);
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            HarmonyHelper.UnapplyPatches(Configurator.Id);
            this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
            this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.ConfigureButton.containsPoint(x, y) && StorageHelper.TryGetOne(this.CurrentMenu.context, out var storage))
        {
            ConfigHelper.SetupSpecificConfig(storage);
            this.IsActive = true;
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !this.Config.ControlScheme.Configure.JustPressed()
            || Game1.player.CurrentItem is not SObject obj
            || !StorageHelper.TryGetOne(obj, out var storage))
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.Configure);
        ConfigHelper.SetupSpecificConfig(storage);
        this.IsActive = true;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is ItemGrabMenu { context: { } context, shippingBin: false } itemGrabMenu && StorageHelper.TryGetOne(context, out _))
        {
            this.CurrentMenu = itemGrabMenu;

            var buttons = new List<ClickableComponent>(
                new[]
                {
                    this.CurrentMenu.junimoNoteIcon,
                    this.CurrentMenu.specialButton,
                    this.CurrentMenu.colorPickerToggleButton,
                    this.CurrentMenu.organizeButton,
                    this.CurrentMenu.fillStacksButton,
                }.OfType<ClickableComponent>().OrderByDescending(button => button.bounds.Y));

            if (!buttons.Any())
            {
                return;
            }

            this.ConfigureButton.bounds.X = buttons[0].bounds.X;
            this.ConfigureButton.bounds.Y = buttons[0].bounds.Bottom;
            if (buttons.Count >= 2)
            {
                this.ConfigureButton.bounds.Y += buttons[0].bounds.Top - buttons[1].bounds.Bottom;
            }

            return;
        }

        this.CurrentMenu = null;
        if (this.IsActive && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.IsActive = false;
            ConfigHelper.SetupMainConfig();

            if (e.NewMenu?.GetType().Name == "ModConfigMenu")
            {
                Game1.activeClickableMenu = null;
            }
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.ConfigureButton.tryHover(x, y);
        this.ConfigureButton.draw(e.SpriteBatch);
        if (this.ConfigureButton.containsPoint(x, y))
        {
            this.CurrentMenu.hoverText = this.ConfigureButton.hoverText;
        }
    }
}