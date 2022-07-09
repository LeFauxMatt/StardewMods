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
    private ModConfig? _config;

    private ConfigHelper(
        IModHelper helper,
        IManifest manifest,
        Dictionary<string, (IFeature Feature, Func<bool> Condition)> features)
    {
        this.Helper = helper;
        this.ModManifest = manifest;
        this.Features = features;
        this.Helper.Events.GameLoop.GameLaunched += ConfigHelper.OnGameLaunched;
    }

    private static ConfigHelper? Instance { get; set; }

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
                config = this.Helper.ReadConfig<ModConfig>();
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

    private Dictionary<string, (IFeature Feature, Func<bool> Condition)> Features { get; }

    private IModHelper Helper { get; }

    private IManifest ModManifest { get; }

    /// <summary>
    ///     Initializes <see cref="ConfigHelper" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="features">Mod features.</param>
    /// <returns>Returns an instance of the <see cref="ConfigHelper" /> class.</returns>
    public static ModConfig Init(
        IModHelper helper,
        IManifest manifest,
        Dictionary<string, (IFeature Feature, Func<bool> Condition)> features)
    {
        ConfigHelper.Instance ??= new(helper, manifest, features);
        return ConfigHelper.Instance.Config;
    }

    /// <summary>
    ///     Sets up the main config menu.
    /// </summary>
    public static void SetupMainConfig()
    {
        ConfigHelper.Instance!.SetupConfig(ConfigHelper.Instance.ModManifest, ConfigHelper.Instance.Config.DefaultChest, true);
    }

    /// <summary>
    ///     Sets up a config menu for a specific storage.
    /// </summary>
    /// <param name="storage">The storage to configure for.</param>
    public static void SetupSpecificConfig(IStorageData storage)
    {
        ConfigHelper.Instance!.SetupConfig(ConfigHelper.Instance.ModManifest, storage, false);
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
                if (Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && bounds.Contains(point))
                {
                    Game1.activeClickableMenu.SetChildMenu(new ItemSelectionMenu(storage, storage.FilterMatcher, this.Helper.Translation));
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
        this.Helper.WriteConfig(this.Config);
        foreach (var (featureName, (feature, condition)) in this.Features)
        {
            if (condition() && !IntegrationHelper.TestConflicts(featureName, out _))
            {
                feature.Activate();
                continue;
            }

            feature.Deactivate();
        }
    }

    private void SetupConfig(IManifest manifest, IStorageData storage, bool main)
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

        if (IntegrationHelper.GMCM.IsRegistered(manifest))
        {
            IntegrationHelper.GMCM.Unregister(manifest);
        }

        // Register mod configuration
        IntegrationHelper.GMCM.Register(
            manifest,
            () => this._config = new(),
            this.SaveConfig);

        // General
        IntegrationHelper.GMCM.API.AddSectionTitle(manifest, I18n.Section_General_Name);
        IntegrationHelper.GMCM.API.AddParagraph(manifest, I18n.Section_General_Description);

        if (main)
        {
            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.BetterShippingBin,
                value => this.Config.BetterShippingBin = value,
                I18n.Config_BetterShippingBin_Name,
                I18n.Config_BetterShippingBin_Tooltip,
                nameof(ModConfig.BetterShippingBin));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => this.Config.CarryChestLimit switch
                {
                    _ when data.CarryChestSlow is FeatureOption.Default => (int)FeatureOption.Default,
                    _ when data.CarryChestSlow is FeatureOption.Disabled => (int)FeatureOption.Disabled,
                    _ => (int)FeatureOption.Enabled + this.Config.CarryChestLimit - 1,
                },
                value =>
                {
                    this.Config.CarryChestLimit = value switch
                    {
                        (int)FeatureOption.Default => 0,
                        (int)FeatureOption.Disabled => 0,
                        >= (int)FeatureOption.Enabled => 1 + value - (int)FeatureOption.Enabled,
                        _ => 0,
                    };
                    data.CarryChestSlow = value switch
                    {
                        (int)FeatureOption.Default => FeatureOption.Default,
                        (int)FeatureOption.Disabled => FeatureOption.Disabled,
                        _ => FeatureOption.Enabled,
                    };
                },
                I18n.Config_CarryChestLimit_Name,
                I18n.Config_CarryChestLimit_Tooltip,
                (int)FeatureOption.Default,
                7,
                1,
                FormatHelper.FormatCarryChestLimit,
                nameof(ModConfig.CarryChestLimit));

            IntegrationHelper.GMCM.API.AddNumberOption(
                manifest,
                () => this.Config.CarryChestSlowAmount,
                value => this.Config.CarryChestSlowAmount = value,
                I18n.Config_CarryChestSlow_Name,
                I18n.Config_CarryChestSlow_Tooltip,
                0,
                4,
                1,
                FormatHelper.FormatCarryChestSlow,
                nameof(ModConfig.CarryChestSlowAmount));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => this.Config.CustomColorPickerArea.ToStringFast(),
                value => this.Config.CustomColorPickerArea = ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
                I18n.Config_CustomColorPickerArea_Name,
                I18n.Config_CustomColorPickerArea_Tooltip,
                new[] { ComponentArea.Left.ToStringFast(), ComponentArea.Right.ToStringFast() },
                FormatHelper.FormatArea,
                nameof(ModConfig.CustomColorPickerArea));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChestGroupBy.ToStringFast(),
                value => data.OrganizeChestGroupBy = GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
                I18n.Config_OrganizeChestGroupBy_Name,
                I18n.Config_OrganizeChestGroupBy_Tooltip,
                GroupByExtensions.GetNames(),
                FormatHelper.FormatGroupBy,
                nameof(IStorageData.OrganizeChestGroupBy));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChestSortBy.ToStringFast(),
                value => data.OrganizeChestSortBy = SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
                I18n.Config_OrganizeChestSortBy_Name,
                I18n.Config_OrganizeChestSortBy_Tooltip,
                SortByExtensions.GetNames(),
                FormatHelper.FormatSortBy,
                nameof(IStorageData.OrganizeChestSortBy));

            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => this.Config.SearchTagSymbol.ToString(),
                value => this.Config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
                I18n.Config_SearchItemsSymbol_Name,
                I18n.Config_SearchItemsSymbol_Tooltip,
                fieldId: nameof(ModConfig.SearchTagSymbol));

            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.SlotLockHold,
                value => this.Config.SlotLockHold = value,
                I18n.Config_SlotLockHold_Name,
                I18n.Config_SlotLockHold_Tooltip,
                nameof(ModConfig.SlotLockHold));
        }

        if (main)
        {
            // Controls
            IntegrationHelper.GMCM.API.AddSectionTitle(manifest, I18n.Section_Controls_Name);
            IntegrationHelper.GMCM.API.AddParagraph(manifest, I18n.Section_Controls_Description);

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.OpenCrafting,
                value => this.Config.ControlScheme.OpenCrafting = value,
                I18n.Config_OpenCrafting_Name,
                I18n.Config_OpenCrafting_Tooltip,
                nameof(Controls.OpenCrafting));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.StashItems,
                value => this.Config.ControlScheme.StashItems = value,
                I18n.Config_StashItems_Name,
                I18n.Config_StashItems_Tooltip,
                nameof(Controls.StashItems));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.Configure,
                value => this.Config.ControlScheme.Configure = value,
                I18n.Config_Configure_Name,
                I18n.Config_Configure_Tooltip,
                nameof(Controls.Configure));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.PreviousTab,
                value => this.Config.ControlScheme.PreviousTab = value,
                I18n.Config_PreviousTab_Name,
                I18n.Config_PreviousTab_Tooltip,
                nameof(Controls.PreviousTab));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.NextTab,
                value => this.Config.ControlScheme.NextTab = value,
                I18n.Config_NextTab_Name,
                I18n.Config_NextTab_Tooltip,
                nameof(Controls.NextTab));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.ScrollUp,
                value => this.Config.ControlScheme.ScrollUp = value,
                I18n.Config_ScrollUp_Name,
                I18n.Config_ScrollUp_Tooltip,
                nameof(Controls.ScrollUp));

            IntegrationHelper.GMCM.API.AddKeybindList(
                manifest,
                () => this.Config.ControlScheme.ScrollDown,
                value => this.Config.ControlScheme.ScrollDown = value,
                I18n.Config_ScrollDown_Name,
                I18n.Config_ScrollDown_Tooltip,
                nameof(Controls.ScrollDown));

            IntegrationHelper.GMCM.API.AddKeybind(
                manifest,
                () => this.Config.ControlScheme.LockSlot,
                value => this.Config.ControlScheme.LockSlot = value,
                I18n.Config_LockSlot_Name,
                I18n.Config_LockSlot_Tooltip,
                nameof(Controls.LockSlot));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ChestLabel,
                value => data.ChestLabel = value,
                I18n.Config_ChestLabel_Name,
                I18n.Config_ChestLabel_Tooltip,
                fieldId: nameof(IStorageData.ChestLabel));

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
                value => data.StashToChestStacks = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_StashToChestStacks_Name,
                I18n.Config_StashToChestStacks_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.StashToChestStacks));
        }

        // Features
        IntegrationHelper.GMCM.API.AddSectionTitle(manifest, I18n.Section_Features_Name);
        IntegrationHelper.GMCM.API.AddParagraph(manifest, I18n.Section_Features_Description);

        // Auto Organize
        if (IntegrationHelper.TestConflicts(nameof(AutoOrganize), out var mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(AutoOrganize)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.AutoOrganize.ToStringFast(),
                value => data.AutoOrganize = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(CarryChest)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CarryChest.ToStringFast(),
                value => data.CarryChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_CarryChest_Name,
                I18n.Config_CarryChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.CarryChest));
        }

        if (!main && storage is IStorageObject storageObject)
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(ChestMenuTabs)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.ChestMenuTabs.ToStringFast(),
                value => data.ChestMenuTabs = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(CollectItems)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CollectItems.ToStringFast(),
                value => data.CollectItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(CraftFromChest)}", modList));
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
                    >= 2 when data.CraftFromChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location + (int)Math.Ceiling(Math.Log2(data.CraftFromChestDistance)) - 1,
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
                        >= (int)FeatureOptionRange.Location => (int)Math.Pow(2, 1 + value - (int)FeatureOptionRange.Location),
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(BetterColorPicker)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.CustomColorPicker.ToStringFast(),
                value => data.CustomColorPicker = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(FilterItems)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.FilterItems.ToStringFast(),
                value => data.FilterItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_FilterItems_Name,
                I18n.Config_FilterItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.FilterItems));
        }

        // Label Chest
        if (IntegrationHelper.TestConflicts(nameof(LabelChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(LabelChest)}", modList));
        }
        else if (main)
        {
            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.LabelChest,
                value => this.Config.LabelChest = value,
                I18n.Config_LabelChest_Name,
                I18n.Config_LabelChest_Tooltip,
                nameof(ModConfig.LabelChest));
        }

        // Open Held Chest
        if (IntegrationHelper.TestConflicts(nameof(OpenHeldChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(OpenHeldChest)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OpenHeldChest.ToStringFast(),
                value => data.OpenHeldChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
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
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(OrganizeChest)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.OrganizeChest.ToStringFast(),
                value => data.OrganizeChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_OrganizeChest_Name,
                I18n.Config_OrganizeChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.OrganizeChest));
        }

        // Resize Chest
        if (IntegrationHelper.TestConflicts(nameof(ResizeChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(ResizeChest)}", modList));
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
                nameof(IStorageData.ResizeChest));
        }

        // Resize Chest Menu
        if (IntegrationHelper.TestConflicts(nameof(ResizeChestMenu), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(ResizeChestMenu)}", modList));
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
                nameof(IStorageData.ResizeChestMenu));
        }

        // Search Items
        if (IntegrationHelper.TestConflicts(nameof(SearchItems), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(SearchItems)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.SearchItems.ToStringFast(),
                value => data.SearchItems = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_SearchItems_Name,
                I18n.Config_SearchItems_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.SearchItems));
        }

        // Slot Lock
        if (IntegrationHelper.TestConflicts(nameof(SlotLock), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(SlotLock)}", modList));
        }
        else if (main)
        {
            IntegrationHelper.GMCM.API.AddBoolOption(
                manifest,
                () => this.Config.SlotLock,
                value => this.Config.SlotLock = value,
                I18n.Config_SlotLock_Name,
                I18n.Config_SlotLock_Tooltip,
                nameof(ModConfig.SlotLock));
        }

        // Stash To Chest
        if (IntegrationHelper.TestConflicts(nameof(StashToChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(StashToChest)}", modList));
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
                    >= 2 when data.StashToChest is FeatureOptionRange.Location => (int)FeatureOptionRange.Location + (int)Math.Ceiling(Math.Log2(data.StashToChestDistance)) - 1,
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
                        >= (int)FeatureOptionRange.Location => (int)Math.Pow(2, 1 + value - (int)FeatureOptionRange.Location),
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

        // Unload Chest
        if (IntegrationHelper.TestConflicts(nameof(UnloadChest), out mods))
        {
            var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
            IntegrationHelper.GMCM.API.AddParagraph(manifest, () => string.Format(I18n.Warn_Incompatibility_Disabled(), $"BetterChests.{nameof(UnloadChest)}", modList));
        }
        else
        {
            IntegrationHelper.GMCM.API.AddTextOption(
                manifest,
                () => data.UnloadChest.ToStringFast(),
                value => data.UnloadChest = FeatureOptionExtensions.TryParse(value, out var option) ? option : FeatureOption.Default,
                I18n.Config_UnloadChest_Name,
                I18n.Config_UnloadChest_Tooltip,
                FeatureOptionExtensions.GetNames(),
                FormatHelper.FormatOption,
                nameof(IStorageData.UnloadChest));
        }

        if (!main)
        {
            IntegrationHelper.GMCM.API.OpenModMenu(manifest);
        }
    }
}