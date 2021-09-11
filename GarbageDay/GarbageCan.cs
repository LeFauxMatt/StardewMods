using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers.ItemData;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace GarbageDay
{
    internal class GarbageCan
    {
        public string MapName { get; }
        public Vector2 Tile { get; }
        public GameLocation Location { get; set; }
        public Chest Chest
        {
            get
            {
                if (Location == null)
                    return null;
                if (Location.Objects.TryGetValue(Tile, out var obj) && obj is Chest chest)
                {
                    chest.modData["furyx639.ExpandedStorage/Storage"] = "Garbage Can";
                    chest.modData["furyx639.GarbageDay/WhichCan"] = _whichCan;
                    chest.modData["Pathoschild.ChestsAnywhere/IsIgnored"] = "true";
                    return chest;
                }
                if (obj != null)
                    return null;
                chest = new Chest(true, Vector2.Zero)
                {
                    Name = "Garbage Can",
                    playerChoiceColor = { Value = Color.DarkGray },
                    modData =
                    {
                        ["furyx639.ExpandedStorage/Storage"] = "Garbage Can",
                        ["furyx639.GarbageDay/WhichCan"] = _whichCan,
                        ["Pathoschild.ChestsAnywhere/IsIgnored"] = "true"
                    }
                };
                Location.Objects.Add(Tile, chest);
                return chest;
            }
        }
        public Color Color
        {
            get
            {
                foreach (var item in Chest.items.Shuffle())
                {
                    if (item.MatchesTagExt("color_red", true) || item.MatchesTagExt("color_dark_red", true))
                        return Color.DarkRed;
                    if (item.MatchesTagExt("color_pale_violet_red", true))
                        return Color.DarkViolet;
                    if (item.MatchesTagExt("color_blue", true))
                        return Color.DarkBlue;
                    if (item.MatchesTagExt("color_green", true) || item.MatchesTagExt("color_dark_green", true) || item.MatchesTagExt("color_jade", true))
                        return Color.DarkGreen;
                    if (item.MatchesTagExt("color_brown", true) || item.MatchesTagExt("color_dark_brown", true))
                        return Color.Brown;
                    if (item.MatchesTagExt("color_yellow", true) || item.MatchesTagExt("color_dark_yellow", true))
                        return Color.Yellow;
                    if (item.MatchesTagExt("color_aquamarine", true))
                        return Color.Aquamarine;
                    if (item.MatchesTagExt("color_purple", true) || item.MatchesTagExt("color_dark_purple", true))
                        return Color.Purple;
                    if (item.MatchesTagExt("color_cyan", true))
                        return Color.DarkCyan;
                    if (item.MatchesTagExt("color_pink", true))
                        return Color.Pink;
                    if (item.MatchesTagExt("color_orange", true))
                        return Color.DarkOrange;
                }
                return Color.Gray;
            }
        }
        private Random Randomizer
        {
            get
            {
                if (GarbageDay.BetterRng.IsLoaded)
                    return GarbageDay.BetterRng.API.GetNamedRandom(_whichCan, (int) Game1.uniqueIDForThisGame);
                var garbageRandom = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + _vanillaCan * 77);
                var prewarm = garbageRandom.Next(0, 100);
                for (var k = 0; k < prewarm; k++)
                {
                    garbageRandom.NextDouble();
                }
                prewarm = garbageRandom.Next(0, 100);
                for (var j = 0; j < prewarm; j++)
                {
                    garbageRandom.NextDouble();
                }
                return garbageRandom;
            }
        }
        private readonly IDictionary<string, float> _customLoot = new Dictionary<string, float>();
        private readonly string _whichCan;
        private readonly int _vanillaCan;
        private bool _checked;
        private bool _dropQiBeans;
        private bool _doubleMega;
        private bool _mega;
        public GarbageCan(string mapName, string whichCan, Vector2 tile)
        {
            MapName = mapName;
            _whichCan = whichCan;
            Tile = tile;
            _vanillaCan = int.TryParse(whichCan, out var vanillaCan) ? vanillaCan : 0;
        }
        public void CheckAction()
        {
            if (_checked)
                return;
            _checked = true;
            Game1.stats.incrementStat("trashCansChecked", 1);
            // Drop Item
            if (_dropQiBeans)
            {
                var origin = Game1.tileSize * (Tile + new Vector2(0.5f, -1));
                Game1.createItemDebris(new Object(890, 1), origin, 2, Location, (int) origin.Y + 64);
                return;
            }
            // Give Hat
            if (_doubleMega)
            {
                Location.playSound("explosion");
                Chest.playerChoiceColor.Value = Color.Black; // Remove Lid
                Game1.player.addItemByMenuIfNecessary(new Hat(66));
                return;
            }
            if (_mega)
            {
                Location.playSound("crit");
            }
        }
        public void AddLoot()
        {
            // Reset daily state
            _checked = false;
            _dropQiBeans = false;
            _doubleMega = false;
            _mega = false;
            
            var garbageRandom = Randomizer;
            
            // Mega/Double-Mega
            _mega = Game1.stats.getStat("trashCansChecked") > 20 && garbageRandom.NextDouble() < 0.01;
            _doubleMega = Game1.stats.getStat("trashCansChecked") > 20 && garbageRandom.NextDouble() < 0.002;
            if (_doubleMega || !_mega && !(garbageRandom.NextDouble() < 0.2 + Game1.player.DailyLuck))
                return;
            
            // Qi Beans
            if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
            {
                _dropQiBeans = true;
                return;
            }
            
            // Vanilla Loot
            if (_vanillaCan is >= 3 and <= 7)
            {
                var localLoot = _vanillaCan switch
                {
                    3 when garbageRandom.NextDouble() < 0.2 + Game1.player.DailyLuck => garbageRandom.NextDouble() < 0.05 ? 749 : 535,
                    4 when garbageRandom.NextDouble() < 0.2 + Game1.player.DailyLuck => 378 + garbageRandom.Next(3) * 2,
                    5 when garbageRandom.NextDouble() < 0.2 + Game1.player.DailyLuck && Game1.dishOfTheDay != null => Game1.dishOfTheDay.ParentSheetIndex != 217 ? Game1.dishOfTheDay.ParentSheetIndex : 216,
                    6 when garbageRandom.NextDouble() < 0.2 + Game1.player.DailyLuck => 223,
                    7 when garbageRandom.NextDouble() < 0.2 => !Utility.HasAnyPlayerSeenEvent(191393) ? 167 : Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja") ? !(garbageRandom.NextDouble() < 0.25) ? 270 : 809 : -1,
                    _ => -1
                };
                if (localLoot != -1)
                {
                    Chest.addItem(new Object(localLoot, 1));
                    Chest.playerChoiceColor.Value = Color;
                    return;
                }
            }
            
            // Seasonal Loot
            var season = Game1.currentLocation.GetSeasonForLocation();
            if (garbageRandom.NextDouble() < 0.1)
            {
                var globalLoot = Utility.getRandomItemFromSeason(season, (int) (Tile.X * 653 + Tile.Y * 777), false);
                if (globalLoot != -1)
                {
                    Chest.addItem(new Object(globalLoot, 1));
                    Chest.playerChoiceColor.Value = Color;
                }
                return;
            }
            
            // Custom Loot
            _customLoot.Clear();
            AddToCustomLoot("All");
            AddToCustomLoot($"Maps/{MapName}");
            AddToCustomLoot($"Cans/{_whichCan}");
            AddToCustomLoot($"Seasons/{season}");
            if (!_customLoot.Any())
                return;
            var totalWeight = _customLoot.Values.Sum();
            var targetIndex = garbageRandom.NextDouble() * totalWeight;
            double currentIndex = 0;
            foreach (var lootItem in _customLoot)
            {
                currentIndex += lootItem.Value;
                if (currentIndex < targetIndex)
                    continue;
                var customLoot = (GarbageDay.Items ??= new ItemRepository().GetAll())
                    .Where(entry => entry.Item.MatchesTagExt(lootItem.Key))
                    .Shuffle()
                    .FirstOrDefault();
                if (customLoot != null)
                {
                    Chest.addItem(customLoot.CreateItem());
                    Chest.playerChoiceColor.Value = Color;
                }
                return;
            }
        }
        private void AddToCustomLoot(string key)
        {
            if (!GarbageDay.Loot.TryGetValue(key, out var lootTable))
                return;
            foreach (var lootItem in lootTable)
            {
                _customLoot.Add(lootItem.Key, lootItem.Value);
            }
        }
    }
}