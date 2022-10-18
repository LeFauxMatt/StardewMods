namespace StardewMods.BetterChests.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
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
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>
///     Handles config options.
/// </summary>
internal sealed class Config
{
#nullable disable
    private static Config Instance;
#nullable enable

    private readonly ModConfig _config;

    private readonly IList<Tuple<Feature, Func<bool>>> _features;
    private readonly IModHelper _helper;
    private readonly IManifest _manifest;

    private Config(IModHelper helper, IManifest manifest, ModConfig config, IList<Tuple<Feature, Func<bool>>> features)
    {
        this._helper = helper;
        this._manifest = manifest;
        this._config = config;
        this._features = features;
        this._helper.Events.GameLoop.GameLaunched += Config.OnGameLaunched;
    }

    private static IEnumerable<Tuple<Feature, Func<bool>>> Features => Config.Instance._features;

    private static IGenericModConfigMenuApi GMCM => Integrations.GMCM.API!;

    private static IInputHelper Input => Config.Instance._helper.Input;

    private static IManifest Manifest => Config.Instance._manifest;

    private static ModConfig ModConfig => Config.Instance._config;

    private static ITranslationHelper Translation => Config.Instance._helper.Translation;

    /// <summary>
    ///     Initializes <see cref="Config" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="config">Mod config data.</param>
    /// <param name="features">Mod features.</param>
    /// <returns>Returns an instance of the <see cref="Config" /> class.</returns>
    public static Config Init(
        IModHelper helper,
        IManifest manifest,
        ModConfig config,
        IList<Tuple<Feature, Func<bool>>> features)
    {
        return Config.Instance ??= new(helper, manifest, config, features);
    }

