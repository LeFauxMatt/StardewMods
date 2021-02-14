using System;
using MoreCraftables.API;
using StardewModdingAPI;

namespace MoreCraftables.Framework
{
    internal class ContentLoader
    {
        private readonly IContentPackHelper _contentPackHelper;
        private readonly IMonitor _monitor;
        private readonly IMoreCraftablesAPI _moreCraftablesAPI;

        internal ContentLoader(IContentPackHelper contentPackHelper, IMonitor monitor, IMoreCraftablesAPI moreCraftablesAPI)
        {
            _contentPackHelper = contentPackHelper;
            _monitor = monitor;
            _moreCraftablesAPI = moreCraftablesAPI;
            _moreCraftablesAPI.ReadyToLoad += OnReadyToLoad;
        }

        /// <summary>Load More Craftables Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnReadyToLoad(object sender, EventArgs e)
        {
            var contentPacks = _contentPackHelper.GetOwned();
            _monitor.Log("Loading More Craftables Content", LogLevel.Info);
            foreach (var contentPack in contentPacks) _moreCraftablesAPI.LoadContentPack(contentPack);
        }
    }
}