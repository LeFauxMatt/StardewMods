using System;
using System.Diagnostics.CodeAnalysis;
using Harmony;
using StardewModdingAPI;
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
            if (Config.AllowCarryingChests)
            {
                harmony.Patch(AccessTools.Method(_objectType, nameof(StardewValley.Object.getDescription)),
                    postfix: new HarmonyMethod(GetType(), nameof(getDescription_Postfix)));
            }
        }

        /// <summary>Adds count of chests contents to its description.</summary>
        public static void getDescription_Postfix(StardewValley.Object __instance, ref string __result)
        {
            if (!(__instance is Chest chest) || !ExpandedStorage.HasConfig(__instance))
                return;
            if (chest.items?.Count > 0)
                __result += "\n" + $"Contains {chest.items.Count} items.";
        }
    }
}