    /// <summary>
    ///     Sets up the main config menu.
    /// </summary>
    public static void SetupMainConfig()
    {
        if (!Integrations.GMCM.IsLoaded)
        {
            return;
        }

        if (Integrations.GMCM.IsRegistered(Config.Manifest))
        {
            Integrations.GMCM.Unregister(Config.Manifest);
        }

        Integrations.GMCM.Register(Config.Manifest, Config.ResetConfig, Config.SaveConfig);

        // General
        Config.GMCM.AddSectionTitle(Config.Manifest, I18n.Section_General_Name);
        Config.GMCM.AddParagraph(Config.Manifest, I18n.Section_General_Description);

        Config.GMCM.AddBoolOption(
            Config.Manifest,
            () => Config.ModConfig.BetterShippingBin,
            value => Config.ModConfig.BetterShippingBin = value,
            I18n.Config_BetterShippingBin_Name,
            I18n.Config_BetterShippingBin_Tooltip);

        Config.GMCM.AddNumberOption(
            Config.Manifest,
            () => Config.ModConfig.CarryChestLimit,
            value => Config.ModConfig.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip);

        Config.GMCM.AddNumberOption(
            Config.Manifest,
            () => Config.ModConfig.CarryChestSlowAmount,
            value => Config.ModConfig.CarryChestSlowAmount = value,
            I18n.Config_CarryChestSlow_Name,
            I18n.Config_CarryChestSlow_Tooltip,
            0,
            4,
            1,
            Formatting.CarryChestSlow);

        Config.GMCM.AddBoolOption(
            Config.Manifest,
            () => Config.ModConfig.ChestFinder,
            value => Config.ModConfig.ChestFinder = value,
            I18n.Config_ChestFinder_Name,
            I18n.Config_ChestFinder_Tooltip);

        // Craft From Workbench
        if (Config.ModConfig.ConfigureMenu is InGameMenu.Advanced)
        {
            Config.GMCM.AddTextOption(
                Config.Manifest,
                () => Config.ModConfig.CraftFromWorkbench.ToStringFast(),
                value => Config.ModConfig.CraftFromWorkbench =
                    FeatureOptionRangeExtensions.TryParse(value, out var range) ? range : FeatureOptionRange.Default,
                I18n.Config_CraftFromWorkbench_Name,
                I18n.Config_CraftFromWorkbench_Tooltip,
                FeatureOptionRangeExtensions.GetNames(),
                Formatting.Range);

            Config.GMCM.AddNumberOption(
                Config.Manifest,
                () => Config.ModConfig.CraftFromWorkbenchDistance,
                value => Config.ModConfig.CraftFromWorkbenchDistance = value,
                I18n.Config_CraftFromWorkbenchDistance_Name,
                I18n.Config_CraftFromWorkbenchDistance_Tooltip);
        }
        else
        {
            Config.GMCM.AddNumberOption(
                Config.Manifest,
                () => Config.ModConfig.CraftFromWorkbenchDistance switch
                {
                    _ when Config.ModConfig.CraftFromWorkbench is FeatureOptionRange.Default => (int)FeatureOptionRange
                        .Default,
                    _ when Config.ModConfig.CraftFromWorkbench is FeatureOptionRange.Disabled => (int)FeatureOptionRange
                        .Disabled,
                    _ when Config.ModConfig.CraftFromWorkbench is FeatureOptionRange.Inventory =>
                        (int)FeatureOptionRange.Inventory,
                    _ when Config.ModConfig.CraftFromWorkbench is FeatureOptionRange.World => (int)FeatureOptionRange
                        .World,
                    >= 2 when Config.ModConfig.CraftFromWorkbench is FeatureOptionRange.Location =>
                        (int)FeatureOptionRange.Location
                      + (int)Math.Ceiling(Math.Log2(Config.ModConfig.CraftFromWorkbenchDistance))
                      - 1,
                    _ when Config.ModConfig.CraftFromWorkbench is FeatureOptionRange.Location => (int)FeatureOptionRange
                            .World
                      - 1,
                    _ => (int)FeatureOptionRange.Default,
                },
                value =>
                {
                    Config.ModConfig.CraftFromWorkbenchDistance = value switch
                    {
                        (int)FeatureOptionRange.Default => 0,
                        (int)FeatureOptionRange.Disabled => 0,
                        (int)FeatureOptionRange.Inventory => 0,
                        (int)FeatureOptionRange.World => 0,
                        (int)FeatureOptionRange.World - 1 => -1,
                        >= (int)FeatureOptionRange.Location => (int)Math.Pow(
                            2,
                            1 + value - (int)FeatureOptionRange.Location),
                        _ => 0,
                    };
                    Config.ModConfig.CraftFromWorkbench = value switch
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
                Formatting.Distance);
        }

        Config.GMCM.AddTextOption(
            Config.Manifest,
            () => Config.ModConfig.CustomColorPickerArea.ToStringFast(),
            value => Config.ModConfig.CustomColorPickerArea =
                ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            new[]
            {
                ComponentArea.Left.ToStringFast(),
                ComponentArea.Right.ToStringFast(),
            },
            Formatting.Area);

        Config.GMCM.AddTextOption(
            Config.Manifest,
            () => Config.ModConfig.SearchTagSymbol.ToString(),
            value => Config.ModConfig.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip);

        if (Integrations.TestConflicts(nameof(SlotLock), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            Config.GMCM.AddParagraph(
                Config.Manifest,
                () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(SlotLock)}", modList));
        }
        else
        {
            Config.GMCM.AddBoolOption(
                Config.Manifest,
                () => Config.ModConfig.SlotLock,
                value => Config.ModConfig.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip);

            Config.GMCM.AddTextOption(
                Config.Manifest,
                () => Config.ModConfig.SlotLockColor.ToStringFast(),
                value => Config.ModConfig.SlotLockColor =
                    ColorsExtensions.TryParse(value, out var color) ? color : Colors.Gray,
                I18n.Config_SlotLockColor_Name,
                I18n.Config_SlotLockColor_Tooltip,
                ColorsExtensions.GetNames());

            Config.GMCM.AddBoolOption(
                Config.Manifest,
                () => Config.ModConfig.SlotLockHold,
                value => Config.ModConfig.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip);
        }

        Config.GMCM.AddBoolOption(
            Config.Manifest,
            () => Config.ModConfig.Experimental,
            value => Config.ModConfig.Experimental = value,
            I18n.Config_Experimental_Name,
            I18n.Config_Experimental_Tooltip);

        // Controls
        Config.GMCM.AddSectionTitle(Config.Manifest, I18n.Section_Controls_Name);
        Config.GMCM.AddParagraph(Config.Manifest, I18n.Section_Controls_Description);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.FindChest,
            value => Config.ModConfig.ControlScheme.FindChest = value,
            I18n.Config_FindChest_Name,
            I18n.Config_FindChest_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.CloseChestFinder,
            value => Config.ModConfig.ControlScheme.CloseChestFinder = value,
            I18n.Config_CloseChestFinder_Name,
            I18n.Config_CloseChestFinder_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.OpenFoundChest,
            value => Config.ModConfig.ControlScheme.OpenFoundChest = value,
            I18n.Config_OpenFoundChest_Name,
            I18n.Config_OpenFoundChest_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.OpenNextChest,
            value => Config.ModConfig.ControlScheme.OpenNextChest = value,
            I18n.Config_OpenNextChest_Name,
            I18n.Config_OpenNextChest_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.OpenCrafting,
            value => Config.ModConfig.ControlScheme.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.StashItems,
            value => Config.ModConfig.ControlScheme.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.Configure,
            value => Config.ModConfig.ControlScheme.Configure = value,
            I18n.Config_Configure_Name,
            I18n.Config_Configure_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.PreviousTab,
            value => Config.ModConfig.ControlScheme.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.NextTab,
            value => Config.ModConfig.ControlScheme.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.ScrollUp,
            value => Config.ModConfig.ControlScheme.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.ScrollDown,
            value => Config.ModConfig.ControlScheme.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.ScrollPage,
            value => Config.ModConfig.ControlScheme.ScrollPage = value,
            I18n.Config_ScrollPage_Name,
            I18n.Config_ScrollPage_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.LockSlot,
            value => Config.ModConfig.ControlScheme.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.Manifest,
            () => Config.ModConfig.ControlScheme.ToggleInfo,
            value => Config.ModConfig.ControlScheme.ToggleInfo = value,
            I18n.Config_ToggleInfo_Name,
            I18n.Config_ToggleInfo_Tooltip);

        // Default Chest
        Config.GMCM.AddSectionTitle(Config.Manifest, I18n.Storage_Default_Name);
        Config.GMCM.AddParagraph(Config.Manifest, I18n.Storage_Default_Tooltip);

        Config.SetupStorageConfig(Config.Manifest, Config.ModConfig);

        // Chest Types
        Config.GMCM.AddSectionTitle(Config.Manifest, I18n.Section_Chests_Name);
        Config.GMCM.AddParagraph(Config.Manifest, I18n.Section_Chests_Description);

        foreach (var (key, _) in Config.ModConfig.VanillaStorages.OrderBy(kvp => Formatting.StorageName(kvp.Key)))
        {
            Config.GMCM.AddPageLink(
                Config.Manifest,
                key,
                () => Formatting.StorageName(key),
                () => Formatting.StorageTooltip(key));
        }

        // Other Chests
        foreach (var (key, value) in Config.ModConfig.VanillaStorages)
        {
            Config.GMCM.AddPage(Config.Manifest, key, () => Formatting.StorageName(key));
            Config.SetupStorageConfig(Config.Manifest, value);
        }
    }

    /// <summary>
    ///     Sets up a config menu for a specific storage.
    /// </summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="storage">The storage to configure for.</param>
    /// <param name="register">Indicates whether to register with GMCM.</param>
    public static void SetupSpecificConfig(IManifest manifest, IStorageData storage, bool register = false)
    {
        if (!Integrations.GMCM.IsLoaded)
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
            Log.Trace(sb.ToString());
        }

        if (register)
        {
            if (Integrations.GMCM.IsRegistered(manifest))
            {
                Integrations.GMCM.Unregister(manifest);
            }

            Integrations.GMCM.Register(manifest, Config.ResetConfig, SaveSpecificConfig);
        }

        Config.SetupStorageConfig(manifest, storage, register);
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
                        new ItemSelectionMenu(storage, storage.FilterMatcher, Config.Input, Config.Translation));
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
                new Vector2(bounds.Left + bounds.Right - dims.X, bounds.Top + bounds.Bottom - dims.Y) / 2f,
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
        if (Integrations.GMCM.IsLoaded)
        {
            Config.SetupMainConfig();
        }
    }

    private static void ResetConfig()
    {
        var defaultConfig = new ModConfig();

        // Copy properties
        Config.ModConfig.BetterShippingBin = defaultConfig.BetterShippingBin;
        Config.ModConfig.CarryChestLimit = defaultConfig.CarryChestLimit;
        Config.ModConfig.CarryChestSlowAmount = defaultConfig.CarryChestSlowAmount;
        Config.ModConfig.ChestFinder = defaultConfig.ChestFinder;
        Config.ModConfig.CraftFromWorkbench = defaultConfig.CraftFromWorkbench;
        Config.ModConfig.CraftFromWorkbenchDistance = defaultConfig.CraftFromWorkbenchDistance;
        Config.ModConfig.CustomColorPickerArea = defaultConfig.CustomColorPickerArea;
        Config.ModConfig.Experimental = defaultConfig.Experimental;
        Config.ModConfig.SearchTagSymbol = defaultConfig.SearchTagSymbol;
        Config.ModConfig.SlotLock = defaultConfig.SlotLock;
        Config.ModConfig.SlotLockColor = defaultConfig.SlotLockColor;
        Config.ModConfig.SlotLockHold = defaultConfig.SlotLockHold;

        // Copy controls
        Config.ModConfig.ControlScheme.CloseChestFinder = defaultConfig.ControlScheme.CloseChestFinder;
        Config.ModConfig.ControlScheme.Configure = defaultConfig.ControlScheme.Configure;
        Config.ModConfig.ControlScheme.FindChest = defaultConfig.ControlScheme.FindChest;
        Config.ModConfig.ControlScheme.LockSlot = defaultConfig.ControlScheme.LockSlot;
        Config.ModConfig.ControlScheme.NextTab = defaultConfig.ControlScheme.NextTab;
        Config.ModConfig.ControlScheme.OpenCrafting = defaultConfig.ControlScheme.OpenCrafting;
        Config.ModConfig.ControlScheme.OpenFoundChest = defaultConfig.ControlScheme.OpenFoundChest;
        Config.ModConfig.ControlScheme.OpenNextChest = defaultConfig.ControlScheme.OpenNextChest;
        Config.ModConfig.ControlScheme.PreviousTab = defaultConfig.ControlScheme.PreviousTab;
        Config.ModConfig.ControlScheme.ScrollDown = defaultConfig.ControlScheme.ScrollDown;
        Config.ModConfig.ControlScheme.ScrollPage = defaultConfig.ControlScheme.ScrollPage;
        Config.ModConfig.ControlScheme.ScrollUp = defaultConfig.ControlScheme.ScrollUp;
        Config.ModConfig.ControlScheme.StashItems = defaultConfig.ControlScheme.StashItems;
        Config.ModConfig.ControlScheme.ToggleInfo = defaultConfig.ControlScheme.ToggleInfo;

        // Copy default storage
        ((IStorageData)defaultConfig).CopyTo(Config.ModConfig);

        // Copy vanilla storages
        var defaultStorage = new StorageData();
        foreach (var (_, storage) in Config.ModConfig.VanillaStorages)
        {
            ((IStorageData)defaultStorage).CopyTo(storage);
        }
    }

    private static void SaveConfig()
    {
        Config.Instance._helper.WriteConfig(Config.ModConfig);
        foreach (var (feature, condition) in Config.Features)
        {
            feature.SetActivated(condition() && !Integrations.TestConflicts(feature.GetType().Name, out _));
        }

        Log.Trace(Config.ModConfig.ToString());
    }

    private static void SetupFeatureConfig(string featureName, IManifest manifest, IStorageData storage, bool inGame)
    {
        if (!Integrations.GMCM.IsLoaded)
        {
            return;
        }

        switch (inGame)
        {
            // Do not add config options when in-game and feature is disabled
            case true:
                switch (featureName)
                {
                    case nameof(IStorageData.ChestLabel) when Config.ModConfig.LabelChest is FeatureOption.Disabled:
                    case nameof(AutoOrganize) when Config.ModConfig.AutoOrganize is FeatureOption.Disabled:
                    case nameof(CarryChest) when Config.ModConfig.CarryChest is FeatureOption.Disabled:
                    case nameof(ChestInfo) when Config.ModConfig.ChestInfo is FeatureOption.Disabled:
                    case nameof(ChestMenuTabs) when Config.ModConfig.ChestMenuTabs is FeatureOption.Disabled:
                    case nameof(CollectItems) when Config.ModConfig.CollectItems is FeatureOption.Disabled:
                    case nameof(Configurator):
                    case nameof(CraftFromChest) when Config.ModConfig.CraftFromChest is FeatureOptionRange.Disabled:
                    case nameof(BetterColorPicker) when Config.ModConfig.CustomColorPicker is FeatureOption.Disabled:
                    case nameof(FilterItems) when Config.ModConfig.FilterItems is FeatureOption.Disabled:
                    case nameof(LabelChest) when Config.ModConfig.LabelChest is FeatureOption.Disabled:
                    case nameof(OpenHeldChest) when Config.ModConfig.OpenHeldChest is FeatureOption.Disabled:
                    case nameof(OrganizeChest) when Config.ModConfig.OrganizeChest is FeatureOption.Disabled:
                    case nameof(ResizeChest) when Config.ModConfig.ResizeChest is FeatureOption.Disabled:
                    case nameof(ResizeChestMenu) when Config.ModConfig.ResizeChestMenu is FeatureOption.Disabled:
                    case nameof(SearchItems) when Config.ModConfig.SearchItems is FeatureOption.Disabled:
                    case nameof(StashToChest) when Config.ModConfig.StashToChest is FeatureOptionRange.Disabled:
                    case nameof(TransferItems) when Config.ModConfig.TransferItems is FeatureOption.Disabled:
                    case nameof(UnloadChest) when Config.ModConfig.UnloadChest is FeatureOption.Disabled:
                        return;
                }

                break;

            // Do not add config options when mod conflicts are detected
            case false when Integrations.TestConflicts(featureName, out var mods):
            {
                var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
                Config.GMCM.AddParagraph(
                    manifest,
                    () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{featureName}", modList));
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
                Config.GMCM.AddComplexOption(
                    manifest,
                    I18n.Config_FilterItemsList_Name,
                    Config.DrawButton(storageNode, I18n.Button_Configure_Name()),
                    I18n.Config_FilterItemsList_Tooltip,
                    height: () => Game1.tileSize);
                return;

            case nameof(IStorageData.ChestLabel) when data is Storage:
                Integrations.GMCM.API.AddTextOption(
                    manifest,
                    () => data.ChestLabel,
                    value => data.ChestLabel = value,
                    I18n.Config_ChestLabel_Name,
                    I18n.Config_ChestLabel_Tooltip);
                return;

            case nameof(AutoOrganize) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.AutoOrganize,
                    value => data.AutoOrganize = value,
                    I18n.Config_AutoOrganize_Name,
                    I18n.Config_AutoOrganize_Tooltip);
                return;

            case nameof(CarryChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CarryChest,
                    value => data.CarryChest = value,
                    I18n.Config_CarryChest_Name,
                    I18n.Config_CarryChest_Tooltip);

                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CarryChestSlow,
                    value => data.CarryChestSlow = value,
                    I18n.Config_CarryChestSlow_Name,
                    I18n.Config_CarryChestSlow_Tooltip);
                return;

            case nameof(ChestInfo) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ChestInfo,
                    value => data.ChestInfo = value,
                    I18n.Config_ChestInfo_Name,
                    I18n.Config_ChestInfo_Tooltip);
                return;

            case nameof(ChestMenuTabs) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ChestMenuTabs,
                    value => data.ChestMenuTabs = value,
                    I18n.Config_ChestMenuTabs_Name,
                    I18n.Config_ChestMenuTabs_Tooltip);
                return;

            case nameof(CollectItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CollectItems,
                    value => data.CollectItems = value,
                    I18n.Config_CollectItems_Name,
                    I18n.Config_CollectItems_Tooltip);
                return;

            case nameof(Configurator):
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.Configurator,
                    value => data.Configurator = value,
                    I18n.Config_Configure_Name,
                    I18n.Config_Configure_Tooltip);

                Integrations.GMCM.API.AddTextOption(
                    manifest,
                    () => data.ConfigureMenu.ToStringFast(),
                    value => data.ConfigureMenu = InGameMenuExtensions.TryParse(value, out var menu)
                        ? menu
                        : InGameMenu.Default,
                    I18n.Config_ConfigureMenu_Name,
                    I18n.Config_ConfigureMenu_Tooltip,
                    InGameMenuExtensions.GetNames(),
                    Formatting.Menu);
                return;

            case nameof(CraftFromChest) when storage.ConfigureMenu is InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOptionRange(
                    manifest,
                    () => data.CraftFromChest,
                    value => data.CraftFromChest = value,
                    I18n.Config_CraftFromChest_Name,
                    I18n.Config_CraftFromChest_Tooltip);

                Integrations.GMCM.API.AddNumberOption(
                    manifest,
                    () => data.StashToChestDistance,
                    value => data.StashToChestDistance = value,
                    I18n.Config_CraftFromChestDistance_Name,
                    I18n.Config_CraftFromChestDistance_Tooltip);
                return;

            case nameof(CraftFromChest) when storage.ConfigureMenu is InGameMenu.Full:
                Integrations.GMCM.AddDistanceOption(
                    manifest,
                    data,
                    featureName,
                    I18n.Config_CraftFromChestDistance_Name,
                    I18n.Config_CraftFromChestDistance_Tooltip);
                return;

            case nameof(BetterColorPicker) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.CustomColorPicker,
                    value => data.CustomColorPicker = value,
                    I18n.Config_CustomColorPicker_Name,
                    I18n.Config_CustomColorPicker_Tooltip);
                return;

            case nameof(FilterItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.FilterItems,
                    value => data.FilterItems = value,
                    I18n.Config_FilterItems_Name,
                    I18n.Config_FilterItems_Tooltip);
                return;

            case nameof(IStorageData.HideItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    Config.Manifest,
                    () => data.HideItems,
                    value => data.HideItems = value,
                    I18n.Config_HideItems_Name,
                    I18n.Config_HideItems_Tooltip);
                return;

            case nameof(LabelChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    Config.Manifest,
                    () => data.LabelChest,
                    value => data.LabelChest = value,
                    I18n.Config_LabelChest_Name,
                    I18n.Config_LabelChest_Tooltip);
                return;

            case nameof(OpenHeldChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.OpenHeldChest,
                    value => data.OpenHeldChest = value,
                    I18n.Config_OpenHeldChest_Name,
                    I18n.Config_OpenHeldChest_Tooltip);
                return;

            case nameof(OrganizeChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.OrganizeChest,
                    value => data.OrganizeChest = value,
                    I18n.Config_OrganizeChest_Name,
                    I18n.Config_OrganizeChest_Tooltip);

                Integrations.GMCM.API.AddTextOption(
                    manifest,
                    () => data.OrganizeChestGroupBy.ToStringFast(),
                    value => data.OrganizeChestGroupBy =
                        GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
                    I18n.Config_OrganizeChestGroupBy_Name,
                    I18n.Config_OrganizeChestGroupBy_Tooltip,
                    GroupByExtensions.GetNames(),
                    Formatting.OrganizeGroupBy);

                Integrations.GMCM.API.AddTextOption(
                    manifest,
                    () => data.OrganizeChestSortBy.ToStringFast(),
                    value => data.OrganizeChestSortBy =
                        SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
                    I18n.Config_OrganizeChestSortBy_Name,
                    I18n.Config_OrganizeChestSortBy_Tooltip,
                    SortByExtensions.GetNames(),
                    Formatting.OrganizeSortBy);
                return;

            case nameof(ResizeChest) when storage.ConfigureMenu is InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ResizeChest,
                    value => data.ResizeChest = value,
                    I18n.Config_ResizeChest_Name,
                    I18n.Config_ResizeChest_Tooltip);

                Integrations.GMCM.API.AddNumberOption(
                    manifest,
                    () => data.ResizeChestCapacity,
                    value => data.ResizeChestCapacity = value,
                    I18n.Config_ResizeChestCapacity_Name,
                    I18n.Config_ResizeChestCapacity_Tooltip);
                return;

            case nameof(ResizeChest) when storage.ConfigureMenu is InGameMenu.Full:
                Integrations.GMCM.AddChestCapacityOption(manifest, data);
                return;

            case nameof(ResizeChestMenu) when storage.ConfigureMenu is InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.ResizeChestMenu,
                    value => data.ResizeChestMenu = value,
                    I18n.Config_ResizeChestMenu_Name,
                    I18n.Config_ResizeChestMenu_Tooltip);

                Integrations.GMCM.API.AddNumberOption(
                    manifest,
                    () => data.ResizeChestMenuRows,
                    value => data.ResizeChestMenuRows = value,
                    I18n.Config_ResizeChestMenuRows_Name,
                    I18n.Config_ResizeChestMenuRows_Tooltip);
                return;

            case nameof(ResizeChestMenu) when storage.ConfigureMenu is InGameMenu.Full:
                Integrations.GMCM.AddChestMenuRowsOption(manifest, data);
                return;

            case nameof(SearchItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.SearchItems,
                    value => data.SearchItems = value,
                    I18n.Config_SearchItems_Name,
                    I18n.Config_SearchItems_Tooltip);
                return;

            case nameof(StashToChest):
                if (storage.ConfigureMenu is InGameMenu.Advanced)
                {
                    Integrations.GMCM.AddFeatureOptionRange(
                        manifest,
                        () => data.StashToChest,
                        value => data.StashToChest = value,
                        I18n.Config_StashToChest_Name,
                        I18n.Config_StashToChest_Tooltip);

                    Config.GMCM.AddNumberOption(
                        manifest,
                        () => data.StashToChestDistance,
                        value => data.StashToChestDistance = value,
                        I18n.Config_StashToChestDistance_Name,
                        I18n.Config_StashToChestDistance_Tooltip);
                }
                else
                {
                    Integrations.GMCM.AddDistanceOption(
                        manifest,
                        data,
                        featureName,
                        I18n.Config_StashToChestDistance_Name,
                        I18n.Config_StashToChestDistance_Tooltip);
                }

                Config.GMCM.AddNumberOption(
                    manifest,
                    () => data.StashToChestPriority,
                    value => data.StashToChestPriority = value,
                    I18n.Config_StashToChestPriority_Name,
                    I18n.Config_StashToChestPriority_Tooltip);

                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.StashToChestStacks,
                    value => data.StashToChestStacks = value,
                    I18n.Config_StashToChestStacks_Name,
                    I18n.Config_StashToChestStacks_Tooltip);
                return;

            case nameof(TransferItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.TransferItems,
                    value => data.TransferItems = value,
                    I18n.Config_TransferItems_Name,
                    I18n.Config_TransferItems_Tooltip);
                return;

            case nameof(UnloadChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
                Integrations.GMCM.AddFeatureOption(
                    manifest,
                    () => data.UnloadChest,
                    value => data.UnloadChest = value,
                    I18n.Config_UnloadChest_Name,
                    I18n.Config_UnloadChest_Tooltip);

                Integrations.GMCM.AddFeatureOption(
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
        Config.SetupFeatureConfig(nameof(IStorageData.ChestLabel), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(IStorageData.FilterItemsList), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(AutoOrganize), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(CarryChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(ChestInfo), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(ChestMenuTabs), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(CollectItems), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(Configurator), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(CraftFromChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(BetterColorPicker), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(FilterItems), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(IStorageData.HideItems), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(LabelChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(OpenHeldChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(OrganizeChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(ResizeChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(ResizeChestMenu), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(SearchItems), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(StashToChest), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(TransferItems), manifest, storage, inGame);
        Config.SetupFeatureConfig(nameof(UnloadChest), manifest, storage, inGame);
    }
}