namespace GarbageDay
{
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
    using xTile.Layers;
    using xTile.ObjectModel;
    using xTile.Tiles;
    using Object = StardewValley.Object;

    public class GarbageDay : Mod, IAssetEditor, IAssetLoader
    {
        private static readonly ItemRepository ItemRepository = new();
        private readonly IDictionary<string, GarbageCan> _garbageCans = new Dictionary<string, GarbageCan>();
        private readonly HashSet<string> _excludedAssets = new();
        private readonly PerScreen<NPC> _npc = new();
        private readonly PerScreen<Chest> _chest = new();
        private Multiplayer _multiplayer;
        private XSLiteIntegration _xsLite;
        private XSPlusIntegration _xsPlus;
        private ModConfig _config;

        internal static IDictionary<string, IDictionary<string, float>> Loot { get; private set; }

        internal static IEnumerable<SearchableItem> Items
        {
            get => GarbageDay.ItemRepository.GetAll();
        }

        internal static EvenBetterRngIntegration BetterRng { get; private set; }

        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            this._xsLite = new XSLiteIntegration(this.Helper.ModRegistry);
            this._xsPlus = new XSPlusIntegration(this.Helper.ModRegistry);
            GarbageDay.BetterRng = new EvenBetterRngIntegration(this.Helper.ModRegistry);
            this._config = this.Helper.ReadConfig<ModConfig>();

            // Console Commands
            this.Helper.ConsoleCommands.Add("garbage_fill", "Adds loot to all Garbage Cans.", this.GarbageFill);
            this.Helper.ConsoleCommands.Add("garbage_kill", "Removes all Garbage Cans.", this.GarbageKill);

            // Events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            if (Context.IsMainPlayer)
            {
                helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
                helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            }

            helper.Events.Display.MenuChanged += this.OnMenuChanged;
        }

        /// <inheritdoc/>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            string[] segments = PathUtilities.GetSegments(asset.AssetName);
            return segments.Length == 2
                   && segments.ElementAt(0).Equals("GarbageDay", StringComparison.OrdinalIgnoreCase)
                   && segments.ElementAt(1).Equals("Loot", StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public T Load<T>(IAssetInfo asset)
        {
            return (T)this.Helper.Content.Load<IDictionary<string, IDictionary<string, float>>>("assets/loot.json");
        }

        /// <inheritdoc />
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.DataType == typeof(Map) && !this._excludedAssets.Contains(asset.AssetName);
        }

