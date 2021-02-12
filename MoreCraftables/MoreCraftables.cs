using System.Collections.Generic;
using MoreCraftables.Framework;
using MoreCraftables.Framework.Models;
using MoreCraftables.Framework.Patches;
using Common.PatternPatches;
using StardewModdingAPI;

namespace MoreCraftables
{
    public class MoreCraftables : Mod
    {
        private MoreCraftablesAPI _moreCraftablesAPI;

        private readonly IList<HandledTypeWrapper> _handledTypes = new List<HandledTypeWrapper>();
        private readonly IList<ObjectFactoryWrapper> _objectFactories = new List<ObjectFactoryWrapper>();
        private ModConfig _config;
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _moreCraftablesAPI = new MoreCraftablesAPI(_handledTypes, _objectFactories);

            // Patches
            new Patcher<ModConfig>(ModManifest.UniqueID).ApplyAll(
                new ItemPatch(Monitor, _config, _handledTypes),
                new ObjectPatch(Monitor, _config, _handledTypes, _objectFactories)
            );
        }

        public override object GetApi() => _moreCraftablesAPI;
    }
}