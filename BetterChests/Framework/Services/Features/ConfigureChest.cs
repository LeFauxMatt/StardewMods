namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature
{
#nullable disable
    private static ConfigureChest instance;
#nullable enable

    private readonly PerScreen<ClickableTextureComponent> configButton;
    private readonly ContainerFactory containerFactory;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public ConfigureChest(
        ILog log,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        IGameContentHelper gameContentHelper,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        ConfigureChest.instance = this;
        this.containerFactory = containerFactory;
        this.modEvents = modEvents;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.configButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                gameContentHelper.Load<Texture2D>(AssetHandler.IconTexturePath),
                new Rectangle(0, 0, 16, 16),
                Game1.pixelZoom)
            {
                name = this.Id,
                hoverText = I18n.Button_Configure_Name(),
                myID = 42069,
            });
    }

    /// <inheritdoc />
    public override bool ShouldBeActive =>
        this.ModConfig.DefaultOptions.ConfigureChest != Option.Disabled
        && this.genericModConfigMenuIntegration.IsLoaded;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
            postfix: new HarmonyMethod(
                typeof(ConfigureChest),
                nameof(ConfigureChest.ItemGrabMenu_RepositionSideButtons_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
            AccessTools.DeclaredMethod(
                typeof(ConfigureChest),
                nameof(ConfigureChest.ItemGrabMenu_RepositionSideButtons_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_RepositionSideButtons_postfix(ItemGrabMenu __instance)
    {
        if (!ConfigureChest.instance.isActive.Value)
        {
            return;
        }

        var configButton = ConfigureChest.instance.configButton.Value;
        if (__instance.allClickableComponents?.Contains(configButton) == false)
        {
            __instance.allClickableComponents.Add(configButton);
        }

        configButton.bounds.Y = 0;
        var buttons =
            new[]
                {
                    __instance.organizeButton,
                    __instance.fillStacksButton,
                    __instance.colorPickerToggleButton,
                    __instance.specialButton,
                    __instance.junimoNoteIcon,
                }
                .Where(component => component is not null)
                .ToList();

        buttons.Add(configButton);
        var stepSize = Game1.tileSize + buttons.Count switch { >= 4 => 8, _ => 16 };
        var yOffset = buttons[0].bounds.Y;
        if (yOffset - (stepSize * (buttons.Count - 1)) < __instance.ItemsToGrabMenu.yPositionOnScreen)
        {
            yOffset += ((stepSize * (buttons.Count - 1)) + __instance.ItemsToGrabMenu.yPositionOnScreen - yOffset) / 2;
        }

        var xPosition = buttons[0].bounds.X;

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

            button.bounds.X = xPosition;
            button.bounds.Y = yOffset - (stepSize * index);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);

        // Show Generic Mod Config Menu
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.isActive.Value)
        {
            return;
        }

        if (!Context.IsPlayerFree
            || !this.ModConfig.Controls.Configure.JustPressed()
            || Game1.player.CurrentItem is null
            || !this.containerFactory.TryGetOne(Game1.player.CurrentItem, out var container)
            || container.Options.ConfigureChest == Option.Disabled)
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.Configure);

        // Show Generic Mod Config Menu
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container?.Options.ConfigureChest != Option.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = true;
        this.itemGrabMenuManager.CurrentMenu.RepositionSideButtons();
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.configButton.Value.tryHover(mouseX, mouseY);
        e.SpriteBatch.Draw(
            this.configButton.Value.texture,
            new Vector2(
                this.configButton.Value.bounds.X + (8 * Game1.pixelZoom),
                this.configButton.Value.bounds.Y + (8 * Game1.pixelZoom)),
            new Rectangle(64, 0, 16, 16),
            Color.White,
            0f,
            new Vector2(8, 8),
            this.configButton.Value.scale,
            SpriteEffects.None,
            0.86f);

        this.configButton.Value.draw(e.SpriteBatch);
        if (this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            this.itemGrabMenuManager.CurrentMenu.hoverText = this.configButton.Value.hoverText;
        }
    }
}