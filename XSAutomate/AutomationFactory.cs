namespace XSAutomate
{
    using Microsoft.Xna.Framework;
    using Pathoschild.Stardew.Automate;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.Locations;
    using StardewValley.TerrainFeatures;
    using Object = StardewValley.Object;

    /// <inheritdoc />
    internal class AutomationFactory : IAutomationFactory
    {
        /// <inheritdoc />
        public IAutomatable GetFor(Object obj, GameLocation location, in Vector2 tile)
        {
            return obj.modData.ContainsKey("furyx639.ExpandedStorage/Storage") ? new Connector(location, tile) : null;
        }

        /// <inheritdoc />
        public IAutomatable GetFor(TerrainFeature feature, GameLocation location, in Vector2 tile)
        {
            return null;
        }

        /// <inheritdoc />
        public IAutomatable GetFor(Building building, BuildableGameLocation location, in Vector2 tile)
        {
            return null;
        }

        /// <inheritdoc />
        public IAutomatable GetForTile(GameLocation location, in Vector2 tile)
        {
            return null;
        }
    }
}