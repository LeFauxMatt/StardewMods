namespace StardewMods.GarbageDay;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.ItemRepository;
using StardewValley.Objects;

/// <summary>
///     Encapsulates logic for each Garbage Can managed by this mod.
/// </summary>
internal class GarbageCan
{
    private static readonly Lazy<List<Item>> ItemsLazy = new(
        () => new(from item in new ItemRepository().GetAll() select item.Item));

    private readonly Chest _chest;
    private readonly Lazy<Random> _randomizer;

    private bool _checked;
    private bool _doubleMega;
    private bool _dropQiBeans;
    private bool _mega;

    /// <summary>Initializes a new instance of the <see cref="GarbageCan" /> class.</summary>
    /// <param name="location">The name of the Map asset.</param>
    /// <param name="chest">A unique name given to the garbage can for its loot table.</param>
    public GarbageCan(GameLocation location, Chest chest)
    {
        this.Location = location;
        this._chest = chest;
        if (!this.ModData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan)
         || !int.TryParse(whichCan, out var vanillaCan))
        {
            vanillaCan = 0;
        }

        this._randomizer = new(() => GarbageCan.VanillaRandomizer(vanillaCan));
    }

    /// <summary>
    ///     Gets or sets a value indicating if the next can will have a hat.
    /// </summary>
    public static bool GarbageHat { get; set; }

    /// <summary>
    ///     Gets the Location where the garbage can is placed.
    /// </summary>
    public GameLocation Location { get; }

    /// <summary>
    ///     Gets the tile of the Garbage Can.
    /// </summary>
    public Vector2 Tile => this._chest.TileLocation;

    private static IEnumerable<Item> AllItems => GarbageCan.ItemsLazy.Value;

    private IList<Item> Items => this._chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    private ModDataDictionary ModData => this._chest.modData;

    private Random Randomizer => this._randomizer.Value;

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

        if (!this.ModData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan))
        {
            return;
        }

        // Vanilla Loot
        if (int.TryParse(whichCan, out var vanillaCan) && vanillaCan is >= 3 and <= 7)
        {
            Log.Trace($"Adding Vanilla Loot to Garbage Can {whichCan}");
            var localLoot = vanillaCan switch
            {
                3 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => this.Randomizer.NextDouble()
                  < 0.05
                        ? 749
                        : 535,
                4 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => 378 + this.Randomizer.Next(3) * 2,
                5 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck && Game1.dishOfTheDay is not null =>
                    Game1.dishOfTheDay.ParentSheetIndex != 217 ? Game1.dishOfTheDay.ParentSheetIndex : 216,
                6 when this.Randomizer.NextDouble() < 0.2 + Game1.player.DailyLuck => 223,
                7 when this.Randomizer.NextDouble() < 0.2 => !Utility.HasAnyPlayerSeenEvent(191393)
                    ? 167
                    : Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater")
                   && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja")
                        ? !(this.Randomizer.NextDouble() < 0.25) ? 270 : 809
                        : -1,
                _ => -1,
            };

            if (localLoot != -1)
            {
                this.AddItem(new SObject(localLoot, 1));
                return;
            }
        }

        // Seasonal Loot
        var season = Game1.currentLocation.GetSeasonForLocation();
        if (this.Randomizer.NextDouble() < 0.1)
        {
            Log.Trace($"Adding Vanilla Seasonal Loot {season} to Garbage Can {whichCan}");
            var globalLoot = Utility.getRandomItemFromSeason(
                season,
                (int)(this.Tile.X * 653 + this.Tile.Y * 777),
                false);
            if (globalLoot != -1)
            {
                this.AddItem(new SObject(globalLoot, 1));
            }

            return;
        }

        if (!this.ModData.TryGetValue("furyx639.GarbageDay/LootKey", out var lootKey))
        {
            return;
        }

        // Custom Loot
        var allLoot = Game1.content.Load<Dictionary<string, Dictionary<string, float>>>("furyx639.GarbageDay/Loot");
        var loot = new Dictionary<string, float>();
        var lootKeys = new[]
        {
            whichCan,
            lootKey,
            season,
        };

        foreach (var key in lootKeys)
        {
            if (!allLoot.TryGetValue(key, out var lootItems))
            {
                continue;
            }

            foreach (var (itemTag, lootChance) in lootItems)
            {
                if (!loot.ContainsKey(itemTag))
                {
                    loot[itemTag] = 0;
                }

                loot[itemTag] += lootChance;
            }
        }

        if (!loot.Any())
        {
            return;
        }

        var targetIndex = this.Randomizer.NextDouble() * loot.Values.Sum();
        var currentIndex = 0d;
        foreach (var (itemTag, lootChance) in loot)
        {
            currentIndex += lootChance;
            if (currentIndex < targetIndex)
            {
                continue;
            }

            var items = GarbageCan.AllItems.Where(
                                      item => item.GetContextTags()
                                                  .Any(tag => tag.Equals(itemTag, StringComparison.OrdinalIgnoreCase)))
                                  .ToList();
            if (!items.Any())
            {
                continue;
            }

            var index = this.Randomizer.Next(items.Count);
            this.AddItem(items[index].getOne());
            return;
        }
    }

    /// <summary>
    ///     Called when a player attempts to open the garbage can.
    /// </summary>
    public void CheckAction()
    {
        if (!this._checked)
        {
            this._checked = true;
            Game1.stats.incrementStat("trashCansChecked", 1);
            return;
        }

        // Drop Item
        if (this._dropQiBeans)
        {
            this._dropQiBeans = false;
            var origin = Game1.tileSize * (this.Tile + new Vector2(0.5f, -1));
            Game1.createItemDebris(new SObject(890, 1), origin, 2, this.Location, (int)origin.Y + 64);
            return;
        }

        // Give Hat
        if (this._doubleMega || GarbageCan.GarbageHat)
        {
            this._doubleMega = false;
            GarbageCan.GarbageHat = false;
            this.Location.playSound("explosion");
            this._chest.playerChoiceColor.Value = Color.Black; // Remove Lid
            Game1.player.addItemByMenuIfNecessary(new Hat(66));
            return;
        }

        if (this._mega)
        {
            this._mega = false;
            this.Location.playSound("crit");
        }

        this._chest.GetMutex()
            .RequestLock(
                () =>
                {
                    Game1.playSound("trashcan");
                    this._chest.ShowMenu();
                });
    }

    /// <summary>
    ///     Empties the trash of all items.
    /// </summary>
    public void EmptyTrash()
    {
        this.Items.Clear();
    }

    private static Random VanillaRandomizer(int whichCan)
    {
        var randomizer = new Random(
            (int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + 777 + whichCan * 77);
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

    private void AddItem(Item item)
    {
        this._chest.addItem(item);
        this.UpdateColor();
    }

    /// <summary>
    ///     Updates the Garbage Can to match a color from one of the trashed items.
    /// </summary>
    private void UpdateColor()
    {
        var colorTags = this.Items.SelectMany(item => item.GetContextTags())
                            .Where(tag => tag.StartsWith("color"))
                            .ToList();
        if (!colorTags.Any())
        {
            this._chest.playerChoiceColor.Value = Color.Gray;
            return;
        }

        var index = this.Randomizer.Next(colorTags.Count);
        this._chest.playerChoiceColor.Value = ColorHelper.FromTag(colorTags[index]);
    }
}