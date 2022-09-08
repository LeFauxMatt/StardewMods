namespace StardewMods.BetterChests.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.StorageHandlers;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>
///     Handles config options.
/// </summary>
internal class Config
{
    private static Config? Instance;

    private readonly Lazy<ModConfig> _config;

    private readonly Dictionary<IFeature, Func<bool>> _features;
    private readonly IModHelper _helper;
    private readonly IManifest _modManifest;

    private Config(IModHelper helper, IManifest manifest, Dictionary<IFeature, Func<bool>> features)
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
        this._helper.Events.GameLoop.GameLaunched += Config.OnGameLaunched;
    }

    private static Dictionary<IFeature, Func<bool>> Features => Config.Instance!._features;

    private static IGenericModConfigMenuApi GMCM => Integrations.GMCM.API!;

    private static IInputHelper Input => Config.Instance!._helper.Input;

    private static ModConfig ModConfig => Config.Instance!._config.Value;

    private static IManifest ModManifest => Config.Instance!._modManifest;

    private static ITranslationHelper Translation => Config.Instance!._helper.Translation;

    /// <summary>
    ///     Initializes <see cref="Helpers.Config" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="features">Mod features.</param>
    /// <returns>Returns an instance of the <see cref="Helpers.Config" /> class.</returns>
    public static ModConfig Init(IModHelper helper, IManifest manifest, Dictionary<IFeature, Func<bool>> features)
    {
        Config.Instance ??= new(helper, manifest, features);
        return Config.ModConfig;
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

        if (Integrations.GMCM.IsRegistered(Config.ModManifest))
        {
            Integrations.GMCM.Unregister(Config.ModManifest);
        }

        Integrations.GMCM.Register(Config.ModManifest, Config.ResetConfig, Config.SaveConfig);

        // General
        Config.GMCM.AddSectionTitle(Config.ModManifest, I18n.Section_General_Name);
        Config.GMCM.AddParagraph(Config.ModManifest, I18n.Section_General_Description);

        Config.GMCM.AddBoolOption(
            Config.ModManifest,
            () => Config.ModConfig.BetterShippingBin,
            value => Config.ModConfig.BetterShippingBin = value,
            I18n.Config_BetterShippingBin_Name,
            I18n.Config_BetterShippingBin_Tooltip);

        Config.GMCM.AddNumberOption(
            Config.ModManifest,
            () => Config.ModConfig.CarryChestLimit,
            value => Config.ModConfig.CarryChestLimit = value,
            I18n.Config_CarryChestLimit_Name,
            I18n.Config_CarryChestLimit_Tooltip);

        Config.GMCM.AddNumberOption(
            Config.ModManifest,
            () => Config.ModConfig.CarryChestSlowAmount,
            value => Config.ModConfig.CarryChestSlowAmount = value,
            I18n.Config_CarryChestSlow_Name,
            I18n.Config_CarryChestSlow_Tooltip,
            0,
            4,
            1,
            Formatting.CarryChestSlow);

        Config.GMCM.AddBoolOption(
            Config.ModManifest,
            () => Config.ModConfig.ChestFinder,
            value => Config.ModConfig.ChestFinder = value,
            I18n.Config_ChestFinder_Name,
            I18n.Config_ChestFinder_Tooltip);

        // Craft From Workbench
        if (Config.ModConfig.ConfigureMenu is InGameMenu.Advanced)
        {
            Config.GMCM.AddTextOption(
                Config.ModManifest,
                () => Config.ModConfig.CraftFromWorkbench.ToStringFast(),
                value => Config.ModConfig.CraftFromWorkbench =
                    FeatureOptionRangeExtensions.TryParse(value, out var range) ? range : FeatureOptionRange.Default,
                I18n.Config_CraftFromWorkbench_Name,
                I18n.Config_CraftFromWorkbench_Tooltip,
                FeatureOptionRangeExtensions.GetNames(),
                Formatting.Range);

            Config.GMCM.AddNumberOption(
                Config.ModManifest,
                () => Config.ModConfig.CraftFromWorkbenchDistance,
                value => Config.ModConfig.CraftFromWorkbenchDistance = value,
                I18n.Config_CraftFromWorkbenchDistance_Name,
                I18n.Config_CraftFromWorkbenchDistance_Tooltip);
        }
        else
        {
            Config.GMCM.AddNumberOption(
                Config.ModManifest,
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
            Config.ModManifest,
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
            Config.ModManifest,
            () => Config.ModConfig.SearchTagSymbol.ToString(),
            value => Config.ModConfig.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
            I18n.Config_SearchItemsSymbol_Name,
            I18n.Config_SearchItemsSymbol_Tooltip);

        if (Integrations.TestConflicts(nameof(SlotLock), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            Config.GMCM.AddParagraph(
                Config.ModManifest,
                () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(SlotLock)}", modList));
        }
        else
        {
            Config.GMCM.AddBoolOption(
                Config.ModManifest,
                () => Config.ModConfig.SlotLock,
                value => Config.ModConfig.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip);

            Config.GMCM.AddTextOption(
                Config.ModManifest,
                () => Config.ModConfig.SlotLockColor.ToStringFast(),
                value => Config.ModConfig.SlotLockColor =
                    ColorsExtensions.TryParse(value, out var color) ? color : Colors.Gray,
                I18n.Config_SlotLockColor_Name,
                I18n.Config_SlotLockColor_Tooltip,
                ColorsExtensions.GetNames());

            Config.GMCM.AddBoolOption(
                Config.ModManifest,
                () => Config.ModConfig.SlotLockHold,
                value => Config.ModConfig.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip);
        }

        Config.GMCM.AddBoolOption(
            Config.ModManifest,
            () => Config.ModConfig.Experimental,
            value => Config.ModConfig.Experimental = value,
            I18n.Config_Experimental_Name,
            I18n.Config_Experimental_Tooltip);

        // Controls
        Config.GMCM.AddSectionTitle(Config.ModManifest, I18n.Section_Controls_Name);
        Config.GMCM.AddParagraph(Config.ModManifest, I18n.Section_Controls_Description);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.FindChest,
            value => Config.ModConfig.ControlScheme.FindChest = value,
            I18n.Config_FindChest_Name,
            I18n.Config_FindChest_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.CloseChestFinder,
            value => Config.ModConfig.ControlScheme.CloseChestFinder = value,
            I18n.Config_CloseChestFinder_Name,
            I18n.Config_CloseChestFinder_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.OpenFoundChest,
            value => Config.ModConfig.ControlScheme.OpenFoundChest = value,
            I18n.Config_OpenFoundChest_Name,
            I18n.Config_OpenFoundChest_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.OpenNextChest,
            value => Config.ModConfig.ControlScheme.OpenNextChest = value,
            I18n.Config_OpenNextChest_Name,
            I18n.Config_OpenNextChest_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.OpenCrafting,
            value => Config.ModConfig.ControlScheme.OpenCrafting = value,
            I18n.Config_OpenCrafting_Name,
            I18n.Config_OpenCrafting_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.StashItems,
            value => Config.ModConfig.ControlScheme.StashItems = value,
            I18n.Config_StashItems_Name,
            I18n.Config_StashItems_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.Configure,
            value => Config.ModConfig.ControlScheme.Configure = value,
            I18n.Config_Configure_Name,
            I18n.Config_Configure_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.PreviousTab,
            value => Config.ModConfig.ControlScheme.PreviousTab = value,
            I18n.Config_PreviousTab_Name,
            I18n.Config_PreviousTab_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.NextTab,
            value => Config.ModConfig.ControlScheme.NextTab = value,
            I18n.Config_NextTab_Name,
            I18n.Config_NextTab_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.ScrollUp,
            value => Config.ModConfig.ControlScheme.ScrollUp = value,
            I18n.Config_ScrollUp_Name,
            I18n.Config_ScrollUp_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.ScrollDown,
            value => Config.ModConfig.ControlScheme.ScrollDown = value,
            I18n.Config_ScrollDown_Name,
            I18n.Config_ScrollDown_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.LockSlot,
            value => Config.ModConfig.ControlScheme.LockSlot = value,
            I18n.Config_LockSlot_Name,
            I18n.Config_LockSlot_Tooltip);

        Config.GMCM.AddKeybindList(
            Config.ModManifest,
            () => Config.ModConfig.ControlScheme.ToggleInfo,
            value => Config.ModConfig.ControlScheme.ToggleInfo = value,
            I18n.Config_ToggleInfo_Name,
            I18n.Config_ToggleInfo_Tooltip);

        // Default Chest
        Config.GMCM.AddSectionTitle(Config.ModManifest, I18n.Storage_Default_Name);
        Config.GMCM.AddParagraph(Config.ModManifest, I18n.Storage_Default_Tooltip);

        Config.SetupConfig(Config.ModManifest, Config.ModConfig);

        // Chest Types
        Config.GMCM.AddSectionTitle(Config.ModManifest, I18n.Section_Chests_Name);
        Config.GMCM.AddParagraph(Config.ModManifest, I18n.Section_Chests_Description);

        foreach (var (key, _) in Config.ModConfig.VanillaStorages)
        {
            Config.GMCM.AddPageLink(
                Config.ModManifest,
                key,
                () => Formatting.StorageName(key),
                () => Formatting.StorageTooltip(key));
        }

        // Other Chests
        foreach (var (key, value) in Config.ModConfig.VanillaStorages.OrderBy(kvp => Formatting.StorageName(kvp.Key)))
        {
            Config.GMCM.AddPage(Config.ModManifest, key, () => Formatting.StorageName(key));
            Config.SetupConfig(Config.ModManifest, value);
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
            if (storage is BaseStorage baseStorage)
            {
                sb.AppendLine(baseStorage.ToString());
                sb.Append(baseStorage.Data);
            }

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

        Config.SetupConfig(manifest, storage, true);
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
        Config.ModConfig.Reset();
    }

    private static void SaveConfig()
    {
        Config.Instance!._helper.WriteConfig(Config.ModConfig);
        foreach (var (feature, condition) in Config.Features)
        {
            if (condition() && !Integrations.TestConflicts(feature.GetType().Name, out _))
            {
                feature.Activate();
                continue;
            }

            feature.Deactivate();
        }

        Log.Trace(Config.ModConfig.ToString());
    }

    private static void SetupConfig(IManifest manifest, IStorageData storage, bool inGame = false)
    {
        if (!Integrations.GMCM.IsLoaded)
        {
            return;
        }

        bool Conflicts(string feature)
        {
            if (!Integrations.TestConflicts(feature, out var mods))
            {
                return false;
            }

            if (inGame)
            {
                return true;
            }

            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            Config.GMCM.AddParagraph(
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
        if (ReferenceEquals(storage, Config.ModConfig))
        {
            allowedOptions = allowedOptions.Except(new[] { nameof(FeatureOption.Default) }).ToArray();
            allowedRanges = allowedRanges.Except(new[] { nameof(FeatureOptionRange.Default) }).ToArray();
        }

        if (storage is IStorageObject storageObject)
        {
            if (data.LabelChest is not FeatureOption.Disabled)
            {
                // Chest Label
                Config.GMCM.AddTextOption(
                    manifest,
                    () => data.ChestLabel,
                    value => data.ChestLabel = value,
                    I18n.Config_ChestLabel_Name,
                    I18n.Config_ChestLabel_Tooltip);
            }

            // Chest Categories
            Config.GMCM.AddComplexOption(
                manifest,
                I18n.Config_FilterItemsList_Name,
                Config.DrawButton(storageObject, I18n.Button_Configure_Name()),
                I18n.Config_FilterItemsList_Tooltip,
                height: () => Game1.tileSize);
        }

        // Auto Organize
        if ((!inGame || (!simpleConfig && Config.ModConfig.AutoOrganize is not FeatureOption.Disabled))
         && !Conflicts(nameof(AutoOrganize)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.AutoOrganize.ToStringFast(),
                value => data.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_AutoOrganize_Name,
                I18n.Config_AutoOrganize_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Carry Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.CarryChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(CarryChest)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.CarryChest.ToStringFast(),
                value => data.CarryChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                allowedOptions,
                Formatting.Option);

            Config.GMCM.AddTextOption(
                manifest,
                () => data.CarryChestSlow.ToStringFast(),
                value => data.CarryChestSlow = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CarryChestSlow_Name,
                I18n.Config_CarryChestSlow_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Chest Info
        if ((!inGame || (!simpleConfig && Config.ModConfig.ChestInfo is not FeatureOption.Disabled))
         && !Conflicts(nameof(ChestInfo)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.ChestInfo.ToStringFast(),
                value => data.ChestInfo = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestInfo_Name,
                I18n.Config_ChestInfo_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Chest Menu Tabs
        if ((!inGame || (!simpleConfig && Config.ModConfig.ChestMenuTabs is not FeatureOption.Disabled))
         && !Conflicts(nameof(ChestMenuTabs)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.ChestMenuTabs.ToStringFast(),
                value => data.ChestMenuTabs = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_ChestMenuTabs_Name,
                I18n.Config_ChestMenuTabs_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Collect Items
        if ((!inGame || (!simpleConfig && Config.ModConfig.CollectItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(CollectItems)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.CollectItems.ToStringFast(),
                value => data.CollectItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Configurator
        if (!inGame && !Conflicts(nameof(Configurator)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.Configurator.ToStringFast(),
                value => data.Configurator = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_Configure_Name,
                I18n.Config_Configure_Tooltip,
                allowedOptions,
                Formatting.Option);

            Config.GMCM.AddTextOption(
                manifest,
                () => data.ConfigureMenu.ToStringFast(),
                value => data.ConfigureMenu = InGameMenuExtensions.TryParse(value, out var menu)
                    ? menu
                    : InGameMenu.Default,
                I18n.Config_ConfigureMenu_Name,
                I18n.Config_ConfigureMenu_Tooltip,
                InGameMenuExtensions.GetNames(),
                Formatting.Menu);
        }

        // Craft From Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.CraftFromChest is not FeatureOptionRange.Disabled))
         && !Conflicts(nameof(CraftFromChest)))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                Config.GMCM.AddTextOption(
                    manifest,
                    () => data.CraftFromChest.ToStringFast(),
                    value => data.CraftFromChest = FeatureOptionRangeExtensions.TryParse(value, out var range)
                        ? range
                        : FeatureOptionRange.Default,
                    I18n.Config_CraftFromChest_Name,
                    I18n.Config_CraftFromChest_Tooltip,
                    allowedRanges,
                    Formatting.Range);

                Config.GMCM.AddNumberOption(
                    manifest,
                    () => data.CraftFromChestDistance,
                    value => data.CraftFromChestDistance = value,
                    I18n.Config_CraftFromChestDistance_Name,
                    I18n.Config_CraftFromChestDistance_Tooltip);
            }
            else
            {
                Config.GMCM.AddNumberOption(
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
                    Formatting.Distance);
            }
        }

        // Custom Color Picker
        if ((!inGame || (!simpleConfig && Config.ModConfig.CustomColorPicker is not FeatureOption.Disabled))
         && !Conflicts(nameof(BetterColorPicker)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.CustomColorPicker.ToStringFast(),
                value => data.CustomColorPicker = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_CustomColorPicker_Name,
                I18n.Config_CustomColorPicker_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Filter Items
        if ((!inGame || (!simpleConfig && Config.ModConfig.FilterItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(FilterItems)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.FilterItems.ToStringFast(),
                value => data.FilterItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_FilterItems_Name,
                I18n.Config_FilterItems_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Hide Items
        if (!inGame || data.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced)
        {
            Config.GMCM.AddTextOption(
                Config.ModManifest,
                () => data.HideItems.ToStringFast(),
                value => data.HideItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_HideItems_Name,
                I18n.Config_HideItems_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Label Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.LabelChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(LabelChest)))
        {
            Config.GMCM.AddTextOption(
                Config.ModManifest,
                () => data.LabelChest.ToStringFast(),
                value => data.LabelChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_LabelChest_Name,
                I18n.Config_LabelChest_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Open Held Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.OpenHeldChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(OpenHeldChest)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.OpenHeldChest.ToStringFast(),
                value => data.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OpenHeldChest_Name,
                I18n.Config_OpenHeldChest_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Organize Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.OrganizeChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(OrganizeChest)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.OrganizeChest.ToStringFast(),
                value => data.OrganizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_OrganizeChest_Name,
                I18n.Config_OrganizeChest_Tooltip,
                allowedOptions,
                Formatting.Option);

            Config.GMCM.AddTextOption(
                manifest,
                () => data.OrganizeChestGroupBy.ToStringFast(),
                value => data.OrganizeChestGroupBy =
                    GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
                I18n.Config_OrganizeChestGroupBy_Name,
                I18n.Config_OrganizeChestGroupBy_Tooltip,
                GroupByExtensions.GetNames(),
                Formatting.OrganizeGroupBy);

            Config.GMCM.AddTextOption(
                manifest,
                () => data.OrganizeChestSortBy.ToStringFast(),
                value => data.OrganizeChestSortBy =
                    SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
                I18n.Config_OrganizeChestSortBy_Name,
                I18n.Config_OrganizeChestSortBy_Tooltip,
                SortByExtensions.GetNames(),
                Formatting.OrganizeSortBy);
        }

        // Resize Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.ResizeChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(ResizeChest)))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                Config.GMCM.AddTextOption(
                    manifest,
                    () => data.ResizeChest.ToStringFast(),
                    value => data.ResizeChest = FeatureOptionExtensions.TryParse(value, out var option)
                        ? option
                        : FeatureOption.Default,
                    I18n.Config_ResizeChest_Name,
                    I18n.Config_ResizeChest_Tooltip,
                    allowedOptions,
                    Formatting.Option);

                Config.GMCM.AddNumberOption(
                    manifest,
                    () => data.ResizeChestCapacity,
                    value => data.ResizeChestCapacity = value,
                    I18n.Config_ResizeChestCapacity_Name,
                    I18n.Config_ResizeChestCapacity_Tooltip);
            }
            else
            {
                Config.GMCM.AddNumberOption(
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
                    Formatting.ChestCapacity);
            }
        }

        // Resize Chest Menu
        if ((!inGame || (!simpleConfig && Config.ModConfig.ResizeChestMenu is not FeatureOption.Disabled))
         && !Conflicts(nameof(ResizeChestMenu)))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                Config.GMCM.AddTextOption(
                    manifest,
                    () => data.ResizeChestMenu.ToStringFast(),
                    value => data.ResizeChestMenu = FeatureOptionExtensions.TryParse(value, out var option)
                        ? option
                        : FeatureOption.Default,
                    I18n.Config_ResizeChestMenu_Name,
                    I18n.Config_ResizeChestMenu_Tooltip,
                    allowedOptions,
                    Formatting.Option);

                Config.GMCM.AddNumberOption(
                    manifest,
                    () => data.ResizeChestMenuRows,
                    value => data.ResizeChestMenuRows = value,
                    I18n.Config_ResizeChestMenuRows_Name,
                    I18n.Config_ResizeChestMenuRows_Tooltip);
            }
            else
            {
                Config.GMCM.AddNumberOption(
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
                    Formatting.ChestMenuRows);
            }
        }

        // Search Items
        if ((!inGame || (!simpleConfig && Config.ModConfig.SearchItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(SearchItems)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.SearchItems.ToStringFast(),
                value => data.SearchItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Stash To Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.StashToChest is not FeatureOptionRange.Disabled))
         && !Conflicts(nameof(StashToChest)))
        {
            if (storage.ConfigureMenu is InGameMenu.Advanced)
            {
                Config.GMCM.AddTextOption(
                    manifest,
                    () => data.StashToChest.ToStringFast(),
                    value => data.StashToChest = FeatureOptionRangeExtensions.TryParse(value, out var range)
                        ? range
                        : FeatureOptionRange.Default,
                    I18n.Config_StashToChest_Name,
                    I18n.Config_StashToChest_Tooltip,
                    allowedRanges,
                    Formatting.Range);

                Config.GMCM.AddNumberOption(
                    manifest,
                    () => data.StashToChestDistance,
                    value => data.StashToChestDistance = value,
                    I18n.Config_StashToChestDistance_Name,
                    I18n.Config_StashToChestDistance_Tooltip);
            }
            else
            {
                Config.GMCM.AddNumberOption(
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
                    Formatting.Distance);
            }
        }

        Config.GMCM.AddNumberOption(
            manifest,
            () => data.StashToChestPriority,
            value => data.StashToChestPriority = value,
            I18n.Config_StashToChestPriority_Name,
            I18n.Config_StashToChestPriority_Tooltip);

        Config.GMCM.AddTextOption(
            manifest,
            () => data.StashToChestStacks.ToStringFast(),
            value => data.StashToChestStacks =
                FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
            I18n.Config_StashToChestStacks_Name,
            I18n.Config_StashToChestStacks_Tooltip,
            allowedOptions,
            Formatting.Option);

        // Transfer Items
        if ((!inGame || (!simpleConfig && Config.ModConfig.TransferItems is not FeatureOption.Disabled))
         && !Conflicts(nameof(TransferItems)))
        {
            Config.GMCM.AddTextOption(
                Config.ModManifest,
                () => data.TransferItems.ToStringFast(),
                value => data.TransferItems = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_TransferItems_Name,
                I18n.Config_TransferItems_Tooltip,
                allowedOptions,
                Formatting.Option);
        }

        // Unload Chest
        if ((!inGame || (!simpleConfig && Config.ModConfig.UnloadChest is not FeatureOption.Disabled))
         && !Conflicts(nameof(UnloadChest)))
        {
            Config.GMCM.AddTextOption(
                manifest,
                () => data.UnloadChest.ToStringFast(),
                value => data.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_UnloadChest_Name,
                I18n.Config_UnloadChest_Tooltip,
                allowedOptions,
                Formatting.Option);

            Config.GMCM.AddTextOption(
                manifest,
                () => data.UnloadChestCombine.ToStringFast(),
                value => data.UnloadChestCombine = FeatureOptionExtensions.TryParse(value, out var option)
                    ? option
                    : FeatureOption.Default,
                I18n.Config_UnloadChestCombine_Name,
                I18n.Config_UnloadChestCombine_Tooltip,
                allowedOptions,
                Formatting.Option);
        }
    }
}