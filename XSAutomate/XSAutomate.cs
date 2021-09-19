namespace XSAutomate
{
    using System.Diagnostics.CodeAnalysis;
    using Common.Integrations.XSLite;
    using CommonHarmony;
    using HarmonyLib;
    using Pathoschild.Stardew.Automate;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    public class XSAutomate : Mod
    {
        private static IReflectionHelper Reflection;
        private static XSLiteIntegration XSLite;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            XSAutomate.XSLite = new XSLiteIntegration(helper.ModRegistry);
            XSAutomate.Reflection = helper.Reflection;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Monitor.LogOnce("Patching Automate for Filtered Items");
            var harmony = new Harmony(this.ModManifest.UniqueID);
            harmony.Patch(
                new AssemblyPatch("Automate").Method("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer", "Store"),
                new HarmonyMethod(typeof(XSAutomate), nameof(XSAutomate.StorePrefix)));
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool StorePrefix(Chest ___Chest, object stack)
        {
            Item item = XSAutomate.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
            return XSAutomate.XSLite.API.AcceptsItem(___Chest, item);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var automate = this.Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
            automate.AddFactory(new AutomationFactory());
        }
    }
}