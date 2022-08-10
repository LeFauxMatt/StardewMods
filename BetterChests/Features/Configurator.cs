namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;

/// <summary>
///     Configure storages individually.
/// </summary>
internal class Configurator : IFeature
{
    private const string Id = "furyx639.BetterChests/Configurator";

    private static Configurator? Instance;

    private readonly ModConfig _config;

    private readonly PerScreen<ClickableTextureComponent> _configButton = new(
        () => new(
            new(0, 0, Game1.tileSize, Game1.tileSize),
            Game1.content.Load<Texture2D>("furyx639.BetterChests/Icons"),
            new(0, 0, 16, 16),
            Game1.pixelZoom)
        {
            name = "Configure",
            hoverText = I18n.Button_Configure_Name(),
        });

    private readonly PerScreen<ItemGrabMenu?> _currentMenu = new();
    private readonly PerScreen<IStorageObject?> _currentStorage = new();
    private readonly IModHelper _helper;
    private readonly IManifest _modManifest;

    private bool _isActivated;
    private bool _isActive;
    private EventHandler<IStorageObject>? _storageEdited;

    private Configurator(IModHelper helper, ModConfig config, IManifest manifest)
    {
        this._helper = helper;
        this._config = config;
        this._modManifest = manifest;
        HarmonyHelper.AddPatches(
            Configurator.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                    typeof(Configurator),
                    nameof(Configurator.ItemGrabMenu_RepositionSideButtons_postfix),
                    PatchType.Postfix),
            });
    }

    /// <summary>
    ///     Raised after an <see cref="IStorageObject" /> has been edited.
    /// </summary>
    public static event EventHandler<IStorageObject> StorageEdited
    {
        add => Configurator.Instance!._storageEdited += value;
        remove => Configurator.Instance!._storageEdited -= value;
    }

    private ClickableTextureComponent ConfigButton => this._configButton.Value;

    private ItemGrabMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private IStorageObject? CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="Configurator" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="Configurator" /> class.</returns>
    public static Configurator Init(IModHelper helper, ModConfig config, IManifest manifest)
    {
        return Configurator.Instance ??= new(helper, config, manifest);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(Configurator.Id);
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(Configurator.Id);
        this._helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        this._helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        Configurator.Instance!.ConfigButton.bounds.Y = 0;
        var buttons = new List<ClickableComponent>(
            new[]
            {
                __instance.organizeButton,
                __instance.fillStacksButton,
                __instance.colorPickerToggleButton,
                __instance.specialButton,
                Configurator.Instance.ConfigButton,
                __instance.junimoNoteIcon,
            }.Where(component => component is not null));

        var yOffset = buttons.Count switch
        {
            <= 3 => __instance.yPositionOnScreen + __instance.height / 3,
            _ => __instance.ItemsToGrabMenu.yPositionOnScreen + __instance.ItemsToGrabMenu.height,
        };

        var stepSize = Game1.tileSize
                     + buttons.Count switch
                       {
                           >= 4 => 8,
                           _ => 16,
                       };

        for (var index = 0; index < buttons.Count; index++)
        {
            var button = buttons[index];
            if (index > 0 && buttons.Count > 1)
            {
                button.downNeighborID = buttons[index - 1].myID;
            }

            if (index < buttons.Count - 1 && buttons.Count > 1)
            {
                button.upNeighborID = buttons[index + 1].myID;
            }

            button.bounds.X = __instance.xPositionOnScreen + __instance.width;
            button.bounds.Y = yOffset - Game1.tileSize - stepSize * index;
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.ConfigButton.containsPoint(x, y)
         || !StorageHelper.TryGetOne(this.CurrentMenu.context, out var storage))
        {
            return;
        }

        ConfigHelper.SetupSpecificConfig(this._modManifest, storage, true);
        IntegrationHelper.GMCM.API!.OpenModMenu(this._modManifest);
        this._isActive = true;
        this._helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
         || !this._config.ControlScheme.Configure.JustPressed()
         || Game1.player.CurrentItem is not SObject obj
         || !StorageHelper.TryGetOne(obj, out var storage))
        {
            return;
        }

        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.Configure);
        ConfigHelper.SetupSpecificConfig(this._modManifest, storage, true);
        IntegrationHelper.GMCM.API!.OpenModMenu(this._modManifest);
        this._isActive = true;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is ItemGrabMenu { context: { } context, shippingBin: false } itemGrabMenu
         && StorageHelper.TryGetOne(context, out var storage))
        {
            this.CurrentMenu = itemGrabMenu;
            this.CurrentStorage = storage;
            this.CurrentMenu.RepositionSideButtons();
            return;
        }

        this.CurrentMenu = null;
        if (!this._isActive || e.OldMenu?.GetType().Name != "SpecificModConfigMenu")
        {
            return;
        }

        this._isActive = false;
        ConfigHelper.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            return;
        }

        if (this.CurrentStorage is not null)
        {
            this._storageEdited.InvokeAll(this, this.CurrentStorage);
            this.CurrentStorage.ShowMenu();
            this.CurrentStorage = null;
            return;
        }

        Game1.activeClickableMenu = null;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.ConfigButton.tryHover(x, y);
        e.SpriteBatch.Draw(
            this.ConfigButton.texture,
            new(this.ConfigButton.bounds.X + 8 * Game1.pixelZoom, this.ConfigButton.bounds.Y + 8 * Game1.pixelZoom),
            new(64, 0, 16, 16),
            Color.White,
            0f,
            new(8, 8),
            this.ConfigButton.scale,
            SpriteEffects.None,
            0.86f);
        this.ConfigButton.draw(e.SpriteBatch);
        if (this.ConfigButton.containsPoint(x, y))
        {
            this.CurrentMenu.hoverText = this.ConfigButton.hoverText;
        }
    }
}