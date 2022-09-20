namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Interfaces;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc cref="StardewMods.Common.Integrations.BetterChests.IStorageData" />
internal class StorageNodeData : IStorageData, IStorageNode
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageNodeData" /> class.
    /// </summary>
    /// <param name="data">Data for this type of storage.</param>
    /// <param name="parent">Default data for any storage.</param>
    public StorageNodeData(IStorageData data, IStorageData parent)
    {
        this.Data = data;
        this.Parent = parent;
    }

    /// <inheritdoc />
    public virtual FeatureOption AutoOrganize
    {
        get => this.Data.AutoOrganize switch
        {
            _ when this.Parent.AutoOrganize is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.AutoOrganize is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.AutoOrganize,
            _ => this.Data.AutoOrganize,
        };
        set => this.Data.AutoOrganize = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption CarryChest
    {
        get => this.Data.CarryChest switch
        {
            _ when this.Parent.CarryChest is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.CarryChest is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.CarryChest,
            _ => this.Data.CarryChest,
        };
        set => this.Data.CarryChest = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption CarryChestSlow
    {
        get => this.Data.CarryChestSlow switch
        {
            _ when this.Parent.CarryChestSlow is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.CarryChestSlow is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.CarryChestSlow,
            _ => this.Data.CarryChestSlow,
        };
        set => this.Data.CarryChestSlow = value;
    }

    /// <inheritdoc />
    public FeatureOption ChestInfo
    {
        get => this.Data.ChestInfo switch
        {
            _ when this.Parent.ChestInfo is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.ChestInfo is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.ChestInfo,
            _ => this.Data.ChestInfo,
        };
        set => this.Data.ChestInfo = value;
    }

    /// <inheritdoc />
    public virtual string ChestLabel
    {
        get => this.Data.ChestLabel;
        set => this.Data.ChestLabel = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption ChestMenuTabs
    {
        get => this.Data.ChestMenuTabs switch
        {
            _ when this.Parent.ChestMenuTabs is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.ChestMenuTabs is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.ChestMenuTabs,
            _ => this.Data.ChestMenuTabs,
        };
        set => this.Data.ChestMenuTabs = value;
    }

    /// <inheritdoc />
    public virtual HashSet<string> ChestMenuTabSet
    {
        get => this.Data.ChestMenuTabSet.Any() ? this.Data.ChestMenuTabSet : this.Parent.ChestMenuTabSet;
        set => this.Data.ChestMenuTabSet = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption CollectItems
    {
        get => this.Data.CollectItems switch
        {
            _ when this.Parent.CollectItems is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.CollectItems is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.CollectItems,
            _ => this.Data.CollectItems,
        };
        set => this.Data.CollectItems = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption Configurator
    {
        get => this.Data.Configurator switch
        {
            _ when this.Parent.Configurator is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.Configurator is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.Configurator,
            _ => this.Data.Configurator,
        };
        set => this.Data.Configurator = value;
    }

    /// <inheritdoc />
    public InGameMenu ConfigureMenu
    {
        get => this.Data.ConfigureMenu switch
        {
            InGameMenu.Default when this.Parent.ConfigureMenu is InGameMenu.Default => InGameMenu.Simple,
            InGameMenu.Default => this.Parent.ConfigureMenu,
            _ => this.Data.ConfigureMenu,
        };
        set => this.Data.ConfigureMenu = value;
    }

    /// <inheritdoc />
    public virtual FeatureOptionRange CraftFromChest
    {
        get => this.Data.CraftFromChest switch
        {
            _ when this.Parent.CraftFromChest is FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            FeatureOptionRange.Default when this.Parent.CraftFromChest is FeatureOptionRange.Default =>
                FeatureOptionRange.Disabled,
            FeatureOptionRange.Default => this.Parent.CraftFromChest,
            _ => this.Data.CraftFromChest,
        };
        set => this.Data.CraftFromChest = value;
    }

    /// <inheritdoc />
    public virtual HashSet<string> CraftFromChestDisableLocations
    {
        get => this.Data.CraftFromChestDisableLocations.Any()
            ? this.Data.CraftFromChestDisableLocations
            : this.Parent.CraftFromChestDisableLocations;
        set => this.Data.CraftFromChestDisableLocations = value;
    }

    /// <inheritdoc />
    public virtual int CraftFromChestDistance
    {
        get => this.Data.CraftFromChestDistance != 0
            ? this.Data.CraftFromChestDistance
            : this.Parent.CraftFromChestDistance;
        set => this.Data.CraftFromChestDistance = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption CustomColorPicker
    {
        get => this.Data.CustomColorPicker switch
        {
            _ when this.Parent.CustomColorPicker is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.CustomColorPicker is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.CustomColorPicker,
            _ => this.Data.CustomColorPicker,
        };
        set => this.Data.CustomColorPicker = value;
    }

    /// <inheritdoc />
    public IStorageData Data { get; set; }

    /// <inheritdoc />
    public virtual FeatureOption FilterItems
    {
        get => this.Data.FilterItems switch
        {
            _ when this.Parent.FilterItems is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.FilterItems is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.FilterItems,
            _ => this.Data.FilterItems,
        };
        set => this.Data.FilterItems = value;
    }

    /// <inheritdoc />
    public virtual HashSet<string> FilterItemsList
    {
        get => this.Data.FilterItemsList.Any() ? this.Data.FilterItemsList : this.Parent.FilterItemsList;
        set => this.Data.FilterItemsList = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption HideItems
    {
        get => this.Data.HideItems switch
        {
            _ when this.Parent.HideItems is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.HideItems is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.HideItems,
            _ => this.Data.HideItems,
        };
        set => this.Data.HideItems = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption LabelChest
    {
        get => this.Data.LabelChest switch
        {
            _ when this.Parent.LabelChest is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.LabelChest is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.LabelChest,
            _ => this.Data.LabelChest,
        };
        set => this.Data.LabelChest = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption OpenHeldChest
    {
        get => this.Data.OpenHeldChest switch
        {
            _ when this.Parent.OpenHeldChest is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.OpenHeldChest is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.OpenHeldChest,
            _ => this.Data.OpenHeldChest,
        };
        set => this.Data.OpenHeldChest = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption OrganizeChest
    {
        get => this.Data.OrganizeChest switch
        {
            _ when this.Parent.OrganizeChest is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.OrganizeChest is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.OrganizeChest,
            _ => this.Data.OrganizeChest,
        };
        set => this.Data.OrganizeChest = value;
    }

    /// <inheritdoc />
    public virtual GroupBy OrganizeChestGroupBy
    {
        get => this.Data.OrganizeChestGroupBy switch
        {
            GroupBy.Default => this.Parent.OrganizeChestGroupBy,
            _ => this.Data.OrganizeChestGroupBy,
        };
        set => this.Data.OrganizeChestGroupBy = value;
    }

    /// <inheritdoc />
    public virtual SortBy OrganizeChestSortBy
    {
        get => this.Data.OrganizeChestSortBy switch
        {
            SortBy.Default => this.Parent.OrganizeChestSortBy,
            _ => this.Data.OrganizeChestSortBy,
        };
        set => this.Data.OrganizeChestSortBy = value;
    }

    /// <inheritdoc />
    public IStorageData Parent { get; set; }

    /// <inheritdoc />
    public virtual FeatureOption ResizeChest
    {
        get => this.Data.ResizeChest switch
        {
            _ when this.Parent.ResizeChest is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.ResizeChest is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.ResizeChest,
            _ => this.Data.ResizeChest,
        };
        set => this.Data.ResizeChest = value;
    }

    /// <inheritdoc />
    public virtual int ResizeChestCapacity
    {
        get => this.Data.ResizeChestCapacity != 0 ? this.Data.ResizeChestCapacity : this.Parent.ResizeChestCapacity;
        set => this.Data.ResizeChestCapacity = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption ResizeChestMenu
    {
        get => this.Data.ResizeChestMenu switch
        {
            _ when this.Parent.ResizeChestMenu is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.ResizeChestMenu is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.ResizeChestMenu,
            _ => this.Data.ResizeChestMenu,
        };
        set => this.Data.ResizeChestMenu = value;
    }

    /// <inheritdoc />
    public virtual int ResizeChestMenuRows
    {
        get => this.Data.ResizeChestMenuRows != 0 ? this.Data.ResizeChestMenuRows : this.Parent.ResizeChestMenuRows;
        set => this.Data.ResizeChestMenuRows = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption SearchItems
    {
        get => this.Data.SearchItems switch
        {
            _ when this.Parent.SearchItems is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.SearchItems is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.SearchItems,
            _ => this.Data.SearchItems,
        };
        set => this.Data.SearchItems = value;
    }

    /// <inheritdoc />
    public virtual FeatureOptionRange StashToChest
    {
        get => this.Data.StashToChest switch
        {
            _ when this.Parent.StashToChest is FeatureOptionRange.Disabled => FeatureOptionRange.Disabled,
            FeatureOptionRange.Default when this.Parent.StashToChest is FeatureOptionRange.Default => FeatureOptionRange
                .Disabled,
            FeatureOptionRange.Default => this.Parent.StashToChest,
            _ => this.Data.StashToChest,
        };
        set => this.Data.StashToChest = value;
    }

    /// <inheritdoc />
    public virtual HashSet<string> StashToChestDisableLocations
    {
        get => this.Data.StashToChestDisableLocations.Any()
            ? this.Data.StashToChestDisableLocations
            : this.Parent.StashToChestDisableLocations;
        set => this.Data.StashToChestDisableLocations = value;
    }

    /// <inheritdoc />
    public virtual int StashToChestDistance
    {
        get => this.Data.StashToChestDistance != 0 ? this.Data.StashToChestDistance : this.Parent.StashToChestDistance;
        set => this.Data.StashToChestDistance = value;
    }

    /// <inheritdoc />
    public virtual int StashToChestPriority
    {
        get => this.Data.StashToChestPriority != 0 ? this.Data.StashToChestPriority : this.Parent.StashToChestPriority;
        set => this.Data.StashToChestPriority = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption StashToChestStacks
    {
        get => this.Data.StashToChestStacks switch
        {
            _ when this.Parent.StashToChestStacks is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.StashToChestStacks is FeatureOption.Default =>
                FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.StashToChestStacks,
            _ => this.Data.StashToChestStacks,
        };
        set => this.Data.StashToChestStacks = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption TransferItems
    {
        get => this.Data.TransferItems switch
        {
            _ when this.Parent.TransferItems is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.TransferItems is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.TransferItems,
            _ => this.Data.TransferItems,
        };
        set => this.Data.TransferItems = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption UnloadChest
    {
        get => this.Data.UnloadChest switch
        {
            _ when this.Parent.UnloadChest is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.UnloadChest is FeatureOption.Default => FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.UnloadChest,
            _ => this.Data.UnloadChest,
        };
        set => this.Data.UnloadChest = value;
    }

    /// <inheritdoc />
    public virtual FeatureOption UnloadChestCombine
    {
        get => this.Data.UnloadChestCombine switch
        {
            _ when this.Parent.UnloadChestCombine is FeatureOption.Disabled => FeatureOption.Disabled,
            FeatureOption.Default when this.Parent.UnloadChestCombine is FeatureOption.Default =>
                FeatureOption.Disabled,
            FeatureOption.Default => this.Parent.UnloadChestCombine,
            _ => this.Data.UnloadChestCombine,
        };
        set => this.Data.UnloadChestCombine = value;
    }
}