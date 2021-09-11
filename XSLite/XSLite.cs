using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using Common.Integrations.XSLite;
using Microsoft.Xna.Framework.Graphics;
using SObject = StardewValley.Object;

namespace XSLite
{
    public class XSLite : Mod, IAssetLoader, IAssetEditor
    {
        internal const string ModPrefix = "furyx639.ExpandedStorage";
        internal static readonly IDictionary<string, Storage> Storages = new Dictionary<string, Storage>();
        internal static readonly IDictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();
        internal static readonly PerScreen<IReflectedField<int>> CurrentLidFrame = new();
        internal static readonly PerScreen<Chest> CurrentChest = new();
        private IXSLiteAPI _api;

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            _api = new XSLiteAPI(Helper, Monitor);

            // Events
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Player.InventoryChanged += OnInventoryChanged;
            Helper.Events.Player.Warped += OnWarped;
            Helper.Events.World.ObjectListChanged += OnObjectListChanged;

            // Patches
            var unused = new Patches(Helper, Monitor, new Harmony(ModManifest.UniqueID));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return _api;
        }

        /// <summary>Invalidate sprite cache for storages each in-game day</summary>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            foreach (var storage in Storages.Values.Where(storage => storage.Format == Storage.AssetFormat.DynamicGameAssets))
            {
                storage.InvalidateCache(Helper.Content);
            }
        }

        /// <summary>Load Expanded Storage content packs</summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Monitor.Log("Loading Expanded Storage Content", LogLevel.Info);
            foreach (var contentPack in Helper.ContentPacks.GetOwned())
            {
                _api.LoadContentPack(contentPack);
            }
        }

        /// <summary>Tick visible chests in inventory</summary>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            if (!ReferenceEquals(Game1.player.CurrentItem, CurrentChest.Value))
            {
                if (Game1.player.CurrentItem is Chest currentChest)
                {
                    CurrentChest.Value = currentChest;
                    CurrentLidFrame.Value = Helper.Reflection.GetField<int>(currentChest, "currentLidFrame");
                }
                else
                {
                    CurrentChest.Value = null;
                    CurrentLidFrame.Value = null;
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
                return;
            var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64f : e.Cursor.Tile;
            pos.X = (int)pos.X;
            pos.Y = (int)pos.Y;
            // Object exists at pos and is within reach of player
            if (!Utility.withinRadiusOfPlayer((int)(64 * pos.X), (int)(64 * pos.Y), 1, Game1.player)
                || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
                return;
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

            // Object supports feature, and player can carry object
            if (!obj.TryGetStorage(out var storage))
                return;
            // Check for chest action
            if (e.Button.IsActionButton() && obj is Chest chest && chest.playerChest.Value)
            {
                if (storage.OpenNearby > 0 || storage.Frames <= 1)
                {
                    Game1.playSound(storage.OpenSound);
                    chest.ShowMenu();
                }
                else
                {
                    chest.GetMutex().RequestLock(delegate
                    {
                        chest.frameCounter.Value = 5;
                        Game1.playSound(storage.OpenSound);
                        Game1.player.Halt();
                        Game1.player.freezePause = 1000;
                    });
                }

                Helper.Input.Suppress(e.Button);
            }
            // Object supports feature, and player can carry object
            else if (e.Button.IsUseToolButton() && Game1.player.CurrentItem is not Tool && storage.Config.EnabledFeatures.Contains("CanCarry") && Game1.player.addItemToInventoryBool(obj, true))
            {
                if (!string.IsNullOrWhiteSpace(storage.CarrySound))
                    Game1.currentLocation.playSound(storage.CarrySound);
                obj.TileLocation = Vector2.Zero;
                storage.ForEachPos(pos, innerPos => _objectListStack.Add(innerPos));
                storage.Remove(Game1.currentLocation, obj);
                Helper.Input.Suppress(e.Button);
            }
        }

        private readonly HashSet<int> _inventoryStack = new();

        /// <summary>Replace Expanded Storage objects with modded Chest</summary>
        [EventPriority(EventPriority.Low)]
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;
            foreach (var item in e.Added)
            {
                if (!item.TryGetStorage(out var storage))
                    continue;
                var index = e.Player.getIndexOfInventoryItem(item);
                if (_inventoryStack.Contains(index))
                {
                    _inventoryStack.Remove(index);
                }
                else
                {
                    _inventoryStack.Add(index);
                    storage.Replace(e.Player, item);
                }
            }
        }
        private static void OnWarped(object sender, WarpedEventArgs e)
        {
            foreach (var chest in e.NewLocation.Objects.Values.OfType<Chest>())
            {
                if (chest.TryGetStorage(out var storage) && storage.OpenNearby > 0)
                    chest.UpdateFarmerNearby(e.NewLocation, false);
            }
        }
        private readonly HashSet<Vector2> _objectListStack = new();
        /// <summary>Remove extra objects for bigger storages</summary>
        [EventPriority(EventPriority.Low)]
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation)
                return;
            foreach (var removed in e.Removed)
            {
                if (_objectListStack.Contains(removed.Key))
                    _objectListStack.Remove(removed.Key);
                else
                {
                    if (!removed.Value.TryGetStorage(out var storage))
                        continue;
                    storage.Remove(e.Location, removed.Value);
                }
            }
        }
        /// <inheritdoc />
        public bool CanLoad<T>(IAssetInfo asset)
        {
            var segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 3
                   && segments.ElementAt(0).Equals("ExpandedStorage", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("SpriteSheets", StringComparison.OrdinalIgnoreCase)
                   && Storages.TryGetValue(segments.ElementAt(2), out var storage)
                   && storage.Format != Storage.AssetFormat.Vanilla;
        }
        /// <inheritdoc />
        public T Load<T>(IAssetInfo asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAt(2);
            // Load placeholder texture in case of failure
            if (Storages.TryGetValue(storageName, out var storage) && storage.Format == Storage.AssetFormat.JsonAssets || !Textures.TryGetValue(storageName, out var texture))
                texture = Helper.Content.Load<Texture2D>("assets/texture.png");
            return (T) (object) texture;
        }
        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            var segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 3
                   && segments.ElementAt(0).Equals("ExpandedStorage", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("SpriteSheets", StringComparison.OrdinalIgnoreCase)
                   && Storages.TryGetValue(segments.ElementAt(2), out var storage)
                   && storage.Format == Storage.AssetFormat.JsonAssets;
        }
        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var storageName = PathUtilities.GetSegments(asset.AssetName).ElementAt(2);
            if (!Storages.ContainsKey(storageName))
                return;
            var editor = asset.AsImage();
            for (var frame = 0; frame < 5; frame++)
            {
                for (var layer = 0; layer < 3; layer++)
                {
                    // Base Layer
                    if (!Textures.TryGetValue($"{storageName}-{layer * 6}", out var texture) && !Textures.TryGetValue($"{storageName}", out texture))
                        break;
                    var sourceArea = new Rectangle(0, 0, 16, 32);
                    var targetArea = new Rectangle(frame * 16, layer * 32, 16, 32);
                    editor.PatchImage(texture, sourceArea, targetArea);
                    
                    // Lid Layer
                    if (!Textures.TryGetValue($"{storageName}-{frame + layer * 6 + 1}", out texture) && !Textures.TryGetValue($"{storageName}", out texture))
                        break;
                    sourceArea.Height = 21;
                    targetArea.Height = 21;
                    editor.PatchImage(texture, sourceArea, targetArea);
                }
            }
        }
    }
}