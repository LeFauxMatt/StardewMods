namespace StardewMods.ExpandedStorage.Framework;

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewValley.Objects;

internal static class Extensions
{
#nullable disable
    public static IGameContentHelper GameContent;
#nullable enable

    public static void Draw(
        this IManagedStorage storage,
        Chest chest,
        string name,
        int currentLidFrame,
        SpriteBatch b,
        Vector2 pos,
        float alpha)
    {
        var layerDepth = Math.Max(0f, (pos.X + 1f) * Game1.tileSize - 24) + pos.Y * 1E-05f;
        var texture = Extensions.GameContent.Load<Texture2D>($"furyx639.ExpandedStorage/Texture/{name}");
        var frame = new Rectangle(
            currentLidFrame * storage.Width,
            !storage.PlayerColor || chest.playerChoiceColor.Value.Equals(Color.Black) ? 0 : storage.Height,
            storage.Width,
            storage.Height);

        var color = !storage.PlayerColor || chest.playerChoiceColor.Value.Equals(Color.Black)
            ? chest.Tint
            : chest.playerChoiceColor.Value;

        // Draw Base Layer
        b.Draw(
            texture,
            pos + (chest.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            color * alpha,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth);
        if (frame.Y == 0)
        {
            return;
        }

        frame.Y = storage.Height * 2;

        // Draw Top Layer
        b.Draw(
            texture,
            pos + (chest.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            chest.Tint * alpha,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth);
    }
}