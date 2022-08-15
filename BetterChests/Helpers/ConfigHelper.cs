namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>
///     Handles config options.
/// </summary>
internal class ConfigHelper
{
    private static ConfigHelper? Instance;

    private readonly Lazy<ModConfig> _config;

    private readonly Dictionary<IFeature, Func<bool>> _features;
    private readonly IModHelper _helper;
    private readonly IManifest _modManifest;

    private ConfigHelper(IModHelper helper, IManifest manifest, Dictionary<IFeature, Func<bool>> features)
    {
        this._config = new(
            () =>
            {
                ModConfig? config = null;
                try
                {
                    config = helper.ReadConfig<ModConfig>();
                }
                catch (Exception)
                {
                    // ignored
                }

                // Attempt to update old config
                if (config is null && !ModConfigOld.TryUpdate(helper, out config))
                {
                    config ??= new();
                }

                // Assign default values
                config.FillDefaults();

                Log.Trace(config.ToString());
                return config;
            });
        this._helper = helper;
        this._modManifest = manifest;
        this._features = features;
        this._helper.Events.GameLoop.GameLaunched += ConfigHelper.OnGameLaunched;
    }

    private static ModConfig Config => ConfigHelper.Instance!._config.Value;

    private static Dictionary<IFeature, Func<bool>> Features => ConfigHelper.Instance!._features;

    private static IInputHelper Input => ConfigHelper.Instance!._helper.Input;

    private static IManifest ModManifest => ConfigHelper.Instance!._modManifest;

    private static ITranslationHelper Translation => ConfigHelper.Instance!._helper.Translation;

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
        return ConfigHelper.Config;
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

        if (IntegrationHelper.GMCM.IsRegistered(ConfigHelper.ModManifest))
        {
            IntegrationHelper.GMCM.Unregister(ConfigHelper.ModManifest);
        }

        IntegrationHelper.GMCM.Register(ConfigHelper.ModManifest, ConfigHelper.ResetConfig, ConfigHelper.SaveConfig);

        // General
        IntegrationHelper.GMCM.API!.AddSectionTitle(ConfigHelper.ModManifest, I18n.Section_General_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.ModManifest, I18n.Section_General_Description);

        IntegrationHelper.GMCM.API.AddBoolOption(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.BetterShippingBin,
            value => ConfigHelper.Config.BetterShippingBin = value,
            I18n.Config_BetterShippingBin_Name,
            I18n.Config_BetterShippingBin_Tooltip,
            nameof(ModConfig.BetterShippingBin));

        IntegrationHelper.GMCM.API.AddNumberOption(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.CarryChestLimit,
            value => ConfigHelper.Config.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip,
            fieldId: nameof(ModConfig.CarryChestLimit));

        IntegrationHelper.GMCM.API.AddNumberOption(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.CarryChestSlowAmount,
            value => ConfigHelper.Config.CarryChestSlowAmount = value,
            I18n.Config_CarryChestSlow_Name,
            I18n.Config_CarryChestSlow_Tooltip,
            0,
            4,
            1,
            FormatHelper.FormatCarryChestSlow,
            nameof(ModConfig.CarryChestSlowAmount));

        IntegrationHelper.GMCM.API.AddBoolOption(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ChestFinder,
            value => ConfigHelper.Config.ChestFinder = value,
            I18n.Config_ChestFinder_Name,
            I18n.Config_ChestFinder_Tooltip,
            nameof(ModConfig.ChestFinder));

