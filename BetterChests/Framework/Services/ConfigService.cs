namespace StardewMods.BetterChests.Framework.Services;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Features;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>Handles config options.</summary>
internal sealed class ConfigService
{
#nullable disable
    private static ConfigService instance;
#nullable enable

    private readonly ModConfig config;

    private readonly IEnumerable<IFeature> features;
    private readonly IModHelper helper;
    private readonly IManifest manifest;
    private readonly IMonitor monitor;

    /// <summary>Initializes a new instance of the <see cref="ConfigService" /> class.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="monitor">Monitoring and logging.</param>
    /// <param name="config">Mod config data.</param>
    /// <param name="features">Mod features.</param>
    public ConfigService(
        IModHelper helper,
        IManifest manifest,
        IMonitor monitor,
        ModConfig config,
        IEnumerable<IFeature> features)
    {
        ConfigService.instance = this;
        this.helper = helper;
        this.manifest = manifest;
        this.monitor = monitor;
        this.config = config;
        this.features = features;
        this.helper.Events.GameLoop.GameLaunched += ConfigService.OnGameLaunched;
    }

    private static IEnumerable<IFeature> Features => ConfigService.instance.features;

    private static IGenericModConfigMenuApi GMCM => IntegrationService.GMCM.Api!;

    private static IInputHelper Input => ConfigService.instance.helper.Input;

    private static IManifest Manifest => ConfigService.instance.manifest;

    private static ModConfig ModConfig => ConfigService.instance.config;

    private static ITranslationHelper Translation => ConfigService.instance.helper.Translation;

