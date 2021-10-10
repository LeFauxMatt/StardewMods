namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Common.Helpers;
    using Common.Integrations.XSLite;
    using CommonHarmony.Services;
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
        private readonly HashSet<int> _inventoryStack = new();
        private readonly HashSet<Vector2> _objectListStack = new();
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
            Content.Init(this.Helper.Content);
            Log.Init(this.Monitor);
            Mixin.Init(this.ModManifest);

            if (this.Helper.ModRegistry.IsLoaded("furyx639.MoreChests"))
            {
                this.Monitor.Log("MoreChests deprecates eXpanded Storage (Lite).\nRemove XSLite from your mods folder!", LogLevel.Warn);
                return;
            }

            this._api = new XSLiteAPI(this.Helper);

            // Events
            this.Helper.Events.GameLoop.DayStarted += XSLite.OnDayStarted;
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Player.Warped += XSLite.OnWarped;

            // Patches
            var unused = new Patches();
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
            this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            foreach (var obj in e.NewLocation.Objects.Values)
            {
                if (obj is Chest chest && chest.TryGetStorage(out var storage) && storage.OpenNearby > 0)
                {
                    chest.UpdateFarmerNearby(e.NewLocation, false);
                }
            }
        }

        /// <summary>Invalidate sprite cache for storages each in-game day.</summary>
        private static void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var storage in XSLite.Storages.Values)
            {
                storage.InvalidateCache();
            }
        }

        /// <summary>Load Expanded Storage content packs.</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in this.Helper.ContentPacks.GetOwned())
            {
                this._api.LoadContentPack(contentPack);
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
                XSLite.CurrentChest.Value = Game1.player.CurrentItem as Chest;
                XSLite.CurrentLidFrame.Value = XSLite.CurrentChest.Value is not null
                    ? this.Helper.Reflection.GetField<int>(XSLite.CurrentChest.Value, "currentLidFrame")
                    : null;
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
                && Game1.currentLocation.Objects.TryGetValue(new(xPos, yPos), out var sourceObj))
            {
                obj = sourceObj;
                pos = new(xPos, yPos);
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

                Game1.currentLocation.Objects.Remove(pos);
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

        /// <summary>Remove extra objects for bigger storages.</summary>
        [EventPriority(EventPriority.Low)]
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation)
            {
                return;
            }

            foreach (var added in e.Added)
            {
                if (this._objectListStack.Contains(added.Key))
                {
                    this._objectListStack.Remove(added.Key);
                    continue;
                }

                if (XSLite.CurrentChest.Value is null || !XSLite.CurrentChest.Value.TryGetStorage(out var storage))
                {
                    continue;
                }

                storage.ForEachPos(
                    added.Key,
                    pos =>
                    {
                        this._objectListStack.Add(pos);

                        // Replace origin object with chest
                        if (pos.Equals(added.Key))
                        {
                            var chest = XSLite.CurrentChest.Value.ToChest(storage);
                            chest.modData[$"{XSLite.ModPrefix}/X"] = added.Key.X.ToString(CultureInfo.InvariantCulture);
                            chest.modData[$"{XSLite.ModPrefix}/Y"] = added.Key.Y.ToString(CultureInfo.InvariantCulture);
                            e.Location.Objects[pos] = chest;
                            return;
                        }

                        // Add generic objects at remaining positions
                        e.Location.Objects[pos] = new(Vector2.Zero, 232)
                        {
                            name = storage.Name,
                            modData =
                            {
                                [$"{XSLite.ModPrefix}/Storage"] = storage.Name,
                                [$"{XSLite.ModPrefix}/X"] = added.Key.X.ToString(CultureInfo.InvariantCulture),
                                [$"{XSLite.ModPrefix}/Y"] = added.Key.Y.ToString(CultureInfo.InvariantCulture),
                            },
                        };
                    });
            }

            foreach (var removed in e.Removed)
            {
                if (this._objectListStack.Contains(removed.Key))
                {
                    this._objectListStack.Remove(removed.Key);
                    continue;
                }

                if (!removed.Value.TryGetStorage(out var storage)
                    || !removed.Value.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr)
                    || !removed.Value.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr)
                    || !int.TryParse(xStr, out var xPos)
                    || !int.TryParse(yStr, out var yPos))
                {
                    continue;
                }

                storage.ForEachPos(
                    xPos,
                    yPos,
                    pos =>
                    {
                        if (pos.Equals(removed.Key))
                        {
                            return;
                        }

                        this._objectListStack.Add(pos);
                        e.Location.Objects.Remove(pos);
                    });
            }
        }
    }
}