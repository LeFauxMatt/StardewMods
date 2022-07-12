namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Objects;

/// <summary>
///     Automatically organizes items between chests during sleep.
/// </summary>
internal class AutoOrganize : IFeature
{
    private AutoOrganize(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static AutoOrganize? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    /// <summary>
    ///     Initializes <see cref="AutoOrganize" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="AutoOrganize" /> class.</returns>
    [MemberNotNull(nameof(AutoOrganize.Instance))]
    public static AutoOrganize Init(IModHelper helper)
    {
        return AutoOrganize.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.GameLoop.DayEnding += AutoOrganize.OnDayEnding;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.GameLoop.DayEnding -= AutoOrganize.OnDayEnding;
        }
    }

    private static void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        var storages =
            StorageHelper.All
                         .Where(storage => storage.AutoOrganize == FeatureOption.Enabled && storage is not ChestStorage { Chest.SpecialChestType: Chest.SpecialChestTypes.JunimoChest })
                         .OrderByDescending(storage => storage.StashToChestPriority)
                         .ToList();

        foreach (var fromStorage in storages)
        {
            for (var index = fromStorage.Items.Count - 1; index >= 0; index--)
            {
                var item = fromStorage.Items[index];
                if (item is null)
                {
                    continue;
                }

                var stack = item.Stack;
                foreach (var toStorage in storages.Where(storage => !ReferenceEquals(fromStorage, storage) && storage.StashToChestPriority > fromStorage.StashToChestPriority))
                {
                    var tmp = toStorage.StashItem(item);
                    if (tmp is null)
                    {
                        Log.Trace($"AutoOrganize: {{ Item: {item.Name}, Quantity: {stack.ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
                        fromStorage.Items.Remove(item);
                        break;
                    }

                    if (stack != item.Stack)
                    {
                        Log.Trace($"AutoOrganize: {{ Item: {item.Name}, Quantity: {(stack - item.Stack).ToString(CultureInfo.InvariantCulture)}, From: {fromStorage}, To: {toStorage}");
                    }
                }
            }
        }

        foreach (var storage in storages)
        {
            storage.OrganizeItems();
        }
    }
}