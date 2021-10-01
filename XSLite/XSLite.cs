namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.XSLite;
    using HarmonyLib;
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
        internal static readonly PerScreen<IReflectedField<int>> CurrentLidFrame = new();
        internal static readonly PerScreen<Chest> CurrentChest = new();
        private readonly HashSet<int> InventoryStack = new();
        private readonly HashSet<Vector2> ObjectListStack = new();
        private IXSLiteAPI API;

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
            Log.Init(this.Monitor);

            if (this.Helper.ModRegistry.IsLoaded("furyx639.MoreChests"))
            {
                this.Monitor.Log("MoreChests deprecates eXpanded Storage (Lite).\nRemove XSLite from your mods folder!", LogLevel.Warn);
                return;
            }

            this.API = new XSLiteAPI(this.Helper);

            // Events
            this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            this.Helper.Events.Player.Warped += this.OnWarped;
            this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

            // Patches
            var unused = new Patches(this.Helper, new Harmony(this.ModManifest.UniqueID));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this.API;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            foreach (var chest in e.NewLocation.Objects.Values.OfType<Chest>())
            {
                if (chest.TryGetStorage(out var storage) && storage.OpenNearby > 0)
                {
                    chest.UpdateFarmerNearby(e.NewLocation, false);
                }
            }
        }

        /// <summary>Invalidate sprite cache for storages each in-game day.</summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var storage in XSLite.Storages.Values.Where(storage => storage.Format != Storage.AssetFormat.Vanilla))
            {
                storage.InvalidateCache(this.Helper.Content);
            }
        }

        /// <summary>Load Expanded Storage content packs.</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in this.Helper.ContentPacks.GetOwned())
            {
                this.API.LoadContentPack(contentPack);
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Context.IsMainPlayer)
            {
                return;
            }

            var locations = Game1.locations.Concat(Game1.locations.OfType<BuildableGameLocation>().SelectMany(location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)));
            foreach (var location in locations)
            {
                var objects = location.Objects.Pairs.Where(obj => obj.Value is Chest chest && chest.playerChest.Value && XSLite.Storages.ContainsKey(chest.Name));
                foreach (var obj in objects)
                {
                    if (obj.Value.modData.ContainsKey($"{XSLite.ModPrefix}/Storage") || !obj.Value.TryGetStorage(out var storage))
                    {
                        continue;
                    }

                    storage.Replace(location, obj.Key, obj.Value);
                }
            }
        }

        /// <summary>Tick visible chests in inventory.</summary>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
            {
                return;
            }

            if (!ReferenceEquals(Game1.player.CurrentItem, XSLite.CurrentChest.Value))
            {
                if (Game1.player.CurrentItem is Chest currentChest)
                {
                    XSLite.CurrentChest.Value = currentChest;
                    XSLite.CurrentLidFrame.Value = this.Helper.Reflection.GetField<int>(currentChest, "currentLidFrame");
                }
                else
                {
                    XSLite.CurrentChest.Value = null;
                    XSLite.CurrentLidFrame.Value = null;
                }
            }

            foreach (var chest in Game1.player.Items.Take(12).OfType<Chest>())
            {
                chest.updateWhenCurrentLocation(Game1.currentGameTime, Game1.player.currentLocation);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsActionButton() && !e.Button.IsUseToolButton())
            {
                return;
            }

            var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64f : e.Cursor.Tile;
            pos.X = (int)pos.X;
            pos.Y = (int)pos.Y;

            // Object exists at pos and is within reach of player
            if (!Utility.withinRadiusOfPlayer((int)(64 * pos.X), (int)(64 * pos.Y), 1, Game1.player)
                || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
            {
                return;
            }

            // Reassign to origin object if applicable
            if (obj.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr)
                && obj.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr)
                && int.TryParse(xStr, out var xPos)
                && int.TryParse(yStr, out var yPos)
                && (xPos != (int)pos.X || yPos != (int)pos.Y)
                && Game1.currentLocation.Objects.TryGetValue(new Vector2(xPos, yPos), out var sourceObj))
            {
                obj = sourceObj;
                pos = new Vector2(xPos, yPos);
            }

            // Object supports feature
            if (!obj.TryGetStorage(out var storage))
            {
                return;
            }

            var chest = obj as Chest ?? obj.heldObject.Value as Chest;

            // Check for chest action
            if (e.Button.IsActionButton() && chest is not null && chest.playerChest.Value)
            {
                if (storage.OpenNearby > 0 || storage.Frames <= 1)
                {
                    Game1.playSound(storage.OpenSound);
                    chest.ShowMenu();
                }
                else
                {
                    chest.GetMutex().RequestLock(
                        () =>
                        {
                            chest.frameCounter.Value = 5;
                            Game1.playSound(storage.OpenSound);
                            Game1.player.Halt();
                            Game1.player.freezePause = 1000;
                        });
                }

                this.Helper.Input.Suppress(e.Button);
            }

            // Object supports feature, and player can carry object
            else if (e.Button.IsUseToolButton() && Game1.player.CurrentItem is not Tool && storage.Config.EnabledFeatures.Contains("CanCarry") && Game1.player.addItemToInventoryBool(obj, true))
            {
                if (!string.IsNullOrWhiteSpace(storage.CarrySound))
                {
                    Game1.currentLocation.playSound(storage.CarrySound);
                }

                obj.TileLocation = Vector2.Zero;
                storage.ForEachPos(pos, innerPos => this.ObjectListStack.Add(innerPos));
                storage.Remove(Game1.currentLocation, pos, obj);
                this.Helper.Input.Suppress(e.Button);
            }
        }

        /// <summary>Replace Expanded Storage objects with modded Chest.</summary>
        [EventPriority(EventPriority.Low)]
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                return;
            }

            foreach (var item in e.Added)
            {
                if (!item.TryGetStorage(out var storage))
                {
                    continue;
                }

                var index = e.Player.getIndexOfInventoryItem(item);
                if (this.InventoryStack.Contains(index))
                {
                    this.InventoryStack.Remove(index);
                }
                else
                {
                    this.InventoryStack.Add(index);
                    storage.Replace(e.Player, index, item);
                }
            }
        }

        /// <summary>Remove extra objects for bigger storages.</summary>
        [EventPriority(EventPriority.Low)]
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation)
            {
                return;
            }

            foreach (var removed in e.Removed)
            {
                if (this.ObjectListStack.Contains(removed.Key))
                {
                    this.ObjectListStack.Remove(removed.Key);
                }
                else
                {
                    if (!removed.Value.TryGetStorage(out var storage))
                    {
                        continue;
                    }

                    storage.Remove(e.Location, removed.Key, removed.Value);
                }
            }
        }
    }
}