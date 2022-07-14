namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
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
    private readonly PerScreen<IStorageObject?> _currentStorage = new();

    private Configurator(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
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

    private IStorageObject? CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
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

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        Configurator.Instance!.ConfigureButton.bounds.Y = 0;
        var buttons = new List<ClickableComponent>(
            new[]
            {
                __instance.organizeButton,
                __instance.fillStacksButton,
                __instance.colorPickerToggleButton,
                __instance.specialButton,
                Configurator.Instance.ConfigureButton,
                __instance.junimoNoteIcon,
            }.Where(component => component is not null));

        var yOffset = buttons.Count switch
        {
            <= 3 => __instance.yPositionOnScreen + __instance.height / 3,
            _ => __instance.ItemsToGrabMenu.yPositionOnScreen + __instance.ItemsToGrabMenu.height,
        };

        var stepSize = Game1.tileSize + buttons.Count switch
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
        if (e.NewMenu is ItemGrabMenu { context: { } context, shippingBin: false } itemGrabMenu && StorageHelper.TryGetOne(context, out var storage))
        {
            this.CurrentMenu = itemGrabMenu;
            this.CurrentStorage = storage;
            this.CurrentMenu.RepositionSideButtons();
            return;
        }

        this.CurrentMenu = null;
        if (this.IsActive && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.IsActive = false;
            ConfigHelper.SetupMainConfig();

            if (e.NewMenu?.GetType().Name == "ModConfigMenu")
            {
                if (this.CurrentStorage is not null)
                {
                    this.CurrentStorage.ShowMenu();
                    this.CurrentStorage = null;
                    return;
                }

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
        e.SpriteBatch.Draw(
            this.ConfigureButton.texture,
            new(this.ConfigureButton.bounds.X + 8 * Game1.pixelZoom, this.ConfigureButton.bounds.Y + 8 * Game1.pixelZoom),
            new(64, 0, 16, 16),
            Color.White,
            0f,
            new(8, 8),
            this.ConfigureButton.scale,
            SpriteEffects.None,
            0.86f);
        this.ConfigureButton.draw(e.SpriteBatch);
        if (this.ConfigureButton.containsPoint(x, y))
        {
            this.CurrentMenu.hoverText = this.ConfigureButton.hoverText;
        }
    }
}