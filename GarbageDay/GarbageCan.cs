namespace GarbageDay;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Helpers;
using Common.Helpers.ItemMatcher;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

/// <summary>
///     Encapsulates logic for each Garbage Can managed by this mod.
/// </summary>
internal class GarbageCan
{
    private readonly IDictionary<string, float> _customLoot = new Dictionary<string, float>();
    private readonly ItemMatcher _itemMatcher = new(string.Empty, true);
    private readonly string _mapLoot;
    private readonly int _vanillaCan;
    private readonly string _whichCan;
    private bool _checked;
    private Chest _chest;
    private bool _doubleMega;
    private bool _dropQiBeans;
    private bool _mega;
    private Random _randomizer;

    /// <summary>Initializes a new instance of the <see cref="GarbageCan" /> class.</summary>
    /// <param name="mapName">The name of the Map asset.</param>
    /// <param name="mapLoot">A map property to specify a loot table.</param>
    /// <param name="whichCan">A unique name given to the garbage can for its loot table.</param>
    /// <param name="tile">The tile where this garbage can is placed.</param>
    public GarbageCan(string mapName, string mapLoot, string whichCan, Vector2 tile)
    {
        this.MapName = mapName;
        this._mapLoot = mapLoot;
        this._whichCan = whichCan;
        this.Tile = tile;
        this._vanillaCan = int.TryParse(whichCan, out var vanillaCan) ? vanillaCan : 0;
    }

    /// <summary>
    ///     Gets the name of the Map asset.
    /// </summary>
    public string MapName { get; }

    /// <summary>
    ///     Gets the tile where this garbage can is placed.
    /// </summary>
    public Vector2 Tile { get; }

    /// <summary>
    ///     Gets or sets the Location where the garbage can is placed.
    /// </summary>
    public GameLocation Location { get; set; }

    /// <summary>
    ///     Gets the actual placed Chest object.
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

            if (this.Location.Objects.TryGetValue(this.Tile, out var obj) && obj is Chest chest)
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

            Log.Trace($"Adding Garbage Can ({this._whichCan}) to location ({this.Location.Name})");
            chest = new(true, Vector2.Zero)
            {
                Name = "Garbage Can",
                playerChoiceColor =
                {
                    Value = Color.DarkGray,
                },
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
            var colorTag = this.Chest.items
                               .SelectMany(item => item.GetContextTags())
                               .Where(tag => tag.StartsWith("color"))
                               .Shuffle()
                               .FirstOrDefault();

            return colorTag is not null ? ColorHelper.FromTag(colorTag) : Color.Gray;
        }
    }

    private Random Randomizer
    {
        get
        {
            return this._randomizer ??= GarbageDay.BetterRng.IsLoaded
                ? GarbageDay.BetterRng.API.GetNamedRandom(this._whichCan)
                : GarbageCan.VanillaRandomizer(this._vanillaCan);
        }
    }

    /// <summary>
    ///     Called when a player attempts to open the garbage can.
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
            var origin = Game1.tileSize * (this.Tile + new Vector2(0.5f, -1));
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
    ///     Adds an item to the garbage can determined by luck and mirroring vanilla chances.
    /// </summary>
    public void AddLoot()
    {
        // Reset daily state
        this._checked = false;
        this._dropQiBeans = false;

        // Mega/Double-Mega
        this._mega = Game1.stats.getStat("trashCansChecked") > 20 && this.Randomizer.NextDouble() < 0.01;
        this._doubleMega = Game1.stats.getStat("trashCansChecked") > 20 && this.Randomizer.NextDouble() < 0.002;
        if (this._doubleMega || !(this._mega || this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck))
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
            Log.Trace($"Adding Vanilla Loot to Garbage Can {this._whichCan}");
            var localLoot = this._vanillaCan switch
            {
                3 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => this.Randomizer.NextDouble() < 0.05 ? 749 : 535,
                4 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => 378 + this.Randomizer.Next(3) * 2,
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
        var season = Game1.currentLocation.GetSeasonForLocation();
        if (this.Randomizer.NextDouble() < 0.1)
        {
            Log.Trace($"Adding Vanilla Seasonal Loot {season} to Garbage Can {this._whichCan}");
            var globalLoot = Utility.getRandomItemFromSeason(season, (int)(this.Tile.X * 653 + this.Tile.Y * 777), false);
            if (globalLoot != -1)
            {
                this.Chest.addItem(new Object(globalLoot, 1));
                this.Chest.playerChoiceColor.Value = this.Color;
            }

            return;
        }

        // Custom Loot
        this._customLoot.Clear();
        Log.Verbose($"Adding Custom Loot to Garbage Can {this._whichCan}");
        this.AddToCustomLoot(this._whichCan);
        Log.Verbose($"Adding Custom Map Loot {this._mapLoot} to Garbage Can {this._whichCan}");
        this.AddToCustomLoot(this._mapLoot);
        Log.Verbose($"Adding Custom Seasonal Loot {season} to Garbage Can {this._whichCan}");
        this.AddToCustomLoot(season);
        if (!this._customLoot.Any())
        {
            return;
        }

        Log.Trace($"Adding custom loot to {this._whichCan}");
        var totalWeight = this._customLoot.Values.Sum();
        var targetIndex = this.Randomizer.NextDouble() * totalWeight;
        double currentIndex = 0;
        foreach (var lootItem in this._customLoot)
        {
            currentIndex += lootItem.Value;
            if (currentIndex < targetIndex)
            {
                continue;
            }

            this._itemMatcher.SetSearch(lootItem.Key);
            var customLoot = GarbageDay.Items
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
        var randomizer = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + whichCan * 77);
        var prewarm = randomizer.Next(0, 100);
        for (var k = 0; k < prewarm; k++)
        {
            randomizer.NextDouble();
        }

        prewarm = randomizer.Next(0, 100);
        for (var j = 0; j < prewarm; j++)
        {
            randomizer.NextDouble();
        }

        return randomizer;
    }

    private void AddToCustomLoot(string key)
    {
        if (!GarbageDay.Loot.TryGetValue(key, out var lootTable))
        {
            return;
        }

        foreach (var lootItem in lootTable)
        {
            this._customLoot.Add(lootItem.Key, lootItem.Value);
        }
    }
}