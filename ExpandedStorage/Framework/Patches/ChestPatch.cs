using System;
using System.Diagnostics.CodeAnalysis;
using ExpandedStorage.Framework.Extensions;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ChestPatches : HarmonyPatch
    {
        private const string CustomChestTypesKey = "aedenthorn.CustomChestTypes/IsCustomChest";

        private readonly Type _type = typeof(Chest);

        internal ChestPatches(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(Chest.checkForAction)),
                new HarmonyMethod(GetType(), nameof(checkForAction_Prefix)));

            harmony.Patch(
                AccessTools.Method(_type, nameof(Chest.draw), new[] {typeof(SpriteBatch), T.Int, T.Int, T.Float}),
                new HarmonyMethod(GetType(), nameof(draw_Prefix)));

            harmony.Patch(
                AccessTools.Method(_type, nameof(Chest.draw),
                    new[] {typeof(SpriteBatch), T.Int, T.Int, T.Float, T.Bool}),
                new HarmonyMethod(GetType(), nameof(drawLocal_Prefix)));

            if (Config.AllowRestrictedStorage)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(Chest.addItem), new[] {typeof(Item)}),
                    new HarmonyMethod(GetType(), nameof(addItem_Prefix)));
            }

            if (Config.AllowModdedCapacity)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(Chest.GetActualCapacity)),
                    new HarmonyMethod(GetType(), nameof(GetActualCapacity_Prefix)));
            }
        }

        public static bool checkForAction_Prefix(Chest __instance, ref bool __result, Farmer who,
            bool justCheckingForActivity)
        {
            if (justCheckingForActivity
                || !__instance.playerChest.Value
                || !Game1.didPlayerJustRightClick(true))
                return true;

            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null || config.IsVanilla)
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
        public static bool addItem_Prefix(Chest __instance, ref Item __result, Item item)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (!ReferenceEquals(__instance, item) &&
                (config == null || config.IsAllowed(item) && !config.IsBlocked(item)))
                return true;

            __result = item;
            return false;
        }

        /// <summary>Draw chest with playerChoiceColor.</summary>
        public static bool draw_Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || __instance.modData.ContainsKey(CustomChestTypesKey)
                || !__instance.playerChest.Value)
                return true;

            var draw_x = (float) x;
            var draw_y = (float) y;
            if (__instance.localKickStartTile.HasValue)
            {
                draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
            }

            var globalPosition = new Vector2(draw_x * 64f, (draw_y - 1f) * 64f);
            var layerDepth = Math.Max(0.0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;

            __instance.Draw(spriteBatch, Game1.GlobalToLocal(Game1.viewport, globalPosition), alpha, layerDepth);
            return false;
        }

        public static bool drawLocal_Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha, bool local)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || __instance.modData.ContainsKey(CustomChestTypesKey)
                || !__instance.playerChest.Value
                || !local)
                return true;

            __instance.Draw(spriteBatch, new Vector2(x, y - 64), alpha);
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