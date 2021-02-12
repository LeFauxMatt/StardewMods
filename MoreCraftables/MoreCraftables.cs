using StardewModdingAPI;

namespace MoreCraftables
{
    public class MoreCraftables : Mod
    {
        private MoreCraftablesAPI _moreCraftablesAPI;

        public override void Entry(IModHelper helper)
        {
            _moreCraftablesAPI = new MoreCraftablesAPI();
        }

        public override object GetApi() => _moreCraftablesAPI;
    }
}