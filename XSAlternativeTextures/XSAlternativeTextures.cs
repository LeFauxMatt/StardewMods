namespace XSAlternativeTextures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AlternativeTextures.Framework.Models;
    using Common.Integrations.XSLite;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using IAlternativeTexturesAPI = AlternativeTextures.Framework.Interfaces.API.IApi;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class XSAlternativeTextures : Mod, IAssetEditor
    {
        private readonly IList<string> _storages = new List<string>();
        private IAlternativeTexturesAPI _alternativeTexturesAPI = null!;
        private XSLiteIntegration _xsLite = null!;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            this._xsLite = new XSLiteIntegration(helper.ModRegistry);
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetName.StartsWith("AlternativeTextures") && asset.AssetName.Contains("ExpandedStorage");
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            IAssetDataForImage editor = asset.AsImage();
            editor.ExtendImage(80, this._storages.Count * 32);
            for (int i = 0; i < this._storages.Count; i++)
            {
                var texture = this.Helper.Content.Load<Texture2D>($"ExpandedStorage/SpriteSheets/{this._storages[i]}", ContentSource.GameContent);
                editor.PatchImage(texture, new Rectangle(0, 0, 16, 32), new Rectangle(0,  i * 32, 16, 32));
                editor.PatchImage(texture, new Rectangle(0, 0, 80, 32), new Rectangle(16,  i * 32, 80, 32));
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._alternativeTexturesAPI = this.Helper.ModRegistry.GetApi<IAlternativeTexturesAPI>("PeacefulEnd.AlternativeTextures");
            var model = new AlternativeTextureModel
            {
                ItemName = "Chest",
                Type = "Craftable",
                TextureWidth = 16,
                TextureHeight = 32,
                Variations = this._storages.Count,
                EnableContentPatcherCheck = true,
            };
            var textures = new List<Texture2D>();
            Texture2D placeholder = this.Helper.Content.Load<Texture2D>("assets/texture.png");
            foreach (string storageName in this._xsLite.API.GetAllStorages().OrderBy(storageName => storageName))
            {
                Texture2D texture = null!;
                try
                {
                    texture = this.Helper.Content.Load<Texture2D>($"ExpandedStorage/SpriteSheets/{storageName}", ContentSource.GameContent);
                }
                catch (Exception)
                {
                    // ignored
                }

                if (texture is null || texture.Width != 80 || (texture.Height != 32 && texture.Height != 96))
                {
                    continue;
                }

                textures.Add(placeholder);
                this._storages.Add(storageName);
                model.ManualVariations.Add(new VariationModel
                {
                    Id = this._storages.Count - 1,
                    Keywords = new List<string> { storageName },
                });
            }

            this._alternativeTexturesAPI.AddAlternativeTexture(model, "ExpandedStorage", textures);
        }
    }
}