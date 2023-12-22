// namespace StardewMods.BetterChests.Framework.Services;
//
// using System.Text;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework.Input;
// using StardewMods.BetterChests.Framework.Enums;
// using StardewMods.BetterChests.Framework.Interfaces;
// using StardewMods.BetterChests.Framework.Services.Features;
// using StardewMods.BetterChests.Framework.UI;
// using StardewMods.Common.Enums;
// using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
// using StardewValley.Menus;
// using HslColorPicker = StardewMods.BetterChests.Framework.Services.Features.HslColorPicker;
//
// /// <summary>Handles config options.</summary>
// internal sealed class ConfigMenu
// {
//     private readonly ModConfig config;
//     private readonly IEnumerable<IFeature> features;
//     private readonly GenericModConfigMenuIntegration gmcm;
//     private readonly IModHelper helper;
//     private readonly IInputHelper input;
//     private readonly IManifest manifest;
//     private readonly IMonitor monitor;
//     private readonly ITranslationHelper translation;
//
//     /// <summary>Initializes a new instance of the <see cref="ConfigMenu" /> class.</summary>
//     /// <param name="monitor">Dependency used for monitoring and logging.</param>
//     /// <param name="config">Dependency used for accessing config data.</param>
//     /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
//     /// <param name="helper">Dependency for events, input, and content.</param>
//     /// <param name="input">Dependency used for checking and changing input state.</param>
//     /// <param name="manifest">Dependency for accessing mod manifest.</param>
//     /// <param name="translation">Dependency used for accessing translations.</param>
//     /// <param name="features">Dependency for managing features.</param>
//     public ConfigMenu(
//         IMonitor monitor,
//         ModConfig config,
//         GenericModConfigMenuIntegration gmcm,
//         IModHelper helper,
//         IInputHelper input,
//         IManifest manifest,
//         ITranslationHelper translation,
//         IEnumerable<IFeature> features)
//     {
//         this.helper = helper;
//         this.input = input;
//         this.manifest = manifest;
//         this.translation = translation;
//         this.monitor = monitor;
//         this.config = config;
//         this.gmcm = gmcm;
//         this.features = features;
//
//         if (this.gmcm.IsLoaded)
//         {
//             this.SetupMainConfig();
//         }
//     }
//
//     /// <summary>Sets up the main config menu.</summary>
//     public void SetupMainConfig()
//     {
//         if (!this.gmcm.IsLoaded || this.gmcm.Api is null)
//         {
//             return;
//         }
//
//         if (this.gmcm.IsRegistered(this.manifest))
//         {
//             this.gmcm.Unregister(this.manifest);
//         }
//
//         this.gmcm.Register(this.manifest, this.ResetConfig, this.SaveConfig);
//
//         // General
//         this.gmcm.Api.AddSectionTitle(this.manifest, I18n.Section_General_Name);
//         this.gmcm.Api.AddParagraph(this.manifest, I18n.Section_General_Description);
//
//         this.gmcm.Api.AddNumberOption(
//             this.manifest,
//             () => this.config.CarryChestLimit,
//             value => this.config.CarryChestLimit = value,
//             I18n.Config_CarryChestLimit_Name,
//             I18n.Config_CarryChestLimit_Tooltip);
//
//         this.gmcm.Api.AddNumberOption(
//             this.manifest,
//             () => this.config.CarryChestSlowAmount,
//             value => this.config.CarryChestSlowAmount = value,
//             I18n.Config_CarryChestSlow_Name,
//             I18n.Config_CarryChestSlow_Tooltip,
//             0,
//             4,
//             1,
//             Formatting.CarryChestSlow);
//
//         this.gmcm.Api.AddBoolOption(
//             this.manifest,
//             () => this.config.ChestFinder,
//             value => this.config.ChestFinder = value,
//             I18n.Config_ChestFinder_Name,
//             I18n.Config_ChestFinder_Tooltip);
//
//         // Craft From Workbench
//         if (this.config.Default.ConfigureMenu == InGameMenu.Advanced)
//         {
//             this.gmcm.Api.AddTextOption(
//                 this.manifest,
//                 () => this.config.CraftFromWorkbench.ToStringFast(),
//                 value => this.config.CraftFromWorkbench = FeatureOptionRangeExtensions.TryParse(value, out var range)
//                     ? range
//                     : FeatureOptionRange.Default,
//                 I18n.Config_CraftFromWorkbench_Name,
//                 I18n.Config_CraftFromWorkbench_Tooltip,
//                 FeatureOptionRangeExtensions.GetNames(),
//                 Formatting.Range);
//
//             this.gmcm.Api.AddNumberOption(
//                 this.manifest,
//                 () => this.config.CraftFromWorkbenchDistance,
//                 value => this.config.CraftFromWorkbenchDistance = value,
//                 I18n.Config_CraftFromWorkbenchDistance_Name,
//                 I18n.Config_CraftFromWorkbenchDistance_Tooltip);
//         }
//         else
//         {
//             this.gmcm.Api.AddNumberOption(
//                 this.manifest,
//                 () => this.config.CraftFromWorkbenchDistance switch
//                 {
//                     _ when this.config.CraftFromWorkbench is FeatureOptionRange.Default => (int)FeatureOptionRange
//                         .Default,
//                     _ when this.config.CraftFromWorkbench is FeatureOptionRange.Disabled => (int)FeatureOptionRange
//                         .Disabled,
//                     _ when this.config.CraftFromWorkbench is FeatureOptionRange.Inventory => (int)FeatureOptionRange
//                         .Inventory,
//                     _ when this.config.CraftFromWorkbench is FeatureOptionRange.World => (int)FeatureOptionRange.World,
//                     >= 2 when this.config.CraftFromWorkbench is FeatureOptionRange.Location => (
//                             (int)FeatureOptionRange.Location
//                             + (int)Math.Ceiling(Math.Log2(this.config.CraftFromWorkbenchDistance)))
//                         - 1,
//                     _ when this.config.CraftFromWorkbench is FeatureOptionRange.Location => (int)FeatureOptionRange
//                             .World
//                         - 1,
//                     _ => (int)FeatureOptionRange.Default,
//                 },
//                 value =>
//                 {
//                     this.config.CraftFromWorkbenchDistance = value switch
//                     {
//                         (int)FeatureOptionRange.Default => 0,
//                         (int)FeatureOptionRange.Disabled => 0,
//                         (int)FeatureOptionRange.Inventory => 0,
//                         (int)FeatureOptionRange.World => 0,
//                         (int)FeatureOptionRange.World - 1 => -1,
//                         >= (int)FeatureOptionRange.Location => (int)Math.Pow(
//                             2,
//                             (1 + value) - (int)FeatureOptionRange.Location),
//                         _ => 0,
//                     };
//
//                     this.config.CraftFromWorkbench = value switch
//                     {
//                         (int)FeatureOptionRange.Default => FeatureOptionRange.Default,
//                         (int)FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
//                         (int)FeatureOptionRange.Inventory => FeatureOptionRange.Inventory,
//                         (int)FeatureOptionRange.World => FeatureOptionRange.World,
//                         (int)FeatureOptionRange.World - 1 => FeatureOptionRange.Location,
//                         _ => FeatureOptionRange.Location,
//                     };
//                 },
//                 I18n.Config_CraftFromWorkbenchDistance_Name,
//                 I18n.Config_CraftFromWorkbenchDistance_Tooltip,
//                 (int)FeatureOptionRange.Default,
//                 (int)FeatureOptionRange.World,
//                 1,
//                 Formatting.Distance);
//         }
//
//         this.gmcm.Api.AddTextOption(
//             this.manifest,
//             () => this.config.CustomColorPickerArea.ToStringFast(),
//             value => this.config.CustomColorPickerArea =
//                 ComponentAreaExtensions.TryParse(value, out var area) ? area : ComponentArea.Right,
//             I18n.Config_CustomColorPickerArea_Name,
//             I18n.Config_CustomColorPickerArea_Tooltip,
//             new[] { ComponentArea.Left.ToStringFast(), ComponentArea.Right.ToStringFast() },
//             Formatting.Area);
//
//         this.gmcm.Api.AddTextOption(
//             this.manifest,
//             () => this.config.SearchTagSymbol.ToString(),
//             value => this.config.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0],
//             I18n.Config_SearchItemsSymbol_Name,
//             I18n.Config_SearchItemsSymbol_Tooltip);
//
//         if (IntegrationsManager.TestConflicts(nameof(SlotLock), out var mods))
//         {
//             var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
//             this.gmcm.Api.AddParagraph(
//                 this.manifest,
//                 () => I18n.Warn_Incompatibility_Disabled($"BetterChests.{nameof(SlotLock)}", modList));
//         }
//         else
//         {
//             this.gmcm.Api.AddBoolOption(
//                 this.manifest,
//                 () => this.config.SlotLock,
//                 value => this.config.SlotLock = value,
//                 I18n.Config_SlotLock_Name,
//                 I18n.Config_SlotLock_Tooltip);
//
//             this.gmcm.Api.AddTextOption(
//                 this.manifest,
//                 () => this.config.SlotLockColor,
//                 value => this.config.SlotLockColor = value,
//                 I18n.Config_SlotLockColor_Name,
//                 I18n.Config_SlotLockColor_Tooltip);
//
//             this.gmcm.Api.AddBoolOption(
//                 this.manifest,
//                 () => this.config.SlotLockHold,
//                 value => this.config.SlotLockHold = value,
//                 I18n.Config_SlotLockHold_Name,
//                 I18n.Config_SlotLockHold_Tooltip);
//         }
//
//         this.gmcm.Api.AddBoolOption(
//             this.manifest,
//             () => this.config.Experimental,
//             value => this.config.Experimental = value,
//             I18n.Config_Experimental_Name,
//             I18n.Config_Experimental_Tooltip);
//
//         // Controls
//         this.gmcm.Api.AddSectionTitle(this.manifest, I18n.Section_Controls_Name);
//         this.gmcm.Api.AddParagraph(this.manifest, I18n.Section_Controls_Description);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.FindChest,
//             value => this.config.Controls.FindChest = value,
//             I18n.Config_FindChest_Name,
//             I18n.Config_FindChest_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.CloseChestFinder,
//             value => this.config.Controls.CloseChestFinder = value,
//             I18n.Config_CloseChestFinder_Name,
//             I18n.Config_CloseChestFinder_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.OpenFoundChest,
//             value => this.config.Controls.OpenFoundChest = value,
//             I18n.Config_OpenFoundChest_Name,
//             I18n.Config_OpenFoundChest_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.OpenNextChest,
//             value => this.config.Controls.OpenNextChest = value,
//             I18n.Config_OpenNextChest_Name,
//             I18n.Config_OpenNextChest_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.OpenCrafting,
//             value => this.config.Controls.OpenCrafting = value,
//             I18n.Config_OpenCrafting_Name,
//             I18n.Config_OpenCrafting_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.StashItems,
//             value => this.config.Controls.StashItems = value,
//             I18n.Config_StashItems_Name,
//             I18n.Config_StashItems_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.Configure,
//             value => this.config.Controls.Configure = value,
//             I18n.Config_Configure_Name,
//             I18n.Config_Configure_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.PreviousTab,
//             value => this.config.Controls.PreviousTab = value,
//             I18n.Config_PreviousTab_Name,
//             I18n.Config_PreviousTab_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.NextTab,
//             value => this.config.Controls.NextTab = value,
//             I18n.Config_NextTab_Name,
//             I18n.Config_NextTab_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.ScrollUp,
//             value => this.config.Controls.ScrollUp = value,
//             I18n.Config_ScrollUp_Name,
//             I18n.Config_ScrollUp_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.ScrollDown,
//             value => this.config.Controls.ScrollDown = value,
//             I18n.Config_ScrollDown_Name,
//             I18n.Config_ScrollDown_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.ScrollPage,
//             value => this.config.Controls.ScrollPage = value,
//             I18n.Config_ScrollPage_Name,
//             I18n.Config_ScrollPage_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.LockSlot,
//             value => this.config.Controls.LockSlot = value,
//             I18n.Config_LockSlot_Name,
//             I18n.Config_LockSlot_Tooltip);
//
//         this.gmcm.Api.AddKeybindList(
//             this.manifest,
//             () => this.config.Controls.ToggleInfo,
//             value => this.config.Controls.ToggleInfo = value,
//             I18n.Config_ToggleInfo_Name,
//             I18n.Config_ToggleInfo_Tooltip);
//
//         // Default Chest
//         this.gmcm.Api.AddSectionTitle(this.manifest, I18n.Storage_Default_Name);
//         this.gmcm.Api.AddParagraph(this.manifest, I18n.Storage_Default_Tooltip);
//
//         this.SetupStorageConfig(this.manifest, this.config);
//
//         // Chest Types
//         this.gmcm.Api.AddSectionTitle(this.manifest, I18n.Section_Chests_Name);
//         this.gmcm.Api.AddParagraph(this.manifest, I18n.Section_Chests_Description);
//
//         foreach (var (key, _) in this.config.VanillaStorages.OrderBy(kvp => Formatting.StorageName(kvp.Key)))
//         {
//             this.gmcm.Api.AddPageLink(
//                 this.manifest,
//                 key,
//                 () => Formatting.StorageName(key),
//                 () => Formatting.StorageTooltip(key));
//         }
//
//         // Other Chests
//         foreach (var (key, value) in this.config.VanillaStorages)
//         {
//             this.gmcm.Api.AddPage(this.manifest, key, () => Formatting.StorageName(key));
//             this.SetupStorageConfig(this.manifest, value);
//         }
//     }
//
//     /// <summary>Sets up a config menu for a specific storage.</summary>
//     /// <param name="modManifest">A manifest to describe the mod.</param>
//     /// <param name="storage">The storage to configure for.</param>
//     /// <param name="register">Indicates whether to register with GMCM.</param>
//     public void SetupSpecificConfig(IManifest modManifest, IStorageData storage, bool register = false)
//     {
//         if (!this.gmcm.IsLoaded)
//         {
//             return;
//         }
//
//         if (register)
//         {
//             if (this.gmcm.IsRegistered(modManifest))
//             {
//                 this.gmcm.Unregister(modManifest);
//             }
//
//             this.gmcm.Register(modManifest, this.ResetConfig, SaveSpecificConfig);
//         }
//
//         this.SetupStorageConfig(modManifest, storage, register);
//         return;
//
//         void SaveSpecificConfig()
//         {
//             var sb = new StringBuilder();
//             sb.AppendLine(" Configure Storage".PadLeft(50, '=')[^50..]);
//             if (storage is Storage storageObject)
//             {
//                 sb.AppendLine(storageObject.Info);
//             }
//
//             sb.AppendLine(storage.ToString());
//             this.monitor.Log(sb.ToString());
//         }
//     }
//
//     /// <summary>Shows the config menu.</summary>
//     /// <param name="modManifest">A manifest to describe the mod.</param>
//     public void ShowMenu(IManifest modManifest)
//     {
//         if (!this.gmcm.IsLoaded)
//         {
//             return;
//         }
//
//         this.gmcm.Api.OpenModMenu(modManifest);
//     }
//
//     private Action<SpriteBatch, Vector2> DrawButton(StorageNode storage, string label)
//     {
//         var dims = Game1.dialogueFont.MeasureString(label);
//         return (b, pos) =>
//         {
//             var bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X + Game1.tileSize, Game1.tileSize);
//             if (Game1.activeClickableMenu.GetChildMenu() is null)
//             {
//                 var point = Game1.getMousePosition();
//                 if (Game1.oldMouseState.LeftButton == ButtonState.Released
//                     && Mouse.GetState().LeftButton == ButtonState.Pressed
//                     && bounds.Contains(point))
//                 {
//                     Game1.activeClickableMenu.SetChildMenu(
//                         new ItemSelectionMenu(storage, storage.FilterMatcher, this.input, this.translation));
//
//                     return;
//                 }
//             }
//
//             IClickableMenu.drawTextureBox(
//                 b,
//                 Game1.mouseCursors,
//                 new(432, 439, 9, 9),
//                 bounds.X,
//                 bounds.Y,
//                 bounds.Width,
//                 bounds.Height,
//                 Color.White,
//                 Game1.pixelZoom,
//                 false,
//                 1f);
//
//             Utility.drawTextWithShadow(
//                 b,
//                 label,
//                 Game1.dialogueFont,
//                 new Vector2((bounds.Left + bounds.Right) - dims.X, (bounds.Top + bounds.Bottom) - dims.Y) / 2f,
//                 Game1.textColor,
//                 1f,
//                 1f,
//                 -1,
//                 -1,
//                 0f);
//         };
//     }
//
//     private void ResetConfig()
//     {
//         var defaultConfig = new ModConfig();
//
//         // Copy properties
//         this.config.CarryChestLimit = defaultConfig.CarryChestLimit;
//         this.config.CarryChestSlowAmount = defaultConfig.CarryChestSlowAmount;
//         this.config.ChestFinder = defaultConfig.ChestFinder;
//         this.config.CraftFromWorkbench = defaultConfig.CraftFromWorkbench;
//         this.config.CraftFromWorkbenchDistance = defaultConfig.CraftFromWorkbenchDistance;
//         this.config.CustomColorPickerArea = defaultConfig.CustomColorPickerArea;
//         this.config.Experimental = defaultConfig.Experimental;
//         this.config.SearchTagSymbol = defaultConfig.SearchTagSymbol;
//         this.config.SlotLock = defaultConfig.SlotLock;
//         this.config.SlotLockColor = defaultConfig.SlotLockColor;
//         this.config.SlotLockHold = defaultConfig.SlotLockHold;
//
//         // Copy controls
//         this.config.Controls.CloseChestFinder = defaultConfig.Controls.CloseChestFinder;
//         this.config.Controls.Configure = defaultConfig.Controls.Configure;
//         this.config.Controls.FindChest = defaultConfig.Controls.FindChest;
//         this.config.Controls.LockSlot = defaultConfig.Controls.LockSlot;
//         this.config.Controls.NextTab = defaultConfig.Controls.NextTab;
//         this.config.Controls.OpenCrafting = defaultConfig.Controls.OpenCrafting;
//         this.config.Controls.OpenFoundChest = defaultConfig.Controls.OpenFoundChest;
//         this.config.Controls.OpenNextChest = defaultConfig.Controls.OpenNextChest;
//         this.config.Controls.PreviousTab = defaultConfig.Controls.PreviousTab;
//         this.config.Controls.ScrollDown = defaultConfig.Controls.ScrollDown;
//         this.config.Controls.ScrollPage = defaultConfig.Controls.ScrollPage;
//         this.config.Controls.ScrollUp = defaultConfig.Controls.ScrollUp;
//         this.config.Controls.StashItems = defaultConfig.Controls.StashItems;
//         this.config.Controls.ToggleInfo = defaultConfig.Controls.ToggleInfo;
//
//         // Copy default storage
//         ((IStorageData)defaultConfig).CopyTo(this.config);
//
//         // Copy vanilla storages
//         var defaultStorage = new StorageData();
//         foreach (var (_, storage) in this.config.VanillaStorages)
//         {
//             ((IStorageData)defaultStorage).CopyTo(storage);
//         }
//     }
//
//     private void SaveConfig()
//     {
//         this.helper.WriteConfig(this.config);
//         foreach (var feature in this.features)
//         {
//             feature.SetActivated();
//         }
//
//         this.monitor.Log(this.config.ToString());
//     }
//
//     private void SetupFeatureConfig(string featureName, IManifest modManifest, IStorageData storage, bool inGame)
//     {
//         if (!this.gmcm.IsLoaded)
//         {
//             return;
//         }
//
//         switch (inGame)
//         {
//             // Do not add config options when in-game and feature is disabled
//             case true:
//                 switch (featureName)
//                 {
//                     case nameof(IStorageData.ChestLabel) when this.config.LabelChest is FeatureOption.Disabled:
//                     case nameof(AutoOrganize) when this.config.AutoOrganize is FeatureOption.Disabled:
//                     case nameof(CarryChest) when this.config.CarryChest is FeatureOption.Disabled:
//                     case nameof(ChestInfo) when this.config.ChestInfo is FeatureOption.Disabled:
//                     case nameof(InventoryTabs) when this.config.ChestMenuTabs is FeatureOption.Disabled:
//                     case nameof(CollectItems) when this.config.CollectItems is FeatureOption.Disabled:
//                     case nameof(ConfigureChest):
//                     case nameof(CraftFromChest) when this.config.CraftFromChest is FeatureOptionRange.Disabled:
//                     case nameof(HslColorPicker) when this.config.CustomColorPicker is FeatureOption.Disabled:
//                     case nameof(FilterItems) when this.config.FilterItems is FeatureOption.Disabled:
//                     case nameof(LabelChest) when this.config.LabelChest is FeatureOption.Disabled:
//                     case nameof(OpenHeldChest) when this.config.OpenHeldChest is FeatureOption.Disabled:
//                     case nameof(OrganizeChest) when this.config.OrganizeChest is FeatureOption.Disabled:
//                     case nameof(ResizeChest) when this.config.ResizeChest is FeatureOption.Disabled:
//                     case nameof(SearchItems) when this.config.SearchItems is FeatureOption.Disabled:
//                     case nameof(StashToChest) when this.config.StashToChest is FeatureOptionRange.Disabled:
//                     case nameof(TransferItems) when this.config.TransferItems is FeatureOption.Disabled:
//                     case nameof(UnloadChest) when this.config.UnloadChest is FeatureOption.Disabled:
//                         return;
//                 }
//
//                 break;
//
//             // Do not add config options when mod conflicts are detected
//             case false when IntegrationsManager.TestConflicts(featureName, out var mods):
//             {
//                 var modList = string.Join(", ", mods.OfType<IModInfo>().Select(mod => mod.Manifest.Name));
//                 this.gmcm.Api.AddParagraph(
//                     modManifest,
//                     () => I18n.Warn_Incompatibility_Disabled($"BetterChests.{featureName}", modList));
//
//                 return;
//             }
//         }
//
//         var data = storage switch
//         {
//             StorageNode storageNode => storageNode.Data,
//             StorageData storageData => storageData,
//             _ => storage,
//         };
//
//         switch (featureName)
//         {
//             case nameof(IStorageData.FilterItemsList) when storage is StorageNode storageNode:
//                 this.gmcm.Api.AddComplexOption(
//                     modManifest,
//                     I18n.Config_FilterItemsList_Name,
//                     this.DrawButton(storageNode, I18n.Button_Configure_Name()),
//                     I18n.Config_FilterItemsList_Tooltip,
//                     height: () => Game1.tileSize);
//
//                 return;
//
//             case nameof(IStorageData.ChestLabel) when data is Storage:
//                 this.gmcm.Api.AddTextOption(
//                     modManifest,
//                     () => data.ChestLabel,
//                     value => data.ChestLabel = value,
//                     I18n.Config_ChestLabel_Name,
//                     I18n.Config_ChestLabel_Tooltip);
//
//                 return;
//
//             case nameof(AutoOrganize) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.AutoOrganize,
//                     value => data.AutoOrganize = value,
//                     I18n.Config_AutoOrganize_Name,
//                     I18n.Config_AutoOrganize_Tooltip);
//
//                 return;
//
//             case nameof(CarryChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.CarryChest,
//                     value => data.CarryChest = value,
//                     I18n.Config_CarryChest_Name,
//                     I18n.Config_CarryChest_Tooltip);
//
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.CarryChestSlow,
//                     value => data.CarryChestSlow = value,
//                     I18n.Config_CarryChestSlow_Name,
//                     I18n.Config_CarryChestSlow_Tooltip);
//
//                 return;
//
//             case nameof(ChestInfo) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.ChestInfo,
//                     value => data.ChestInfo = value,
//                     I18n.Config_ChestInfo_Name,
//                     I18n.Config_ChestInfo_Tooltip);
//
//                 return;
//
//             case nameof(InventoryTabs) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.ChestMenuTabs,
//                     value => data.ChestMenuTabs = value,
//                     I18n.Config_ChestMenuTabs_Name,
//                     I18n.Config_ChestMenuTabs_Tooltip);
//
//                 return;
//
//             case nameof(CollectItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.CollectItems,
//                     value => data.CollectItems = value,
//                     I18n.Config_CollectItems_Name,
//                     I18n.Config_CollectItems_Tooltip);
//
//                 return;
//
//             case nameof(ConfigureChest):
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.Configurator,
//                     value => data.Configurator = value,
//                     I18n.Config_Configure_Name,
//                     I18n.Config_Configure_Tooltip);
//
//                 this.gmcm.Api.AddTextOption(
//                     modManifest,
//                     () => data.ConfigureMenu.ToStringFast(),
//                     value => data.ConfigureMenu = InGameMenuExtensions.TryParse(value, out var menu)
//                         ? menu
//                         : InGameMenu.Default,
//                     I18n.Config_ConfigureMenu_Name,
//                     I18n.Config_ConfigureMenu_Tooltip,
//                     InGameMenuExtensions.GetNames(),
//                     Formatting.Menu);
//
//                 return;
//
//             case nameof(CraftFromChest) when storage.ConfigureMenu is InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOptionRange(
//                     modManifest,
//                     () => data.CraftFromChest,
//                     value => data.CraftFromChest = value,
//                     I18n.Config_CraftFromChest_Name,
//                     I18n.Config_CraftFromChest_Tooltip);
//
//                 this.gmcm.Api.AddNumberOption(
//                     modManifest,
//                     () => data.StashToChestDistance,
//                     value => data.StashToChestDistance = value,
//                     I18n.Config_CraftFromChestDistance_Name,
//                     I18n.Config_CraftFromChestDistance_Tooltip);
//
//                 return;
//
//             case nameof(CraftFromChest) when storage.ConfigureMenu is InGameMenu.Full:
//                 this.gmcm.AddDistanceOption(
//                     modManifest,
//                     data,
//                     featureName,
//                     I18n.Config_CraftFromChestDistance_Name,
//                     I18n.Config_CraftFromChestDistance_Tooltip);
//
//                 return;
//
//             case nameof(HslColorPicker) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.CustomColorPicker,
//                     value => data.CustomColorPicker = value,
//                     I18n.Config_CustomColorPicker_Name,
//                     I18n.Config_CustomColorPicker_Tooltip);
//
//                 return;
//
//             case nameof(FilterItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.FilterItems,
//                     value => data.FilterItems = value,
//                     I18n.Config_FilterItems_Name,
//                     I18n.Config_FilterItems_Tooltip);
//
//                 return;
//
//             case nameof(IStorageData.HideItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     this.manifest,
//                     () => data.HideItems,
//                     value => data.HideItems = value,
//                     I18n.Config_HideItems_Name,
//                     I18n.Config_HideItems_Tooltip);
//
//                 return;
//
//             case nameof(LabelChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     this.manifest,
//                     () => data.LabelChest,
//                     value => data.LabelChest = value,
//                     I18n.Config_LabelChest_Name,
//                     I18n.Config_LabelChest_Tooltip);
//
//                 return;
//
//             case nameof(OpenHeldChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.OpenHeldChest,
//                     value => data.OpenHeldChest = value,
//                     I18n.Config_OpenHeldChest_Name,
//                     I18n.Config_OpenHeldChest_Tooltip);
//
//                 return;
//
//             case nameof(OrganizeChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.OrganizeChest,
//                     value => data.OrganizeChest = value,
//                     I18n.Config_OrganizeChest_Name,
//                     I18n.Config_OrganizeChest_Tooltip);
//
//                 this.gmcm.Api.AddTextOption(
//                     modManifest,
//                     () => data.OrganizeChestGroupBy.ToStringFast(),
//                     value => data.OrganizeChestGroupBy =
//                         GroupByExtensions.TryParse(value, out var groupBy) ? groupBy : GroupBy.Default,
//                     I18n.Config_OrganizeChestGroupBy_Name,
//                     I18n.Config_OrganizeChestGroupBy_Tooltip,
//                     GroupByExtensions.GetNames(),
//                     Formatting.OrganizeGroupBy);
//
//                 this.gmcm.Api.AddTextOption(
//                     modManifest,
//                     () => data.OrganizeChestSortBy.ToStringFast(),
//                     value => data.OrganizeChestSortBy =
//                         SortByExtensions.TryParse(value, out var sortBy) ? sortBy : SortBy.Default,
//                     I18n.Config_OrganizeChestSortBy_Name,
//                     I18n.Config_OrganizeChestSortBy_Tooltip,
//                     SortByExtensions.GetNames(),
//                     Formatting.OrganizeSortBy);
//
//                 return;
//
//             case nameof(ResizeChest) when storage.ConfigureMenu is InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.ResizeChest,
//                     value => data.ResizeChest = value,
//                     I18n.Config_ResizeChest_Name,
//                     I18n.Config_ResizeChest_Tooltip);
//
//                 this.gmcm.Api.AddNumberOption(
//                     modManifest,
//                     () => data.ResizeChestCapacity,
//                     value => data.ResizeChestCapacity = value,
//                     I18n.Config_ResizeChestCapacity_Name,
//                     I18n.Config_ResizeChestCapacity_Tooltip);
//
//                 return;
//
//             case nameof(SearchItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.SearchItems,
//                     value => data.SearchItems = value,
//                     I18n.Config_SearchItems_Name,
//                     I18n.Config_SearchItems_Tooltip);
//
//                 return;
//
//             case nameof(StashToChest):
//                 if (storage.ConfigureMenu is InGameMenu.Advanced)
//                 {
//                     this.gmcm.AddFeatureOptionRange(
//                         modManifest,
//                         () => data.StashToChest,
//                         value => data.StashToChest = value,
//                         I18n.Config_StashToChest_Name,
//                         I18n.Config_StashToChest_Tooltip);
//
//                     this.gmcm.Api.AddNumberOption(
//                         modManifest,
//                         () => data.StashToChestDistance,
//                         value => data.StashToChestDistance = value,
//                         I18n.Config_StashToChestDistance_Name,
//                         I18n.Config_StashToChestDistance_Tooltip);
//                 }
//                 else
//                 {
//                     this.gmcm.AddDistanceOption(
//                         modManifest,
//                         data,
//                         featureName,
//                         I18n.Config_StashToChestDistance_Name,
//                         I18n.Config_StashToChestDistance_Tooltip);
//                 }
//
//                 this.gmcm.Api.AddNumberOption(
//                     modManifest,
//                     () => data.StashToChestPriority,
//                     value => data.StashToChestPriority = value,
//                     I18n.Config_StashToChestPriority_Name,
//                     I18n.Config_StashToChestPriority_Tooltip);
//
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.StashToChestStacks,
//                     value => data.StashToChestStacks = value,
//                     I18n.Config_StashToChestStacks_Name,
//                     I18n.Config_StashToChestStacks_Tooltip);
//
//                 return;
//
//             case nameof(TransferItems) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.TransferItems,
//                     value => data.TransferItems = value,
//                     I18n.Config_TransferItems_Name,
//                     I18n.Config_TransferItems_Tooltip);
//
//                 return;
//
//             case nameof(UnloadChest) when storage.ConfigureMenu is InGameMenu.Full or InGameMenu.Advanced:
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.UnloadChest,
//                     value => data.UnloadChest = value,
//                     I18n.Config_UnloadChest_Name,
//                     I18n.Config_UnloadChest_Tooltip);
//
//                 this.gmcm.AddFeatureOption(
//                     modManifest,
//                     () => data.UnloadChestCombine,
//                     value => data.UnloadChestCombine = value,
//                     I18n.Config_UnloadChestCombine_Name,
//                     I18n.Config_UnloadChestCombine_Tooltip);
//
//                 return;
//         }
//     }
//
//     private void SetupStorageConfig(IManifest modManifest, IStorageData storage, bool inGame = false)
//     {
//         this.SetupFeatureConfig(nameof(IStorageData.ChestLabel), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(IStorageData.FilterItemsList), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(AutoOrganize), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(CarryChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(ChestInfo), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(InventoryTabs), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(CollectItems), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(ConfigureChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(CraftFromChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(HslColorPicker), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(FilterItems), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(IStorageData.HideItems), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(LabelChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(OpenHeldChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(OrganizeChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(ResizeChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(SearchItems), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(StashToChest), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(TransferItems), modManifest, storage, inGame);
//         this.SetupFeatureConfig(nameof(UnloadChest), modManifest, storage, inGame);
//     }
// }


