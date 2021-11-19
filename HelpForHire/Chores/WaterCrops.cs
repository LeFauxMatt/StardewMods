namespace HelpForHire.Chores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Services;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.TerrainFeatures;

    internal class WaterCrops : GenericChore
    {
        public WaterCrops(ServiceManager serviceManager)
            : base("water-crops", serviceManager)
        {
        }

        protected override bool DoChore()
        {
            var cropsWatered = false;
            IDictionary<GameLocation, List<Vector2>> sprinklerCoverage = new Dictionary<GameLocation, List<Vector2>>();

            foreach (var (location,  hoeDirt) in WaterCrops.GetWaterSpots())
            {
                if (!sprinklerCoverage.TryGetValue(location, out var coverage))
                {
                    coverage = new();

                    var sprinklers =
                        from obj in location.Objects.Values
                        where obj.Name.Contains("Sprinkler")
                        select obj;

                    foreach (var sprinkler in sprinklers)
                    {
                        switch (sprinkler.ParentSheetIndex)
                        {
                            case 599:
                                coverage.AddRange(Utility.getAdjacentTileLocations(sprinkler.TileLocation));
                                break;
                            case 621:
                                coverage.AddRange(Utility.getSurroundingTileLocationsArray(sprinkler.TileLocation));
                                break;
                            case 645:
                                for (var tileX = -2; tileX <= 2; ++tileX)
                                {
                                    for (var tileY = -2; tileY <= 2; ++tileY)
                                    {
                                        coverage.Add(sprinkler.TileLocation + new Vector2(tileX, tileY));
                                    }
                                }
                                break;
                        }
                    }
                    sprinklerCoverage.Add(location, coverage);
                }

                if (coverage.Contains(hoeDirt.currentTileLocation))
                {
                    continue;
                }

                hoeDirt.state.Value = HoeDirt.watered;
                cropsWatered = true;
            }

            return cropsWatered;
        }

        protected override bool TestChore()
        {
            return WaterCrops.GetWaterSpots().Any();
        }

        private static IEnumerable<Tuple<GameLocation, HoeDirt>> GetWaterSpots()
        {
            var locations = Game1.locations.AsEnumerable();

            locations = locations.Concat(
                from location in Game1.locations.OfType<BuildableGameLocation>()
                from building in location.buildings
                where building.indoors.Value is not null
                select building.indoors.Value
            );

            foreach (var location in locations)
            {
                foreach (var hoeDirt in location.terrainFeatures.Values.OfType<HoeDirt>())
                {
                    if (hoeDirt.needsWatering() && hoeDirt.state.Value != HoeDirt.watered)
                    {
                        continue;
                    }

                    yield return new(location, hoeDirt);
                }
            }
        }
    }
}