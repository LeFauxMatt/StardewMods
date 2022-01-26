namespace BetterChests.Models;

using System;
using System.Collections.Generic;
using BetterChests.Enums;
using BetterChests.Features;
using BetterChests.Interfaces;
using FuryCore.Enums;
using FuryCore.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <summary>
/// Encapsulates a <see cref="ConfigData" /> wrapper class.
/// </summary>
internal class ConfigModel : IConfigModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigModel"/> class.
    /// </summary>
    /// <param name="configData">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public ConfigModel(IConfigData configData, IModHelper helper, IServiceLocator services)
    {
        this.Data = configData;
        this.Helper = helper;
        this.Services = services;
    }

    /// <summary>
    /// Gets string values allowed for <see cref="FeatureOptionRange" />.
    /// </summary>
    public static string[] RangeValues { get; } =
    {
        "Disabled",
        "Inventory",
        "Location",
        "World",
    };

    /// <summary>
    /// Gets string values allowed for <see cref="FeatureOptionRange" />.
    /// </summary>
    public static string[] AreaValues { get; } =
    {
        "Right",
        "Left",
    };

    // ****************************************************************************************
    // General

    /// <inheritdoc/>
    public int CraftFromChestDistance
    {
        get => this.Data.CraftFromChestDistance;
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc/>
    public ComponentArea CustomColorPickerArea
    {
        get => this.Data.CustomColorPickerArea;
        set => this.Data.CustomColorPickerArea = value;
    }

    /// <inheritdoc/>
    public bool FillStacks
    {
        get => this.Data.FillStacks;
        set => this.Data.FillStacks = value;
    }

    /// <inheritdoc/>
    public HashSet<string> FilterItemsList
    {
        get => this.Data.FilterItemsList;
        set
        {
            this.Data.FilterItemsList = value;
            this.Services.FindService<FilterItems>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public int ResizeChestCapacity
    {
        get => this.Data.ResizeChestCapacity;
        set
        {
            this.Data.ResizeChestMenuRows = value;
            this.Services.FindService<ResizeChestMenu>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public int ResizeChestMenuRows
    {
        get => this.Data.ResizeChestMenuRows;
        set
        {
            this.Data.ResizeChestMenuRows = value;
            this.Services.FindService<ResizeChestMenu>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public int StashToChestDistance
    {
        get => this.Data.StashToChestDistance;
        set => this.Data.StashToChestDistance = value;
    }

    /// <inheritdoc/>
    public char SearchTagSymbol
    {
        get => this.Data.SearchTagSymbol;
        set => this.Data.SearchTagSymbol = value;
    }

    /// <inheritdoc/>
    public string SearchTagSymbolString
    {
        get => this.Data.SearchTagSymbol.ToString();
        set => this.Data.SearchTagSymbol = string.IsNullOrWhiteSpace(value) ? '#' : value.ToCharArray()[0];
    }

    // ****************************************************************************************
    // Features

    /// <inheritdoc/>
    public FeatureOption CarryChest
    {
        get => this.Data.CarryChest;
        set
        {
            this.Data.CarryChest = value;
            this.Services.FindService<CarryChest>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption CategorizeChest
    {
        get => this.Data.CategorizeChest;
        set
        {
            this.Data.CategorizeChest = value;
            this.Services.FindService<CategorizeChest>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption ChestMenuTabs
    {
        get => this.Data.ChestMenuTabs;
        set
        {
            this.Data.ChestMenuTabs = value;
            this.Services.FindService<ChestMenuTabs>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption CollectItems
    {
        get => this.Data.CollectItems;
        set
        {
            this.Data.CollectItems = value;
            this.Services.FindService<CollectItems>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOptionRange CraftFromChest
    {
        get => this.Data.CraftFromChest;
        set
        {
            this.Data.CraftFromChest = value;
            this.Services.FindService<CraftFromChest>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption CustomColorPicker
    {
        get => this.Data.CustomColorPicker;
        set
        {
            this.Data.CustomColorPicker = value;
            this.Services.FindService<CustomColorPicker>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption FilterItems
    {
        get => this.Data.FilterItems;
        set
        {
            this.Data.FilterItems = value;
            this.Services.FindService<FilterItems>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption OpenHeldChest
    {
        get => this.Data.OpenHeldChest;
        set
        {
            this.Data.OpenHeldChest = value;
            this.Services.FindService<OpenHeldChest>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChest
    {
        get => this.Data.ResizeChest;
        set
        {
            this.Data.ResizeChest = value;
            this.Services.FindService<ResizeChest>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption ResizeChestMenu
    {
        get => this.Data.ResizeChestMenu;
        set
        {
            this.Data.ResizeChestMenu = value;
            this.Services.FindService<ResizeChestMenu>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOption SearchItems
    {
        get => this.Data.SearchItems;
        set
        {
            this.Data.SearchItems = value;
            this.Services.FindService<SearchItems>()?.Toggle();
        }
    }

    /// <inheritdoc/>
    public FeatureOptionRange StashToChest
    {
        get => this.Data.StashToChest;
        set
        {
            this.Data.StashToChest = value;
            this.Services.FindService<StashToChest>()?.Toggle();
        }
    }

    // ****************************************************************************************
    // Controls

    /// <inheritdoc/>
    public KeybindList OpenCrafting
    {
        get => this.Data.OpenCrafting;
        set => this.Data.OpenCrafting = value;
    }

    /// <inheritdoc/>
    public KeybindList StashItems
    {
        get => this.Data.StashItems;
        set => this.Data.StashItems = value;
    }

    /// <inheritdoc/>
    public KeybindList ScrollUp
    {
        get => this.Data.ScrollUp;
        set => this.Data.ScrollUp = value;
    }

    /// <inheritdoc/>
    public KeybindList ScrollDown
    {
        get => this.Data.ScrollDown;
        set => this.Data.ScrollDown = value;
    }

    /// <inheritdoc/>
    public KeybindList PreviousTab
    {
        get => this.Data.PreviousTab;
        set => this.Data.PreviousTab = value;
    }

    /// <inheritdoc/>
    public KeybindList NextTab
    {
        get => this.Data.NextTab;
        set => this.Data.NextTab = value;
    }

    // ****************************************************************************************
    // Other

    /// <inheritdoc/>
    public string CraftFromChestString
    {
        get => ConfigModel.GetRangeString(this.CraftFromChest);
        set => this.CraftFromChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : FeatureOptionRange.Location;
    }

    /// <inheritdoc/>
    public string CustomColorPickerAreaString
    {
        get => ConfigModel.GetAreaString(this.Data.CustomColorPickerArea);
        set => this.Data.CustomColorPickerArea = Enum.TryParse(value, out ComponentArea area) ? area : ComponentArea.Right;
    }

    /// <inheritdoc/>
    public string StashToChestString
    {
        get => ConfigModel.GetRangeString(this.StashToChest);
        set => this.StashToChest = Enum.TryParse(value, out FeatureOptionRange range) ? range : FeatureOptionRange.Location;
    }

    private IConfigData Data { get; }

    private IModHelper Helper { get; }

    private IServiceLocator Services { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string FormatRangeValue(string value)
    {
        if (!Enum.TryParse(value, out FeatureOptionRange option))
        {
            return value;
        }

        return option switch
        {
            FeatureOptionRange.Disabled => I18n.Option_Disabled_Name(),
            FeatureOptionRange.Inventory => I18n.Option_Inventory_Name(),
            FeatureOptionRange.Location => I18n.Option_Location_Name(),
            FeatureOptionRange.World => I18n.Option_World_Name(),
            _ => value,
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string FormatAreaValue(string value)
    {
        if (!Enum.TryParse(value, out ComponentArea area))
        {
            return value;
        }

        return area switch
        {
            ComponentArea.Top => I18n.Area_Top_Name(),
            ComponentArea.Right => I18n.Area_Right_Name(),
            ComponentArea.Bottom => I18n.Area_Bottom_Name(),
            ComponentArea.Left => I18n.Area_Left_Name(),
            ComponentArea.Custom => I18n.Area_Custom_Name(),
            _ => value,
        };
    }

    /// <inheritdoc/>
    public void Reset()
    {
        ((IConfigData)new ConfigData()).CopyConfigDataTo(this.Data);
    }

    /// <inheritdoc/>
    public void Save()
    {
        this.Helper.WriteConfig((ConfigData)this.Data);
    }

    private static string GetRangeString(FeatureOptionRange range)
    {
        return range switch
        {
            FeatureOptionRange.Default => "Default",
            FeatureOptionRange.Disabled => "Disabled",
            FeatureOptionRange.Inventory => "Inventory",
            FeatureOptionRange.Location => "Location",
            FeatureOptionRange.World => "World",
            _ => throw new ArgumentOutOfRangeException(nameof(range), range, null),
        };
    }

    private static string GetAreaString(ComponentArea area)
    {
        return area switch
        {
            ComponentArea.Top => "Top",
            ComponentArea.Right => "Right",
            ComponentArea.Bottom => "Bottom",
            ComponentArea.Left => "Left",
            ComponentArea.Custom => "Custom",
            _ => throw new ArgumentOutOfRangeException(nameof(area), area, null),
        };
    }
}