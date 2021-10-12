namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.XSLite;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class XSLite : Mod, IAssetLoader
    {
        internal const string ModPrefix = "furyx639.ExpandedStorage";
        internal static readonly IDictionary<string, Storage> Storages = new Dictionary<string, Storage>();
        internal static readonly IDictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private readonly HashSet<int> _inventoryStack = new();
        private IXSLiteAPI _api;

        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            var segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 3
                   && segments.ElementAt(0).Equals("ExpandedStorage", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("SpriteSheets", StringComparison.OrdinalIgnoreCase)
                   && XSLite.Storages.TryGetValue(segments.ElementAt(2), out var storage)
                   && storage.Format != Storage.AssetFormat.Vanilla;
        }

        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAt(2);

            // Load placeholder texture in case of failure
            if (!XSLite.Textures.TryGetValue(storageName, out var texture))
            {
                texture = this.Helper.Content.Load<Texture2D>("assets/texture.png");
            }

            return (T)(object)texture;
        }

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            Storage.LoadContent = this.Helper.Content.Load<Texture2D>;
            Log.Init(this.Monitor);

            if (this.Helper.ModRegistry.IsLoaded("furyx639.MoreChests"))
            {
                this.Monitor.Log("MoreChests deprecates eXpanded Storage (Lite).\nRemove XSLite from your mods folder!", LogLevel.Warn);
                return;
            }

            this._api = new XSLiteAPI(this.Helper);

            // Events
            this.Helper.Events.GameLoop.DayEnding += XSLite.OnDayEnding;
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            // Patches
            var unused = new Patches(new(this.ModManifest.UniqueID));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this._api;
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                var locations = Game1.locations.Concat(Game1.locations.OfType<BuildableGameLocation>().SelectMany(location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)));
                foreach (var location in locations)
                {
                    foreach (var obj in location.Objects.Pairs)
                    {
                        if (obj.Value is not Chest {playerChest: {Value: true}} || !obj.Value.TryGetStorage(out var storage))
                        {
                            continue;
                        }

                        storage.ForEachPos(
                            obj.Key,
                            pos =>
                            {
                                // Replace origin object with chest
                                if (pos.Equals(obj.Key))
                                {
                                    var chest = obj.Value.ToChest(storage);
                                    chest.modData[$"{XSLite.ModPrefix}/X"] = obj.Key.X.ToString(CultureInfo.InvariantCulture);
                                    chest.modData[$"{XSLite.ModPrefix}/Y"] = obj.Key.Y.ToString(CultureInfo.InvariantCulture);
                                    location.Objects[pos] = chest;
                                    return;
                                }

                                // Add generic objects at remaining positions
                                location.Objects[pos] = new(Vector2.Zero, 232)
                                {
                                    name = storage.Name,
                                    modData =
                                    {
                                        [$"{XSLite.ModPrefix}/Storage"] = storage.Name,
                                        [$"{XSLite.ModPrefix}/X"] = obj.Key.X.ToString(CultureInfo.InvariantCulture),
                                        [$"{XSLite.ModPrefix}/Y"] = obj.Key.Y.ToString(CultureInfo.InvariantCulture),
                                    },
                                };
                            });
                    }
                }
            }

            for (var index = 0; index < Game1.player.Items.Count; index++)
            {
                var item = Game1.player.Items[index];
                if (item is null || item is Chest && item.modData.ContainsKey($"{XSLite.ModPrefix}/Storage") || !item.TryGetStorage(out var storage))
                {
                    continue;
                }

                var chest = item.ToChest(storage);
                chest.modData.Remove($"{XSLite.ModPrefix}/X");
                chest.modData.Remove($"{XSLite.ModPrefix}/Y");
                Game1.player.Items[index] = chest;
            }

            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        }

        /// <summary>Invalidate sprite cache for storages each in-game day.</summary>
        private static void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            foreach (var storage in XSLite.Storages.Values)
            {
                storage.ReloadTexture();
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in this.Helper.ContentPacks.GetOwned())
            {
                this._api.LoadContentPack(contentPack);
            }
        }

        [EventPriority(EventPriority.Low)]
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                return;
            }

            var items = e.Added.Concat(e.QuantityChanged.Select(stack => stack.Item))
                         .Where(item => (item is not Chest || item.modData.ContainsKey($"{XSLite.ModPrefix}/Storage")) && XSLite.Storages.ContainsKey(item.Name))
                         .ToList();

            for (var index = 0; index < e.Player.Items.Count; index++)
            {
                var item = e.Player.Items[index];
                if (item is null || !items.Contains(item) || !item.TryGetStorage(out var storage))
                {
                    continue;
                }

                if (this._inventoryStack.Contains(index))
                {
                    this._inventoryStack.Remove(index);
                    continue;
                }

                this._inventoryStack.Add(index);
                items.Remove(item);
                e.Player.Items[index] = item.ToChest(storage);
            }
        }
    }
}