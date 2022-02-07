namespace StardewMods.BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class ModIntegrations : IModService
{
    /// <summary>
    ///     ModId Horse Overhaul
    /// </summary>
    public const string HorseOverhaulId = "Goldenrevolver.HorseOverhaul";

    private readonly List<string> _modIds = new()
    {
        ModIntegrations.HorseOverhaulId,
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
        foreach (var modId in this._modIds.Where(modId => !this.Helper.ModRegistry.IsLoaded(modId)))
        {
            this._modIds.Remove(modId);
        }
    }
}