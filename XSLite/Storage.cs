namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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

        public AssetFormat Format { get; set; } = AssetFormat.Vanilla;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _path = $"ExpandedStorage/SpriteSheets/{_name}";
            }
        }

        public bool IsFridge { get; set; } = false;

        public bool HeldStorage { get; set; } = false;

        public string Image { get; set; }

        public int Frames { get; set; } = 5;

        public string Animation { get; set; } = "none";

        public bool PlayerColor { get; set; } = false;

        public bool PlayerConfig { get; set; } = true;

        public int Depth { get; set; } = 0;

        public int Capacity { get; set; } = 0;

        public string OpenSound { get; set; } = "openChest";

        public string PlaceSound { get; set; } = "axe";

        public string CarrySound { get; set; } = "pickUpItem";

        public float OpenNearby { get; set; } = 0;

        public string OpenNearbySound { get; set; } = "doorCreak";

        public string CloseNearbySound { get; set; } = "doorCreakReverse";

        public HashSet<string> EnabledFeatures { get; set; }

        public Dictionary<string, bool> FilterItems { get; set; }

        public IDictionary<string, string> ModData { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public IManifest Manifest { get; set; }

        public ModConfig Config { get; set; }

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
                float tilesWide = this.Width / 16f;
                float tilesHigh = this.Height / 16f;
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

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int TileWidth { get; private set; } = 1;

        public int TileHeight { get; private set; } = 1;

        public float ScaleSize { get; private set; }

        private Chest.SpecialChestTypes SpecialChestType { get; set; }

        private string _name;
        private string _path;
        private Texture2D _texture;

        /// <summary>Initializes a new instance of the <see cref="Storage"/> class.</summary>
        /// <param name="specialChestType">The type of chest this storage will be created as.</param>
        /// <param name="modData">Mod data to add when this storage is created.</param>
        /// <param name="allowList">Items this storage is able to accept.</param>
        /// <param name="blockList">Items this storage is not able to accept.</param>
        /// <param name="enabledFeatures">Special features to enable for this storage.</param>
        [JsonConstructor]
        public Storage(
            string specialChestType,
            IDictionary<string, string> modData,
            HashSet<string> allowList,
            HashSet<string> blockList,
            HashSet<string> enabledFeatures)
        {
            this.SpecialChestType = Enum.TryParse(specialChestType, out Chest.SpecialChestTypes specialChestTypes) ? specialChestTypes : Chest.SpecialChestTypes.None;
            this.ModData = modData ?? new Dictionary<string, string>();
            this.FilterItems = new Dictionary<string, bool>();
            if (blockList is not null)
            {
                foreach (string blockItem in blockList)
                {
                    this.FilterItems.Add(blockItem, true);
                }
            }

            if (allowList is not null)
            {
                foreach (string allowItem in allowList)
                {
                    this.FilterItems.Add(allowItem, true);
                }
            }

            this.EnabledFeatures = enabledFeatures ?? new HashSet<string>();
        }

        /// <summary>Clears cached textures to reload them.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        public void InvalidateCache(IContentHelper contentHelper)
        {
            var texture = contentHelper.Load<Texture2D>(this._path, ContentSource.GameContent);
            if (texture == null && !XSLite.Textures.TryGetValue(this._name, out texture))
            {
                return;
            }

            this.Texture = texture;
        }

        /// <summary>Perform an action for each tile the chest occupies.</summary>
        /// <param name="origin">The top-left coordinate of the object.</param>
        /// <param name="doAction">The action to perform for each tile.</param>
        public void ForEachPos(Vector2 origin, Action<Vector2> doAction)
        {
            this.ForEachPos((int)origin.X, (int)origin.Y, doAction);
        }

        /// <summary>Perform an action for each tile the chest occupies.</summary>
        /// <param name="x">The leftmost coordinate of the object.</param>
        /// <param name="y">The topmost coordinate of the object.</param>
        /// <param name="doAction">The action to perform for each tile.</param>
        public void ForEachPos(int x, int y, Action<Vector2> doAction)
        {
            for (int i = 0; i < this.TileWidth; i++)
            {
                for (int j = 0; j < this.TileHeight; j++)
                {
                    var pos = new Vector2(x + i, y + j);
                    doAction.Invoke(pos);
                }
            }
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
            if (this.Texture == null)
            {
                return false;
            }

            var chest = obj as Chest;
            if (currentFrame >= (chest?.startingLidFrame.Value ?? 0))
            {
                currentFrame -= chest?.startingLidFrame.Value ?? 0;
            }

            bool drawColored = this.PlayerColor && chest != null && !chest.playerChoiceColor.Value.Equals(Color.Black);
            int startLayer = drawColored && this.PlayerColor ? 1 : 0;
            int endLayer = startLayer == 0 ? 1 : 3;
            for (int layer = startLayer; layer < endLayer; layer++)
            {
                Color color = (layer % 2 == 0 || !drawColored) && chest != null
                    ? chest.Tint
                    : chest?.playerChoiceColor.Value ?? Color.White;

                spriteBatch.Draw(
                    this.Texture,
                    pos + Storage.ShakeOffset(obj, -1, 2),
                    new Rectangle(this.Width * currentFrame, this.Height * layer, this.Width, this.Height),
                    color * alpha,
                    0f,
                    origin,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth + ((1 + layer - startLayer) * 1E-05f));
            }

            return true;
        }

        /// <summary>Replaces a vanilla chest in player inventory with an expanded storage.</summary>
        /// <param name="player">The player whose inventory to replace.</param>
        /// <param name="index">The item slot of the chest in player inventory.</param>
        /// <param name="item">The item to replace.</param>
        public void Replace(Farmer player, int index, Item item)
        {
            int stack = item.Stack;
            player.Items[index] = this.Create(item);
            player.Items[index].Stack = stack;
        }

        /// <summary>Replaces a vanilla chest placed in the world with an expanded storage.</summary>
        /// <param name="location">The location to replace chest at.</param>
        /// <param name="pos">The position of the chest at location.</param>
        /// <param name="obj">The object to replace.</param>
        public void Replace(GameLocation location, Vector2 pos, SObject obj)
        {
            Chest chest = this.Create(obj);
            location.Objects[pos] = chest;
            chest.modData[$"{XSLite.ModPrefix}/X"] = pos.X.ToString(CultureInfo.InvariantCulture);
            chest.modData[$"{XSLite.ModPrefix}/Y"] = pos.Y.ToString(CultureInfo.InvariantCulture);
            if (this.TileHeight == 1 && this.TileWidth == 1)
            {
                return;
            }

            // Add objects for extra Tile spaces
            this.ForEachPos(pos, innerPos =>
            {
                if (innerPos.Equals(pos) || location.Objects.ContainsKey(innerPos))
                {
                    return;
                }

                var extraObj = new SObject(Vector2.Zero, 130)
                {
                    name = this.Name,
                };

                // Copy modData from original item
                foreach (var modData in chest.modData)
                {
                    extraObj.modData.CopyFrom(modData);
                }

                location.Objects.Add(innerPos, extraObj);
            });
        }

        /// <summary>Removes chest and artifacts from location.</summary>
        /// <param name="location">The location to remove chest from.</param>
        /// <param name="pos">The position of the object at location.</param>
        /// <param name="obj">The object to remove.</param>
        public void Remove(GameLocation location, Vector2 pos, SObject obj)
        {
            if (obj.modData.TryGetValue($"{XSLite.ModPrefix}/X", out string xStr)
                && obj.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out string yStr)
                && int.TryParse(xStr, out int xPos)
                && int.TryParse(yStr, out int yPos))
            {
                this.ForEachPos(xPos, yPos, innerPos =>
                {
                    location.Objects.Remove(innerPos);
                });
            }

            location.Objects.Remove(pos);
        }

        private static Vector2 ShakeOffset(SObject instance, int minValue, int maxValue)
        {
            return instance.shakeTimer > 0
                ? new Vector2(Game1.random.Next(minValue, maxValue), 0)
                : Vector2.Zero;
        }

        private Chest Create(Item item)
        {
            var chest = new Chest(true, Vector2.Zero, this.Format == AssetFormat.Vanilla ? item.ParentSheetIndex : 130)
            {
                Name = this.Name,
                SpecialChestType = this.SpecialChestType,
                fridge = { Value = this.IsFridge },
                lidFrameCount = { Value = this.Frames },
                modData = { [$"{XSLite.ModPrefix}/Storage"] = this.Name },
            };
            if (item is Chest oldChest)
            {
                if (oldChest.items.Any())
                {
                    chest.items.CopyFrom(oldChest.items);
                }

                chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            }

            if (this.HeldStorage)
            {
                if (item is not SObject obj || obj.heldObject.Value is not Chest heldChest)
                {
                    heldChest = new Chest(true, Vector2.Zero);
                }

                chest.heldObject.Value = heldChest;
            }

            // Copy modData from original item
            foreach (var modData in item.modData)
            {
                chest.modData.CopyFrom(modData);
            }

            // Copy modData from config
            foreach (var modData in this.ModData)
            {
                if (!chest.modData.ContainsKey(modData.Key))
                {
                    chest.modData.Add(modData.Key, modData.Value);
                }
            }

            return chest;
        }
    }
}