        IntegrationHelper.GMCM.API.AddTextOption(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.CustomColorPickerArea.ToStringFast(),
            value => ConfigHelper.Config.CustomColorPickerArea =
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

        IntegrationHelper.GMCM.API.AddTextOption(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.SearchTagSymbol.ToString(),
            value => ConfigHelper.Config.SearchTagSymbol =
                string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip,
            fieldId: nameof(ModConfig.SearchTagSymbol));

        if (IntegrationHelper.TestConflicts(nameof(SlotLock), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(
                ConfigHelper.ModManifest,
                () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(SlotLock)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddBoolOption(
                ConfigHelper.ModManifest,
                () => ConfigHelper.Config.SlotLock,
                value => ConfigHelper.Config.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip,
                nameof(ModConfig.SlotLock));

            IntegrationHelper.GMCM.API.AddTextOption(
                ConfigHelper.ModManifest,
                () => ConfigHelper.Config.SlotLockColor.ToStringFast(),
                value => ConfigHelper.Config.SlotLockColor =
                    ColorsExtensions.TryParse(value, out var color) ? color : Colors.Gray,
                I18n.Config_SlotLockColor_Name,
                I18n.Config_SlotLockColor_Tooltip,
                ColorsExtensions.GetNames(),
                fieldId: nameof(ModConfig.SlotLockColor));

            IntegrationHelper.GMCM.API.AddBoolOption(
                ConfigHelper.ModManifest,
                () => ConfigHelper.Config.SlotLockHold,
                value => ConfigHelper.Config.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip,
                nameof(ModConfig.SlotLockHold));
        }

        // Controls
        IntegrationHelper.GMCM.API.AddSectionTitle(ConfigHelper.ModManifest, I18n.Section_Controls_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.ModManifest, I18n.Section_Controls_Description);

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.FindChest,
            value => ConfigHelper.Config.ControlScheme.FindChest = value,
            I18n.Config_FindChest_Name,
            I18n.Config_FindChest_Tooltip,
            nameof(Controls.FindChest));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.OpenCrafting,
            value => ConfigHelper.Config.ControlScheme.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip,
            nameof(Controls.OpenCrafting));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.StashItems,
            value => ConfigHelper.Config.ControlScheme.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip,
            nameof(Controls.StashItems));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.Configure,
            value => ConfigHelper.Config.ControlScheme.Configure = value,
            I18n.Config_Configure_Name,
            I18n.Config_Configure_Tooltip,
            nameof(Controls.Configure));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.PreviousTab,
            value => ConfigHelper.Config.ControlScheme.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip,
            nameof(Controls.PreviousTab));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.NextTab,
            value => ConfigHelper.Config.ControlScheme.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip,
            nameof(Controls.NextTab));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.ScrollUp,
            value => ConfigHelper.Config.ControlScheme.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip,
            nameof(Controls.ScrollUp));

        IntegrationHelper.GMCM.API.AddKeybindList(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.ScrollDown,
            value => ConfigHelper.Config.ControlScheme.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip,
            nameof(Controls.ScrollDown));

        IntegrationHelper.GMCM.API.AddKeybind(
            ConfigHelper.ModManifest,
            () => ConfigHelper.Config.ControlScheme.LockSlot,
            value => ConfigHelper.Config.ControlScheme.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip,
            nameof(Controls.LockSlot));

        // Default Chest
        IntegrationHelper.GMCM.API.AddSectionTitle(ConfigHelper.ModManifest, I18n.Storage_Default_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.ModManifest, I18n.Storage_Default_Tooltip);

        ConfigHelper.SetupConfig(ConfigHelper.ModManifest, ConfigHelper.Config);

        // Chest Types
        IntegrationHelper.GMCM.API.AddSectionTitle(ConfigHelper.ModManifest, I18n.Section_Chests_Name);
        IntegrationHelper.GMCM.API.AddParagraph(ConfigHelper.ModManifest, I18n.Section_Chests_Description);

        foreach (var (key, _) in ConfigHelper.Config.VanillaStorages)
        {
            IntegrationHelper.GMCM.API.AddPageLink(
                ConfigHelper.ModManifest,
                key,
                () => FormatHelper.FormatStorageName(key),
                () => FormatHelper.FormatStorageTooltip(key));
        }

        // Other Chests
        foreach (var (key, value) in ConfigHelper.Config.VanillaStorages)
        {
            IntegrationHelper.GMCM.API.AddPage(
                ConfigHelper.ModManifest,
                key,
                () => FormatHelper.FormatStorageName(key));
            ConfigHelper.SetupConfig(ConfigHelper.ModManifest, value);
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

            IntegrationHelper.GMCM.Register(manifest, ConfigHelper.ResetConfig, ConfigHelper.SaveConfig);
        }

        ConfigHelper.SetupConfig(manifest, storage, true);
    }

    private static Action<SpriteBatch, Vector2> DrawButton(IStorageObject storage, string label)
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
                            ConfigHelper.Input,
                            ConfigHelper.Translation));
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
        if (IntegrationHelper.GMCM.IsLoaded)
        {
            ConfigHelper.SetupMainConfig();
        }
    }

    private static void ResetConfig()
    {
        ConfigHelper.Config.Reset();
    }

    private static void SaveConfig()
    {
        ConfigHelper.Instance!._helper.WriteConfig(ConfigHelper.Config);
        foreach (var (feature, condition) in ConfigHelper.Features)
        {
            if (condition() && !IntegrationHelper.TestConflicts(feature.GetType().Name, out _))
            {
                feature.Activate();
                continue;
            }

            feature.Deactivate();
        }
    }

    private static void SetupConfig(IManifest manifest, IStorageData storage, bool inGame = false)
    {
        if (!IntegrationHelper.GMCM.IsLoaded)
        {
            return;
        }

        bool Conflicts(string feature, GenericModConfigMenuIntegration gmcm)
        {
            if (!IntegrationHelper.TestConflicts(feature, out var mods))
            {
                return false;
            }

            if (inGame)
            {
                return true;
            }

            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            gmcm.API!.AddParagraph(
                manifest,
                () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{feature}", modList));
            return true;
        }

        var data = storage switch
        {
            StorageData storageData => storageData,
            IStorageObject { Data: { } storageData } => storageData,
            _ => storage,
        };

        var simpleConfig = storage.ConfigureMenu is not (InGameMenu.Full or InGameMenu.Advanced);
        var allowedOptions = FeatureOptionExtensions.GetNames();
        var allowedRanges = FeatureOptionRangeExtensions.GetNames();
        if (ReferenceEquals(storage, ConfigHelper.Config))
        {
            allowedOptions = allowedOptions.Except(new[] { nameof(FeatureOption.Default) }).ToArray();
            allowedRanges = allowedRanges.Except(new[] { nameof(FeatureOptionRange.Default) }).ToArray();
        }

        if (storage is IStorageObject storageObject)
        {
            if (ConfigHelper.Config.LabelChest is not FeatureOption.Disabled)
            {
                // Chest Label
                IntegrationHelper.GMCM.API.AddTextOption(
                    manifest,
                    () => data.ChestLabel,
                    value => data.ChestLabel = value,
                    I18n.Config_ChestLabel_Name,
                    I18n.Config_ChestLabel_Tooltip,
                    fieldId: nameof(IStorageData.ChestLabel));
            }

            // Chest Categories
            IntegrationHelper.GMCM.API.AddComplexOption(
                manifest,
                I18n.Config_FilterItemsList_Name,
                ConfigHelper.DrawButton(storageObject, I18n.Button_Configure_Name()),
                I18n.Config_FilterItemsList_Tooltip,
                height: () => Game1.tileSize,
                fieldId: nameof(IStorageData.FilterItemsList));
        }

        // Auto Organize
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.AutoOrganize is not FeatureOption.Disabled))
         && !Conflicts(nameof(AutoOrganize), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.AutoOrganize.ToStringFast(),
                value => data.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.AutoOrganize));
        }

        // Carry Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.CarryChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(CarryChest), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CarryChest.ToStringFast(),
                value => data.CarryChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                allowedOptions,
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
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.CarryChestSlow));
        }

        // Chest Menu Tabs
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.ChestMenuTabs is not FeatureOption.Disabled))
         && !Conflicts(nameof(ChestMenuTabs), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ChestMenuTabs.ToStringFast(),
                value => data.ChestMenuTabs = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestMenuTabs_Name,
                I18n.Config_ChestMenuTabs_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.ChestMenuTabs));
        }

        // Collect Items
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.CollectItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(CollectItems), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CollectItems.ToStringFast(),
                value => data.CollectItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.CollectItems));
        }

        // Configurator
        if (!inGame && !Conflicts(nameof(Configurator), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.Configurator.ToStringFast(),
                value => data.Configurator = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_Configure_Name,
                I18n.Config_Configure_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.Configurator));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ConfigureMenu.ToStringFast(),
                value => data.ConfigureMenu = InGameMenuExtensions.TryParse(value, out var menu)
                    ? menu
                    : InGameMenu.Default,
                I18n.Config_ConfigureMenu_Name,
                I18n.Config_ConfigureMenu_Tooltip,
                InGameMenuExtensions.GetNames(),
                FormatHelper.FormatMenu,
                nameof(IStorageData.ConfigureMenu));
        }

        // Craft From Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.CraftFromChest is not FeatureOptionRange.Disabled))
         && !Conflicts(nameof(CraftFromChest), IntegrationHelper.GMCM))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                IntegrationHelper.GMCM.API.AddTextOption(
                    manifest,
                    () => data.CraftFromChest.ToStringFast(),
                    value => data.CraftFromChest = FeatureOptionRangeExtensions.TryParse(value, out var range)
                        ? range
                        : FeatureOptionRange.Default,
                    I18n.Config_CraftFromChest_Name,
                    I18n.Config_CraftFromChest_Tooltip,
                    allowedRanges,
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
        }

        // Custom Color Picker
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.CustomColorPicker is not FeatureOption.Disabled))
         && !Conflicts(nameof(BetterColorPicker), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CustomColorPicker.ToStringFast(),
                value => data.CustomColorPicker = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CustomColorPicker_Name,
                I18n.Config_CustomColorPicker_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.CustomColorPicker));
        }

        // Filter Items
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.FilterItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(FilterItems), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.FilterItems.ToStringFast(),
                value => data.FilterItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_FilterItems_Name,
                I18n.Config_FilterItems_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.FilterItems));
        }

        // Hide Items
        if (!inGame || data.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced)
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                ConfigHelper.ModManifest,
                () => data.HideItems.ToStringFast(),
                value => data.HideItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_HideItems_Name,
                I18n.Config_HideItems_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.HideItems));
        }

        // Label Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.LabelChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(LabelChest), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                ConfigHelper.ModManifest,
                () => ConfigHelper.Config.LabelChest.ToStringFast(),
                value => ConfigHelper.Config.LabelChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_LabelChest_Name,
                I18n.Config_LabelChest_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.LabelChest));
        }

        // Open Held Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.OpenHeldChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(OpenHeldChest), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OpenHeldChest.ToStringFast(),
                value => data.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.OpenHeldChest));
        }

        // Organize Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.OrganizeChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(OrganizeChest), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChest.ToStringFast(),
                value => data.OrganizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OrganizeChest_Name,
                I18n.Config_OrganizeChest_Tooltip,
                allowedOptions,
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
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.ResizeChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(ResizeChest), IntegrationHelper.GMCM))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                IntegrationHelper.GMCM.API.AddTextOption(
                    manifest,
                    () => data.ResizeChest.ToStringFast(),
                    value => data.ResizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                        ? option
                        : FeatureOption.Default,
                    I18n.Config_ResizeChest_Name,
                    I18n.Config_ResizeChest_Tooltip,
                    allowedOptions,
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
        }

        // Resize Chest Menu
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.ResizeChestMenu is not FeatureOption.Disabled))
         && !Conflicts(nameof(ResizeChestMenu), IntegrationHelper.GMCM))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                IntegrationHelper.GMCM.API.AddTextOption(
                    manifest,
                    () => data.ResizeChestMenu.ToStringFast(),
                    value => data.ResizeChestMenu = FeatureOptionExtensions.TryParse(value, out var option)
                        ? option
                        : FeatureOption.Default,
                    I18n.Config_ResizeChestMenu_Name,
                    I18n.Config_ResizeChestMenu_Tooltip,
                    allowedOptions,
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
        }

        // Search Items
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.SearchItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(SearchItems), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.SearchItems.ToStringFast(),
                value => data.SearchItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.SearchItems));
        }

        // Stash To Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.StashToChest is not FeatureOptionRange.Disabled))
         && !Conflicts(nameof(StashToChest), IntegrationHelper.GMCM))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                IntegrationHelper.GMCM.API.AddTextOption(
                    manifest,
                    () => data.StashToChest.ToStringFast(),
                    value => data.StashToChest = FeatureOptionRangeExtensions.TryParse(value, out var range)
                        ? range
                        : FeatureOptionRange.Default,
                    I18n.Config_StashToChest_Name,
                    I18n.Config_StashToChest_Tooltip,
                    allowedRanges,
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
            allowedOptions,
            FormatHelper.FormatOption,
            nameof(IStorageData.StashToChestStacks));

        // Transfer Items
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.TransferItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(TransferItems), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                ConfigHelper.ModManifest,
                () => ConfigHelper.Config.TransferItems.ToStringFast(),
                value => ConfigHelper.Config.TransferItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_TransferItems_Name,
                I18n.Config_TransferItems_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.TransferItems));
        }

        // Unload Chest
        if ((!inGame || (!simpleConfig && ConfigHelper.Config.UnloadChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(UnloadChest), IntegrationHelper.GMCM))
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.UnloadChest.ToStringFast(),
                value => data.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_UnloadChest_Name,
                I18n.Config_UnloadChest_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.UnloadChest));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.UnloadChestCombine.ToStringFast(),
                value => data.UnloadChestCombine = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_UnloadChestCombine_Name,
                I18n.Config_UnloadChestCombine_Tooltip,
                allowedOptions,
                FormatHelper.FormatOption,
                nameof(IStorageData.UnloadChestCombine));
        }
    }
}