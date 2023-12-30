namespace StardewMods.HelpfulSpouses.Framework.Services;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Extensions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ProjectFluent;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewMods.HelpfulSpouses.Framework.Models;
using StardewValley.Extensions;

/// <summary>Responsible for managing chores performed by spouses.</summary>
internal sealed class ChoreManager : BaseService
{
    private readonly IEnumerable<IChore> chores;
    private readonly IFluent<string> fluent;
    private readonly IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="ChoreManager" /> class.</summary>
    /// <param name="chores">Dependency for accessing chores.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="projectFluentIntegration">Dependency for integration with Project Fluent.</param>
    public ChoreManager(
        IEnumerable<IChore> chores,
        Harmony harmony,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents,
        ProjectFluentIntegration projectFluentIntegration)
        : base(log, manifest)
    {
        // Init
        this.chores = chores;
        this.modConfig = modConfig;
        this.fluent = projectFluentIntegration.Api!.GetLocalizationsForCurrentLocale(manifest);

        // Events
        modEvents.GameLoop.DayStarted += this.OnDayStarted;

        // Patches
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(NPC), nameof(NPC.marriageDuties)),
            new HarmonyMethod(typeof(ChoreManager), nameof(ChoreManager.NPC_marriageDuties_prefix)));
    }

    private static void NPC_marriageDuties_prefix()
    {
        NPC.hasSomeoneFedTheAnimals = true;
        NPC.hasSomeoneFedThePet = true;
        NPC.hasSomeoneRepairedTheFences = true;
        NPC.hasSomeoneWateredCrops = true;
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
            CharacterOptions? characterOptions = null;
            foreach (var (customFieldKey, customFieldValue) in spouse.GetData().CustomFields)
            {
                var keyParts = customFieldKey.Split('/');
                if (keyParts.Length != 2
                    || !keyParts[0].Equals(this.ModId, StringComparison.OrdinalIgnoreCase)
                    || !ChoreOptionExtensions.TryParse(keyParts[1], out var choreOption)
                    || !double.TryParse(customFieldValue, out var value))
                {
                    continue;
                }

                characterOptions ??= new CharacterOptions();
                characterOptions[choreOption] = value;
            }

            characterOptions ??= this.modConfig.DefaultOptions;
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