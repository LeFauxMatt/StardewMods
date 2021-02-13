using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.PatternPatches;
using ExpandedStorage.Framework.Extensions;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Global

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ObjectPatch : Patch<ModConfig>
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new();

        internal ObjectPatch(IMonitor monitor, ModConfig config)
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
            // harmony.Patch(
            //     AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
            //     new HarmonyMethod(GetType(), nameof(PlacementActionPrefix))
            // );

            if (!Config.AllowCarryingChests)
                return;

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.drawWhenHeld)),
                new HarmonyMethod(GetType(), nameof(DrawWhenHeldPrefix))
            );
        }

        public static bool PlacementActionPrefix(Object __instance,
            ref bool __result,
            GameLocation location,
            int x,
            int y,
            Farmer who)
        {
            var config = ExpandedStorage.GetConfig(__instance);

            // Disallow non-placeable storages
            if (config != null && !config.IsPlaceable)
            {
                __result = false;
                return false;
            }

            if (config == null)
                return true;

            var pos = new Vector2(x, y) / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;
            if (location.objects.ContainsKey(pos) || location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                __result = false;
                return false;
            }

            // Place Expanded Storage Chest
            var chest = __instance.ToChest(config);
            chest.shakeTimer = 50;
            chest.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;

            // Automate preferences
            if (config.ModData != null)
                foreach (var modData in config.ModData)
                    if (!chest.modData.ContainsKey(modData.Key))
                        chest.modData.Add(modData.Key, modData.Value);

            location.objects.Add(pos, chest);
            location.playSound("hammer");
            __result = true;
            return false;
        }

        public static bool DrawWhenHeldPrefix(Object __instance,
            SpriteBatch spriteBatch,
            Vector2 objectPosition,
            Farmer f)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || __instance is not Chest chest
                || !chest.playerChest.Value
                || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;

            chest.Draw(spriteBatch, objectPosition, Vector2.Zero);
            return false;
        }
    }
}