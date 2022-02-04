namespace XSAutomate
{
    using Microsoft.Xna.Framework;
    using Pathoschild.Stardew.Automate;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.Locations;
    using StardewValley.TerrainFeatures;

    /// <inheritdoc />
    public class ModEntry : Mod
    {
        private const string AutomateAPI = "Pathoschild.Automate";

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var automate = this.Helper.ModRegistry.GetApi<IAutomateAPI>(ModEntry.AutomateAPI);
            automate.AddFactory(new AutomationFactory());
        }
    }

    /// <inheritdoc />
    internal class AutomationFactory : IAutomationFactory
    {
        private const string XSModDataKey = "furyx639.ExpandedStorage/Storage";
        /// <inheritdoc />
        public IAutomatable GetFor(Object obj, GameLocation location, in Vector2 tile)
        {
            return obj.modData.ContainsKey(AutomationFactory.XSModDataKey) ? new Connector(location, tile) : null;
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

    internal class Connector : IAutomatable
    {
        /// <summary>Initializes a new instance of the <see cref="Connector" /> class.</summary>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="tile">The tile covered by the machine.</param>
        public Connector(GameLocation location, Vector2 tile)
            : this(location, new Rectangle((int)tile.X, (int)tile.Y, 1, 1))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Connector" /> class.</summary>
        /// <param name="location">The location which contains the machine.</param>
        /// <param name="tileArea">The tile area covered by the machine.</param>
        private Connector(GameLocation location, Rectangle tileArea)
        {
            this.Location = location;
            this.TileArea = tileArea;
        }

        /// <summary>Gets the location which contains the machine.</summary>
        public GameLocation Location { get; }

        /// <summary>Gets the tile area covered by the machine.</summary>
        public Rectangle TileArea { get; }
    }
}