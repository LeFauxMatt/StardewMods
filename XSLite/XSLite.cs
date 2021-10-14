namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.XSLite;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class XSLite : Mod, IAssetLoader
    {
        internal const string ModPrefix = "furyx639.ExpandedStorage";
        internal static readonly IDictionary<string, Storage> Storages = new Dictionary<string, Storage>();
        internal static readonly IDictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        private readonly PerScreen<Chest> _currentChest = new();
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
            this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

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
            for (var index = 0; index < Game1.player.Items.Count; index++)
            {
                var item = Game1.player.Items[index];
                if (item is null || !item.TryGetStorage(out var storage))
                {
                    continue;
                }

                switch (item)
                {
                    case SObject {bigCraftable: {Value: true}} and not Chest:
                        Game1.player.Items[index] = item.ToChest(storage);
                        break;
                    case Chest chest:
                        chest.modData.Remove($"{XSLite.ModPrefix}/X");
                        chest.modData.Remove($"{XSLite.ModPrefix}/Y");
                        break;
                }
            }
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

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (Context.IsPlayerFree)
            {
                this._currentChest.Value = Game1.player.CurrentItem as Chest;
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
                         .OfType<SObject>()
                         .Where(item => item.bigCraftable.Value && item is not Chest && XSLite.Storages.ContainsKey(item.Name))
                         .ToList();

            if (items.Count == 0)
            {
                return;
            }

            for (var index = 0; index < e.Player.Items.Count; index++)
            {
                var item = e.Player.Items[index] as SObject;
                if (item is null || !items.Contains(item) || !item.TryGetStorage(out var storage))
                {
                    continue;
                }

                var stack = e.Player.Items[index].Stack;
                var chest = item.ToChest(storage);
                chest.Stack = stack;
                e.Player.Items[index] = chest;
                items.Remove(item);
                if (items.Count == 0)
                {
                    return;
                }
            }
        }

        [EventPriority(EventPriority.High)]
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation || this._currentChest.Value is null)
            {
                return;
            }

            var chest = e.Added.Select(added => added.Value).OfType<Chest>().SingleOrDefault();
            if (chest is not null && this._currentChest.Value.TryGetStorage(out var storage))
            {
                chest.Name = storage.Name;
                chest.SpecialChestType = storage.SpecialChestType;
                chest.fridge.Value = storage.IsFridge;
                chest.lidFrameCount.Value = storage.Frames;
                chest.playerChoiceColor.Value = this._currentChest.Value.playerChoiceColor.Value;

                if (this._currentChest.Value.items.Any())
                {
                    chest.items.CopyFrom(this._currentChest.Value.items);
                }

                foreach (var modData in storage.ModData)
                {
                    chest.modData[modData.Key] = modData.Value;
                }

                foreach (var modData in this._currentChest.Value.modData.Pairs)
                {
                    if (!chest.modData.ContainsKey(modData.Key))
                    {
                        chest.modData[modData.Key] = modData.Value;
                    }
                }

                chest.modData.Remove($"{XSLite.ModPrefix}/X");
                chest.modData.Remove($"{XSLite.ModPrefix}/Y");
            }
        }
    }
}