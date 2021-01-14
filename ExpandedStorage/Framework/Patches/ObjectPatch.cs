using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using StardewModdingAPI;
using StardewValley.Objects;
using SDVObject = StardewValley.Object;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ObjectPatch : HarmonyPatch
    {
        private readonly Type _objectType = typeof(SDVObject);
        internal ObjectPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        
        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (!Config.AllowCarryingChests)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(_objectType, nameof(SDVObject.getDescription)),
                postfix: new HarmonyMethod(GetType(), nameof(getDescription_Postfix)));
        }

        /// <summary>Adds count of chests contents to its description.</summary>
        public static void getDescription_Postfix(SDVObject __instance, ref string __result)
        {
            if (!(__instance is Chest chest) || chest.ParentSheetIndex != 130 && !ExpandedStorage.HasConfig(chest.name))
                return;
            if (chest.items?.Count > 0)
                __result += "\n" + $"Contains {chest.items.Count} items.";
        }
    }
}