        /// <inheritdoc />
        public void Edit<T>(IAssetData asset)
        {
            Map map = asset.AsMap().Data;
            if (!asset.AssetNameEquals(@"Maps\Town") && !map.Properties.ContainsKey("GarbageDay"))
            {
                this._excludedAssets.Add(asset.AssetName);
                return;
            }

            for (int x = 0; x < map.Layers[0].LayerWidth; x++)
            {
                for (int y = 0; y < map.Layers[0].LayerHeight; y++)
                {
                    Layer layer = map.GetLayer("Buildings");
                    Tile tile = layer.PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                    if (tile == null)
                    {
                        continue;
                    }

                    // Look for Action: Garbage [WhichCan]
                    tile.Properties.TryGetValue("Action", out PropertyValue property);
                    if (property == null)
                    {
                        continue;
                    }

                    string[] parts = property.ToString().Split(' ');
                    if (parts.Length != 2 || parts[0] != "Garbage")
                    {
                        continue;
                    }

                    string whichCan = parts[1];
                    if (string.IsNullOrWhiteSpace(whichCan))
                    {
                        continue;
                    }

                    if (!this._garbageCans.TryGetValue(whichCan, out GarbageCan garbageCan))
                    {
                        garbageCan = new GarbageCan(PathUtilities.NormalizeAssetName(asset.AssetName), whichCan, new Vector2(x, y));
                        this._garbageCans.Add(whichCan, garbageCan);
                    }

                    // Remove base tile
                    if (layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet.Id == "Town" && layer.Tiles[x, y].TileIndex == 78)
                    {
                        layer.Tiles[x, y] = null;
                    }

                    // Remove Lid tile
                    layer = map.GetLayer("Front");
                    if (layer.Tiles[x, y] != null && layer.Tiles[x, y].TileSheet.Id == "Town" && layer.Tiles[x, y].TileIndex == 46)
                    {
                        layer.Tiles[x, y] = null;
                    }

                    // Add NoPath to tile
                    map.GetLayer("Back").PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size)?.Properties.Add("NoPath", string.Empty);
                }
            }
        }

        private void GarbageFill(string command, string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int amount))
            {
                amount = 1;
            }

            foreach (var garbageCan in this._garbageCans)
            {
                if (garbageCan.Value.Chest == null)
                {
                    continue;
                }

                for (int i = 0; i < amount; i++)
                {
                    garbageCan.Value.AddLoot();
                }
            }
        }

        private void GarbageKill(string command, string[] args)
        {
            foreach (var garbageCan in this._garbageCans)
            {
                if (garbageCan.Value.Location.Objects.TryGetValue(garbageCan.Value.Tile, out Object obj) && obj is Chest)
                {
                    garbageCan.Value.Location.Objects.Remove(garbageCan.Value.Tile);
                }
            }
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Load GarbageCan using XSLite
            this._xsLite.API.LoadContentPack(this.ModManifest, this.Helper.DirectoryPath);

            // Enable/Disable XSPlus features
            this._xsPlus.API.EnableWithModData("CraftFromChest", "furyx639.ExpandedStorage/Storage", "Garbage Can", false);
            this._xsPlus.API.EnableWithModData("SearchItems", "furyx639.ExpandedStorage/Storage", "Garbage Can", false);
            this._xsPlus.API.EnableWithModData("StashToChest", "furyx639.ExpandedStorage/Storage", "Garbage Can", false);
            this._xsPlus.API.EnableWithModData("Unbreakable", "furyx639.ExpandedStorage/Storage", "Garbage Can", true);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Utility.ForAllLocations(delegate(GameLocation location)
            {
                string mapPath = PathUtilities.NormalizeAssetName(location.mapPath.Value);
                foreach (var garbageCan in this._garbageCans.Where(gc => gc.Value.MapName.Equals(mapPath)))
                {
                    garbageCan.Value.Location = location;
                }
            });
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            GarbageDay.Loot = this.Helper.Content.Load<IDictionary<string, IDictionary<string, float>>>("GarbageDay/Loot", ContentSource.GameContent);
            foreach (var garbageCan in this._garbageCans)
            {
                if (garbageCan.Value.Chest == null)
                {
                    continue;
                }

                // Empty chest on garbage day
                if (Game1.dayOfMonth % 7 == this._config.GarbageDay)
                {
                    garbageCan.Value.Chest.items.Clear();
                }

                garbageCan.Value.AddLoot();
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Open Can
            if (e.NewMenu is ItemGrabMenu { context: Chest chest } && chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out string whichCan) && this._garbageCans.TryGetValue(whichCan, out GarbageCan garbageCan))
            {
                Character character = Utility.isThereAFarmerOrCharacterWithinDistance(garbageCan.Tile, 7, garbageCan.Location);
                if (character is not (NPC npc and not Horse))
                {
                    return;
                }

                this._npc.Value = npc;
                this._chest.Value = chest;
                this._multiplayer.globalChatInfoMessage("TrashCan", Game1.player.Name, npc.Name);
                if (npc.Name.Equals("Linus"))
                {
                    npc.doEmote(32);
                    npc.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"), true, true);
                    Game1.player.changeFriendship(5, npc);
                    this._multiplayer.globalChatInfoMessage("LinusTrashCan");
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
            else if (e.OldMenu is ItemGrabMenu && this._npc.Value != null)
            {
                Game1.drawDialogue(this._npc.Value);
                if (!this._chest.Value.items.Any() && !this._chest.Value.playerChoiceColor.Value.Equals(Color.Black))
                {
                    this._chest.Value.playerChoiceColor.Value = Color.DarkGray;
                }

                this._npc.Value = null;
                this._chest.Value = null;
            }
        }
    }
}