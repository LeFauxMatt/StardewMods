using System.Diagnostics.CodeAnalysis;
using Common.Integrations.XSLite;
using CommonHarmony;
using HarmonyLib;
using Pathoschild.Stardew.Automate;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace XSAutomate
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class XSAutomate : Mod
    {
        private static IReflectionHelper _reflection;
        private static XSLiteIntegration _xsLite;
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            _xsLite = new XSLiteIntegration(helper.ModRegistry);
            _reflection = helper.Reflection;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Monitor.LogOnce("Patching Automate for Filtered Items");
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                new AssemblyPatch("Automate").Method("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer", "Store"),
                new HarmonyMethod(typeof(XSAutomate), nameof(XSAutomate.StorePrefix))
            );
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var automate = Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
            automate.AddFactory(new AutomationFactoryController());
        }
        private static bool StorePrefix(Chest ___Chest, object stack)
        {
            var item = _reflection.GetProperty<Item>(stack, "Sample").GetValue();
            return _xsLite.API.AcceptsItem(___Chest, item);
        }
    }
}