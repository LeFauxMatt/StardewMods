namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
internal sealed class TemporaryStorageOptions : DefaultStorageOptions
{
    private readonly IStorageOptions defaultOptions;
    private readonly IStorageOptions storageOptions;

    /// <summary>Initializes a new instance of the <see cref="TemporaryStorageOptions" /> class.</summary>
    /// <param name="storageOptions">The storage options to copy.</param>
    /// <param name="defaultOptions">The default storage options.</param>
    public TemporaryStorageOptions(IStorageOptions storageOptions, IStorageOptions defaultOptions)
    {
        this.storageOptions = storageOptions;
        this.defaultOptions = defaultOptions;
        this.AutoOrganize = storageOptions.AutoOrganize;
        this.CarryChest = storageOptions.CarryChest;
        this.CategorizeChest = storageOptions.CategorizeChest;
        this.CategorizeChestAutomatically = storageOptions.CategorizeChestAutomatically;
        this.CategorizeChestMethod = storageOptions.CategorizeChestMethod;
        this.CategorizeChestTags = [..storageOptions.CategorizeChestTags];
        this.ChestFinder = storageOptions.ChestFinder;
        this.ChestInfo = storageOptions.ChestInfo;
        this.CollectItems = storageOptions.CollectItems;
        this.ConfigureChest = storageOptions.ConfigureChest;
        this.CraftFromChest = storageOptions.CraftFromChest;
        this.CraftFromChestDistance = storageOptions.CraftFromChestDistance;
        this.HslColorPicker = storageOptions.HslColorPicker;
        this.InventoryTabs = storageOptions.InventoryTabs;
        this.InventoryTabList = [..storageOptions.InventoryTabList];
        this.OpenHeldChest = storageOptions.OpenHeldChest;
        this.ResizeChest = storageOptions.ResizeChest;
        this.SearchItems = storageOptions.SearchItems;
        this.StashToChest = storageOptions.StashToChest;
        this.StashToChestDistance = storageOptions.StashToChestDistance;
        this.StashToChestPriority = storageOptions.StashToChestPriority;
    }

    /// <inheritdoc />
    public override string GetDisplayName() => this.storageOptions.GetDisplayName();

    /// <inheritdoc />
    public override string GetDescription() => this.storageOptions.GetDescription();

    /// <summary>Saves the options back to the default.</summary>
    public void Reset()
    {
        this.AutoOrganize = this.defaultOptions.AutoOrganize;
        this.CarryChest = this.defaultOptions.CarryChest;
        this.CategorizeChest = this.defaultOptions.CategorizeChest;
        this.CategorizeChestAutomatically = this.defaultOptions.CategorizeChestAutomatically;
        this.CategorizeChestMethod = this.defaultOptions.CategorizeChestMethod;
        this.CategorizeChestTags = [..this.defaultOptions.CategorizeChestTags];
        this.ChestFinder = this.defaultOptions.ChestFinder;
        this.ChestInfo = this.defaultOptions.ChestInfo;
        this.CollectItems = this.defaultOptions.CollectItems;
        this.ConfigureChest = this.defaultOptions.ConfigureChest;
        this.CraftFromChest = this.defaultOptions.CraftFromChest;
        this.CraftFromChestDistance = this.defaultOptions.CraftFromChestDistance;
        this.HslColorPicker = this.defaultOptions.HslColorPicker;
        this.InventoryTabs = this.defaultOptions.InventoryTabs;
        this.InventoryTabList = [..this.defaultOptions.InventoryTabList];
        this.OpenHeldChest = this.defaultOptions.OpenHeldChest;
        this.ResizeChest = this.defaultOptions.ResizeChest;
        this.SearchItems = this.defaultOptions.SearchItems;
        this.StashToChest = this.defaultOptions.StashToChest;
        this.StashToChestDistance = this.defaultOptions.StashToChestDistance;
        this.StashToChestPriority = this.defaultOptions.StashToChestPriority;
    }

    /// <summary>Saves the changes back to storage options.</summary>
    public void Save()
    {
        this.storageOptions.AutoOrganize = this.AutoOrganize;
        this.storageOptions.CarryChest = this.CarryChest;
        this.storageOptions.CategorizeChest = this.CategorizeChest;
        this.storageOptions.CategorizeChestAutomatically = this.CategorizeChestAutomatically;
        this.storageOptions.CategorizeChestMethod = this.CategorizeChestMethod;
        this.storageOptions.CategorizeChestTags = [..this.CategorizeChestTags];
        this.storageOptions.ChestFinder = this.ChestFinder;
        this.storageOptions.ChestInfo = this.ChestInfo;
        this.storageOptions.CollectItems = this.CollectItems;
        this.storageOptions.ConfigureChest = this.ConfigureChest;
        this.storageOptions.CraftFromChest = this.CraftFromChest;
        this.storageOptions.CraftFromChestDistance = this.CraftFromChestDistance;
        this.storageOptions.HslColorPicker = this.HslColorPicker;
        this.storageOptions.InventoryTabs = this.InventoryTabs;
        this.storageOptions.InventoryTabList = [..this.InventoryTabList];
        this.storageOptions.OpenHeldChest = this.OpenHeldChest;
        this.storageOptions.ResizeChest = this.ResizeChest;
        this.storageOptions.SearchItems = this.SearchItems;
        this.storageOptions.StashToChest = this.StashToChest;
        this.storageOptions.StashToChestDistance = this.StashToChestDistance;
        this.storageOptions.StashToChestPriority = this.StashToChestPriority;
    }
}