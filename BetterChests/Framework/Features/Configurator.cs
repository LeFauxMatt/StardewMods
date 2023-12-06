namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class Configurator : BaseFeature
{
    private static readonly MethodBase ItemGrabMenuRepositionSideButtons = AccessTools.DeclaredMethod(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.RepositionSideButtons));

#nullable disable
    private static Configurator instance;
#nullable enable

    private readonly ModConfig config;
    private readonly ConfigMenu configMenu;
    private readonly PerScreen<ClickableTextureComponent> configButton;
    private readonly PerScreen<ItemGrabMenu?> currentMenu = new();
    private readonly PerScreen<StorageNode?> currentStorage = new();
    private readonly IModEvents events;
    private readonly Harmony harmony;
    private readonly IInputHelper input;
    private readonly StorageHandler storages;
    private readonly IManifest manifest;
    private readonly ITranslationHelper translation;

    private bool isActive;

    /// <summary>Initializes a new instance of the <see cref="Configurator" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="configMenu">Dependency for handling the config menu.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="storages">Dependency for the managing storages.</param>
    /// <param name="translation">Dependency used for accessing translations.</param>
    public Configurator(
        IMonitor monitor,
        ModConfig config,
        ConfigMenu configMenu,
        IManifest manifest,
        IModEvents events,
        IGameContentHelper gameContent,
        Harmony harmony,
        IInputHelper input,
        StorageHandler storages,
        ITranslationHelper translation)
        : base(
            monitor,
            nameof(Configurator),
            () => config.Configurator is not FeatureOption.Disabled && Integrations.GMCM.IsLoaded)
    {
        Configurator.instance = this;
        this.config = config;
        this.configMenu = configMenu;
        this.manifest = manifest;
        this.events = events;
        this.harmony = harmony;
        this.input = input;
        this.storages = storages;
        this.translation = translation;
        this.configButton = new(
            () => new(
                new(0, 0, Game1.tileSize, Game1.tileSize),
                gameContent.Load<Texture2D>("furyx639.BetterChests/Icons"),
                new(0, 0, 16, 16),
                Game1.pixelZoom)
            {
                name = "Configure",
                hoverText = I18n.Button_Configure_Name(),
                myID = 42069,
            });
    }

    private static ClickableTextureComponent ConfigButton => Configurator.instance.configButton.Value;

    private ItemGrabMenu? CurrentMenu
    {
        get => this.currentMenu.Value;
        set => this.currentMenu.Value = value;
    }

    private StorageNode? CurrentStorage
    {
        get => this.currentStorage.Value;
        set => this.currentStorage.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Display.MenuChanged += this.OnMenuChanged;
        this.events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.events.Input.ButtonPressed += this.OnButtonPressed;
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;

        // Patches
        this.harmony.Patch(
            Configurator.ItemGrabMenuRepositionSideButtons,
            postfix: new(typeof(Configurator), nameof(Configurator.ItemGrabMenu_RepositionSideButtons_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Display.MenuChanged -= this.OnMenuChanged;
        this.events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.events.Input.ButtonPressed -= this.OnButtonPressed;
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Patches
        this.harmony.Unpatch(
            Configurator.ItemGrabMenuRepositionSideButtons,
            AccessTools.Method(typeof(Configurator), nameof(Configurator.ItemGrabMenu_RepositionSideButtons_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        if (__instance.allClickableComponents?.Contains(Configurator.ConfigButton) == false)
        {
            __instance.allClickableComponents.Add(Configurator.ConfigButton);
        }

        Configurator.ConfigButton.bounds.Y = 0;
        var buttons = new List<ClickableComponent>(
            new[]
            {
                __instance.organizeButton,
                __instance.fillStacksButton,
                __instance.colorPickerToggleButton,
                __instance.specialButton,
                Configurator.ConfigButton,
                __instance.junimoNoteIcon,
            }.Where(component => component is not null));

        var yOffset = buttons.Count switch
        {
            <= 3 => __instance.yPositionOnScreen + (__instance.height / 3),
            _ => __instance.ItemsToGrabMenu.yPositionOnScreen + __instance.ItemsToGrabMenu.height,
        };

        var stepSize = Game1.tileSize
            + buttons.Count switch
            {
                >= 4 => 8,
                _ => 16,
            };

        for (var index = 0; index < buttons.Count; ++index)
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
            button.bounds.Y = yOffset - Game1.tileSize - (stepSize * index);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null || e.Button is not (SButton.MouseLeft or SButton.ControllerA))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!Configurator.ConfigButton.containsPoint(x, y) || BetterItemGrabMenu.Context is null)
        {
            return;
        }

        if (BetterItemGrabMenu.Context is
            {
                ConfigureMenu: InGameMenu.Categorize,
            })
        {
            Game1.activeClickableMenu = new ItemSelectionMenu(
                BetterItemGrabMenu.Context,
                BetterItemGrabMenu.Context.FilterMatcher,
                this.input,
                this.translation);
        }
        else
        {
            this.configMenu.SetupSpecificConfig(this.manifest, BetterItemGrabMenu.Context, true);
            this.configMenu.ShowMenu(this.manifest);
            this.isActive = true;
        }

        this.input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !this.config.ControlScheme.Configure.JustPressed()
            || StorageHandler.CurrentItem is null)
        {
            return;
        }

        this.input.SuppressActiveKeybinds(this.config.ControlScheme.Configure);
        this.configMenu.SetupSpecificConfig(this.manifest, StorageHandler.CurrentItem, true);
        Integrations.GMCM.Api!.OpenModMenu(this.manifest);
        this.isActive = true;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is ItemGrabMenu
                {
                    shippingBin: false,
                } itemGrabMenu
                and not ItemSelectionMenu
            && BetterItemGrabMenu.Context is not null)
        {
            this.CurrentMenu = itemGrabMenu;
            this.CurrentStorage = BetterItemGrabMenu.Context;
            this.CurrentMenu.RepositionSideButtons();
            return;
        }

        this.CurrentMenu = null;
        if (!this.isActive || e.OldMenu?.GetType().Name != "SpecificModConfigMenu")
        {
            return;
        }

        this.isActive = false;
        this.configMenu.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            return;
        }

        if (this.CurrentStorage is
            {
                Data: Storage storageObject,
            })
        {
            this.storages.InvokeStorageEdited(this.CurrentStorage);
            storageObject.ShowMenu();
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
        Configurator.ConfigButton.tryHover(x, y);
        e.SpriteBatch.Draw(
            Configurator.ConfigButton.texture,
            new(
                Configurator.ConfigButton.bounds.X + (8 * Game1.pixelZoom),
                Configurator.ConfigButton.bounds.Y + (8 * Game1.pixelZoom)),
            new(64, 0, 16, 16),
            Color.White,
            0f,
            new(8, 8),
            Configurator.ConfigButton.scale,
            SpriteEffects.None,
            0.86f);

        Configurator.ConfigButton.draw(e.SpriteBatch);
        if (Configurator.ConfigButton.containsPoint(x, y))
        {
            this.CurrentMenu.hoverText = Configurator.ConfigButton.hoverText;
        }
    }
}
