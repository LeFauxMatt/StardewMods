using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreCraftables.API;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace MoreCraftables.Framework.HandledObjects.BigCraftables
{
    public class HandledChest : HandledBigCraftable
    {
        private static readonly HashSet<int> HideColorPickerIds = new() {216, 248, 256};
        private static readonly HashSet<int> ShowBottomBraceIds = new() {130, 232};
        private static IReflectionHelper _reflection;

        private readonly bool _fridge;

        private readonly Chest.SpecialChestTypes _specialChestType;

        public HandledChest(IHandledObject handledObject, IReflectionHelper reflection) : base(handledObject)
        {
            _reflection = reflection;

            _specialChestType =
                Properties.TryGetValue("SpecialChestType", out var specialChestType)
                && specialChestType is string specialChestTypeString
                && Enum.TryParse(specialChestTypeString, out Chest.SpecialChestTypes specialChestTypeValue)
                    ? specialChestTypeValue
                    : Chest.SpecialChestTypes.None;
            _fridge = Properties.TryGetValue("fridge", out var fridge) && (bool) fridge;
        }

        public override Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            if (location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                return null;
            }

            PlaySoundOrDefault(location, "axe");
            var chest = new Chest(true, pos, obj.ParentSheetIndex)
            {
                shakeTimer = 50,
                SpecialChestType = _specialChestType
            };
            chest.fridge.Value = _fridge;

            if (obj is not Chest otherChest)
                return chest;

            chest.playerChoiceColor.Value = otherChest.playerChoiceColor.Value;
            if (otherChest.items.Any())
                chest.items.CopyFrom(otherChest.items);

            return chest;
        }

        public override bool Draw(Object obj, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f, IHandledObject.DrawContext drawContext = IHandledObject.DrawContext.Placed, Color color = default)
        {
            if (obj is not Chest chest)
                return true;

            var currentLidFrameReflected = _reflection.GetField<int>(chest, "currentLidFrame");
            var currentLidFrame = currentLidFrameReflected.GetValue();
            if (currentLidFrame == 0)
                currentLidFrame = chest.startingLidFrame.Value;

            if (chest.playerChoiceColor.Value.Equals(Color.Black) || HideColorPickerIds.Contains(chest.ParentSheetIndex))
            {
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                    pos + ShakeOffset(chest, -1, 2),
                    Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, chest.ParentSheetIndex, 16, 32),
                    chest.Tint * alpha,
                    0f,
                    origin,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth);

                spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                    pos + ShakeOffset(chest, -1, 2),
                    Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame, 16, 32),
                    chest.Tint * alpha,
                    0f,
                    origin,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth + 1E-05f);

                return false;
            }

            var baseOffset = chest.ParentSheetIndex switch {130 => 38, 232 => 0, _ => 6};
            var aboveOffset = chest.ParentSheetIndex switch {130 => 46, 232 => 8, _ => 11};

            // Draw Storage Layer (Colorized)
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                pos + ShakeOffset(chest, -1, 2),
                Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, chest.ParentSheetIndex + baseOffset, 16, 32),
                chest.playerChoiceColor.Value * alpha,
                0f,
                origin,
                scaleSize,
                SpriteEffects.None,
                layerDepth);

            // Draw Lid Layer (Colorized)
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                pos + ShakeOffset(chest, -1, 2),
                Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + baseOffset, 16, 32),
                chest.playerChoiceColor.Value * alpha * alpha,
                0f,
                origin,
                scaleSize,
                SpriteEffects.None,
                layerDepth + 1E-05f);

            // Draw Brace Layer (Non-Colorized)
            spriteBatch.Draw(Game1.bigCraftableSpriteSheet,
                pos + ShakeOffset(chest, -1, 2),
                Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, currentLidFrame + aboveOffset, 16, 32),
                Color.White * alpha,
                0f,
                origin,
                scaleSize,
                SpriteEffects.None,
                layerDepth + 2E-05f);

            if (!ShowBottomBraceIds.Contains(chest.ParentSheetIndex))
                return false;

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
                origin,
                scaleSize,
                SpriteEffects.None,
                layerDepth + 3E-05f);
            return false;
        }

        private static Vector2 ShakeOffset(Object instance, int minValue, int maxValue)
        {
            return instance.shakeTimer > 0
                ? new Vector2(Game1.random.Next(minValue, maxValue), 0)
                : Vector2.Zero;
        }
    }
}