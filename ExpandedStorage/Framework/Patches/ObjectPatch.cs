using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ObjectPatch : HarmonyPatch
    {
        private readonly Type _objectType = typeof(StardewValley.Object);
        internal ObjectPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        
        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_objectType, nameof(StardewValley.Object.placementAction)),
                new HarmonyMethod(GetType(), nameof(PlacementAction)));
            
            if (Config.AllowCarryingChests)
            {
                harmony.Patch(AccessTools.Method(_objectType, nameof(StardewValley.Object.getDescription)),
                    postfix: new HarmonyMethod(GetType(), nameof(getDescription_Postfix)));
            }
        }
        
        public static bool PlacementAction(StardewValley.Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config != null && !config.IsPlaceable)
            {
                __result = false;
                return false;
            }
            
            if (config == null || ExpandedStorage.IsVanilla(__instance))
                return true;
            

            var pos = new Vector2(x, y) / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;
            if (location.objects.ContainsKey(pos) || location is MineShaft || location is VolcanoDungeon)
                return true;
            
            __instance.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;
            
            // Place Expanded Storage Chest
            if (__instance is not Chest chest)
                chest = new Chest(true, pos, __instance.ParentSheetIndex)
                {
                    name = __instance.Name,
                    shakeTimer = 50
                };
            chest.resetLidFrame();
            foreach (var modData in __instance.modData)
                chest.modData.CopyFrom(modData);
            
            location.objects.Add(pos, chest);
            location.playSound("hammer");
            __result = true;
            return false;
        }

        /// <summary>Adds count of chests contents to its description.</summary>
        public static void getDescription_Postfix(StardewValley.Object __instance, ref string __result)
        {
            if (__instance is not Chest chest || !ExpandedStorage.HasConfig(__instance))
                return;
            if (chest.items?.Count > 0)
                __result += "\n" + $"Contains {chest.items.Count} items.";
        }
    }
}