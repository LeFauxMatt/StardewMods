namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using StardewMods.Common.Extensions;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.Extensions;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class BirthdayGift : BaseChore<BirthdayGift>
{
    private static readonly Lazy<List<Item>> Items = new(
        delegate
        {
            return ItemRegistry
                .GetObjectTypeDefinition()
                .GetAllIds()
                .Select(localId => ItemRegistry.Create(ItemRegistry.type_object + localId))
                .ToList();
        });

    private Item? birthdayGift;
    private NPC? birthdayNpc;

    /// <summary>Initializes a new instance of the <see cref="BirthdayGift" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public BirthdayGift(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.BirthdayGift;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens)
    {
        if (this.birthdayNpc is null || this.birthdayGift is null)
        {
            return;
        }

        tokens["Birthday"] = this.birthdayNpc.getName();
        tokens["BirthdayGender"] = this.birthdayNpc.Gender switch { 1 => "Female", _ => "Male" };
        tokens["ItemId"] = $"[{this.birthdayGift.QualifiedItemId}]";
        tokens["ItemName"] = this.birthdayGift.DisplayName;
    }

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse)
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

    /// <inheritdoc />
    public override bool TryPerformChore(NPC spouse)
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
                case 0 when rnd.NextBool(this.Config.BirthdayGift.ChanceForLove):
                    this.birthdayGift = item;
                    return true;

                // Liked item
                case 2 when rnd.NextBool(this.Config.BirthdayGift.ChanceForLike):
                    this.birthdayGift = item;
                    return true;

                // Disliked item
                case 4 when rnd.NextBool(this.Config.BirthdayGift.ChanceForDislike):
                    this.birthdayGift = item;
                    return true;

                // Hated item
                case 6 when rnd.NextBool(this.Config.BirthdayGift.ChanceForHate):
                    this.birthdayGift = item;
                    return true;

                // Neutral item
                case 8 when rnd.NextBool(this.Config.BirthdayGift.ChanceForNeutral):
                    this.birthdayGift = item;
                    return true;
            }
        }

        return false;
    }
}