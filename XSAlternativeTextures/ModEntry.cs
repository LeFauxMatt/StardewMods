namespace XSAlternativeTextures
{
    using System.Collections.Generic;
    using System.Linq;
    using AlternativeTextures.Framework.Models;
    using Common.Integrations.XSLite;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using IAlternativeTexturesAPI = AlternativeTextures.Framework.Interfaces.API.IApi;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class ModEntry : Mod, IAssetEditor
    {
        private const string ModDataKey = "XSAlternativeTextures/Storages";
        private static readonly HashSet<string> VanillaNames = new()
        {
            "Chest",
            "Stone Chest",
            "Junimo Chest",
            "Mini-Shipping Bin",
            "Mini-Fridge",
            "Auto-Grabber",
        };
        private IAlternativeTexturesAPI _alternativeTexturesAPI;
        private List<string> _expectedStorages;
        private IList<string> _loadedStorages;
        private Texture2D _placeholder;
        private bool _registeredTextures;
        private XSLiteIntegration _xsLite;

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetName.StartsWith("AlternativeTextures") && asset.AssetName.Contains("ExpandedStorage");
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var editor = asset.AsImage();
            editor.ExtendImage(80, this._expectedStorages.Count * 32);
            for (var i = 0; i < this._expectedStorages.Count; i++)
            {
                var storageName = this._expectedStorages[i];
                var texture = this._loadedStorages.Contains(storageName)
                    ? this.Helper.Content.Load<Texture2D>($"ExpandedStorage/SpriteSheets/{storageName}", ContentSource.GameContent)
                    : this._placeholder;

                editor.PatchImage(texture, new Rectangle(0, 0, 16, 32), new Rectangle(0, i * 32, 16, 32));
                if (texture.Width == 80)
                {
                    editor.PatchImage(texture, new Rectangle(0, 0, 80, 32), new Rectangle(16, i * 32, 80, 32));
                }
                else
                {
                    for (var x = 16; x < 80; x += 16)
                    {
                        editor.PatchImage(texture, new Rectangle(0, 0, 16, 32), new Rectangle(x, i * 32, 16, 32));
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.Helper.Events.Multiplayer.PeerConnected += this.OnPeerConnected;
            this.Helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            bool IsCorrectTexture(string storage)
            {
                var texture = this.Helper.Content.Load<Texture2D>($"ExpandedStorage/SpriteSheets/{storage}", ContentSource.GameContent);
                return texture.Width is 16 or 80 && texture.Height is 32 or 96;
            }

            this._placeholder = this.Helper.Content.Load<Texture2D>("assets/texture.png");
            this._alternativeTexturesAPI = this.Helper.ModRegistry.GetApi<IAlternativeTexturesAPI>("PeacefulEnd.AlternativeTextures");
            this._xsLite = new(this.Helper.ModRegistry);
            this._loadedStorages = this._xsLite.API.GetAllStorages().Except(ModEntry.VanillaNames).Where(IsCorrectTexture).ToList();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            this.RegisterAlternativeTextures();
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (!Context.IsMainPlayer || e.Peer.IsHost)
            {
                return;
            }

            this.Helper.Multiplayer.SendMessage(
                Game1.player.modData[ModEntry.ModDataKey],
                ModEntry.ModDataKey,
                new[]
                {
                    this.ModManifest.UniqueID,
                },
                new[]
                {
                    e.Peer.PlayerID,
                });
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (Context.IsMainPlayer || e.FromModID != this.ModManifest.UniqueID || e.Type != ModEntry.ModDataKey)
            {
                return;
            }

            Game1.player.modData[ModEntry.ModDataKey] = e.ReadAs<string>();
            this.RegisterAlternativeTextures();
        }

        private void RegisterAlternativeTextures()
        {
            if (this._registeredTextures)
            {
                return;
            }

            this._expectedStorages = Game1.player.modData.TryGetValue(ModEntry.ModDataKey, out var storages) ? storages.Split('/').ToList() : new();
            this._expectedStorages.AddRange(this._loadedStorages.Except(this._expectedStorages));
            var model = new AlternativeTextureModel
            {
                ItemName = "Chest",
                Type = "Craftable",
                TextureWidth = 16,
                TextureHeight = 32,
                Variations = this._expectedStorages.Count,
                ManualVariations = this._expectedStorages.Select(
                    storageName => new VariationModel
                    {
                        Id = this._expectedStorages.IndexOf(storageName),
                        Keywords = new()
                        {
                            storageName,
                        },
                    }).ToList(),
                EnableContentPatcherCheck = true,
            };

            var textures = Enumerable.Repeat(this._placeholder, this._expectedStorages.Count).ToList();
            this._alternativeTexturesAPI.AddAlternativeTexture(model, "ExpandedStorage", textures);
            Game1.player.modData[ModEntry.ModDataKey] = string.Join("/", this._expectedStorages);
            this._registeredTextures = true;
        }
    }
}