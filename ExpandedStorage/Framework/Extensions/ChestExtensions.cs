using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Extensions
{
    public static class ChestExtensions
    {
        private static IReflectionHelper _reflection;

        internal static void Init(IReflectionHelper reflection)
        {
            _reflection = reflection;
        }
        
        public static void Draw(this Chest chest, SpriteBatch spriteBatch, Vector2 pos, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f)
        {
            var hideColorPicker = new List<int>{216, 248, 256};
            var currentLidFrameReflected = _reflection.GetField<int>(chest, "currentLidFrame");
            var currentLidFrame = currentLidFrameReflected.GetValue();
            
            if (chest.playerChoiceColor.Value.Equals(Color.Black) || hideColorPicker.Contains(chest.ParentSheetIndex))
            {
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                    pos + ShakeOffset(chest, -1, 2),
                    Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, chest.ParentSheetIndex, 16, 32),
                    chest.Tint * alpha,
                    0f,
                    Vector2.Zero,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth);
                
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                    pos + ShakeOffset(chest, -1, 2),
                    Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32),
                    chest.Tint * alpha,
                    0f,
                    Vector2.Zero,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth + 1E-05f);
                
                return;
            }

            var baseOffset = chest.ParentSheetIndex switch
            {
                130 => 38,
                232 => 0,
                _ => 6
            };
            
            var aboveOffset = chest.ParentSheetIndex switch
            {
                130 => 46,
                232 => 8,
                _ => 11
            };

            // Draw Storage Layer (Colorized)
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                pos + ShakeOffset(chest, -1, 2),
                Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, chest.ParentSheetIndex + baseOffset, 16, 32),
                chest.playerChoiceColor.Value * alpha,
                0f,
                Vector2.Zero,
                scaleSize,
                SpriteEffects.None,
                layerDepth);
            
            // Draw Lid Layer (Colorized)
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                pos + ShakeOffset(chest, -1, 2),
                Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + baseOffset, 16, 32),
                chest.playerChoiceColor.Value * alpha * alpha,
                0f,
                Vector2.Zero,
                scaleSize,
                SpriteEffects.None,
                layerDepth + 1E-05f);

            // Draw Brace Layer (Non-Colorized)
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                pos + ShakeOffset(chest, -1, 2),
                Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + aboveOffset, 16, 32),
                Color.White * alpha,
                0f,
                Vector2.Zero,
                scaleSize,
                SpriteEffects.None,
                layerDepth + 2E-05f);

            if (chest.ParentSheetIndex == 130 || chest.ParentSheetIndex == 232)
            {
                // Draw Bottom Brace Layer (Non-Colorized)
                var rect = Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, chest.ParentSheetIndex + aboveOffset, 16, 32);
                rect.Y += 20;
                rect.Height -= 20;
                pos.Y += 20 * scaleSize;
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                    pos,
                    rect,
                    Color.White * alpha,
                    0f,
                    Vector2.Zero,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth + 3E-05f);
            }
        }
        
        private static Vector2 ShakeOffset(Object instance, int minValue, int maxValue) =>
            instance.shakeTimer > 0
                ? new Vector2(Game1.random.Next(minValue, maxValue), 0)
                : Vector2.Zero;
    }
}