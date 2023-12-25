namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Automatically organizes items between chests during sleep.</summary>
internal sealed class AutoOrganize : BaseFeature
{
    private readonly ContainerFactory containerFactory;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="AutoOrganize" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    public AutoOrganize(ILog log, ModConfig modConfig, IModEvents modEvents, ContainerFactory containerFactory)
        : base(log, modConfig)
    {
        this.modEvents = modEvents;
        this.containerFactory = containerFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.AutoOrganize != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.modEvents.GameLoop.DayEnding += this.OnDayEnding;

    /// <inheritdoc />
    protected override void Deactivate() => this.modEvents.GameLoop.DayEnding -= this.OnDayEnding;

    private void OnDayEnding(object? sender, DayEndingEventArgs e) => this.OrganizeAll();

    private void OrganizeAll()
    {
        var containerGroups = this
            .containerFactory.GetAll(container => container.Options.AutoOrganize == Option.Enabled)
            .GroupBy(container => container.Options.StashToChestPriority)
            .ToDictionary(containerGroup => containerGroup.Key, group => group.ToList());

        var topPriority = containerGroups.Keys.Max();
        var bottomPriority = containerGroups.Keys.Min();

        for (var priorityTo = topPriority; priorityTo >= bottomPriority; --priorityTo)
        {
            if (!containerGroups.TryGetValue(priorityTo, out var containersTo))
            {
                continue;
            }

            for (var priorityFrom = priorityTo - 1; priorityFrom >= bottomPriority; --priorityFrom)
            {
                if (!containerGroups.TryGetValue(priorityFrom, out var containersFrom))
                {
                    continue;
                }

                foreach (var containerTo in containersTo)
                {
                    this.OrganizeFrom(containerTo, containersFrom);
                }
            }

            foreach (var containerTo in containersTo)
            {
                containerTo.OrganizeItems();
            }
        }
    }

    private void OrganizeFrom(IContainer containerFrom, List<IContainer> containersTo) =>
        containerFrom.ForEachItem(
            item =>
            {
                var stack = item.Stack;
                foreach (var toStorage in containersTo)
                {
                    if (!containerFrom.Transfer(item, toStorage, out var remaining))
                    {
                        continue;
                    }

                    var amount = stack - (remaining?.Stack ?? 0);
                    this.Log.Trace(
                        "{0}: Organized {1} ({2}) from {3} to {4}",
                        this.Id,
                        item.Name,
                        amount,
                        containerFrom,
                        toStorage);

                    if (remaining is null)
                    {
                        return true;
                    }

                    stack = remaining.Stack;
                }

                return true;
            });
}