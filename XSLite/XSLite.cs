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
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <inheritdoc cref="StardewModdingAPI.Mod" />
    public class XSLite : Mod, IAssetLoader, IAssetEditor
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
        public override void Entry(IModHelper helper)
        {
            Log.Init(this.Monitor);
            this.API = new XSLiteAPI(this.Helper);

            // Events
            this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            this.Helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            this.Helper.Events.Player.Warped += XSLite.OnWarped;
            this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

            // Patches
            var unused = new Patches(this.Helper, this.Monitor, new Harmony(this.ModManifest.UniqueID));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return this.API;
        }

        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            string[] segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 3
                   && segments.ElementAt(0).Equals("ExpandedStorage", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("SpriteSheets", StringComparison.OrdinalIgnoreCase)
                   && XSLite.Storages.TryGetValue(segments.ElementAt(2), out var storage)
                   && storage.Format != Storage.AssetFormat.Vanilla;
        }

        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            string storageName = PathUtilities.GetSegments(asset.AssetName).ElementAt(2);

            // Load placeholder texture in case of failure
            if ((XSLite.Storages.TryGetValue(storageName, out Storage storage) && storage.Format == Storage.AssetFormat.JsonAssets) || !XSLite.Textures.TryGetValue(storageName, out Texture2D texture))
            {
                texture = this.Helper.Content.Load<Texture2D>("assets/texture.png");
            }

            return (T)(object)texture;
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            string[] segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 3
                   && segments.ElementAt(0).Equals("ExpandedStorage", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("SpriteSheets", StringComparison.OrdinalIgnoreCase)
                   && XSLite.Storages.TryGetValue(segments.ElementAt(2), out var storage)
                   && storage.Format == Storage.AssetFormat.JsonAssets;
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            string storageName = PathUtilities.GetSegments(asset.AssetName).ElementAt(2);
            if (!XSLite.Storages.ContainsKey(storageName))
            {
                return;
            }

            IAssetDataForImage editor = asset.AsImage();
            for (int frame = 0; frame < 5; frame++)
            {
                for (int layer = 0; layer < 3; layer++)
                {
                    // Base Layer
                    if (!XSLite.Textures.TryGetValue($"{storageName}-{layer * 6}", out var texture) && !XSLite.Textures.TryGetValue($"{storageName}", out texture))
                    {
                        break;
                    }

                    var sourceArea = new Rectangle(0, 0, 16, 32);
                    var targetArea = new Rectangle(frame * 16, layer * 32, 16, 32);
                    editor.PatchImage(texture, sourceArea, targetArea);

                    // Lid Layer
                    if (!XSLite.Textures.TryGetValue($"{storageName}-{frame + (layer * 6) + 1}", out texture) && !XSLite.Textures.TryGetValue($"{storageName}", out texture))
                    {
                        break;
                    }

                    sourceArea.Height = 21;
                    targetArea.Height = 21;
                    editor.PatchImage(texture, sourceArea, targetArea);
                }
            }
        }

        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            foreach (Chest chest in e.NewLocation.Objects.Values.OfType<Chest>())
            {
                if (chest.TryGetStorage(out Storage storage) && storage.OpenNearby > 0)
                {
                    chest.UpdateFarmerNearby(e.NewLocation, false);
                }
            }
        }

        /// <summary>Invalidate sprite cache for storages each in-game day.</summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (Storage storage in XSLite.Storages.Values.Where(storage => storage.Format == Storage.AssetFormat.DynamicGameAssets))
            {
                storage.InvalidateCache(this.Helper.Content);
            }
        }

        /// <summary>Load Expanded Storage content packs.</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                this.API.LoadContentPack(contentPack);
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

            foreach (Chest chest in Game1.player.Items.Take(12).OfType<Chest>())
            {
                chest.updateWhenCurrentLocation(Game1.currentGameTime, Game1.player.currentLocation);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || (!e.Button.IsActionButton() && !e.Button.IsUseToolButton()))
            {
                return;
            }

            Vector2 pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64f : e.Cursor.Tile;
            pos.X = (int)pos.X;
            pos.Y = (int)pos.Y;

            // Object exists at pos and is within reach of player
            if (!Utility.withinRadiusOfPlayer((int)(64 * pos.X), (int)(64 * pos.Y), 1, Game1.player)
                || !Game1.currentLocation.Objects.TryGetValue(pos, out SObject obj))
            {
                return;
            }

            // Reassign to origin object if applicable
            if (obj.modData.TryGetValue($"{XSLite.ModPrefix}/X", out string xStr)
                && obj.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out string yStr)
                && int.TryParse(xStr, out int xPos)
                && int.TryParse(yStr, out int yPos)
                && (xPos != (int)pos.X || yPos != (int)pos.Y)
                && Game1.currentLocation.Objects.TryGetValue(new Vector2(xPos, yPos), out SObject sourceObj))
            {
                obj = sourceObj;
                pos = new Vector2(xPos, yPos);
            }

            // Object supports feature
            if (!obj.TryGetStorage(out Storage storage))
            {
                return;
            }

            Chest chest = obj as Chest ?? obj.heldObject.Value as Chest;

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
                    chest.GetMutex().RequestLock(() =>
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

            foreach (Item item in e.Added)
            {
                if (!item.TryGetStorage(out Storage storage))
                {
                    continue;
                }

                int index = e.Player.getIndexOfInventoryItem(item);
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
                    if (!removed.Value.TryGetStorage(out Storage storage))
                    {
                        continue;
                    }

                    storage.Remove(e.Location, removed.Key, removed.Value);
                }
            }
        }
    }
}