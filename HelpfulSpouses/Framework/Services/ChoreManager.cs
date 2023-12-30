namespace StardewMods.HelpfulSpouses.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Extensions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ProjectFluent;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.Extensions;

/// <summary>Responsible for managing chores performed by spouses.</summary>
internal sealed class ChoreManager : BaseService
{
    private readonly AssetHandler assetHandler;
    private readonly IEnumerable<IChore> chores;
    private readonly IFluent<string> fluent;
    private readonly IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="ChoreManager" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="chores">Dependency for accessing chores.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="projectFluentIntegration">Dependency for integration with Project Fluent.</param>
    public ChoreManager(
        AssetHandler assetHandler,
        IEnumerable<IChore> chores,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents,
        ProjectFluentIntegration projectFluentIntegration)
        : base(log, manifest)
    {
        // Init
        this.assetHandler = assetHandler;
        this.chores = chores;
        this.modConfig = modConfig;
        this.fluent = projectFluentIntegration.Api!.GetLocalizationsForCurrentLocale(manifest);

        // Events
        modEvents.GameLoop.DayStarted += this.OnDayStarted;
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

        if (!rnd.NextBool(this.modConfig.GlobalChance))
        {
            return;
        }

        var spouses = new HashSet<NPC>();
        foreach (var (name, friendshipData) in Game1.player.friendshipData.Pairs)
        {
            if (!(friendshipData.IsMarried() || friendshipData.IsRoommate())
                || (this.modConfig.HeartsNeeded > 0 && friendshipData.Points / 250 < this.modConfig.HeartsNeeded))
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
            if (!this.assetHandler.Data.TryGetValue(spouse.Name, out var characterOptions))
            {
                characterOptions = this.modConfig.DefaultChance;
            }

            var maxChores = this.modConfig.DailyLimit;

            // Randomly choose spouse chores
            foreach (var chore in this.chores.Shuffle())
            {
                if (selectedChores.Contains(chore.Option)
                    || !rnd.NextBool(characterOptions[chore.Option])
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

                selectedChores.Add(chore.Option);
                chore.AddTokens(tokens);

                // Add dialogue for chore
                var dialogue = this.fluent.Get($"dialogue-{chore.Option.ToStringFast()}", tokens);
                dialogue = TokenParser.ParseText(dialogue);
                spouse.setNewDialogue(dialogue, true);

                if (--maxChores <= 0)
                {
                    break;
                }
            }
        }
    }
}