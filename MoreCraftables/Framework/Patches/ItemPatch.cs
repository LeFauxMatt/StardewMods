using System.Collections.Generic;
using System.Linq;
using Common.PatternPatches;
using Harmony;
using MoreCraftables.API;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;
using StardewValley;

// ReSharper disable InconsistentNaming

namespace MoreCraftables.Framework.Patches
{
    internal class ItemPatch : Patch<ModConfig>
    {
        private static IDictionary<string, IHandledObject> _handledTypes;

        public ItemPatch(IMonitor monitor, ModConfig config, IDictionary<string, IHandledObject> handledTypes)
            : base(monitor, config)
        {
            _handledTypes = handledTypes;
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                new HarmonyMethod(GetType(), nameof(CanStackWithPrefix))
            );
        }

        public static bool CanStackWithPrefix(Item __instance, ref bool __result, ISalable other)
        {
            // Verify this is a handled item type
            var handledType = _handledTypes
                .Select(t => t.Value)
                .LastOrDefault(t => t.IsHandledItem(__instance));
            if (handledType == null)
                return true;

            // Verify instance is Object
            if (__instance is not Object obj)
                return true;

            // Verify other item is Object
            if (other is not Object otherObj)
                return true;

            // Yield return to handled type
            __result = __instance.maximumStackSize() > 1
                       && other.maximumStackSize() > 1
                       && obj.ParentSheetIndex == otherObj.ParentSheetIndex
                       && obj.bigCraftable.Value == otherObj.bigCraftable.Value
                       && obj.Quality == otherObj.Quality
                       && handledType.CanStackWith(__instance, (Item) other);
            return false;
        }
    }
}