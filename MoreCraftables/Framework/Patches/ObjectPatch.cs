using System.Collections.Generic;
using System.Linq;
using Common.PatternPatches;
using Harmony;
using Microsoft.Xna.Framework;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable InconsistentNaming

namespace MoreCraftables.Framework.Patches
{
    public class ObjectPatch : Patch<ModConfig>
    {
        private static IList<HandledTypeWrapper> _handledTypes;
        private static IList<ObjectFactoryWrapper> _objectFactories;

        public ObjectPatch(IMonitor monitor, ModConfig config, IList<HandledTypeWrapper> handledTypes, IList<ObjectFactoryWrapper> objectFactories)
            : base(monitor, config)
        {
            _handledTypes = handledTypes;
            _objectFactories = objectFactories;
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
            var handledType = _handledTypes.FirstOrDefault(t => t.HandledType.IsHandledItem(__instance));
            if (handledType == null)
                return true;

            // Verify a factory exists for this handled type
            var objectFactory = _objectFactories
                .Where(f => f.ObjectFactory.IsHandledType(handledType.HandledType))
                .OrderByDescending(f => f.ModUniqueId.Equals(handledType.ModUniqueId))
                .ThenByDescending(f => f.ModUniqueId.Equals("furyx639.MoreCraftables"))
                .FirstOrDefault();

            // Get instance of object to place
            var obj = objectFactory?.ObjectFactory.CreateInstance(handledType.HandledType, __instance, location, pos);
            if (obj == null)
            {
                __result = false;
                return false;
            }

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