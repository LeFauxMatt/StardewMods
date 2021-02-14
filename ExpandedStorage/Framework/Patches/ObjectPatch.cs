using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Common.PatternPatches;
using ExpandedStorage.Common.Extensions;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

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
            if (!Config.AllowCarryingChests)
                return;

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.drawWhenHeld)),
                new HarmonyMethod(GetType(), nameof(DrawWhenHeldPrefix))
            );
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