namespace MoreChests.Services
{
    using System.Collections.Generic;
    using Common.Services;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;

    internal class AssetHandler : BaseService, IAssetLoader
    {
        private readonly IDictionary<string, Texture2D> _cachedTextures = new Dictionary<string, Texture2D>();
        private readonly IContentHelper _content;
        private readonly IDictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

        internal AssetHandler(ServiceManager serviceManager)
            : base("AssetHandler")
        {
            // Init
            this._content = serviceManager.Helper.Content;
            serviceManager.Helper.Content.AssetLoaders.Add(this);

            // Events
            serviceManager.Helper.Events.GameLoop.DayEnding += this.OnDayEnding;
        }

        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return this._textures.ContainsKey(asset.AssetName);
        }

        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)this._textures[asset.AssetName];
        }

        public bool TryGetAsset(string name, out Texture2D texture)
        {
            return this._cachedTextures.TryGetValue(name, out texture);
        }

        public void AddAsset(string name, Texture2D texture)
        {
            this._textures.Add(PathUtilities.NormalizeAssetName($"{ModEntry.ModPrefix}/{name}"), texture);
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            this._cachedTextures.Clear();
            foreach (var path in this._textures.Keys)
            {
                var key = PathUtilities.GetSegments(path)[1];
                this._cachedTextures.Add(key, this._content.Load<Texture2D>(path, ContentSource.GameContent));
            }
        }
    }
}