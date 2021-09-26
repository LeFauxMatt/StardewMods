namespace GarbageDay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Extensions;
    using Common.Helpers.ItemMatcher;
    using Common.Helpers.ItemRepository;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Objects;
    using Object = StardewValley.Object;

    /// <summary>
    /// Encapsulates logic for each Garbage Can managed by this mod.
    /// </summary>
    internal class GarbageCan
    {
        private readonly IDictionary<string, float> _customLoot = new Dictionary<string, float>();
        private readonly ItemMatcher _itemMatcher = new(string.Empty, true);
        private readonly string _whichCan;
        private readonly int _vanillaCan;
        private Random _randomizer;
        private Chest _chest;
        private bool _checked;
        private bool _dropQiBeans;
        private bool _doubleMega;
        private bool _mega;

        /// <summary>Initializes a new instance of the <see cref="GarbageCan"/> class.</summary>
        /// <param name="mapName">The name of the Map asset.</param>
        /// <param name="whichCan">A name given to the garbage can for its loot table.</param>
        /// <param name="tile">The tile where this garbage can is placed.</param>
        public GarbageCan(string mapName, string whichCan, Vector2 tile)
        {
            this.MapName = mapName;
            this._whichCan = whichCan;
            this.Tile = tile;
            this._vanillaCan = int.TryParse(whichCan, out int vanillaCan) ? vanillaCan : 0;
        }

        /// <summary>
        /// Gets the name of the Map asset.
        /// </summary>
        public string MapName { get; }

        /// <summary>
        /// Gets the tile where this garbage can is placed.
        /// </summary>
        public Vector2 Tile { get; }

        /// <summary>
        /// Gets or sets the Location where the garbage can is placed.
        /// </summary>
        public GameLocation Location { get; set; }

        /// <summary>
        /// Gets the actual placed Chest object.
        /// </summary>
        public Chest Chest
        {
            get
            {
                if (this._chest is not null)
                {
                    return this._chest;
                }

                if (this.Location is null)
                {
                    return null;
                }

                if (this.Location.Objects.TryGetValue(this.Tile, out Object obj) && obj is Chest chest)
                {
                    chest.modData["furyx639.ExpandedStorage/Storage"] = "Garbage Can";
                    chest.modData["furyx639.GarbageDay/WhichCan"] = this._whichCan;
                    chest.modData["Pathoschild.ChestsAnywhere/IsIgnored"] = "true";
                    this._chest = chest;
                    return this._chest;
                }

                if (obj is not null)
                {
                    return null;
                }

                chest = new Chest(true, Vector2.Zero)
                {
                    Name = "Garbage Can",
                    playerChoiceColor = { Value = Color.DarkGray },
                    modData =
                    {
                        ["furyx639.ExpandedStorage/Storage"] = "Garbage Can",
                        ["furyx639.GarbageDay/WhichCan"] = this._whichCan,
                        ["Pathoschild.ChestsAnywhere/IsIgnored"] = "true",
                    },
                };
                this.Location.Objects.Add(this.Tile, chest);
                this._chest = chest;
                return this._chest;
            }
        }

        private Color Color
        {
            get
            {
                foreach (Item item in this.Chest.items.Shuffle())
                {
                    string colorTag = item.GetContextTags().Where(tag => tag.StartsWith("color")).Shuffle().FirstOrDefault();
                    if (colorTag is null)
                    {
                        continue;
                    }

                    return colorTag switch
                    {
                        "color_red" => Color.Red,
                        "color_dark_red" => Color.DarkRed,
                        "color_pale_violet_red" => Color.PaleVioletRed,
                        "color_blue" => Color.Blue,
                        "color_green" => Color.Green,
                        "color_dark_green" => Color.DarkGreen,
                        "color_jade" => Color.Teal,
                        "color_brown" => Color.Brown,
                        "color_dark_brown" => Color.Maroon,
                        "color_yellow" => Color.Yellow,
                        "color_dark_yellow" => Color.Goldenrod,
                        "color_aquamarine" => Color.Aquamarine,
                        "color_purple" => Color.Purple,
                        "color_dark_purple" => Color.Indigo,
                        "color_cyan" => Color.Cyan,
                        "color_pink" => Color.Pink,
                        "color_orange" => Color.Orange,
                        _ => Color.Gray,
                    };
                }

                return Color.Gray;
            }
        }

        private Random Randomizer
        {
            get
            {
                return this._randomizer ??= GarbageDay.BetterRng.IsLoaded
                    ? GarbageDay.BetterRng.API.GetNamedRandom(this._whichCan, (int)Game1.uniqueIDForThisGame)
                    : GarbageCan.VanillaRandomizer(this._vanillaCan);
            }
        }

        /// <summary>
        /// Called when a player attempts to open the garbage can.
        /// </summary>
        public void CheckAction()
        {
            if (this._checked)
            {
                return;
            }

            this._checked = true;
            Game1.stats.incrementStat("trashCansChecked", 1);

            // Drop Item
            if (this._dropQiBeans)
            {
                Vector2 origin = Game1.tileSize * (this.Tile + new Vector2(0.5f, -1));
                Game1.createItemDebris(new Object(890, 1), origin, 2, this.Location, (int)origin.Y + 64);
                return;
            }

            // Give Hat
            if (this._doubleMega)
            {
                this.Location.playSound("explosion");
                this.Chest.playerChoiceColor.Value = Color.Black; // Remove Lid
                Game1.player.addItemByMenuIfNecessary(new Hat(66));
                return;
            }

            if (this._mega)
            {
                this.Location.playSound("crit");
            }
        }

        /// <summary>
        /// Adds an item to the garbage can determined by luck and mirroring vanilla chances.
        /// </summary>
        public void AddLoot()
        {
            // Reset daily state
            this._checked = false;
            this._dropQiBeans = false;

            // Mega/Double-Mega
            this._mega = Game1.stats.getStat("trashCansChecked") > 20 && this.Randomizer.NextDouble() < 0.01;
            this._doubleMega = Game1.stats.getStat("trashCansChecked") > 20 && this.Randomizer.NextDouble() < 0.002;
            if (this._doubleMega || (!this._mega && !(this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck)))
            {
                return;
            }

            // Qi Beans
            if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
            {
                this._dropQiBeans = true;
                return;
            }

            // Vanilla Loot
            if (this._vanillaCan is >= 3 and <= 7)
            {
                int localLoot = this._vanillaCan switch
                {
                    3 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => this.Randomizer.NextDouble() < 0.05 ? 749 : 535,
                    4 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => 378 + (this.Randomizer.Next(3) * 2),
                    5 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck && Game1.dishOfTheDay is not null => Game1.dishOfTheDay.ParentSheetIndex != 217 ? Game1.dishOfTheDay.ParentSheetIndex : 216,
                    6 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => 223,
                    7 when this.Randomizer.NextDouble() < 0.2 => !Utility.HasAnyPlayerSeenEvent(191393) ? 167 : Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja") ? !(this.Randomizer.NextDouble() < 0.25) ? 270 : 809 : -1,
                    _ => -1,
                };
                if (localLoot != -1)
                {
                    this.Chest.addItem(new Object(localLoot, 1));
                    this.Chest.playerChoiceColor.Value = this.Color;
                    return;
                }
            }

            // Seasonal Loot
            string season = Game1.currentLocation.GetSeasonForLocation();
            if (this.Randomizer.NextDouble() < 0.1)
            {
                int globalLoot = Utility.getRandomItemFromSeason(season, (int)((this.Tile.X * 653) + (this.Tile.Y * 777)), false);
                if (globalLoot != -1)
                {
                    this.Chest.addItem(new Object(globalLoot, 1));
                    this.Chest.playerChoiceColor.Value = this.Color;
                }

                return;
            }

            // Custom Loot
            this._customLoot.Clear();
            this.AddToCustomLoot("All");
            this.AddToCustomLoot($"Maps/{this.MapName}");
            this.AddToCustomLoot($"Cans/{this._whichCan}");
            this.AddToCustomLoot($"Seasons/{season}");
            if (!this._customLoot.Any())
            {
                return;
            }

            float totalWeight = this._customLoot.Values.Sum();
            double targetIndex = this.Randomizer.NextDouble() * totalWeight;
            double currentIndex = 0;
            foreach (KeyValuePair<string, float> lootItem in this._customLoot)
            {
                currentIndex += lootItem.Value;
                if (currentIndex < targetIndex)
                {
                    continue;
                }

                this._itemMatcher.SetSearch(lootItem.Key);
                SearchableItem customLoot = GarbageDay.Items
                    .Where(entry => this._itemMatcher.Matches(entry.Item))
                    .Shuffle()
                    .FirstOrDefault();
                if (customLoot is not null)
                {
                    this.Chest.addItem(customLoot.CreateItem());
                    this.Chest.playerChoiceColor.Value = this.Color;
                }

                return;
            }
        }

        private static Random VanillaRandomizer(int whichCan)
        {
            var randomizer = new Random(((int)Game1.uniqueIDForThisGame / 2) + (int)Game1.stats.DaysPlayed + 777 + (whichCan * 77));
            int prewarm = randomizer.Next(0, 100);
            for (int k = 0; k < prewarm; k++)
            {
                randomizer.NextDouble();
            }

            prewarm = randomizer.Next(0, 100);
            for (int j = 0; j < prewarm; j++)
            {
                randomizer.NextDouble();
            }

            return randomizer;
        }

        private void AddToCustomLoot(string key)
        {
            if (!GarbageDay.Loot.TryGetValue(key, out IDictionary<string, float> lootTable))
            {
                return;
            }

            foreach (KeyValuePair<string, float> lootItem in lootTable)
            {
                this._customLoot.Add(lootItem.Key, lootItem.Value);
            }
        }
    }
}