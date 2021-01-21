using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InvertIf")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class AutomatePatch : HarmonyPatch
    {
        private readonly Type _type;
        private readonly bool _isAutomateLoaded;
        private static IReflectionHelper _reflection;
        internal AutomatePatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection, bool isAutomateLoaded)
            : base(monitor, config)
        {
            _reflection = reflection;
            _isAutomateLoaded = isAutomateLoaded;
            
            if (!isAutomateLoaded)
                return;
            
            var automateAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Automate,"));
            _type = automateAssembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer");
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            if (_isAutomateLoaded && Config.AllowRestrictedStorage)
            {
                Monitor.Log("Patching Automate");
                var methodInfo = AccessTools.GetDeclaredMethods(_type)
                    .Find(m => m.Name.Equals("Store", StringComparison.OrdinalIgnoreCase));
                harmony.Patch(methodInfo, new HarmonyMethod(GetType(), nameof(Store_Prefix)));
            }
        }

        public static bool Store_Prefix(object __instance, ITrackedStack stack)
        {
            var reflectedChest = _reflection.GetField<Chest>(__instance, "Chest");
            var reflectedSample = _reflection.GetProperty<Item>(stack, "Sample");
            var config = ExpandedStorage.GetConfig(reflectedChest.GetValue());
            return config == null || config.IsAllowed(reflectedSample.GetValue()) && !config.IsBlocked(reflectedSample.GetValue());
        }
    }
}