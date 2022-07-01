#nullable disable

namespace HelpForHire.Chores;

using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;

internal class RepairFences : GenericChore
{
    public RepairFences(ServiceLocator serviceLocator)
        : base("repair-fences", serviceLocator)
    {
    }

    protected override bool DoChore()
    {
        var fencesRepaired = false;

        foreach (var fence in RepairFences.GetFences())
        {
            fence.repair();
            fencesRepaired = true;
        }

        return fencesRepaired;
    }

    protected override bool TestChore()
    {
        return RepairFences.GetFences().Any();
    }

    private static IEnumerable<Fence> GetFences()
    {
        var locations = Game1.locations.AsEnumerable();

        locations = locations.Concat(
            from location in Game1.locations.OfType<BuildableGameLocation>()
            from building in location.buildings
            where building.indoors.Value is not null
            select building.indoors.Value
        );

        return
            from location in locations
            from fence in location.Objects.Values.OfType<Fence>()
            where !(fence.getHealth() >= fence.maxHealth.Value)
            select fence;
    }
}