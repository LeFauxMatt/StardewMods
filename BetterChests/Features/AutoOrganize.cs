namespace StardewMods.BetterChests.Features;

using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;
using StardewMods.Common.Enums;
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

    /// <summary>
    ///     Initializes <see cref="AutoOrganize" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="AutoOrganize" /> class.</returns>
    public static AutoOrganize Init(IModHelper helper)
    {
        return AutoOrganize.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        this.Helper.Events.GameLoop.DayEnding += AutoOrganize.OnDayEnding;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        this.Helper.Events.GameLoop.DayEnding -= AutoOrganize.OnDayEnding;
    }

    private static void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        var storages = StorageHelper.All
                                    .Where(storage => storage.AutoOrganize == FeatureOption.Enabled && storage is not ChestStorage { Chest.SpecialChestType: Chest.SpecialChestTypes.JunimoChest })
                                    .OrderByDescending(storage => storage.StashToChestPriority)
                                    .ToList();

        var items =
            from storage in storages
            from item in storage.Items
            select (item, storage);

        foreach (var (item, fromStorage) in items)
        {
            if (storages.Any(storage => storage.FilterMatches(item) && storage.StashItem(item) is null))
            {
                fromStorage.Items.Remove(item);
            }
        }
    }
}