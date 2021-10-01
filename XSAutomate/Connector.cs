namespace XSAutomate
{
    using Microsoft.Xna.Framework;
    using Pathoschild.Stardew.Automate;
    using StardewValley;

    /// <summary>An entity which connects machines and chests in a machine group, but otherwise has no logic of its own.</summary>
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