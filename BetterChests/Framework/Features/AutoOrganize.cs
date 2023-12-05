namespace StardewMods.BetterChests.Framework.Features;

using System.Globalization;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;

/// <summary>Automatically organizes items between chests during sleep.</summary>
internal sealed class AutoOrganize : BaseFeature
{
    private readonly IModEvents events;

    /// <summary>Initializes a new instance of the <see cref="AutoOrganize" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    public AutoOrganize(IMonitor monitor, ModConfig config, IModEvents events)
        : base(monitor, nameof(AutoOrganize), () => config.AutoOrganize is not FeatureOption.Disabled) =>
        this.events = events;

    /// <inheritdoc />
    protected override void Activate() => this.events.GameLoop.DayEnding += this.OnDayEnding;

    /// <inheritdoc />
    protected override void Deactivate() => this.events.GameLoop.DayEnding -= this.OnDayEnding;

    private void OnDayEnding(object? sender, DayEndingEventArgs e) => this.OrganizeAll();

    private void OrganizeAll()
    {
        var storages = StorageService.All.ToArray();
        Array.Sort(storages);

        foreach (var fromStorage in storages)
        {
            this.OrganizeFrom(fromStorage, storages);
        }

        foreach (var storage in storages)
        {
            storage.OrganizeItems();
        }
    }

    private void OrganizeFrom(StorageNode fromStorage, StorageNode[] storages)
    {
        if (fromStorage is not
            {
                Data: Storage fromStorageObject,
                AutoOrganize: FeatureOption.Enabled,
            })
        {
            return;
        }

        for (var index = fromStorageObject.Inventory.Count - 1; index >= 0; --index)
        {
            this.OrganizeTo(fromStorage, storages, fromStorageObject, index);
        }
    }

    private void OrganizeTo(
        StorageNode fromStorage,
        IEnumerable<StorageNode> storages,
        Storage fromStorageObject,
        int index)
    {
        var item = fromStorageObject.Inventory[index];
        if (item is null)
        {
            return;
        }

        var stack = item.Stack;
        foreach (var toStorage in storages)
        {
            if (!this.TransferStack(fromStorage, toStorage, item, stack))
            {
                break;
            }
        }
    }

    private bool TransferStack(StorageNode fromStorage, StorageNode toStorage, Item item, int stack)
    {
        if (fromStorage == toStorage || fromStorage.StashToChestPriority >= toStorage.StashToChestPriority)
        {
            return true;
        }

        var tmp = toStorage.StashItem(this.Monitor, item);
        if (tmp is null)
        {
            this.Monitor.Log(
                $"AutoOrganize: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");

            fromStorage.RemoveItem(item);
            return false;
        }

        if (stack != item.Stack)
        {
            this.Monitor.Log(
                $"AutoOrganize: {{ Item: {item.Name}, Quantity: {(stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
        }

        return false;
    }
}
