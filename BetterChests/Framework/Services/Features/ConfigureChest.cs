namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>Configure storages individually.</summary>
internal sealed class ConfigureChest : BaseFeature<ConfigureChest>
{
    private static ConfigureChest instance = null!;

    private readonly PerScreen<ClickableTextureComponent> configButton;
    private readonly ConfigManager configManager;
    private readonly ContainerFactory containerFactory;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly Func<CategorizeOption> getCategorizeOption;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<IStorageContainer?> lastContainer = new();
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigureChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="getCategorizeOption">Gets a new instance of <see cref="CategorizeOption" />.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ConfigureChest(
        AssetHandler assetHandler,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        Func<CategorizeOption> getCategorizeOption,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest)
        : base(eventManager, log, manifest, configManager)
    {
        ConfigureChest.instance = this;
        this.configManager = configManager;
        this.containerFactory = containerFactory;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.getCategorizeOption = getCategorizeOption;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.manifest = manifest;
        this.configButton = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                new Rectangle(0, 0, Game1.tileSize, Game1.tileSize),
                gameContentHelper.Load<Texture2D>(assetHandler.IconTexturePath),
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
        this.Config.DefaultOptions.ConfigureChest != FeatureOption.Disabled
        && this.genericModConfigMenuIntegration.IsLoaded;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

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
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

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

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.ControllerA)
            || this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.configButton.Value.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.ShowMenu(this.itemGrabMenuManager.Top.Container);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!this.isActive.Value)
        {
            return;
        }

        if (!Context.IsPlayerFree
            || !this.Config.Controls.ConfigureChest.JustPressed()
            || !this.containerFactory.TryGetOneFromPlayer(Game1.player, out var container)
            || container.Options.ConfigureChest != FeatureOption.Enabled)
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ConfigureChest);
        this.ShowMenu(container);
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container?.Options.ConfigureChest != FeatureOption.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = true;
        this.itemGrabMenuManager.CurrentMenu.RepositionSideButtons();
    }

    private void OnMenuChanged(MenuChangedEventArgs e)
    {
        if (this.lastContainer.Value is null || e.OldMenu?.GetType().Name != "SpecificModConfigMenu")
        {
            return;
        }

        this.configManager.SetupMainConfig();

        if (e.NewMenu?.GetType().Name != "ModConfigMenu")
        {
            this.lastContainer.Value = null;
            return;
        }

        this.lastContainer.Value.ShowMenu();
        this.lastContainer.Value = null;
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
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

    private void ShowMenu(IStorageContainer container)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var defaultOptions = new DefaultStorageOptions();
        var options = new TemporaryStorageOptions(container.Options, defaultOptions);
        this.genericModConfigMenuIntegration.Register(options.Reset, options.Save);

        if (container.Options.StashToChest is not (RangeOption.Disabled or RangeOption.Default))
        {
            gmcm.AddNumberOption(
                this.manifest,
                () => options.StashToChestPriority,
                value => options.StashToChestPriority = value,
                I18n.Config_StashToChestPriority_Name,
                I18n.Config_StashToChestPriority_Tooltip);
        }

        gmcm.AddPageLink(this.manifest, "Main", I18n.Section_Main_Name, I18n.Section_Main_Description);
        gmcm.AddPageLink(
            this.manifest,
            "Categories",
            I18n.Section_Categorize_Name,
            I18n.Section_Categorize_Description);

        gmcm.AddPage(this.manifest, "Main", I18n.Section_Main_Name);
        this.configManager.AddMainOption(options);

        gmcm.AddPage(this.manifest, "Categories", I18n.Section_Categorize_Name);

        var categorizeOption = this.getCategorizeOption();
        categorizeOption.Init(options.CategorizeChestTags);
        this.genericModConfigMenuIntegration.AddComplexOption(categorizeOption);

        gmcm.OpenModMenu(this.manifest);
        this.lastContainer.Value = container;
    }
}