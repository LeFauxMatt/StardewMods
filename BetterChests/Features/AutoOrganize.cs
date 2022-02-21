namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class AutoOrganize : Feature
{
    private readonly Lazy<OrganizeChest> _organizeChest;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoOrganize" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public AutoOrganize(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._organizeChest = services.Lazy<OrganizeChest>();
    }

    private IEnumerable<IManagedStorage> EligibleStorages
    {
        get
        {
            IList<IManagedStorage> eligibleStorages =
                this.ManagedObjects.InventoryStorages
                    .Select(inventoryStorage => inventoryStorage.Value)
                    .Where(playerChest => playerChest.AutoOrganize == FeatureOption.Enabled)
                    .ToList();
            foreach (var (_, locationStorage) in this.ManagedObjects.LocationStorages)
            {
                // Disabled in config
                if (locationStorage.AutoOrganize == FeatureOption.Enabled)
                {
                    eligibleStorages.Add(locationStorage);
                }
            }

            return eligibleStorages;
        }
    }

    private OrganizeChest OrganizeChest
    {
        get => this._organizeChest.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Helper.Events.GameLoop.DayEnding += this.OnDayEnding;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Helper.Events.GameLoop.DayEnding -= this.OnDayEnding;
    }

    private void OnDayEnding(object sender, DayEndingEventArgs e)
    {
        var allStorages = this.EligibleStorages.ToList();
        foreach (var fromStorage in allStorages)
        {
            var toStorages = allStorages
                             .Where(storage => storage.StashToChestPriority > fromStorage.StashToChestPriority)
                             .OrderByDescending(storage => storage.StashToChestPriority)
                             .ToList();
            if (!toStorages.Any())
            {
                this.OrganizeChest.OrganizeItems(fromStorage, true);
                continue;
            }

            foreach (var toStorage in toStorages)
            {
                for (var index = 0; index < fromStorage.Items.Count; index++)
                {
                    var item = fromStorage.Items[index];
                    if (item is null)
                    {
                        continue;
                    }

                    var tmp = toStorage.StashItem(item);
                    if (tmp is null)
                    {
                        fromStorage.Items.Remove(item);
                    }
                }

                if (fromStorage.Items.All(item => item is null))
                {
                    break;
                }
            }

            this.OrganizeChest.OrganizeItems(fromStorage, true);
        }
    }
}