using System;
using System.Linq;
using Harmony;
using Pathoschild.Stardew.Automate;
using StardewModdingAPI;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    internal class AutomatePatch : HarmonyPatch
    {
        private readonly Type _type;
        private readonly bool _isAutomateLoaded;
        private static IReflectionHelper Reflection;
        internal AutomatePatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection, bool isAutomateLoaded)
            : base(monitor, config)
        {
            Reflection = reflection;
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
                    .Find(m => m.GetParameters().Any(p => p.ParameterType == typeof(ITrackedStack)));
                harmony.Patch(methodInfo, new HarmonyMethod(GetType(), nameof(Store_Prefix)));
            }
        }

        public static bool Store_Prefix(object __instance, ITrackedStack stack)
        {
            var reflectedChest = Reflection.GetField<Chest>(__instance, "Chest");
            var config = ExpandedStorage.GetConfig(reflectedChest.GetValue());
            if (config == null)
                return true;
            if (config.AllowList.Any() && !config.AllowList.Contains(stack.Sample.Category))
                return false;
            if (config.BlockList.Any() && config.BlockList.Contains(stack.Sample.Category))
                return false;
            return true;
        }
    }
}