    /// <summary>Sets up the main config menu.</summary>
    public static void SetupMainConfig()
    {
        if (!IntegrationService.GMCM.IsLoaded)
        {
            return;
        }

        if (IntegrationService.GMCM.IsRegistered(ConfigService.Manifest))
        {
            IntegrationService.GMCM.Unregister(ConfigService.Manifest);
        }

        IntegrationService.GMCM.Register(ConfigService.Manifest, ConfigService.ResetConfig, ConfigService.SaveConfig);

        // General
        ConfigService.GMCM.AddSectionTitle(ConfigService.Manifest, I18n.Section_General_Name);
        ConfigService.GMCM.AddParagraph(ConfigService.Manifest, I18n.Section_General_Description);

        ConfigService.GMCM.AddNumberOption(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.CarryChestLimit,
            value => ConfigService.ModConfig.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip);

        ConfigService.GMCM.AddNumberOption(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.CarryChestSlowAmount,
            value => ConfigService.ModConfig.CarryChestSlowAmount = value,
            I18n.Config_CarryChestSlow_Name,
            I18n.Config_CarryChestSlow_Tooltip,
            0,
            4,
            1,
            FormatService.CarryChestSlow);

        ConfigService.GMCM.AddBoolOption(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ChestFinder,
            value => ConfigService.ModConfig.ChestFinder = value,
            I18n.Config_ChestFinder_Name,
            I18n.Config_ChestFinder_Tooltip);

        // Craft From Workbench
        if (ConfigService.ModConfig.ConfigureMenu is InGameMenu.Advanced)
        {
            ConfigService.GMCM.AddTextOption(
                ConfigService.Manifest,
                () => ConfigService.ModConfig.CraftFromWorkbench.ToStringFast(),
                value => ConfigService.ModConfig.CraftFromWorkbench =
                    FeatureOptionRangeExtensions.TryParse(value, out var range) ? range : FeatureOptionRange.Default,
                I18n.Config_CraftFromWorkbench_Name,
                I18n.Config_CraftFromWorkbench_Tooltip,
                FeatureOptionRangeExtensions.GetNames(),
                FormatService.Range);

            ConfigService.GMCM.AddNumberOption(
                ConfigService.Manifest,
                () => ConfigService.ModConfig.CraftFromWorkbenchDistance,
                value => ConfigService.ModConfig.CraftFromWorkbenchDistance = value,
                I18n.Config_CraftFromWorkbenchDistance_Name,
                I18n.Config_CraftFromWorkbenchDistance_Tooltip);
        }
        else
        {
            ConfigService.GMCM.AddNumberOption(
                ConfigService.Manifest,
                () => ConfigService.ModConfig.CraftFromWorkbenchDistance switch
                {
                    _ when ConfigService.ModConfig.CraftFromWorkbench is FeatureOptionRange.Default =>
                        (int)FeatureOptionRange.Default,
                    _ when ConfigService.ModConfig.CraftFromWorkbench is FeatureOptionRange.Disabled =>
                        (int)FeatureOptionRange.Disabled,
                    _ when ConfigService.ModConfig.CraftFromWorkbench is FeatureOptionRange.Inventory =>
                        (int)FeatureOptionRange.Inventory,
                    _ when ConfigService.ModConfig.CraftFromWorkbench is FeatureOptionRange.World =>
                        (int)FeatureOptionRange.World,
                    >= 2 when ConfigService.ModConfig.CraftFromWorkbench is FeatureOptionRange.Location => (
                            (int)FeatureOptionRange.Location
                            + (int)Math.Ceiling(Math.Log2(ConfigService.ModConfig.CraftFromWorkbenchDistance)))
                        - 1,
                    _ when ConfigService.ModConfig.CraftFromWorkbench is FeatureOptionRange.Location =>
                        (int)FeatureOptionRange.World - 1,
                    _ => (int)FeatureOptionRange.Default,
                },
                value =>
                {
                    ConfigService.ModConfig.CraftFromWorkbenchDistance = value switch
                    {
                        (int)FeatureOptionRange.Default => 0,
                        (int)FeatureOptionRange.Disabled => 0,
                        (int)FeatureOptionRange.Inventory => 0,
                        (int)FeatureOptionRange.World => 0,
                        (int)FeatureOptionRange.World - 1 => -1,
                        >= (int)FeatureOptionRange.Location => (int)Math.Pow(
                            2,
                            (1 + value) - (int)FeatureOptionRange.Location),
                        _ => 0,
                    };

                    ConfigService.ModConfig.CraftFromWorkbench = value switch
                    {
                        (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
                        (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
                        (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
                        (int)FeatureOptionRange.World => FeatureOptionRange.World,
                        (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
                        _ => FeatureOptionRange.Location,
                    };
                },
                I18n.Config_CraftFromWorkbenchDistance_Name,
                I18n.Config_CraftFromWorkbenchDistance_Tooltip,
                (int)FeatureOptionRange.Default,
                (int)FeatureOptionRange.World,
                1,
                FormatService.Distance);
        }

        ConfigService.GMCM.AddTextOption(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.CustomColorPickerArea.ToStringFast(),
            value => ConfigService.ModConfig.CustomColorPickerArea =
                ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            new[] { ComponentArea.Left.ToStringFast(), ComponentArea.Right.ToStringFast() },
            FormatService.Area);

        ConfigService.GMCM.AddTextOption(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.SearchTagSymbol.ToString(),
            value => ConfigService.ModConfig.SearchTagSymbol =
                string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip);

        if (IntegrationService.TestConflicts(nameof(SlotLock), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            ConfigService.GMCM.AddParagraph(
                ConfigService.Manifest,
                () => I18n.Warn_Incompatibility_Disabled($"BetterChests.{nameof(SlotLock)}", modList));
        }
        else
        {
            ConfigService.GMCM.AddBoolOption(
                ConfigService.Manifest,
                () => ConfigService.ModConfig.SlotLock,
                value => ConfigService.ModConfig.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip);

            ConfigService.GMCM.AddTextOption(
                ConfigService.Manifest,
                () => ConfigService.ModConfig.SlotLockColor,
                value => ConfigService.ModConfig.SlotLockColor = value,
                I18n.Config_SlotLockColor_Name,
                I18n.Config_SlotLockColor_Tooltip);

            ConfigService.GMCM.AddBoolOption(
                ConfigService.Manifest,
                () => ConfigService.ModConfig.SlotLockHold,
                value => ConfigService.ModConfig.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip);
        }

        ConfigService.GMCM.AddBoolOption(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.Experimental,
            value => ConfigService.ModConfig.Experimental = value,
            I18n.Config_Experimental_Name,
            I18n.Config_Experimental_Tooltip);

        // Controls
        ConfigService.GMCM.AddSectionTitle(ConfigService.Manifest, I18n.Section_Controls_Name);
        ConfigService.GMCM.AddParagraph(ConfigService.Manifest, I18n.Section_Controls_Description);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.FindChest,
            value => ConfigService.ModConfig.ControlScheme.FindChest = value,
            I18n.Config_FindChest_Name,
            I18n.Config_FindChest_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.CloseChestFinder,
            value => ConfigService.ModConfig.ControlScheme.CloseChestFinder = value,
            I18n.Config_CloseChestFinder_Name,
            I18n.Config_CloseChestFinder_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.OpenFoundChest,
            value => ConfigService.ModConfig.ControlScheme.OpenFoundChest = value,
            I18n.Config_OpenFoundChest_Name,
            I18n.Config_OpenFoundChest_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.OpenNextChest,
            value => ConfigService.ModConfig.ControlScheme.OpenNextChest = value,
            I18n.Config_OpenNextChest_Name,
            I18n.Config_OpenNextChest_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.OpenCrafting,
            value => ConfigService.ModConfig.ControlScheme.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.StashItems,
            value => ConfigService.ModConfig.ControlScheme.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.Configure,
            value => ConfigService.ModConfig.ControlScheme.Configure = value,
            I18n.Config_Configure_Name,
            I18n.Config_Configure_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.PreviousTab,
            value => ConfigService.ModConfig.ControlScheme.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.NextTab,
            value => ConfigService.ModConfig.ControlScheme.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.ScrollUp,
            value => ConfigService.ModConfig.ControlScheme.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.ScrollDown,
            value => ConfigService.ModConfig.ControlScheme.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.ScrollPage,
            value => ConfigService.ModConfig.ControlScheme.ScrollPage = value,
            I18n.Config_ScrollPage_Name,
            I18n.Config_ScrollPage_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.LockSlot,
            value => ConfigService.ModConfig.ControlScheme.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip);

        ConfigService.GMCM.AddKeybindList(
            ConfigService.Manifest,
            () => ConfigService.ModConfig.ControlScheme.ToggleInfo,
            value => ConfigService.ModConfig.ControlScheme.ToggleInfo = value,
            I18n.Config_ToggleInfo_Name,
            I18n.Config_ToggleInfo_Tooltip);

        // Default Chest
        ConfigService.GMCM.AddSectionTitle(ConfigService.Manifest, I18n.Storage_Default_Name);
        ConfigService.GMCM.AddParagraph(ConfigService.Manifest, I18n.Storage_Default_Tooltip);

        ConfigService.SetupStorageConfig(ConfigService.Manifest, ConfigService.ModConfig);

        // Chest Types
        ConfigService.GMCM.AddSectionTitle(ConfigService.Manifest, I18n.Section_Chests_Name);
        ConfigService.GMCM.AddParagraph(ConfigService.Manifest, I18n.Section_Chests_Description);

        foreach (var (key, _) in ConfigService.ModConfig.VanillaStorages.OrderBy(
            kvp => FormatService.StorageName(kvp.Key)))
        {
            ConfigService.GMCM.AddPageLink(
                ConfigService.Manifest,
                key,
                () => FormatService.StorageName(key),
                () => FormatService.StorageTooltip(key));
        }

        // Other Chests
        foreach (var (key, value) in ConfigService.ModConfig.VanillaStorages)
        {
            ConfigService.GMCM.AddPage(ConfigService.Manifest, key, () => FormatService.StorageName(key));
            ConfigService.SetupStorageConfig(ConfigService.Manifest, value);
        }
    }

    /// <summary>Sets up a config menu for a specific storage.</summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="storage">The storage to configure for.</param>
    /// <param name="register">Indicates whether to register with GMCM.</param>
    public static void SetupSpecificConfig(IManifest manifest, IStorageData storage, bool register = false)
    {
        if (!IntegrationService.GMCM.IsLoaded)
        {
            return;
        }

        void SaveSpecificConfig()
        {
            var sb = new StringBuilder();
            sb.AppendLine(" Configure Storage".PadLeft(50, '=')[^50..]);
            if (storage is Storage storageObject)
            {
                sb.AppendLine(storageObject.Info);
            }

            sb.AppendLine(storage.ToString());
            ConfigService.instance.monitor.Log(sb.ToString());
        }

        if (register)
        {
            if (IntegrationService.GMCM.IsRegistered(manifest))
            {
                IntegrationService.GMCM.Unregister(manifest);
            }

            IntegrationService.GMCM.Register(manifest, ConfigService.ResetConfig, SaveSpecificConfig);
        }

        ConfigService.SetupStorageConfig(manifest, storage, register);
    }

    private static Action<SpriteBatch, Vector2> DrawButton(StorageNode storage, string label)
    {
        var dims = Game1.dialogueFont.MeasureString(label);
        return (b, pos) =>
        {
            var bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X + Game1.tileSize, Game1.tileSize);
            if (Game1.activeClickableMenu.GetChildMenu() is null)
            {
                var point = Game1.getMousePosition();
                if (Game1.oldMouseState.LeftButton == ButtonState.Released
                    && Mouse.GetState().LeftButton == ButtonState.Pressed
                    && bounds.Contains(point))
                {
                    Game1.activeClickableMenu.SetChildMenu(
                        new ItemSelectionMenu(
                            storage,
                            storage.FilterMatcher,
                            ConfigService.Input,
                            ConfigService.Translation));

                    return;
                }
            }

            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new(432, 439, 9, 9),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                Color.White,
                Game1.pixelZoom,
                false,
                1f);

            Utility.drawTextWithShadow(
                b,
                label,
                Game1.dialogueFont,
                new Vector2((bounds.Left + bounds.Right) - dims.X, (bounds.Top + bounds.Bottom) - dims.Y) / 2f,
                Game1.textColor,
                1f,
                1f,
                -1,
                -1,
                0f);
        };
    }

    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (IntegrationService.GMCM.IsLoaded)
        {
            ConfigService.SetupMainConfig();
        }
    }

    private static void ResetConfig()
    {
        var defaultConfig = new ModConfig();

        // Copy properties
        ConfigService.ModConfig.CarryChestLimit = defaultConfig.CarryChestLimit;
        ConfigService.ModConfig.CarryChestSlowAmount = defaultConfig.CarryChestSlowAmount;
        ConfigService.ModConfig.ChestFinder = defaultConfig.ChestFinder;
        ConfigService.ModConfig.CraftFromWorkbench = defaultConfig.CraftFromWorkbench;
        ConfigService.ModConfig.CraftFromWorkbenchDistance = defaultConfig.CraftFromWorkbenchDistance;
        ConfigService.ModConfig.CustomColorPickerArea = defaultConfig.CustomColorPickerArea;
        ConfigService.ModConfig.Experimental = defaultConfig.Experimental;
        ConfigService.ModConfig.SearchTagSymbol = defaultConfig.SearchTagSymbol;
        ConfigService.ModConfig.SlotLock = defaultConfig.SlotLock;
        ConfigService.ModConfig.SlotLockColor = defaultConfig.SlotLockColor;
        ConfigService.ModConfig.SlotLockHold = defaultConfig.SlotLockHold;

        // Copy controls
        ConfigService.ModConfig.ControlScheme.CloseChestFinder = defaultConfig.ControlScheme.CloseChestFinder;
        ConfigService.ModConfig.ControlScheme.Configure = defaultConfig.ControlScheme.Configure;
        ConfigService.ModConfig.ControlScheme.FindChest = defaultConfig.ControlScheme.FindChest;
        ConfigService.ModConfig.ControlScheme.LockSlot = defaultConfig.ControlScheme.LockSlot;
        ConfigService.ModConfig.ControlScheme.NextTab = defaultConfig.ControlScheme.NextTab;
        ConfigService.ModConfig.ControlScheme.OpenCrafting = defaultConfig.ControlScheme.OpenCrafting;
        ConfigService.ModConfig.ControlScheme.OpenFoundChest = defaultConfig.ControlScheme.OpenFoundChest;
        ConfigService.ModConfig.ControlScheme.OpenNextChest = defaultConfig.ControlScheme.OpenNextChest;
        ConfigService.ModConfig.ControlScheme.PreviousTab = defaultConfig.ControlScheme.PreviousTab;
        ConfigService.ModConfig.ControlScheme.ScrollDown = defaultConfig.ControlScheme.ScrollDown;
        ConfigService.ModConfig.ControlScheme.ScrollPage = defaultConfig.ControlScheme.ScrollPage;
        ConfigService.ModConfig.ControlScheme.ScrollUp = defaultConfig.ControlScheme.ScrollUp;
        ConfigService.ModConfig.ControlScheme.StashItems = defaultConfig.ControlScheme.StashItems;
        ConfigService.ModConfig.ControlScheme.ToggleInfo = defaultConfig.ControlScheme.ToggleInfo;

        // Copy default storage
        ((IStorageData)defaultConfig).CopyTo(ConfigService.ModConfig);

        // Copy vanilla storages
        var defaultStorage = new StorageData();
        foreach (var (_, storage) in ConfigService.ModConfig.VanillaStorages)
        {
            ((IStorageData)defaultStorage).CopyTo(storage);
        }
    }

    private static void SaveConfig()
    {
        ConfigService.instance.helper.WriteConfig(ConfigService.ModConfig);
        foreach (var feature in ConfigService.Features)
        {
            feature.SetActivated();
        }

        ConfigService.instance.monitor.Log(ConfigService.ModConfig.ToString());
    }

    private static void SetupFeatureConfig(string featureName, IManifest manifest, IStorageData storage, bool inGame)
    {
        if (!IntegrationService.GMCM.IsLoaded)
        {
            return;
        }

        switch (inGame)
        {
            // Do not add config options when in-game and feature is disabled
            case true:
                switch (featureName)
                {
                    case nameof(IStorageData.ChestLabel)
                        when ConfigService.ModConfig.LabelChest is FeatureOption.Disabled:
                    case nameof(AutoOrganize) when ConfigService.ModConfig.AutoOrganize is FeatureOption.Disabled:
                    case nameof(CarryChest) when ConfigService.ModConfig.CarryChest is FeatureOption.Disabled:
                    case nameof(ChestInfo) when ConfigService.ModConfig.ChestInfo is FeatureOption.Disabled:
                    case nameof(ChestMenuTabs) when ConfigService.ModConfig.ChestMenuTabs is FeatureOption.Disabled:
                    case nameof(CollectItems) when ConfigService.ModConfig.CollectItems is FeatureOption.Disabled:
                    case nameof(Configurator):
                    case nameof(CraftFromChest)
                        when ConfigService.ModConfig.CraftFromChest is FeatureOptionRange.Disabled:
                    case nameof(BetterColorPicker)
                        when ConfigService.ModConfig.CustomColorPicker is FeatureOption.Disabled:
                    case nameof(FilterItems) when ConfigService.ModConfig.FilterItems is FeatureOption.Disabled:
                    case nameof(LabelChest) when ConfigService.ModConfig.LabelChest is FeatureOption.Disabled:
                    case nameof(OpenHeldChest) when ConfigService.ModConfig.OpenHeldChest is FeatureOption.Disabled:
                    case nameof(OrganizeChest) when ConfigService.ModConfig.OrganizeChest is FeatureOption.Disabled:
                    case nameof(ResizeChest) when ConfigService.ModConfig.ResizeChest is FeatureOption.Disabled:
                    case nameof(SearchItems) when ConfigService.ModConfig.SearchItems is FeatureOption.Disabled:
                    case nameof(StashToChest) when ConfigService.ModConfig.StashToChest is FeatureOptionRange.Disabled:
                    case nameof(TransferItems) when ConfigService.ModConfig.TransferItems is FeatureOption.Disabled:
                    case nameof(UnloadChest) when ConfigService.ModConfig.UnloadChest is FeatureOption.Disabled:
                        return;
                }

                break;

            // Do not add config options when mod conflicts are detected
            case false when IntegrationService.TestConflicts(featureName, out var mods):
            {
                var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
                ConfigService.GMCM.AddParagraph(
                    manifest,
                    () => I18n.Warn_Incompatibility_Disabled($"BetterChests.{featureName}", modList));

                return;
            }
        }

        var data = storage switch
        {
            StorageNode storageNode => storageNode.Data,
            StorageData storageData => storageData,
            _ => storage,
        };

        switch (featureName)
        {
            case nameof(IStorageData.FilterItemsList) when storage is StorageNode storageNode:
                ConfigService.GMCM.AddComplexOption(
                    manifest,
                    I18n.Config_FilterItemsList_Name,
                    ConfigService.DrawButton(storageNode, I18n.Button_Configure_Name()),
                    I18n.Config_FilterItemsList_Tooltip,
                    height: () => Game1.tileSize);

                return;

            case nameof(IStorageData.ChestLabel) when data is Storage:
                IntegrationService.GMCM.Api.AddTextOption(
                    manifest,
                    () => data.ChestLabel,
                    value => data.ChestLabel = value,
                    I18n.Config_ChestLabel_Name,
                    I18n.Config_ChestLabel_Tooltip);

                return;

            case nameof(AutoOrganize) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.AutoOrganize,
                    value => data.AutoOrganize = value,
                    I18n.Config_AutoOrganize_Name,
                    I18n.Config_AutoOrganize_Tooltip);

                return;

            case nameof(CarryChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CarryChest,
                    value => data.CarryChest = value,
                    I18n.Config_CarryChest_Name,
                    I18n.Config_CarryChest_Tooltip);

                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CarryChestSlow,
                    value => data.CarryChestSlow = value,
                    I18n.Config_CarryChestSlow_Name,
                    I18n.Config_CarryChestSlow_Tooltip);

                return;

            case nameof(ChestInfo) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ChestInfo,
                    value => data.ChestInfo = value,
                    I18n.Config_ChestInfo_Name,
                    I18n.Config_ChestInfo_Tooltip);

