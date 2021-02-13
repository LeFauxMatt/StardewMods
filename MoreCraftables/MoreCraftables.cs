using System.Collections.Generic;
using Common.PatternPatches;
using MoreCraftables.Framework;
using MoreCraftables.Framework.API;
using MoreCraftables.Framework.Models;
using MoreCraftables.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;

// ReSharper disable UnusedType.Global

namespace MoreCraftables
{
    public class MoreCraftables : Mod
    {
        private readonly IList<IHandledType> _handledTypes = new List<IHandledType>();
        private readonly IList<IObjectFactory> _objectFactories = new List<IObjectFactory>();
        private ModConfig _config;
        private IMoreCraftablesAPI _moreCraftablesAPI;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _moreCraftablesAPI = new MoreCraftablesAPI(Monitor, _handledTypes, _objectFactories);

            // Register Default Factory
            _moreCraftablesAPI.AddObjectFactory(new ObjectFactory());

            // Patches
            new Patcher<ModConfig>(ModManifest.UniqueID).ApplyAll(
                new ItemPatch(Monitor, _config, _handledTypes),
                new ObjectPatch(Monitor, _config, _handledTypes, _objectFactories)
            );
            
            // Events
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            
        }

        public override object GetApi()
        {
            Monitor.Log("API Requested");
            return new MoreCraftablesAPI(Monitor, _handledTypes, _objectFactories);
        }
    }
}