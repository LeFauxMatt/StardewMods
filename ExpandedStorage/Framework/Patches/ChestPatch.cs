using System;
using System.Collections.Generic;
using System.Linq;
using Common.PatternPatches;
using ExpandedStorage.Framework.Extensions;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.UI;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

// ReSharper disable UnusedParameter.Global

// ReSharper disable InconsistentNaming

namespace ExpandedStorage.Framework.Patches
{
    internal class ChestPatch : Patch<ModConfig>
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new();

        internal ChestPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config)
        {
        }

        internal static void AddExclusion(string modDataKey)
        {
            if (!ExcludeModDataKeys.Contains(modDataKey))
                ExcludeModDataKeys.Add(modDataKey);
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                new HarmonyMethod(GetType(), nameof(CheckForActionPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.grabItemFromChest)),
                postfix: new HarmonyMethod(GetType(), nameof(GrabItemFromChestPostfix)));

            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.grabItemFromInventory)),
                postfix: new HarmonyMethod(GetType(), nameof(GrabItemFromInventoryPostfix)));

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

            if (Config.AllowRestrictedStorage)
                harmony.Patch(
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem), new[] {typeof(Item)}),
                    new HarmonyMethod(GetType(), nameof(AddItemPrefix))
                );

            if (Config.AllowModdedCapacity)
                harmony.Patch(
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    new HarmonyMethod(GetType(), nameof(GetActualCapacity_Prefix))
                );
        }

        public static bool CheckForActionPrefix(Chest __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity
                || !__instance.playerChest.Value
                || !Game1.didPlayerJustRightClick(true))
                return true;

            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null || config.SourceType != SourceType.JsonAssets)
                return true;
            __instance.GetMutex().RequestLock(delegate
            {
                __instance.frameCounter.Value = 5;
                Game1.playSound(config.OpenSound);
                Game1.player.Halt();
                Game1.player.freezePause = 1000;
            });
            __result = true;
            return false;
        }

        /// <summary>Prevent adding item if filtered.</summary>
        public static bool AddItemPrefix(Chest __instance, ref Item __result, Item item)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (!ReferenceEquals(__instance, item) && (config == null || config.Filter(item)))
                return true;

            __result = item;
            return false;
        }

        /// <summary>Refresh inventory after item grabbed from chest.</summary>
        public static void GrabItemFromChestPostfix()
        {
            MenuViewModel.RefreshItems();
        }

        /// <summary>Refresh inventory after item grabbed from inventory.</summary>
        public static void GrabItemFromInventoryPostfix()
        {
            MenuViewModel.RefreshItems();
        }

        /// <summary>Draw chest with playerChoiceColor and lid animation when placed.</summary>
        public static bool DrawPrefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || !__instance.playerChest.Value
                || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;

            if (!config.IsPlaceable)
                return false;

            var draw_x = (float) x;
            var draw_y = (float) y;
            if (__instance.localKickStartTile.HasValue)
            {
                draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
            }

            var globalPosition = new Vector2(draw_x * 64f, (draw_y - 1f) * 64f);
            var layerDepth = Math.Max(0.0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;

            __instance.Draw(spriteBatch, Game1.GlobalToLocal(Game1.viewport, globalPosition), Vector2.Zero, alpha, layerDepth);
            return false;
        }

        /// <summary>Draw chest with playerChoiceColor and lid animation when held.</summary>
        public static bool DrawLocalPrefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha, bool local)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || !local
                || !__instance.playerChest.Value
                || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;

            __instance.Draw(spriteBatch, new Vector2(x, y - 64), Vector2.Zero, alpha);
            return false;
        }

        /// <summary>Draw chest with playerChoiceColor and lid animation in menu.</summary>
        public static bool DrawInMenuPrefix(Chest __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || !__instance.playerChest.Value
                || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;

            __instance.Draw(spriteBatch, location + new Vector2(32, 32), new Vector2(8, 16), transparency, layerDepth, 4f * (scaleSize < 0.2 ? scaleSize : scaleSize / 2f));

            // Draw Stack
            if (__instance.Stack > 1)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) - 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);

            // Draw Held Items
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            if (items > 0)
                Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize, 2f * scaleSize), 3f * scaleSize, 1f, color);
            return false;
        }

        /// <summary>Returns modded capacity for storage.</summary>
        public static bool GetActualCapacity_Prefix(Chest __instance, ref int __result)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null || config.Capacity == 0)
                return true;

            __result = config.Capacity == -1
                ? int.MaxValue
                : config.Capacity;
            return false;
        }
    }
}