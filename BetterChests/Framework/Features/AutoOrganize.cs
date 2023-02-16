namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;

/// <summary>
///     Automatically organizes items between chests during sleep.
/// </summary>
internal sealed class AutoOrganize : Feature
{
#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly IModHelper _helper;

    private AutoOrganize(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Initializes <see cref="AutoOrganize" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="AutoOrganize" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return AutoOrganize.Instance ??= new AutoOrganize(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this._helper.Events.GameLoop.DayEnding += AutoOrganize.OnDayEnding;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this._helper.Events.GameLoop.DayEnding -= AutoOrganize.OnDayEnding;
    }

    private static void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        var storages = Storages.All.ToArray();
        Array.Sort(storages);

        foreach (var fromStorage in storages)
        {
            AutoOrganize.OrganizeFrom(fromStorage, storages);
        }

        foreach (var storage in storages)
        {
            storage.OrganizeItems();
        }
    }

    private static void OrganizeFrom(StorageNode fromStorage, StorageNode[] storages)
    {
        if (fromStorage is not { Data: Storage fromStorageObject, AutoOrganize: FeatureOption.Enabled })
        {
            return;
        }

        for (var index = fromStorageObject.Items.Count - 1; index >= 0; --index)
        {
            AutoOrganize.OrganizeTo(fromStorage, storages, fromStorageObject, index);
        }
    }

    private static void OrganizeTo(
        StorageNode fromStorage,
        IEnumerable<StorageNode> storages,
        Storage fromStorageObject,
        int index)
    {
        var item = fromStorageObject.Items[index];
        if (item is null)
        {
            return;
        }

        var stack = item.Stack;
        foreach (var toStorage in storages)
        {
            if (!AutoOrganize.TransferStack(fromStorage, toStorage, item, stack))
            {
                break;
            }
        }
    }

    private static bool TransferStack(StorageNode fromStorage, StorageNode toStorage, Item item, int stack)
    {
        if (ReferenceEquals(fromStorage, toStorage)
            || fromStorage.StashToChestPriority >= toStorage.StashToChestPriority)
        {
            return true;
        }

        var tmp = toStorage.StashItem(item);
        if (tmp is null)
        {
            Log.Trace(
                $"AutoOrganize: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
            fromStorage.RemoveItem(item);
            return false;
        }

        if (stack != item.Stack)
        {
            Log.Trace(
                $"AutoOrganize: {{ Item: {item.Name}, Quantity: {(stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
        }

        return false;
    }
}