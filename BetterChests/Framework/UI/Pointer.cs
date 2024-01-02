namespace StardewMods.BetterChests.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <summary>Overlay which points to an object on the map.</summary>
internal sealed class Pointer
{
    /// <summary>Initializes a new instance of the <see cref="Pointer" /> class.</summary>
    /// <param name="container">The container that the pointer points to.</param>
    public Pointer(IStorageContainer container) => this.Container = container;

    /// <summary>Gets the container that the pointer points to.</summary>
    public IStorageContainer Container { get; }

    /// <summary>Draws the sprite representing the object on the screen.</summary>
    /// <param name="spriteBatch">The sprite batch used for drawing.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        var position = new Vector2(this.Container.TileLocation.X + 0.5f, this.Container.TileLocation.Y - 0.75f)
            * Game1.tileSize;

        var onScreenPos = default(Vector2);
        if (Utility.isOnScreen(position, 64))
        {
            onScreenPos = Game1.GlobalToLocal(Game1.viewport, position + Vector2.Zero);
            onScreenPos = Utility.ModifyCoordinatesForUIScale(onScreenPos);
            spriteBatch.Draw(
                Game1.mouseCursors,
                onScreenPos,
                new Rectangle(412, 495, 5, 4),
                Color.White,
                (float)Math.PI,
                new Vector2(2f, 2f),
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);

            return;
        }

        var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        var rotation = 0f;
        if (position.X > Game1.viewport.MaxCorner.X - 64)
        {
            onScreenPos.X = bounds.Right - 8f;
            rotation = (float)Math.PI / 2f;
        }
        else if (position.X < Game1.viewport.X)
        {
            onScreenPos.X = 8f;
            rotation = -(float)Math.PI / 2f;
        }
        else
        {
            onScreenPos.X = position.X - Game1.viewport.X;
        }

        if (position.Y > Game1.viewport.MaxCorner.Y - 64)
        {
            onScreenPos.Y = bounds.Bottom - 8f;
            rotation = (float)Math.PI;
        }
        else if (position.Y < Game1.viewport.Y)
        {
            onScreenPos.Y = 8f;
        }
        else
        {
            onScreenPos.Y = position.Y - Game1.viewport.Y;
        }

        if ((int)onScreenPos.X == 8 && (int)onScreenPos.Y == 8)
        {
            rotation += (float)Math.PI / 4f;
        }
        else if ((int)onScreenPos.X == 8 && (int)onScreenPos.Y == bounds.Bottom - 8)
        {
            rotation += (float)Math.PI / 4f;
        }
        else if ((int)onScreenPos.X == bounds.Right - 8 && (int)onScreenPos.Y == 8)
        {
            rotation -= (float)Math.PI / 4f;
        }
        else if ((int)onScreenPos.X == bounds.Right - 8 && (int)onScreenPos.Y == bounds.Bottom - 8)
        {
            rotation -= (float)Math.PI / 4f;
        }

        onScreenPos = Utility.makeSafe(onScreenPos, new Vector2(5 * Game1.pixelZoom, 4 * Game1.pixelZoom));
        spriteBatch.Draw(
            Game1.mouseCursors,
            onScreenPos,
            new Rectangle(412, 495, 5, 4),
            Color.White,
            rotation,
            new Vector2(2f, 2f),
            Game1.pixelZoom,
            SpriteEffects.None,
            1f);
    }
}