namespace StardewMods.HelpfulSpouses.Chores;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.HelpfulSpouses.Helpers;
using StardewValley.BellsAndWhistles;

/// <summary>
///     Gives an item to the Farmer that will be liked/loved by an NPC whose Birthday is today.
/// </summary>
internal class BirthdayShopping : IChore
{
    private static BirthdayShopping? Instance;

    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private Item? _gift;
    private NPC? _npc;

    private BirthdayShopping(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        this.Dialogue = string.Empty;

        // Tokens
        Integrations.RegisterToken("BirthdayShoppingGiftTaste", () => this.GiftTaste);
        Integrations.RegisterToken("BirthdayShoppingPronoun", () => this.Pronoun);
        Integrations.RegisterToken("BirthdayShoppingWhat", () => this._gift?.Name);
        Integrations.RegisterToken("BirthdayShoppingWho", () => this._npc?.Name);
    }

    /// <inheritdoc />
    public string? Dialogue { get; private set; }

    /// <inheritdoc />
    public bool IsPossible => this.NPCDispositions.Any(BirthdayShopping.BirthdayToday);

    private Dictionary<string, string> NPCDispositions =>
        this._helper.GameContent.Load<Dictionary<string, string>>("Data/NPCDispositions");

    private string? GiftTaste => this._gift is null || this._npc is null
        ? null
        : this._npc.getGiftTasteForThisItem(this._gift) switch
        {
            0 => "love",
            2 => "like",
            4 => "dislike",
            6 => "hate",
            8 or _ => "neutral",
        };

    private string? Pronoun => this._npc?.Gender is null ? null : Lexicon.getPronoun(this._npc.Gender == 0);

    /// <summary>
    ///     Initializes <see cref="BirthdayShopping" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BirthdayShopping" /> class.</returns>
    public static BirthdayShopping Init(IModHelper helper, ModConfig config)
    {
        return BirthdayShopping.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public bool TryToDo(NPC spouse)
    {
        this._npc = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
        var gift = this.GetRandomGift(this._npc.Name);
        this._gift = new SObject(gift, 1);
        return true;
    }

    private static bool BirthdayToday(KeyValuePair<string, string> kvp)
    {
        return Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth) is not null;
    }

    private int GetRandomGift(string npcName)
    {
        string[]? giftTastes = null;
        string[]? gifts = null;
        if (Game1.NPCGiftTastes.TryGetValue(npcName, out var data))
        {
            giftTastes = data.Split(' ');
        }

        if (Game1.random.NextDouble() <= this._config.BirthdayShoppingLovedItemChance)
        {
            gifts ??= !string.IsNullOrWhiteSpace(giftTastes?[1]) ? giftTastes[1].Split(' ') : null;
            gifts ??= Game1.NPCGiftTastes["Universal_Love"].Split(' ');
        }

        if (gifts is null && Game1.random.NextDouble() <= this._config.BirthdayShoppingLikedItemChance)
        {
            gifts ??= !string.IsNullOrWhiteSpace(giftTastes?[3]) ? giftTastes[3].Split(' ') : null;
            gifts ??= Game1.NPCGiftTastes["Universal_Like"].Split(' ');
        }

        gifts ??= !string.IsNullOrWhiteSpace(giftTastes?[9]) ? giftTastes[9].Split(' ') : null;
        gifts ??= Game1.NPCGiftTastes["Universal_Neutral"].Split(' ');
        var index = Game1.random.Next(gifts.Length);
        return Convert.ToInt32(gifts[index]);
    }
}