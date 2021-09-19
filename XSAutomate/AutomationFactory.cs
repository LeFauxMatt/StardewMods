namespace XSAutomate
{
    using System;
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
        private const string ConnectorType = "Pathoschild.Stardew.Automate.Framework.Connector, Automate";
        private static readonly Type Connector = Type.GetType(AutomationFactory.ConnectorType);

        /// <inheritdoc />
        public IAutomatable GetFor(Object obj, GameLocation location, in Vector2 tile)
        {
            if (obj.modData.ContainsKey("furyx639.ExpandedStorage/Storage"))
            {
                return (IAutomatable)Activator.CreateInstance(AutomationFactory.Connector, location, tile);
            }

            return null;
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