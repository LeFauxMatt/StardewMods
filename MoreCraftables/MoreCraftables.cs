using System.Collections.Generic;
using Common.PatternPatches;
using MoreCraftables.API;
using MoreCraftables.Framework;
using MoreCraftables.Framework.Models;
using MoreCraftables.Framework.Patches;
using StardewModdingAPI;

// ReSharper disable UnusedType.Global

namespace MoreCraftables
{
    public class MoreCraftables : Mod
    {
        private readonly IDictionary<string, IHandledObject> _handledObjects = new Dictionary<string, IHandledObject>();
        private ModConfig _config;
        private IMoreCraftablesAPI _moreCraftablesAPI;

        public override void Entry(IModHelper helper)
        {
            _config = Helper.ReadConfig<ModConfig>();
            
            _moreCraftablesAPI = new MoreCraftablesAPI(Helper, Monitor, _handledObjects);
            var unused = new ContentLoader(Helper.ContentPacks, Monitor, _moreCraftablesAPI);

            // Patches
            new Patcher<ModConfig>(ModManifest.UniqueID).ApplyAll(
                new ItemPatch(Monitor, _config, _handledObjects),
                new ObjectPatch(Monitor, _config, _handledObjects)
            );
        }

        public override object GetApi()
        {
            return _moreCraftablesAPI;
        }
    }
}