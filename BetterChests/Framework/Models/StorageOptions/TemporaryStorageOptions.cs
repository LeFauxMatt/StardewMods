namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal sealed class TemporaryStorageOptions : DefaultStorageOptions
{
    private static readonly IStorageOptions DefaultOptions = new DefaultStorageOptions();

    private readonly IStorageOptions storageOptions;

    /// <summary>Initializes a new instance of the <see cref="TemporaryStorageOptions" /> class.</summary>
    /// <param name="storageOptions">The storage options to copy.</param>
    public TemporaryStorageOptions(IStorageOptions storageOptions)
    {
        this.storageOptions = storageOptions;
        this.AutoOrganize = storageOptions.AutoOrganize;
        this.CarryChest = storageOptions.CarryChest;
        this.CategorizeChest = storageOptions.CategorizeChest;
        this.CategorizeChestTags = [..storageOptions.CategorizeChestTags];
        this.ChestFinder = storageOptions.ChestFinder;
        this.ChestInfo = storageOptions.ChestInfo;
        this.ChestLabel = storageOptions.ChestLabel;
        this.CollectItems = storageOptions.CollectItems;
        this.ConfigureChest = storageOptions.ConfigureChest;
        this.CraftFromChest = storageOptions.CraftFromChest;
        this.CraftFromChestDistance = storageOptions.CraftFromChestDistance;
        this.HslColorPicker = storageOptions.HslColorPicker;
        this.InventoryTabs = storageOptions.InventoryTabs;
        this.InventoryTabList = [..storageOptions.InventoryTabList];
        this.OpenHeldChest = storageOptions.OpenHeldChest;
        this.OrganizeItems = storageOptions.OrganizeItems;
        this.OrganizeItemsGroupBy = storageOptions.OrganizeItemsGroupBy;
        this.OrganizeItemsSortBy = storageOptions.OrganizeItemsSortBy;
        this.ResizeChest = storageOptions.ResizeChest;
        this.SearchItems = storageOptions.SearchItems;
        this.StashToChest = storageOptions.StashToChest;
        this.StashToChestDistance = storageOptions.StashToChestDistance;
        this.StashToChestPriority = storageOptions.StashToChestPriority;
        this.TransferItems = storageOptions.TransferItems;
        this.UnloadChest = storageOptions.UnloadChest;
    }

    /// <inheritdoc />
    public override string GetDisplayName() => this.storageOptions.GetDisplayName();

    /// <inheritdoc />
    public override string GetDescription() => this.storageOptions.GetDescription();

    /// <summary>Saves the options back to the default.</summary>
    public void Reset()
    {
        this.AutoOrganize = TemporaryStorageOptions.DefaultOptions.AutoOrganize;
        this.CarryChest = TemporaryStorageOptions.DefaultOptions.CarryChest;
        this.CategorizeChest = TemporaryStorageOptions.DefaultOptions.CategorizeChest;
        this.CategorizeChestTags = [..TemporaryStorageOptions.DefaultOptions.CategorizeChestTags];
        this.ChestFinder = TemporaryStorageOptions.DefaultOptions.ChestFinder;
        this.ChestInfo = TemporaryStorageOptions.DefaultOptions.ChestInfo;
        this.ChestLabel = TemporaryStorageOptions.DefaultOptions.ChestLabel;
        this.CollectItems = TemporaryStorageOptions.DefaultOptions.CollectItems;
        this.ConfigureChest = TemporaryStorageOptions.DefaultOptions.ConfigureChest;
        this.CraftFromChest = TemporaryStorageOptions.DefaultOptions.CraftFromChest;
        this.CraftFromChestDistance = TemporaryStorageOptions.DefaultOptions.CraftFromChestDistance;
        this.HslColorPicker = TemporaryStorageOptions.DefaultOptions.HslColorPicker;
        this.InventoryTabs = TemporaryStorageOptions.DefaultOptions.InventoryTabs;
        this.InventoryTabList = [..TemporaryStorageOptions.DefaultOptions.InventoryTabList];
        this.OpenHeldChest = TemporaryStorageOptions.DefaultOptions.OpenHeldChest;
        this.OrganizeItems = TemporaryStorageOptions.DefaultOptions.OrganizeItems;
        this.OrganizeItemsGroupBy = TemporaryStorageOptions.DefaultOptions.OrganizeItemsGroupBy;
        this.OrganizeItemsSortBy = TemporaryStorageOptions.DefaultOptions.OrganizeItemsSortBy;
        this.ResizeChest = TemporaryStorageOptions.DefaultOptions.ResizeChest;
        this.SearchItems = TemporaryStorageOptions.DefaultOptions.SearchItems;
        this.StashToChest = TemporaryStorageOptions.DefaultOptions.StashToChest;
        this.StashToChestDistance = TemporaryStorageOptions.DefaultOptions.StashToChestDistance;
        this.StashToChestPriority = TemporaryStorageOptions.DefaultOptions.StashToChestPriority;
        this.TransferItems = TemporaryStorageOptions.DefaultOptions.TransferItems;
        this.UnloadChest = TemporaryStorageOptions.DefaultOptions.UnloadChest;
    }

    /// <summary>Saves the changes back to storage options.</summary>
    public void Save()
    {
        this.storageOptions.AutoOrganize = this.AutoOrganize;
        this.storageOptions.CarryChest = this.CarryChest;
        this.storageOptions.CategorizeChest = this.CategorizeChest;
        this.storageOptions.CategorizeChestTags = [..this.CategorizeChestTags];
        this.storageOptions.ChestFinder = this.ChestFinder;
        this.storageOptions.ChestInfo = this.ChestInfo;
        this.storageOptions.ChestLabel = this.ChestLabel;
        this.storageOptions.CollectItems = this.CollectItems;
        this.storageOptions.ConfigureChest = this.ConfigureChest;
        this.storageOptions.CraftFromChest = this.CraftFromChest;
        this.storageOptions.CraftFromChestDistance = this.CraftFromChestDistance;
        this.storageOptions.HslColorPicker = this.HslColorPicker;
        this.storageOptions.InventoryTabs = this.InventoryTabs;
        this.storageOptions.InventoryTabList = [..this.InventoryTabList];
        this.storageOptions.OpenHeldChest = this.OpenHeldChest;
        this.storageOptions.OrganizeItems = this.OrganizeItems;
        this.storageOptions.OrganizeItemsGroupBy = this.OrganizeItemsGroupBy;
        this.storageOptions.OrganizeItemsSortBy = this.OrganizeItemsSortBy;
        this.storageOptions.ResizeChest = this.ResizeChest;
        this.storageOptions.SearchItems = this.SearchItems;
        this.storageOptions.StashToChest = this.StashToChest;
        this.storageOptions.StashToChestDistance = this.StashToChestDistance;
        this.storageOptions.StashToChestPriority = this.StashToChestPriority;
        this.storageOptions.TransferItems = this.TransferItems;
        this.storageOptions.UnloadChest = this.UnloadChest;
    }
}