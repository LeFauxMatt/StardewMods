namespace HelpForHire.Chores
{
    using System.Collections.Generic;
    using System.Linq;
    using Common.Services;
    using StardewValley;

    internal class WaterSlimes : GenericChore
    {
        public WaterSlimes(ServiceManager serviceManager)
            : base("water-slimes", serviceManager)
        {
        }

        protected override bool DoChore()
        {
            var slimesWatered = false;

            foreach (var slimeHutch in WaterSlimes.GetSlimeHutches())
            {
                for (var index = 0; index < slimeHutch.waterSpots.Count; ++index)
                {
                    if (slimeHutch.waterSpots[index])
                    {
                        continue;
                    }

                    slimeHutch.waterSpots[index] = true;
                    slimesWatered = true;
                }
            }

            return slimesWatered;
        }

        protected override bool TestChore()
        {
            return WaterSlimes.GetSlimeHutches().Any();
        }

        private static IEnumerable<SlimeHutch> GetSlimeHutches()
        {
            return
                from building in Game1.getFarm().buildings
                where building.daysOfConstructionLeft.Value <= 0 && building.indoors.Value is SlimeHutch
                select building.indoors.Value as SlimeHutch;
        }
    }
}