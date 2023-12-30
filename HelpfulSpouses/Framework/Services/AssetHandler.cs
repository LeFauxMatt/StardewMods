namespace StardewMods.HelpfulSpouses.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.ContentPatcher;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Models;

/// <summary>Responsible for handling spouse chore data.</summary>
internal sealed class AssetHandler : BaseService
{
    private const string AssetPath = "Data/Characters";

    private readonly Dictionary<string, CharacterOptions> data = new();

    /// <summary>Initializes a new instance of the <see cref="AssetHandler" /> class.</summary>
    /// <param name="contentPatcherIntegration">Dependency for Content Patcher integration.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public AssetHandler(
        ContentPatcherIntegration contentPatcherIntegration,
        ILog log,
        IManifest manifest,
        IModEvents modEvents)
        : base(log, manifest)
    {
        modEvents.Content.AssetReady += this.OnAssetReady;
        contentPatcherIntegration.ConditionsApiReady += this.OnConditionsApiReady;
    }

    /// <summary>Gets the Storage Data for Expanded Storage objects.</summary>
    public IReadOnlyDictionary<string, CharacterOptions> Data => this.data;

    private void OnAssetReady(object? sender, AssetReadyEventArgs e)
    {
        if (!e.Name.IsEquivalentTo(AssetHandler.AssetPath))
        {
            return;
        }

        this.data.Clear();
        foreach (var (characterId, characterData) in Game1.characterData)
        {
            foreach (var (customFieldKey, customFieldValue) in characterData.CustomFields)
            {
                var keyParts = customFieldKey.Split('/');
                if (keyParts.Length != 2
                    || !keyParts[0].Equals(this.ModId, StringComparison.OrdinalIgnoreCase)
                    || !ChoreOptionExtensions.TryParse(keyParts[1], out var choreOption)
                    || !double.TryParse(customFieldValue, out var value))
                {
                    continue;
                }

                if (!this.data.TryGetValue(characterId, out var spouseChores))
                {
                    spouseChores = new CharacterOptions();
                    this.data.Add(characterId, spouseChores);
                }

                spouseChores[choreOption] = value;
            }
        }
    }

    private void OnConditionsApiReady(object? sender, EventArgs e) => _ = DataLoader.Characters(Game1.content);
}