                return;

            case nameof(ChestMenuTabs) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ChestMenuTabs,
                    value => data.ChestMenuTabs = value,
                    I18n.Config_ChestMenuTabs_Name,
                    I18n.Config_ChestMenuTabs_Tooltip);

                return;

            case nameof(CollectItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CollectItems,
                    value => data.CollectItems = value,
                    I18n.Config_CollectItems_Name,
                    I18n.Config_CollectItems_Tooltip);

                return;

            case nameof(Configurator):
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.Configurator,
                    value => data.Configurator = value,
                    I18n.Config_Configure_Name,
                    I18n.Config_Configure_Tooltip);

                IntegrationService.GMCM.Api.AddTextOption(
                    manifest,
                    () => data.ConfigureMenu.ToStringFast(),
                    value => data.ConfigureMenu = InGameMenuExtensions.TryParse(value, out var menu)
                        ? menu
                        : InGameMenu.Default,
                    I18n.Config_ConfigureMenu_Name,
                    I18n.Config_ConfigureMenu_Tooltip,
                    InGameMenuExtensions.GetNames(),
                    FormatService.Menu);

                return;

            case nameof(CraftFromChest) when storage.ConfigureMenu is InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOptionRange(
                    manifest,
                    () => data.CraftFromChest,
                    value => data.CraftFromChest = value,
                    I18n.Config_CraftFromChest_Name,
                    I18n.Config_CraftFromChest_Tooltip);

                IntegrationService.GMCM.Api.AddNumberOption(
                    manifest,
                    () => data.StashToChestDistance,
                    value => data.StashToChestDistance = value,
                    I18n.Config_CraftFromChestDistance_Name,
                    I18n.Config_CraftFromChestDistance_Tooltip);

                return;

            case nameof(CraftFromChest) when storage.ConfigureMenu is InGameMenu.Full:
                IntegrationService.GMCM.AddDistanceOption(
                    manifest,
                    data,
                    featureName,
                    I18n.Config_CraftFromChestDistance_Name,
                    I18n.Config_CraftFromChestDistance_Tooltip);

                return;

            case nameof(BetterColorPicker) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CustomColorPicker,
                    value => data.CustomColorPicker = value,
                    I18n.Config_CustomColorPicker_Name,
                    I18n.Config_CustomColorPicker_Tooltip);

                return;

            case nameof(FilterItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.FilterItems,
                    value => data.FilterItems = value,
                    I18n.Config_FilterItems_Name,
                    I18n.Config_FilterItems_Tooltip);

                return;

            case nameof(IStorageData.HideItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    ConfigService.Manifest,
                    () => data.HideItems,
                    value => data.HideItems = value,
                    I18n.Config_HideItems_Name,
                    I18n.Config_HideItems_Tooltip);

                return;

            case nameof(LabelChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    ConfigService.Manifest,
                    () => data.LabelChest,
                    value => data.LabelChest = value,
                    I18n.Config_LabelChest_Name,
                    I18n.Config_LabelChest_Tooltip);

                return;

            case nameof(OpenHeldChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.OpenHeldChest,
                    value => data.OpenHeldChest = value,
                    I18n.Config_OpenHeldChest_Name,
                    I18n.Config_OpenHeldChest_Tooltip);

                return;

            case nameof(OrganizeChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.OrganizeChest,
                    value => data.OrganizeChest = value,
                    I18n.Config_OrganizeChest_Name,
                    I18n.Config_OrganizeChest_Tooltip);

                IntegrationService.GMCM.Api.AddTextOption(
                    manifest,
                    () => data.OrganizeChestGroupBy.ToStringFast(),
                    value => data.OrganizeChestGroupBy =
                        GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
                    I18n.Config_OrganizeChestGroupBy_Name,
                    I18n.Config_OrganizeChestGroupBy_Tooltip,
                    GroupByExtensions.GetNames(),
                    FormatService.OrganizeGroupBy);

                IntegrationService.GMCM.Api.AddTextOption(
                    manifest,
                    () => data.OrganizeChestSortBy.ToStringFast(),
                    value => data.OrganizeChestSortBy =
                        SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
                    I18n.Config_OrganizeChestSortBy_Name,
                    I18n.Config_OrganizeChestSortBy_Tooltip,
                    SortByExtensions.GetNames(),
                    FormatService.OrganizeSortBy);

                return;

            case nameof(ResizeChest) when storage.ConfigureMenu is InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ResizeChest,
                    value => data.ResizeChest = value,
                    I18n.Config_ResizeChest_Name,
                    I18n.Config_ResizeChest_Tooltip);

                IntegrationService.GMCM.Api.AddNumberOption(
                    manifest,
                    () => data.ResizeChestCapacity,
                    value => data.ResizeChestCapacity = value,
                    I18n.Config_ResizeChestCapacity_Name,
                    I18n.Config_ResizeChestCapacity_Tooltip);

                return;

            case nameof(SearchItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.SearchItems,
                    value => data.SearchItems = value,
                    I18n.Config_SearchItems_Name,
                    I18n.Config_SearchItems_Tooltip);

                return;

            case nameof(StashToChest):
                if (storage.ConfigureMenu is InGameMenu.Advanced)
                {
                    IntegrationService.GMCM.AddFeatureOptionRange(
                        manifest,
                        () => data.StashToChest,
                        value => data.StashToChest = value,
                        I18n.Config_StashToChest_Name,
                        I18n.Config_StashToChest_Tooltip);

                    ConfigService.GMCM.AddNumberOption(
                        manifest,
                        () => data.StashToChestDistance,
                        value => data.StashToChestDistance = value,
                        I18n.Config_StashToChestDistance_Name,
                        I18n.Config_StashToChestDistance_Tooltip);
                }
                else
                {
                    IntegrationService.GMCM.AddDistanceOption(
                        manifest,
                        data,
                        featureName,
                        I18n.Config_StashToChestDistance_Name,
                        I18n.Config_StashToChestDistance_Tooltip);
                }

                ConfigService.GMCM.AddNumberOption(
                    manifest,
                    () => data.StashToChestPriority,
                    value => data.StashToChestPriority = value,
                    I18n.Config_StashToChestPriority_Name,
                    I18n.Config_StashToChestPriority_Tooltip);

                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.StashToChestStacks,
                    value => data.StashToChestStacks = value,
                    I18n.Config_StashToChestStacks_Name,
                    I18n.Config_StashToChestStacks_Tooltip);

                return;

            case nameof(TransferItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.TransferItems,
                    value => data.TransferItems = value,
                    I18n.Config_TransferItems_Name,
                    I18n.Config_TransferItems_Tooltip);

                return;

            case nameof(UnloadChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.UnloadChest,
                    value => data.UnloadChest = value,
                    I18n.Config_UnloadChest_Name,
                    I18n.Config_UnloadChest_Tooltip);

                IntegrationService.GMCM.AddFeatureOption(
                    manifest,
                    () => data.UnloadChestCombine,
                    value => data.UnloadChestCombine = value,
                    I18n.Config_UnloadChestCombine_Name,
                    I18n.Config_UnloadChestCombine_Tooltip);

                return;
        }
    }

    private static void SetupStorageConfig(IManifest manifest, IStorageData storage, bool inGame = false)
    {
        ConfigService.SetupFeatureConfig(nameof(IStorageData.ChestLabel), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(IStorageData.FilterItemsList), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(AutoOrganize), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(CarryChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(ChestInfo), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(ChestMenuTabs), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(CollectItems), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(Configurator), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(CraftFromChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(BetterColorPicker), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(FilterItems), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(IStorageData.HideItems), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(LabelChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(OpenHeldChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(OrganizeChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(ResizeChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(SearchItems), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(StashToChest), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(TransferItems), manifest, storage, inGame);
        ConfigService.SetupFeatureConfig(nameof(UnloadChest), manifest, storage, inGame);
    }
}
