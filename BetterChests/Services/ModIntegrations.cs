namespace StardewMods.BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class ModIntegrations : IModService
{
    /// <summary>Unique ModId for Horse Overhaul.</summary>
    public const string HorseOverhaulModUniqueId = "Goldenrevolver.HorseOverhaul";

    /// <summary>Unique Mod Id for Automate.</summary>
    public const string AutomateModUniqueId = "Pathochild.Automate";

    /// <summary>Fully qualified name for Automate Container Type.</summary>
    public const string AutomateChestContainerType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";

    private readonly List<string> _modIds = new()
    {
        ModIntegrations.HorseOverhaulModUniqueId,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ModIntegrations"/> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public ModIntegrations(IModHelper helper)
    {
        this.Helper = helper;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <summary>
    ///     Gets a list of integrated Mod Ids.
    /// </summary>
    public IEnumerable<string> ModIds
    {
        get => this._modIds;
    }

    private IModHelper Helper { get; }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var removedMods = this._modIds.Where(modId => !this.Helper.ModRegistry.IsLoaded(modId)).ToList();
        foreach (var modId in removedMods)
        {
            this._modIds.Remove(modId);
        }
    }
}