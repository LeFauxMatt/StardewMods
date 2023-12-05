namespace StardewMods.HelpfulSpouses.Chores;

using StardewMods.Common.Extensions;
using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class BirthdayGift : IChore
{
    private static readonly Lazy<List<Item>> Items = new(
        delegate
        {
            return ItemRegistry.GetObjectTypeDefinition()
                .GetAllIds()
                .Select(localId => ItemRegistry.Create(ItemRegistry.type_object + localId))
                .ToList();
        });

    private readonly Config config;

    private Item? birthdayGift;

    private NPC? birthdayNpc;

    /// <summary>
    /// Initializes a new instance of the <see cref="BirthdayGift"/> class.
    /// </summary>
    /// <param name="config">Config data for <see cref="BirthdayGift"/>.</param>
    public BirthdayGift(Config config) => this.config = config;

    /// <inheritdoc/>
    public void AddTokens(Dictionary<string, object> tokens)
    {
        if (this.birthdayNpc is null || this.birthdayGift is null)
        {
            return;
        }

        tokens["Birthday"] = this.birthdayNpc.getName();
        tokens["BirthdayGender"] = this.birthdayNpc.Gender switch
        {
            1 => "Female",
            _ => "Male",
        };
        tokens["ItemId"] = $"[{this.birthdayGift.QualifiedItemId}]";
        tokens["ItemName"] = this.birthdayGift.DisplayName;
    }

    /// <inheritdoc/>
    public bool IsPossibleForSpouse(NPC spouse)
    {
        this.birthdayNpc = null;
        Utility.ForEachVillager(
            npc =>
            {
                if (npc == spouse || !npc.isBirthday())
                {
                    return true;
                }

                this.birthdayNpc = npc;
                return false;
            });

        return this.birthdayNpc is not null;
    }

    /// <inheritdoc/>
    public bool TryPerformChore(NPC spouse)
    {
        if (this.birthdayNpc is null)
        {
            return false;
        }

        var rnd = Utility.CreateRandom(
            Game1.stats.DaysPlayed,
            Game1.uniqueIDForThisGame,
            Game1.player.UniqueMultiplayerID);

        foreach (var item in BirthdayGift.Items.Value.Shuffle())
        {
            var taste = this.birthdayNpc.getGiftTasteForThisItem(item);
            switch (taste)
            {
                // Loved item
                case 0 when rnd.NextBool(this.config.ChanceForLove):
                    this.birthdayGift = item;
                    return true;

                // Liked item
                case 2 when rnd.NextBool(this.config.ChanceForLike):
                    this.birthdayGift = item;
                    return true;

                // Disliked item
                case 4 when rnd.NextBool(this.config.ChanceForDislike):
                    this.birthdayGift = item;
                    return true;

                // Hated item
                case 6 when rnd.NextBool(this.config.ChanceForHate):
                    this.birthdayGift = item;
                    return true;

                // Neutral item
                case 8 when rnd.NextBool(this.config.ChanceForNeutral):
                    this.birthdayGift = item;
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Config data for <see cref="BirthdayGift" />.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Gets or sets the chance that a disliked item will be given.
        /// </summary>
        public double ChanceForDislike { get; set; }

        /// <summary>
        /// Gets or sets the chance that a hated item will be given.
        /// </summary>
        public double ChanceForHate { get; set; }

        /// <summary>
        /// Gets or sets the chance that a liked item will be given.
        /// </summary>
        public double ChanceForLike { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the chance that a loved item will be given.
        /// </summary>
        public double ChanceForLove { get; set; } = 0.2;

        /// <summary>
        /// Gets or sets the chance that a neutral item will be given.
        /// </summary>
        public double ChanceForNeutral { get; set; } = 0.1;
    }
}
