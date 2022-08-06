namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Handles config options.
/// </summary>
internal class ConfigHelper
{
    private static ConfigHelper? Instance;

    private readonly Dictionary<IFeature, Func<bool>> _features;
    private readonly IModHelper _helper;
    private readonly IManifest _modManifest;

    private ModConfig? _config;

    private ConfigHelper(IModHelper helper, IManifest manifest, Dictionary<IFeature, Func<bool>> features)
    {
        this._helper = helper;
        this._modManifest = manifest;
        this._features = features;
        this._helper.Events.GameLoop.GameLaunched += ConfigHelper.OnGameLaunched;
    }

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this._helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = config ?? new ModConfig();
            Log.Trace(this._config.ToString());
            return this._config;
        }
    }

    /// <summary>
    ///     Initializes <see cref="ConfigHelper" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="features">Mod features.</param>
    /// <returns>Returns an instance of the <see cref="ConfigHelper" /> class.</returns>
    public static ModConfig Init(IModHelper helper, IManifest manifest, Dictionary<IFeature, Func<bool>> features)
    {
        ConfigHelper.Instance ??= new(helper, manifest, features);
        return ConfigHelper.Instance.Config;
    }

    /// <summary>
    ///     Sets up the main config menu.
    /// </summary>
    public static void SetupMainConfig()
    {
        if (!IntegrationHelper.GMCM.IsLoaded)
        {
            return;
        }

        if (IntegrationHelper.GMCM.IsRegistered(ConfigHelper.Instance!._modManifest))
        {
            IntegrationHelper.GMCM.Unregister(ConfigHelper.Instance._modManifest);
        }

        IntegrationHelper.GMCM.Register(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance._config = new(),
            ConfigHelper.Instance.SaveConfig);

        // General
        IntegrationHelper.GMCM.API!.AddSectionTitle(ConfigHelper.Instance._modManifest, I18n.Section_General_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.Instance._modManifest, I18n.Section_General_Description);

        IntegrationHelper.GMCM.API.AddBoolOption(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.BetterShippingBin,
            value => ConfigHelper.Instance.Config.BetterShippingBin = value,
            I18n.Config_BetterShippingBin_Name,
            I18n.Config_BetterShippingBin_Tooltip,
            nameof(ModConfig.BetterShippingBin));

        IntegrationHelper.GMCM.API.AddNumberOption(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.CarryChestLimit,
            value => ConfigHelper.Instance.Config.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip,
            fieldId: nameof(ModConfig.CarryChestLimit));

        IntegrationHelper.GMCM.API.AddNumberOption(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.CarryChestSlowAmount,
            value => ConfigHelper.Instance.Config.CarryChestSlowAmount = value,
            I18n.Config_CarryChestSlow_Name,
            I18n.Config_CarryChestSlow_Tooltip,
            0,
            4,
            1,
            FormatHelper.FormatCarryChestSlow,
            nameof(ModConfig.CarryChestSlowAmount));

        IntegrationHelper.GMCM.API.AddBoolOption(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ChestFinder,
            value => ConfigHelper.Instance.Config.ChestFinder = value,
            I18n.Config_ChestFinder_Name,
            I18n.Config_ChestFinder_Tooltip,
            nameof(ModConfig.ChestFinder));

        IntegrationHelper.GMCM.API.AddTextOption(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.CustomColorPickerArea.ToStringFast(),
            value => ConfigHelper.Instance.Config.CustomColorPickerArea =
                ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
            I18n.Config_CustomColorPickerArea_Name,
            I18n.Config_CustomColorPickerArea_Tooltip,
            new[]
            {
                ComponentArea.Left.ToStringFast(),
                ComponentArea.Right.ToStringFast(),
            },
            FormatHelper.FormatArea,
            nameof(ModConfig.CustomColorPickerArea));

        if (IntegrationHelper.TestConflicts(nameof(LabelChest), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                ConfigHelper.Instance._modManifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(LabelChest)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddBoolOption(
                ConfigHelper.Instance._modManifest,
                () => ConfigHelper.Instance.Config.LabelChest,
                value => ConfigHelper.Instance.Config.LabelChest = value,
                I18n.Config_LabelChest_Name,
                I18n.Config_LabelChest_Tooltip,
                nameof(ModConfig.LabelChest));
        }

        IntegrationHelper.GMCM.API.AddTextOption(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.SearchTagSymbol.ToString(),
            value => ConfigHelper.Instance.Config.SearchTagSymbol =
                string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip,
            fieldId: nameof(ModConfig.SearchTagSymbol));

        if (IntegrationHelper.TestConflicts(nameof(SlotLock), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                ConfigHelper.Instance._modManifest,
                () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(SlotLock)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddBoolOption(
                ConfigHelper.Instance._modManifest,
                () => ConfigHelper.Instance.Config.SlotLock,
                value => ConfigHelper.Instance.Config.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip,
                nameof(ModConfig.SlotLock));

            IntegrationHelper.GMCM.API.AddBoolOption(
                ConfigHelper.Instance._modManifest,
                () => ConfigHelper.Instance.Config.SlotLockHold,
                value => ConfigHelper.Instance.Config.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip,
                nameof(ModConfig.SlotLockHold));
        }

        // Controls
        IntegrationHelper.GMCM.API.AddSectionTitle(ConfigHelper.Instance._modManifest, I18n.Section_Controls_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.Instance._modManifest, I18n.Section_Controls_Description);

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.FindChest,
            value => ConfigHelper.Instance.Config.ControlScheme.FindChest = value,
            I18n.Config_FindChest_Name,
            I18n.Config_FindChest_Tooltip,
            nameof(Controls.FindChest));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.OpenCrafting,
            value => ConfigHelper.Instance.Config.ControlScheme.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip,
            nameof(Controls.OpenCrafting));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.StashItems,
            value => ConfigHelper.Instance.Config.ControlScheme.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip,
            nameof(Controls.StashItems));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.Configure,
            value => ConfigHelper.Instance.Config.ControlScheme.Configure = value,
            I18n.Config_Configure_Name,
            I18n.Config_Configure_Tooltip,
            nameof(Controls.Configure));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.PreviousTab,
            value => ConfigHelper.Instance.Config.ControlScheme.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip,
            nameof(Controls.PreviousTab));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.NextTab,
            value => ConfigHelper.Instance.Config.ControlScheme.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip,
            nameof(Controls.NextTab));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.ScrollUp,
            value => ConfigHelper.Instance.Config.ControlScheme.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip,
            nameof(Controls.ScrollUp));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.ScrollDown,
            value => ConfigHelper.Instance.Config.ControlScheme.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip,
            nameof(Controls.ScrollDown));

        IntegrationHelper.GMCM.API.AddKeybind(
            ConfigHelper.Instance._modManifest,
            () => ConfigHelper.Instance.Config.ControlScheme.LockSlot,
            value => ConfigHelper.Instance.Config.ControlScheme.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip,
            nameof(Controls.LockSlot));

        // Chest Types
        IntegrationHelper.GMCM.API.AddSectionTitle(ConfigHelper.Instance._modManifest, I18n.Section_Chests_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.Instance._modManifest, I18n.Section_Chests_Description);

        IntegrationHelper.GMCM.API.AddPageLink(
            ConfigHelper.Instance._modManifest,
            "Default",
            I18n.Storage_Default_Name,
            I18n.Storage_Default_Tooltip);

        foreach (var (key, _) in ConfigHelper.Instance.Config.VanillaStorages)
        {
            IntegrationHelper.GMCM.API.AddPageLink(
                ConfigHelper.Instance._modManifest,
                key,
                () => FormatHelper.FormatStorageName(key),
                () => FormatHelper.FormatStorageTooltip(key));
        }

        // Default Chest
        IntegrationHelper.GMCM.API.AddPage(ConfigHelper.Instance._modManifest, "Default", I18n.Storage_Default_Name);
        ConfigHelper.Instance.SetupConfig(
            ConfigHelper.Instance._modManifest,
            ConfigHelper.Instance.Config.DefaultChest);

        // Other Chests
        foreach (var (key, value) in ConfigHelper.Instance.Config.VanillaStorages)
        {
            IntegrationHelper.GMCM.API.AddPage(
                ConfigHelper.Instance._modManifest,
                key,
                () => FormatHelper.FormatStorageName(key));
            ConfigHelper.Instance.SetupConfig(ConfigHelper.Instance._modManifest, value);
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
        if (!IntegrationHelper.GMCM.IsLoaded)
        {
            return;
        }

        if (register)
        {
            if (IntegrationHelper.GMCM.IsRegistered(manifest))
            {
                IntegrationHelper.GMCM.Unregister(manifest);
            }

            IntegrationHelper.GMCM.Register(
                manifest,
                () => ConfigHelper.Instance!._config = new(),
                ConfigHelper.Instance!.SaveConfig);
        }

        ConfigHelper.Instance!.SetupConfig(manifest, storage);
    }

    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (IntegrationHelper.GMCM.IsLoaded)
        {
            ConfigHelper.SetupMainConfig();
        }
    }

    private Action<SpriteBatch, Vector2> DrawButton(IStorageObject storage, string label)
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
                            this._helper.Input,
                            this._helper.Translation));
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

    private void SaveConfig()
    {
        this._helper.WriteConfig(this.Config);
        foreach (var (feature, condition) in this._features)
        {
            if (condition() && !IntegrationHelper.TestConflicts(feature.GetType().Name, out _))
            {
                feature.Activate();
                continue;
            }

            feature.Deactivate();
        }
    }

    private void SetupConfig(IManifest manifest, IStorageData storage)
    {
        if (!IntegrationHelper.GMCM.IsLoaded)
        {
            return;
        }

        var data = storage switch
        {
            StorageData storageData => storageData,
            IStorageObject { Data: { } storageData } => storageData,
            _ => storage,
        };

        if (storage is IStorageObject)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ChestLabel,
                value => data.ChestLabel = value,
                I18n.Config_ChestLabel_Name,
                I18n.Config_ChestLabel_Tooltip,
                fieldId: nameof(IStorageData.ChestLabel));
        }

        // Auto Organize
        if (IntegrationHelper.TestConflicts(nameof(AutoOrganize), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(AutoOrganize)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.AutoOrganize.ToStringFast(),
                value => data.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.AutoOrganize));
        }

        // Carry Chest
        if (IntegrationHelper.TestConflicts(nameof(CarryChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(CarryChest)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CarryChest.ToStringFast(),
                value => data.CarryChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CarryChest));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CarryChestSlow.ToStringFast(),
                value => data.CarryChestSlow = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChestSlow_Name,
                I18n.Config_CarryChestSlow_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CarryChestSlow));
        }

        // Chest categories
        if (storage is IStorageObject storageObject)
        {
            IntegrationHelper.GMCM.API.AddComplexOption(
                manifest,
                I18n.Config_FilterItemsList_Name,
                this.DrawButton(storageObject, I18n.Button_Configure_Name()),
                I18n.Config_FilterItemsList_Tooltip,
                height: () => Game1.tileSize,
                fieldId: nameof(IStorageData.FilterItemsList));
        }

        // Chest Menu Tabs
        if (IntegrationHelper.TestConflicts(nameof(ChestMenuTabs), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(ChestMenuTabs)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ChestMenuTabs.ToStringFast(),
                value => data.ChestMenuTabs = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestMenuTabs_Name,
                I18n.Config_ChestMenuTabs_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.ChestMenuTabs));
        }

        // Collect Items
        if (IntegrationHelper.TestConflicts(nameof(CollectItems), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(CollectItems)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CollectItems.ToStringFast(),
                value => data.CollectItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CollectItems));
        }

        // Craft From Chest
        if (IntegrationHelper.TestConflicts(nameof(CraftFromChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(CraftFromChest)}",
                    modList));
        }
        else if (this.Config.AdvancedConfig)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CraftFromChest.ToStringFast(),
                value => data.CraftFromChest = FeatureOptionRangeExtensions.TryParse(value, out var range)
                    ? range
                    : FeatureOptionRange.Default,
                I18n.Config_CraftFromChest_Name,
                I18n.Config_CraftFromChest_Tooltip,
                FeatureOptionRangeExtensions.GetNames(),
                FormatHelper.FormatRange,
                nameof(IStorageData.CraftFromChest));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.CraftFromChestDistance,
                value => data.CraftFromChestDistance = value,
                I18n.Config_CraftFromChestDistance_Name,
                I18n.Config_CraftFromChestDistance_Tooltip,
                fieldId: nameof(IStorageData.CraftFromChest));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.CraftFromChestDistance switch
                {
                    _ when data.CraftFromChest is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
                    _ when data.CraftFromChest is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
                    _ when data.CraftFromChest is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
                    _ when data.CraftFromChest is FeatureOptionRange.World => (int)FeatureOptionRange.World,
                    >= 2 when data.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location
                      + (int)Math.Ceiling(Math.Log2(data.CraftFromChestDistance))
                      - 1,
                    _ when data.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
                    _ => (int)FeatureOptionRange.Default,
                },
                value =>
                {
                    data.CraftFromChestDistance = value switch
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
                    data.CraftFromChest = value switch
                    {
                        (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
                        (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
                        (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
                        (int)FeatureOptionRange.World => FeatureOptionRange.World,
                        (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
                        _ => FeatureOptionRange.Location,
                    };
                },
                I18n.Config_CraftFromChestDistance_Name,
                I18n.Config_CraftFromChestDistance_Tooltip,
                (int)FeatureOptionRange.Default,
                (int)FeatureOptionRange.World,
                1,
                FormatHelper.FormatRangeDistance,
                nameof(IStorageData.CraftFromChest));
        }

        // Custom Color Picker
        if (IntegrationHelper.TestConflicts(nameof(BetterColorPicker), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(BetterColorPicker)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CustomColorPicker.ToStringFast(),
                value => data.CustomColorPicker = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CustomColorPicker_Name,
                I18n.Config_CustomColorPicker_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CustomColorPicker));
        }

        // Filter Items
        if (IntegrationHelper.TestConflicts(nameof(FilterItems), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(FilterItems)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.FilterItems.ToStringFast(),
                value => data.FilterItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_FilterItems_Name,
                I18n.Config_FilterItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.FilterItems));
        }

        // Open Held Chest
        if (IntegrationHelper.TestConflicts(nameof(OpenHeldChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(OpenHeldChest)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OpenHeldChest.ToStringFast(),
                value => data.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.OpenHeldChest));
        }

        // Organize Chest
        if (IntegrationHelper.TestConflicts(nameof(OrganizeChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(OrganizeChest)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChest.ToStringFast(),
                value => data.OrganizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OrganizeChest_Name,
                I18n.Config_OrganizeChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.OrganizeChest));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChestGroupBy.ToStringFast(),
                value => data.OrganizeChestGroupBy =
                    GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
                I18n.Config_OrganizeChestGroupBy_Name,
                I18n.Config_OrganizeChestGroupBy_Tooltip,
                GroupByExtensions.GetNames(),
                FormatHelper.FormatGroupBy,
                nameof(IStorageData.OrganizeChestGroupBy));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChestSortBy.ToStringFast(),
                value => data.OrganizeChestSortBy =
                    SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
                I18n.Config_OrganizeChestSortBy_Name,
                I18n.Config_OrganizeChestSortBy_Tooltip,
                SortByExtensions.GetNames(),
                FormatHelper.FormatSortBy,
                nameof(IStorageData.OrganizeChestSortBy));
        }

        // Resize Chest
        if (IntegrationHelper.TestConflicts(nameof(ResizeChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(ResizeChest)}",
                    modList));
        }
        else if (this.Config.AdvancedConfig)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ResizeChest.ToStringFast(),
                value => data.ResizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ResizeChest_Name,
                I18n.Config_ResizeChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.ResizeChest));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.ResizeChestCapacity,
                value => data.ResizeChestCapacity = value,
                I18n.Config_ResizeChestCapacity_Name,
                I18n.Config_ResizeChestCapacity_Tooltip,
                fieldId: nameof(IStorageData.ResizeChestCapacity));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.ResizeChestCapacity switch
                {
                    _ when data.ResizeChest is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when data.ResizeChest is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                    -1 => 8,
                    _ => (int)FeatureOption.Enabled + data.ResizeChestCapacity / 12 - 1,
                },
                value =>
                {
                    data.ResizeChestCapacity = value switch
                    {
                        (int)FeatureOption.Default => 0,
                        (int)FeatureOption.Disabled => 0,
                        8 => -1,
                        >= (int)FeatureOption.Enabled => 12 * (1 + value - (int)FeatureOption.Enabled),
                        _ => 0,
                    };
                    data.ResizeChest = value switch
                    {
                        (int)FeatureOption.Default => FeatureOption.Default,
                        (int)FeatureOption.Disabled => FeatureOption.Disabled,
                        _ => FeatureOption.Enabled,
                    };
                },
                I18n.Config_ResizeChestCapacity_Name,
                I18n.Config_ResizeChestCapacity_Tooltip,
                (int)FeatureOption.Default,
                8,
                1,
                FormatHelper.FormatChestCapacity,
                nameof(IStorageData.ResizeChestCapacity));
        }

        // Resize Chest Menu
        if (IntegrationHelper.TestConflicts(nameof(ResizeChestMenu), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(ResizeChestMenu)}",
                    modList));
        }
        else if (this.Config.AdvancedConfig)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ResizeChestMenu.ToStringFast(),
                value => data.ResizeChestMenu = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ResizeChestMenu_Name,
                I18n.Config_ResizeChestMenu_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.ResizeChestMenu));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.ResizeChestMenuRows,
                value => data.ResizeChestMenuRows = value,
                I18n.Config_ResizeChestMenuRows_Name,
                I18n.Config_ResizeChestMenuRows_Tooltip,
                fieldId: nameof(IStorageData.ResizeChestMenuRows));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.ResizeChestMenuRows switch
                {
                    _ when data.ResizeChestMenu is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when data.ResizeChestMenu is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                    _ => (int)FeatureOption.Enabled + data.ResizeChestMenuRows - 3,
                },
                value =>
                {
                    data.ResizeChestMenuRows = value switch
                    {
                        (int)FeatureOption.Default => 0,
                        (int)FeatureOption.Disabled => 0,
                        _ => 3 + value - (int)FeatureOption.Enabled,
                    };
                    data.ResizeChestMenu = value switch
                    {
                        (int)FeatureOption.Default => FeatureOption.Default,
                        (int)FeatureOption.Disabled => FeatureOption.Disabled,
                        _ => FeatureOption.Enabled,
                    };
                },
                I18n.Config_ResizeChestMenuRows_Name,
                I18n.Config_ResizeChestMenuRows_Tooltip,
                (int)FeatureOption.Default,
                5,
                1,
                FormatHelper.FormatChestMenuRows,
                nameof(IStorageData.ResizeChestMenuRows));
        }

        // Search Items
        if (IntegrationHelper.TestConflicts(nameof(SearchItems), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(SearchItems)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.SearchItems.ToStringFast(),
                value => data.SearchItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.SearchItems));
        }

        // Stash To Chest
        if (IntegrationHelper.TestConflicts(nameof(StashToChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(StashToChest)}",
                    modList));
        }
        else if (this.Config.AdvancedConfig)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.StashToChest.ToStringFast(),
                value => data.StashToChest = FeatureOptionRangeExtensions.TryParse(value, out var range)
                    ? range
                    : FeatureOptionRange.Default,
                I18n.Config_StashToChest_Name,
                I18n.Config_StashToChest_Tooltip,
                FeatureOptionRangeExtensions.GetNames(),
                FormatHelper.FormatRange,
                nameof(IStorageData.StashToChest));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.StashToChestDistance,
                value => data.StashToChestDistance = value,
                I18n.Config_StashToChestDistance_Name,
                I18n.Config_StashToChestDistance_Tooltip,
                fieldId: nameof(IStorageData.StashToChest));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => data.StashToChestDistance switch
                {
                    _ when data.StashToChest is FeatureOptionRange.Default => (int)FeatureOptionRange.Default,
                    _ when data.StashToChest is FeatureOptionRange.Disabled => (int)FeatureOptionRange.Disabled,
                    _ when data.StashToChest is FeatureOptionRange.Inventory => (int)FeatureOptionRange.Inventory,
                    _ when data.StashToChest is FeatureOptionRange.World => (int)FeatureOptionRange.World,
                    >= 2 when data.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location
                      + (int)Math.Ceiling(Math.Log2(data.StashToChestDistance))
                      - 1,
                    _ when data.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.World - 1,
                    _ => (int)FeatureOptionRange.Default,
                },
                value =>
                {
                    data.StashToChestDistance = value switch
                    {
                        (int)FeatureOptionRange.Default => 0,
                        (int)FeatureOptionRange.Disabled => 0,
                        (int)FeatureOptionRange.Inventory => 0,
                        (int)FeatureOptionRange.World - 1 => -1,
                        (int)FeatureOptionRange.World => 0,
                        >= (int)FeatureOptionRange.Location => (int)Math.Pow(
                            2,
                            1 + value - (int)FeatureOptionRange.Location),
                        _ => 0,
                    };
                    data.StashToChest = value switch
                    {
                        (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
                        (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
                        (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
                        (int)FeatureOptionRange.World => FeatureOptionRange.World,
                        (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
                        _ => FeatureOptionRange.Location,
                    };
                },
                I18n.Config_StashToChestDistance_Name,
                I18n.Config_StashToChestDistance_Tooltip,
                (int)FeatureOptionRange.Default,
                (int)FeatureOptionRange.World,
                1,
                FormatHelper.FormatRangeDistance,
                nameof(IStorageData.StashToChest));
        }

        IntegrationHelper.GMCM.API.AddNumberOption(
            manifest,
            () => data.StashToChestPriority,
            value => data.StashToChestPriority = value,
            I18n.Config_StashToChestPriority_Name,
            I18n.Config_StashToChestPriority_Tooltip,
            fieldId: nameof(IStorageData.StashToChestPriority));

        IntegrationHelper.GMCM.API.AddTextOption(
            manifest,
            () => data.StashToChestStacks.ToStringFast(),
            value => data.StashToChestStacks =
                FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip,
            FeatureOptionExtensions.GetNames(),
            FormatHelper.FormatOption,
            nameof(IStorageData.StashToChestStacks));

        // Unload Chest
        if (IntegrationHelper.TestConflicts(nameof(UnloadChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                manifest,
                () => string.Format(
                    I18n.Warn_Incompatibility_Disabled(),
                    $"BetterChests.{nameof(UnloadChest)}",
                    modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.UnloadChest.ToStringFast(),
                value => data.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_UnloadChest_Name,
                I18n.Config_UnloadChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.UnloadChest));
        }
    }
}