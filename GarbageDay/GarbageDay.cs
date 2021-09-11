using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers.ItemData;
using Common.Integrations.EvenBetterRNG;
using Common.Integrations.XSLite;
using Common.Integrations.XSPlus;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;

namespace GarbageDay
{
    public class GarbageDay : Mod, IAssetEditor, IAssetLoader
    {
        internal static IDictionary<string, IDictionary<string, float>> Loot;
        internal static IEnumerable<SearchableItem> Items;
        internal static EvenBetterRNGIntegration BetterRng;
        private readonly IDictionary<string, GarbageCan> _garbageCans = new Dictionary<string, GarbageCan>();
        private readonly HashSet<string> _excludedAssets = new();
        private readonly PerScreen<NPC> _npc = new();
        private readonly PerScreen<Chest> _chest = new();
        private Multiplayer _multiplayer;
        private XSLiteIntegration _xsLite;
        private XSPlusIntegration _xsPlus;
        private ModConfig _config;
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            _xsLite = new XSLiteIntegration(Helper.ModRegistry);
            _xsPlus = new XSPlusIntegration(Helper.ModRegistry);
            BetterRng = new EvenBetterRNGIntegration(Helper.ModRegistry);
            _config = Helper.ReadConfig<ModConfig>();
            
            // Console Commands
            Helper.ConsoleCommands.Add("garbage_fill", "Adds loot to all Garbage Cans.", GarbageFill);
            Helper.ConsoleCommands.Add("garbage_kill", "Removes all Garbage Cans.", GarbageKill);
            
            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            if (Context.IsMainPlayer)
            {
                helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
                helper.Events.GameLoop.DayStarted += OnDayStarted;
            }
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }
        private void GarbageFill(string command, string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out var amount))
                amount = 1;
            foreach (var garbageCan in _garbageCans)
            {
                if (garbageCan.Value.Chest == null)
                    continue;
                for (var i = 0; i < amount; i++)
                {
                    garbageCan.Value.AddLoot();
                }
            }
        }
        private void GarbageKill(string command, string[] args)
        {
            foreach (var garbageCan in _garbageCans)
            {
                if (garbageCan.Value.Location.Objects.TryGetValue(garbageCan.Value.Tile, out var obj) && obj is Chest chest)
                {
                    garbageCan.Value.Location.Objects.Remove(garbageCan.Value.Tile);
                }
            }
        }
        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.DataType == typeof(Map) && !_excludedAssets.Contains(asset.AssetName);
        }
        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            var map = asset.AsMap().Data;
            if (!asset.AssetNameEquals(@"Maps\Town") && !map.Properties.TryGetValue("GarbageDay", out var mapLoot))
            {
                _excludedAssets.Add(asset.AssetName);
                return;
            }
            for (var x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (var y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                    var layer = map.GetLayer("Buildings");
                    var tile = layer.PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile == null)
                        continue;
                    // Look for Action: Garbage [WhichCan]
                    tile.Properties.TryGetValue("Action", out var property);
                    if (property == null)
                        continue;
                    var parts = property.ToString().Split(' ');
                    if (parts.Length != 2 || parts[0] != "Garbage")
                        continue;
                    var whichCan = parts[1];
                    if (string.IsNullOrWhiteSpace(whichCan))
                        continue;
                    if (!_garbageCans.TryGetValue(whichCan, out var garbageCan))
                    {
                        garbageCan = new GarbageCan(PathUtilities.NormalizeAssetName(asset.AssetName), whichCan, new Vector2(x, y));
                        _garbageCans.Add(whichCan, garbageCan);
                    }
                    // Remove base tile
                    if (layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet.Id == "Town" && layer.Tiles[x, y].TileIndex == 78)
                        layer.Tiles[x, y] = null;
                    // Remove Lid tile
                    layer = map.GetLayer("Front");
                    if (layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet.Id == "Town" && layer.Tiles[x, y].TileIndex == 46)
                        layer.Tiles[x, y] = null;
                    // Add NoPath to tile
                    map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size)?.Properties.Add("NoPath", "");
                }
            }
        }
        /// <summary>Load Garbage Can</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _multiplayer = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            
            // Load GarbageCan using XSLite
            _xsLite.API.LoadContentPack(ModManifest, Helper.DirectoryPath);
            
            // Enable/Disable XSPlus features
            _xsPlus.API.EnableWithModData("CraftFromChest", "furyx639.ExpandedStorage/Storage", "Garbage Can", false);
            _xsPlus.API.EnableWithModData("SearchItems", "furyx639.ExpandedStorage/Storage", "Garbage Can", false);
            _xsPlus.API.EnableWithModData("StashToChest", "furyx639.ExpandedStorage/Storage", "Garbage Can", false);
            _xsPlus.API.EnableWithModData("Unbreakable", "furyx639.ExpandedStorage/Storage", "Garbage Can", true);
        }
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Utility.ForAllLocations(delegate(GameLocation location)
            {
                var mapPath = PathUtilities.NormalizeAssetName(location.mapPath.Value);
                foreach (var garbageCan in _garbageCans.Where(gc => gc.Value.MapName.Equals(mapPath)))
                {
                    garbageCan.Value.Location = location;
                }
            });
        }
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Loot = Helper.Content.Load<IDictionary<string, IDictionary<string, float>>>("GarbageDay/Loot", ContentSource.GameContent);
            foreach (var garbageCan in _garbageCans)
            {
                if (garbageCan.Value.Chest == null)
                    continue;
                // Empty chest on garbage day
                if (Game1.dayOfMonth % 7 == _config.GarbageDay)
                    garbageCan.Value.Chest.items.Clear();
                garbageCan.Value.AddLoot();
            }
        }
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Open Can
            if (e.NewMenu is ItemGrabMenu { context: Chest chest } && chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan) && _garbageCans.TryGetValue(whichCan, out var garbageCan))
            {
                var character = Utility.isThereAFarmerOrCharacterWithinDistance(garbageCan.Tile, 7, garbageCan.Location);
                if (character is not (NPC npc and not Horse))
                    return;
                _npc.Value = npc;
                _chest.Value = chest;
                _multiplayer.globalChatInfoMessage("TrashCan", Game1.player.Name, npc.Name);
                if (npc.Name.Equals("Linus"))
                {
                    npc.doEmote(32);
                    npc.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"), true, true);
                    Game1.player.changeFriendship(5, npc);
                    _multiplayer.globalChatInfoMessage("LinusTrashCan");
                }
                else
                {
                    switch (npc.Age)
                    {
                        case 2:
                            npc.doEmote(28);
                            npc.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Child"), true, true);
                            Game1.player.changeFriendship(-25, npc);
                            break;
                        case 1:
                            npc.doEmote(8);
                            npc.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen"), true, true);
                            Game1.player.changeFriendship(-25, npc);
                            break;
                        default:
                            npc.doEmote(12);
                            npc.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult"), true, true);
                            Game1.player.changeFriendship(-25, npc);
                            break;
                    }
                }
                garbageCan.CheckAction();
            }
            // Close Can
            else if (e.OldMenu is ItemGrabMenu && _npc.Value != null)
            {
                Game1.drawDialogue(_npc.Value);
                if (!_chest.Value.items.Any() && !_chest.Value.playerChoiceColor.Value.Equals(Color.Black))
                    _chest.Value.playerChoiceColor.Value = Color.DarkGray;
                _npc.Value = null;
                _chest.Value = null;
            }
        }
        public bool CanLoad<T>(IAssetInfo asset)
        {
            var segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 2
                   && segments.ElementAt(0).Equals("GarbageDay", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("Loot", StringComparison.OrdinalIgnoreCase);
        }
        public T Load<T>(IAssetInfo asset)
        {
            return (T) Helper.Content.Load<IDictionary<string, IDictionary<string, float>>>("assets/loot.json");
        }
    }
}