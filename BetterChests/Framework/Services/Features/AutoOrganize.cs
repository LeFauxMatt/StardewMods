namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;

/// <summary>Automatically organizes items between chests during sleep.</summary>
internal sealed class AutoOrganize : BaseFeature
{
    private readonly ContainerFactory containerFactory;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="AutoOrganize" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    public AutoOrganize(ILogging logging, ModConfig modConfig, IModEvents modEvents, ContainerFactory containerFactory)
        : base(logging, modConfig)
    {
        this.modEvents = modEvents;
        this.containerFactory = containerFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.AutoOrganize != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.modEvents.GameLoop.DayEnding += this.OnDayEnding;

    /// <inheritdoc />
    protected override void Deactivate() => this.modEvents.GameLoop.DayEnding -= this.OnDayEnding;

    private void OnDayEnding(object? sender, DayEndingEventArgs e) => this.OrganizeAll();

    private void OrganizeAll()
    {
        var containerGroups = this
            .containerFactory.GetAll(container => container.Options.AutoOrganize == FeatureOption.Enabled)
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

                foreach (var storageTo in containersTo)
                {
                    this.OrganizeFrom(storageTo, containersFrom);
                }
            }
        }
    }

    private void OrganizeFrom(IContainer containerFrom, List<IContainer> containersTo)
    {
        for (var index = containerFrom.Items.Count - 1; index >= 0; --index)
        {
            var item = containerFrom.Items[index];
            if (item is null)
            {
                return;
            }

            var stack = item.Stack;
            foreach (var toStorage in containersTo)
            {
                if (!containerFrom.Transfer(item, toStorage, out var remaining))
                {
                    continue;
                }

                var amount = stack - (remaining?.Stack ?? 0);
                this.Logging.Trace("AutoOrganize: {{ Item: {0}, Quantity: {1}, From: {2}, To: {3} }}", item.Name, amount.ToString(CultureInfo.InvariantCulture), containerFrom, toStorage);

                if (remaining is null)
                {
                    return;
                }

                stack = remaining.Stack;
            }
        }
    }
}
