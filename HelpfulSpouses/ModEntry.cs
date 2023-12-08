namespace StardewMods.HelpfulSpouses;

using System.Collections.Generic;
using StardewModdingAPI.Events;
using StardewMods.Common.Extensions;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ProjectFluent;
using StardewMods.HelpfulSpouses.Chores;
using StardewValley.Extensions;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private readonly Dictionary<ChoreOption, IChore> chores = new();

    private readonly Dictionary<string, Dictionary<ChoreOption, double>> spouseRules = new();

#nullable disable
    private ModConfig config;

    private IFluent<string> fluent;
#nullable enable

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        // Init
        ModPatches.Init(this.ModManifest);
        this.config = CommonHelpers.GetConfig<ModConfig>(this.Helper);
        this.chores[ChoreOption.BirthdayGift] = new BirthdayGift(this.config.BirthdayGiftOptions);
        this.chores[ChoreOption.FeedTheAnimals] = new FeedTheAnimals(this.config.FeedTheAnimalsOptions);
        this.chores[ChoreOption.LoveThePets] = new LoveThePets(this.config.LoveThePetsOptions);
        this.chores[ChoreOption.MakeBreakfast] = new MakeBreakfast();
        this.chores[ChoreOption.PetTheAnimals] = new PetTheAnimals(this.config.PetTheAnimalsOptions);
        this.chores[ChoreOption.RepairTheFences] = new RepairTheFences(this.config.RepairTheFencesOptions);
        this.chores[ChoreOption.WaterTheCrops] = new WaterTheCrops(this.config.WaterTheCropsOptions);
        this.chores[ChoreOption.WaterTheSlimes] = new WaterTheSlimes(this.config.WaterTheSlimesOptions);

        // Update spouse chores
        var spouseData = this.Helper.ModContent.Load<Dictionary<string, Dictionary<ChoreOption, double>>>("assets/spouseRules.json");
        foreach (var (spouse, choreOptions) in spouseData)
        {
            this.spouseRules.Add(spouse, choreOptions);
        }

        // Events
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!Game1.player.isMarriedOrRoommates())
        {
            return;
        }

        var rnd = Utility.CreateRandom(
            Game1.stats.DaysPlayed,
            Game1.uniqueIDForThisGame,
            Game1.player.UniqueMultiplayerID);
        if (!rnd.NextBool(this.config.GlobalChance))
        {
            return;
        }

        var spouses = new HashSet<NPC>();
        foreach (var (name, friendshipData) in Game1.player.friendshipData.Pairs)
        {
            if (!(friendshipData.IsMarried() || friendshipData.IsRoommate()) ||
                (this.config.HeartsNeeded > 0 && friendshipData.Points / 250 < this.config.HeartsNeeded))
            {
                continue;
            }

            var character = Game1.getCharacterFromName(name);
            if (character is not null)
            {
                spouses.Add(character);
            }
        }

        var selectedChores = new HashSet<ChoreOption>();
        foreach (var spouse in spouses)
        {
            if (!this.spouseRules.TryGetValue(spouse.Name, out var choreOptions))
            {
                choreOptions = this.config.DefaultChance;
            }

            var maxChores = this.config.DailyLimit;

            // Randomly choose spouse chores
            foreach (var (choreOption, chance) in choreOptions.Shuffle())
            {
                if (selectedChores.Contains(choreOption)
                    || !rnd.NextBool(chance)
                    || !this.chores.TryGetValue(choreOption, out var chore)
                    || !chore.IsPossibleForSpouse(spouse)
                    || !chore.TryPerformChore(spouse))
                {
                    continue;
                }

                var tokens = new Dictionary<string, object>
                {
                    ["PlayerName"] = Game1.player.displayName,
                    ["NickName"] = spouse.getTermOfSpousalEndearment(),
                };
                selectedChores.Add(choreOption);
                chore.AddTokens(tokens);

                // Add dialogue for chore
                var dialogue = this.fluent.Get($"dialogue-{choreOption.ToStringFast()}", tokens);
                dialogue = TokenParser.ParseText(dialogue);
                spouse.setNewDialogue(dialogue, add: true);

                if (--maxChores <= 0)
                {
                    break;
                }
            }
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var projectFluent = new ProjectFluentIntegration(this.Helper.ModRegistry);
        if (projectFluent.IsLoaded)
        {
            this.fluent = projectFluent.Api.GetLocalizationsForCurrentLocale(this.ModManifest);
        }
    }
}
