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
        var containerGroupsTo = this
            .containerFactory.GetAll(container => container.Options.AutoOrganize == Option.Enabled)
            .GroupBy(container => container.Options.StashToChestPriority)
            .ToDictionary(containerGroup => containerGroup.Key, group => group.ToList());

        var containerGroupsFrom = new Dictionary<int, List<IContainer>>();
        foreach (var (priority, containers) in containerGroupsTo)
        {
            containerGroupsFrom.Add(priority, new List<IContainer>(containers));
        }

        var topPriority = containerGroupsTo.Keys.Max();
        var bottomPriority = containerGroupsTo.Keys.Min();

        for (var priorityTo = topPriority; priorityTo >= bottomPriority; --priorityTo)
        {
            if (!containerGroupsTo.TryGetValue(priorityTo, out var containersTo))
            {
                continue;
            }

            for (var priorityFrom = priorityTo - 1; priorityFrom >= bottomPriority; --priorityFrom)
            {
                if (!containerGroupsFrom.TryGetValue(priorityFrom, out var containersFrom))
                {
                    continue;
                }

                for (var indexTo = containersTo.Count - 1; indexTo >= 0; --indexTo)
                {
                    var containerTo = containersTo[indexTo];
                    for (var indexFrom = containersFrom.Count - 1; indexFrom >= 0; --indexFrom)
                    {
                        if (containerTo.Items.Count >= containerTo.Capacity)
                        {
                            break;
                        }

                        var containerFrom = containersFrom[indexFrom];
                        if (!containerFrom.Transfer(containerTo, out var amounts))
                        {
                            containersFrom.RemoveAt(indexFrom);
                            continue;
                        }

                        foreach (var (name, amount) in amounts)
                        {
                            if (amount > 0)
                            {
                                this.Log.Trace(
                                    "{0}: {{ Item: {1}, Quantity: {2}, From: {3}, To: {4} }}",
                                    this.Id,
                                    name,
                                    amount,
                                    containerFrom,
                                    containerTo);
                            }
                        }
                    }

                    containerTo.OrganizeItems();
                }
            }
        }
    }
}