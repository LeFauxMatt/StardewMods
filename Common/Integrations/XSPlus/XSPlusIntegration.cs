using StardewModdingAPI;

namespace Common.Integrations.XSPlus
{
    internal class XSPlusIntegration : ModIntegration<IXSPlusAPI>
    {
        public XSPlusIntegration(IModRegistry modRegistry)
            : base(modRegistry, "furyx639.XSPlus")
        {
        }
    }
}