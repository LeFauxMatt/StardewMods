namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Newtonsoft.Json;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <summary>Custom storages that are managed by the Expanded Storage mod.</summary>
    internal class Storage
    {
        /// <summary>The asset loader used to add this object into the game.</summary>
        public enum AssetFormat
        {
            /// <summary>Assets loaded by Dynamic Game Assets.</summary>
            DynamicGameAssets,

            /// <summary>Assets loaded by Json Assets.</summary>
            JsonAssets,

            /// <summary>Assets loaded through the standard Content Pipeline without DGA or JA.</summary>
            Vanilla,
        }
        private string _name;
        private string _path;
        private Texture2D _texture;

        /// <summary>Initializes a new instance of the <see cref="Storage" /> class.</summary>
        /// <param name="specialChestType">The type of chest this storage will be created as.</param>
        /// <param name="allowList">Items this storage is able to accept.</param>
        /// <param name="blockList">Items this storage is not able to accept.</param>
        [JsonConstructor]
        public Storage(string specialChestType, HashSet<string> allowList, HashSet<string> blockList)
        {
            this.SpecialChestType = Enum.TryParse(specialChestType, out Chest.SpecialChestTypes specialChestTypes) ? specialChestTypes : Chest.SpecialChestTypes.None;
            this.FilterItems = new();
            if (blockList is not null)
            {
                foreach (var blockItem in blockList)
                {
                    this.FilterItems.Add(blockItem, false);
                }
            }

            if (allowList is not null)
            {
                foreach (var allowItem in allowList)
                {
                    this.FilterItems.Add(allowItem, true);
                }
            }
        }

        public static Func<string, ContentSource, Texture2D> LoadContent { get; set; }

        public AssetFormat Format { get; set; } = AssetFormat.Vanilla;

        public string Name
        {
            get => this._name;
            set
            {
                this._name = value;
                this._path = $"ExpandedStorage/SpriteSheets/{this._name}";
            }
        }

        public bool IsFridge { get; set; } = false;

        public bool HeldStorage { get; set; } = false;

        public string Image { get; set; }

        public int Frames { get; set; } = 5;

        public bool PlayerColor { get; set; } = false;

        public bool PlayerConfig { get; set; } = true;

        public int Depth { get; set; } = 0;

        public int Capacity { get; set; } = 0;

        public string OpenSound { get; set; } = "openChest";

        public float OpenNearby { get; set; } = 0;

        public string OpenNearbySound { get; set; } = "doorCreak";

        public string CloseNearbySound { get; set; } = "doorCreakReverse";

        public HashSet<string> EnabledFeatures { get; set; } = new();

        public HashSet<string> DisabledFeatures { get; set; } = new();

        public Dictionary<string, bool> FilterItems { get; set; }

        public IDictionary<string, string> ModData { get; set; } = new Dictionary<string, string>();

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public IManifest Manifest { get; set; }

        public ModConfig Config { get; set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int TileWidth { get; private set; } = 1;

        public int TileHeight { get; private set; } = 1;

        public float ScaleSize { get; private set; }

        public Chest.SpecialChestTypes SpecialChestType { get; }

        private Texture2D Texture
        {
            get => this._texture;
            set
            {
                this._texture = value;
                this.Width = this._texture.Width / Math.Max(1, this.Frames);
                this.Height = this.PlayerColor ? this._texture.Height / 3 : this._texture.Height;
                this.TileWidth = this.Width / 16;
                this.TileHeight = (this.Depth > 0 ? this.Depth : this.Height - 16) / 16;
                var tilesWide = this.Width / 16f;
                var tilesHigh = this.Height / 16f;
                this.ScaleSize = tilesWide switch
                {
                    >= 7 => 0.5f,
                    >= 6 => 0.66f,
                    >= 5 => 0.75f,
                    _ => tilesHigh switch
                    {
                        >= 5 => 0.8f,
                        >= 3 => 1f,
                        _ => tilesWide switch
                        {
                            <= 2 => 2f,
                            <= 4 => 1f,
                            _ => 0.1f,
                        },
                    },
                };
            }
        }

        /// <summary>Clears cached textures to reload them.</summary>
        public void ReloadTexture(Texture2D texture = null)
        {
            if (this.Format == AssetFormat.Vanilla)
            {
                return;
            }

            texture ??= Storage.LoadContent(this._path, ContentSource.GameContent);
            if (texture is null && !XSLite.Textures.TryGetValue(this._name, out texture))
            {
                return;
            }

            this.Texture = texture;
        }

        /// <summary>Draws an expanded storage.</summary>
        /// <param name="obj">The object to draw.</param>
        /// <param name="currentFrame">Which frame of lid animation to draw the object at.</param>
        /// <param name="spriteBatch">The sprite batch to draw to.</param>
        /// <param name="pos">The coordinates to draw the object at.</param>
        /// <param name="origin">The origin to align the object to.</param>
        /// <param name="alpha">The alpha to draw the object at.</param>
        /// <param name="layerDepth">The depth to draw the object relative to other objects.</param>
        /// <param name="scaleSize">The size of the object to draw.</param>
        /// <returns>Returns true if the object could be drawn.</returns>
        public bool Draw(SObject obj, int currentFrame, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f)
        {
            if (this.Texture is null)
            {
                return false;
            }

            var chest = obj as Chest;
            if (currentFrame >= (chest?.startingLidFrame.Value ?? 0))
            {
                currentFrame -= chest?.startingLidFrame.Value ?? 0;
            }

            var drawColored = this.PlayerColor && chest is not null && !chest.playerChoiceColor.Value.Equals(Color.Black);
            var startLayer = drawColored && this.PlayerColor ? 1 : 0;
            var endLayer = startLayer == 0 ? 1 : 3;
            for (var layer = startLayer; layer < endLayer; layer++)
            {
                var color = (layer % 2 == 0 || !drawColored) && chest is not null
                    ? chest.Tint
                    : chest?.playerChoiceColor.Value ?? Color.White;

                spriteBatch.Draw(this.Texture, pos + Storage.ShakeOffset(obj, -1, 2), new Rectangle(this.Width * currentFrame, this.Height * layer, this.Width, this.Height), color * alpha, 0f, origin, scaleSize, SpriteEffects.None, layerDepth + (1 + layer - startLayer) * 1E-05f);
            }

            return true;
        }

        private static Vector2 ShakeOffset(SObject instance, int minValue, int maxValue)
        {
            return instance.shakeTimer > 0
                ? new(Game1.random.Next(minValue, maxValue), 0)
                : Vector2.Zero;
        }
    }
}