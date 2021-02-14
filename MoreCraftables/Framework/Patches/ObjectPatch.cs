using System.Collections.Generic;
using System.Linq;
using Common.PatternPatches;
using Harmony;
using Microsoft.Xna.Framework;
using MoreCraftables.API;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable InconsistentNaming

namespace MoreCraftables.Framework.Patches
{
    internal class ObjectPatch : Patch<ModConfig>
    {
        private static IDictionary<string, IHandledObject> _handledTypes;

        public ObjectPatch(IMonitor monitor, ModConfig config, IDictionary<string, IHandledObject> handledTypes)
            : base(monitor, config)
        {
            _handledTypes = handledTypes;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                new HarmonyMethod(GetType(), nameof(PlacementActionPrefix))
            );
        }

        public static bool PlacementActionPrefix(Object __instance,
            ref bool __result,
            GameLocation location,
            int x,
            int y,
            Farmer who)
        {
            // Verify pos is not already occupied
            var pos = new Vector2(x, y) / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;

            if (location.objects.ContainsKey(pos))
                return true;

            // Verify this is a handled item type
            var handledType = _handledTypes
                .Select(t => t.Value)
                .LastOrDefault(t => t.IsHandledItem(__instance));
            
            if (handledType == null)
                return true;

            // Get instance of object to place
            var obj = handledType.CreateInstance(__instance, location, pos);
            if (obj == null)
            {
                __result = false;
                return false;
            }
            
            // Copy properties from instance
            obj.ParentSheetIndex = __instance.ParentSheetIndex;
            obj.Name = __instance.Name;
            obj.Price = __instance.Price;
            obj.Edibility =  __instance.Edibility;
            obj.Type = __instance.Type;
            obj.Category = __instance.Category;
            obj.Fragility = __instance.Fragility;
            obj.setOutdoors.Value = __instance.setOutdoors.Value;
            obj.setIndoors.Value = __instance.setIndoors.Value;
            obj.isLamp.Value = __instance.isLamp.Value;

            // Copy modData from original object
            foreach (var modData in __instance.modData)
                obj.modData.CopyFrom(modData);

            // Place object at location
            location.objects.Add(pos, obj);
            __instance.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;
            __result = true;
            return false;
        }
    }
}