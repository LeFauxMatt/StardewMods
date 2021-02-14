using System;
using System.Collections.Generic;
using System.Linq;
using Common.PatternPatches;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreCraftables.API;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace MoreCraftables.Framework.Patches
{
    internal class ChestPatch : Patch<ModConfig>
    {
        private static IDictionary<string, IHandledObject> _handledTypes;

        public ChestPatch(IMonitor monitor, ModConfig config, IDictionary<string, IHandledObject> handledTypes)
            : base(monitor, config)
        {
            _handledTypes = handledTypes;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)}),
                new HarmonyMethod(GetType(), nameof(DrawPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)}),
                new HarmonyMethod(GetType(), nameof(DrawLocalPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.drawInMenu), new[] {typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)}),
                new HarmonyMethod(GetType(), nameof(DrawInMenuPrefix))
            );
        }

        /// <summary>Draw chest with playerChoiceColor and lid animation when placed.</summary>
        public static bool DrawPrefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            // Verify this is a handled item type
            var handledType = _handledTypes
                .Select(t => t.Value)
                .LastOrDefault(t => t.IsHandledItem(__instance));

            if (handledType == null)
                return true;

            var drawX = (float) x;
            var drawY = (float) y;
            if (__instance.localKickStartTile.HasValue)
            {
                drawX = Utility.Lerp(__instance.localKickStartTile.Value.X, drawX, __instance.kickProgress);
                drawY = Utility.Lerp(__instance.localKickStartTile.Value.Y, drawY, __instance.kickProgress);
            }

            var globalPosition = new Vector2(drawX * 64f, (drawY - 1f) * 64f);
            var layerDepth = Math.Max(0.0f, ((drawY + 1f) * 64f - 24f) / 10000f) + drawX * 1E-05f;

            return handledType.Draw(__instance, spriteBatch, Game1.GlobalToLocal(Game1.viewport, globalPosition), Vector2.Zero, alpha, layerDepth);
        }

        /// <summary>Draw chest with playerChoiceColor and lid animation when held.</summary>
        public static bool DrawLocalPrefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha, bool local)
        {
            // Verify this is a handled item type
            var handledType = _handledTypes
                .Select(t => t.Value)
                .LastOrDefault(t => t.IsHandledItem(__instance));

            return handledType == null || handledType.Draw(__instance, spriteBatch, new Vector2(x, y - 64), Vector2.Zero, alpha, drawContext: IHandledObject.DrawContext.Held);
        }

        /// <summary>Draw chest with playerChoiceColor and lid animation in menu.</summary>
        public static bool DrawInMenuPrefix(Chest __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            // Verify this is a handled item type
            var handledType = _handledTypes
                .Select(t => t.Value)
                .LastOrDefault(t => t.IsHandledItem(__instance));

            if (handledType == null)
                return true;

            if (handledType.Draw(__instance, spriteBatch, location + new Vector2(32, 32), new Vector2(8, 16), transparency, layerDepth, 4f * (scaleSize < 0.2 ? scaleSize : scaleSize / 2f), IHandledObject.DrawContext.Menu, color))
                return true;

            // Draw Stack
            if (drawStackNumber == StackDrawType.Draw && __instance.Stack > 1)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) - 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);

            // Draw held item count
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            if (items > 0)
                Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize, 2f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            return false;
        }
    }
}