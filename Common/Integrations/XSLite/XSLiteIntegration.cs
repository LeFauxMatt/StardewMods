using StardewModdingAPI;

namespace Common.Integrations.XSLite
{
    internal class XSLiteIntegration : ModIntegration<IXSLiteAPI>
    {
        public XSLiteIntegration(IModRegistry modRegistry)
            : base(modRegistry, "furyx639.ExpandedStorage")
        {
        }
    }
}