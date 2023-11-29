namespace StardewMods.GarbageDay;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewMods.Common.Helpers;
using StardewValley.Mods;
using StardewValley.Objects;

/// <summary>
///     Encapsulates logic for each Garbage Can managed by this mod.
/// </summary>
internal sealed class GarbageCan
{
    private readonly Chest chest;

    private bool checkedToday;
    private bool doubleMega;
    private bool dropQiBeans;
    private bool mega;
    private Random random;
    private Item? specialItem;

    /// <summary>Initializes a new instance of the <see cref="GarbageCan" /> class.</summary>
    /// <param name="location">The name of the Map asset.</param>
    /// <param name="chest">A unique name given to the garbage can for its loot table.</param>
    public GarbageCan(GameLocation location, Chest chest)
    {
        this.Location = location;
        this.chest = chest;
        this.random = new();
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the next can will drop a hat.
    /// </summary>
    public static bool GarbageHat { get; set; }

    /// <summary>
    ///     Gets the Location where the garbage can is placed.
    /// </summary>
    public GameLocation Location { get; }

    /// <summary>
    ///     Gets the tile of the Garbage Can.
    /// </summary>
    public Vector2 Tile => this.chest.TileLocation;

    private IList<Item> Items => this.chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    private ModDataDictionary ModData => this.chest.modData;

    /// <summary>
    ///     Adds an item to the garbage can determined by luck and mirroring vanilla chances.
    /// </summary>
    public void AddLoot()
    {
        // Reset daily state
        this.checkedToday = false;
        this.dropQiBeans = false;
        this.doubleMega = false;
        this.mega = false;
        if (!this.ModData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan))
        {
            return;
        }

        this.Location.TryGetGarbageItem(
            whichCan,
            Game1.player.DailyLuck,
            out var item,
            out var selected,
            out var garbageRandom);

        this.random = garbageRandom;
        if (selected is null)
        {
            return;
        }

        if (selected.ItemId == "(O)890")
        {
            this.dropQiBeans = true;
            this.specialItem = item;
            return;
        }

        this.doubleMega = selected.IsDoubleMegaSuccess;
        this.mega = !this.doubleMega && selected.IsMegaSuccess;
        if (selected.AddToInventoryDirectly)
        {
            this.specialItem = item;
            return;
        }

        this.AddItem(item);
    }

    /// <summary>
    ///     Called when a player attempts to open the garbage can.
    /// </summary>
    public void CheckAction()
    {
        if (!this.checkedToday)
        {
            this.checkedToday = true;
            Game1.stats.Increment("trashCansChecked");
            return;
        }

        // Drop Item
        if (this.dropQiBeans)
        {
            var origin = Game1.tileSize * (this.Tile + new Vector2(0.5f, -1));
            Game1.createItemDebris(this.specialItem, origin, 2, this.Location, (int)origin.Y + Game1.tileSize);
            this.dropQiBeans = false;
            return;
        }

        // Give Hat
        if (this.doubleMega || GarbageCan.GarbageHat)
        {
            this.doubleMega = false;
            this.Location.playSound("explosion");
        }

        if (this.mega)
        {
            this.mega = false;
            this.Location.playSound("crit");
        }

        if (this.specialItem is not null)
        {
            if (this.specialItem.ItemId == "(H)66")
            {
                GarbageCan.GarbageHat = false;
                this.chest.playerChoiceColor.Value = Color.Black; // Remove Lid
            }

            Game1.player.addItemByMenuIfNecessary(this.specialItem);
            return;
        }

        this.chest.GetMutex()
            .RequestLock(
                () =>
                {
                    Game1.playSound("trashcan");
                    this.chest.ShowMenu();
                });
    }

    /// <summary>
    ///     Empties the trash of all items.
    /// </summary>
    public void EmptyTrash()
    {
        this.Items.Clear();
    }

    private void AddItem(Item item)
    {
        this.chest.addItem(item);
        this.UpdateColor();
    }

    /// <summary>
    ///     Updates the Garbage Can to match a color from one of the trashed items.
    /// </summary>
    private void UpdateColor()
    {
        var colors = this.Items.Select(ItemContextTagManager.GetColorFromTags).OfType<Color>().ToList();
        if (!colors.Any())
        {
            this.chest.playerChoiceColor.Value = Color.Gray;
            return;
        }

        var index = this.random.Next(colors.Count);
        this.chest.playerChoiceColor.Value = colors[index];
    }
}