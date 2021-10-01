namespace XSAutomate
{
    using Pathoschild.Stardew.Automate;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;

    public class XSAutomate : Mod
    {

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var automate = this.Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
            automate.AddFactory(new AutomationFactory());
        }
    }
}