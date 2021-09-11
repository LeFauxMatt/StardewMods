using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using Newtonsoft.Json;
using SObject = StardewValley.Object;

namespace XSLite
{
    internal class Storage
    {
        public enum AssetFormat
        {
            DynamicGameAssets,
            JsonAssets,
            Vanilla
        }
        #region ContentModel
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(Chest.SpecialChestTypes.None)]
        public Chest.SpecialChestTypes SpecialChestType { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool IsFridge { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool HeldStorage { get; set; }
        public string Image { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(5)]
        public int Frames { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("none")]
        public string Animation { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(false)]
        public bool PlayerColor { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(true)]
        public bool PlayerConfig { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public int Depth { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public int Capacity { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("openChest")]
        public string OpenSound { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("axe")]
        public string PlaceSound { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("pickUpItem")]
        public string CarrySound { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(0)]
        public float OpenNearby { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("doorCreak")]
        public string OpenNearbySound { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue("doorCreakReverse")]
        public string CloseNearbySound { get; set; }
        public HashSet<string> EnabledFeatures { get; set; }
        public Dictionary<string, bool> FilterItems { get; set; }
        public IDictionary<string, string> ModData { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [DefaultValue(AssetFormat.Vanilla)]
        public AssetFormat Format { get; set; }
        #endregion
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _path = $"ExpandedStorage/SpriteSheets/{_name}";
            }
        }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public IManifest Manifest;
        public ModConfig Config;
        public Texture2D Texture
        {
            get => _texture;
            private set
            {
                _texture = value;
                Width = _texture.Width / Math.Max(1, Frames);
                Height = PlayerColor ? _texture.Height / 3 : _texture.Height;
                TileWidth = Width / 16;
                TileHeight = (Depth > 0 ? Depth : Height - 16) / 16;
                var tilesWide = Width / 16f;
                var tilesHigh = Height / 16f;
                ScaleSize = tilesWide switch
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
                            _ => 0.1f
                        }
                    }
                };
            }
        }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; } = 1;
        public int TileHeight { get; private set; } = 1;
        public float ScaleSize { get; private set; }
        private string _name;
        private string _path;
        private Texture2D _texture;
        [JsonConstructor]
        public Storage(
            string specialChestType,
            string image,
            string animation,
            bool playerColor,
            bool playerConfig,
            int frames,
            int depth,
            int capacity,
            bool isFridge,
            bool heldStorage,
            string openSound,
            string placeSound,
            string carrySound,
            float openNearby,
            string openNearbySound,
            string closeNearbySound,
            IDictionary<string, string> modData,
            HashSet<string> allowList,
            HashSet<string> blockList,
            HashSet<string> enabledFeatures,
            AssetFormat format
        )
        {
            SpecialChestType = Enum.TryParse(specialChestType, out Chest.SpecialChestTypes specialChestTypes) ? specialChestTypes : Chest.SpecialChestTypes.None;
            IsFridge = isFridge;
            HeldStorage = heldStorage;
            Image = image;
            Frames = frames;
            Animation = animation;
            PlayerColor = playerColor;
            PlayerConfig = playerConfig;
            Depth = depth;
            Capacity = capacity;
            OpenSound = openSound;
            PlaceSound = placeSound;
            CarrySound = carrySound;
            OpenNearby = openNearby;
            OpenNearbySound = openNearbySound;
            CloseNearbySound = closeNearbySound;
            ModData = modData ?? new Dictionary<string, string>();
            FilterItems = new Dictionary<string, bool>();
            if (blockList is not null)
            {
                foreach (var blockItem in blockList)
                {
                    FilterItems.Add(blockItem, true);
                }
            }
            if (allowList is not null)
            {
                foreach (var allowItem in allowList)
                {
                    FilterItems.Add(allowItem, true);
                }
            }
            EnabledFeatures = enabledFeatures ?? new HashSet<string>();
            Format = format;
        }
        internal void InvalidateCache(IContentHelper contentHelper)
        {
            var texture = contentHelper.Load<Texture2D>(_path, ContentSource.GameContent);
            if (texture == null && !XSLite.Textures.TryGetValue(_name, out texture))
                return;
            Texture = texture;
        }
        public void ForEachPos(int x, int y, Action<Vector2> doAction)
        {
            for (var i = 0; i < TileWidth; i++)
            {
                for (var j = 0; j < TileHeight; j++)
                {
                    var pos = new Vector2(x + i, y + j);
                    doAction.Invoke(pos);
                }
            }
        }
        public bool Draw(SObject obj, int currentFrame, SpriteBatch spriteBatch, Vector2 pos, Vector2 origin, float alpha = 1f, float layerDepth = 0.89f, float scaleSize = 4f)
        {
            if (Texture == null)
                return false;
            var chest = obj as Chest;
            if (currentFrame >= (chest?.startingLidFrame.Value ?? 0))
                currentFrame -= chest?.startingLidFrame.Value ?? 0;
            var drawColored = PlayerColor && chest != null && !chest.playerChoiceColor.Value.Equals(Color.Black);
            var startLayer = drawColored && PlayerColor ? 1 : 0;
            var endLayer = startLayer == 0 ? 1 : 3;
            for (var layer = startLayer; layer < endLayer; layer++)
            {
                var color = (layer % 2 == 0 || !drawColored) && chest != null
                    ? chest.Tint
                    : chest?.playerChoiceColor.Value ?? Color.White;
                
                spriteBatch.Draw(Texture,
                    pos + ShakeOffset(obj, -1, 2),
                    new Rectangle(Width * currentFrame, Height * layer, Width, Height),
                    color * alpha,
                    0f,
                    origin,
                    scaleSize,
                    SpriteEffects.None,
                    layerDepth + (1 + layer - startLayer) * 1E-05f);
            }
            return true;
        }
        private Chest Create(Item item)
        {
            var chest = new Chest(true, Vector2.Zero, Format == AssetFormat.Vanilla ? item.ParentSheetIndex : 130)
            {
                Stack = item.Stack,
                Name = Name,
                SpecialChestType = SpecialChestType,
                fridge = { Value = IsFridge },
                lidFrameCount = { Value = Frames },
                modData = { [$"{XSLite.ModPrefix}/Storage"] = Name }
            };
            if (item is Chest oldChest)
            {
                if (oldChest.items.Any())
                    chest.items.CopyFrom(oldChest.items);
                chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            }
            if (HeldStorage)
            {
                if (item is not SObject obj || obj.heldObject.Value is not Chest heldChest)
                    heldChest = new Chest(true, Vector2.Zero);
                chest.heldObject.Value = heldChest;
            }
            // Copy modData from original item
            foreach (var modData in item.modData)
                chest.modData.CopyFrom(modData);
            // Copy modData from config
            foreach (var modData in ModData)
            {
                if (!chest.modData.ContainsKey(modData.Key))
                    chest.modData.Add(modData.Key, modData.Value);
            }
            return chest;
        }
        public void Replace(Farmer player, Item item)
        {
            var chest = Create(item);
            var index = player.getIndexOfInventoryItem(item);
            player.Items[index] = chest;
            chest.modData.Remove($"{XSLite.ModPrefix}/X");
            chest.modData.Remove($"{XSLite.ModPrefix}/Y");
        }
        public void Replace(GameLocation location, Vector2 pos, SObject obj)
        {
            var chest = Create(obj);
            location.Objects.Remove(pos);
            location.Objects.Add(pos, chest);
            chest.modData[$"{XSLite.ModPrefix}/X"] = pos.X.ToString(CultureInfo.InvariantCulture);
            chest.modData[$"{XSLite.ModPrefix}/Y"] = pos.Y.ToString(CultureInfo.InvariantCulture);
            if (TileHeight == 1 && TileWidth == 1)
                return;
            // Add objects for extra Tile spaces
            ForEachPos((int) pos.X, (int) pos.Y, innerPos =>
            {
                if (innerPos.Equals(pos) || location.Objects.ContainsKey(innerPos))
                    return;
                
                var extraObj = new SObject(Vector2.Zero, 130)
                {
                    name = Name
                };
                // Copy modData from original item
                foreach (var modData in chest.modData)
                    extraObj.modData.CopyFrom(modData);
                location.Objects.Add(innerPos, extraObj);
            });
        }
        public void Remove(GameLocation location, Vector2 pos, SObject obj)
        {
            if (TileHeight == 1 && TileWidth == 1
                || !obj.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr)
                || !obj.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos))
                return;
            ForEachPos(xPos, yPos, innerPos =>
            {
                if (innerPos.Equals(pos)
                    || !location.Objects.TryGetValue(innerPos, out var innerObj)
                    || !innerObj.modData.TryGetValue($"{XSLite.ModPrefix}/Storage", out var storageName)
                    || storageName != Name)
                    return;
                innerObj.modData.Remove($"{XSLite.ModPrefix}/X");
                innerObj.modData.Remove($"{XSLite.ModPrefix}/Y");
                location.Objects.Remove(innerPos);
            });
        }
        private static Vector2 ShakeOffset(SObject instance, int minValue, int maxValue)
        {
            return instance.shakeTimer > 0
                ? new Vector2(Game1.random.Next(minValue, maxValue), 0)
                : Vector2.Zero;
        }
    }
}