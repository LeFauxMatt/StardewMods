namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
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
        var storages = (
            from storage in StorageHelper.All
            where storage.AutoOrganize == FeatureOption.Enabled && storage is not ChestStorage { Chest.SpecialChestType: Chest.SpecialChestTypes.JunimoChest }
            orderby storage.StashToChestPriority descending
            select storage).ToList();

        var items =
            from storage in storages
            from item in storage.Items
            select (item, storage);

        foreach (var (item, fromStorage) in items.ToList())
        {
            if (storages.Any(storage => storage.FilterMatches(item) && storage.StashItem(item) is null))
            {
                fromStorage.Items.Remove(item);
            }
        }

        foreach (var storage in storages)
        {
            storage.OrganizeItems();
        }
    }
}