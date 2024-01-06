namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Automatically organizes items between chests during sleep.</summary>
internal sealed class AutoOrganize : BaseFeature<AutoOrganize>
{
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="AutoOrganize" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public AutoOrganize(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        ILog log,
        IManifest manifest,
        IModEvents modEvents)
        : base(log, manifest, configManager)
    {
        this.containerHandler = containerHandler;
        this.modEvents = modEvents;
        this.containerFactory = containerFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.AutoOrganize != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.modEvents.GameLoop.DayEnding += this.OnDayEnding;

    /// <inheritdoc />
    protected override void Deactivate() => this.modEvents.GameLoop.DayEnding -= this.OnDayEnding;

    private void OnDayEnding(object? sender, DayEndingEventArgs e) => this.OrganizeAll();

    private void OrganizeAll()
    {
        var containerGroupsTo = this
            .containerFactory.GetAll(container => container.Options.AutoOrganize == FeatureOption.Enabled)
            .GroupBy(container => container.Options.StashToChestPriority)
            .ToDictionary(containerGroup => containerGroup.Key, group => group.ToList());

        var containerGroupsFrom = new Dictionary<int, List<IStorageContainer>>();
        foreach (var (priority, containers) in containerGroupsTo)
        {
            containerGroupsFrom.Add(priority, new List<IStorageContainer>(containers));
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
                        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts))
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

                    this.containerHandler.OrganizeItems(containerTo);
                }
            }
        }
    }
}