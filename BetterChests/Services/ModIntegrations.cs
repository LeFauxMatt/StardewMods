namespace StardewMods.BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

/// <inheritdoc />
internal class ModIntegrations : IModService
{
    /// <summary>Fully qualified name for Automate Container Type.</summary>
    public const string AutomateChestContainerType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";

    /// <summary>Unique Mod Id for Automate.</summary>
    public const string AutomateModUniqueId = "Pathochild.Automate";

    /// <summary>Unique ModId for Horse Overhaul.</summary>
    public const string HorseOverhaulModUniqueId = "Goldenrevolver.HorseOverhaul";

    private readonly List<string> _modIds = new()
    {
        ModIntegrations.HorseOverhaulModUniqueId,
    };

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModIntegrations" /> class.
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

    /// <summary>
    ///     Gets mod integrated placed storages.
    /// </summary>
    /// <param name="location">The location to get storages for.</param>
    /// <returns>An enumerable of location storages for integrated mods.</returns>
    public IEnumerable<LocationChest> GetLocationChests(GameLocation location)
    {
        if (location is Farm farm && this.ModIds.Contains(ModIntegrations.HorseOverhaulModUniqueId))
        {
            // Attempt to load saddle bags
            foreach (var stable in farm.buildings.OfType<Stable>())
            {
                if (!stable.modData.TryGetValue($"{ModIntegrations.HorseOverhaulModUniqueId}/stableID", out var stableId) || !int.TryParse(stableId, out var x))
                {
                    continue;
                }

                if (!location.Objects.TryGetValue(new(x, 0), out var obj) || obj is not Chest saddleBag || !saddleBag.modData.ContainsKey($"{ModIntegrations.HorseOverhaulModUniqueId}/isSaddleBag"))
                {
                    continue;
                }

                var horse = stable.getStableHorse();
                if (horse is not null)
                {
                    yield return new(horse.currentLocation, horse.Position / 64f, saddleBag, "SaddleBag");
                }
            }
        }
    }

    /// <summary>
    ///     Gets mod integrated placed storages.
    /// </summary>
    /// <param name="player">The player to get storages for.</param>
    /// <returns>An enumerable of storages for integrated mods.</returns>
    public IEnumerable<PlayerChest> GetPlayerChests(Farmer player)
    {
        if (this.ModIds.Contains(ModIntegrations.HorseOverhaulModUniqueId) && player.mount is not null)
        {
            // Attempt to load saddle bags
            var farm = Game1.getFarm();
            foreach (var stable in farm.buildings.OfType<Stable>())
            {
                if (!stable.modData.TryGetValue($"{ModIntegrations.HorseOverhaulModUniqueId}/stableID", out var stableId) || !int.TryParse(stableId, out var x))
                {
                    continue;
                }

                if (!farm.Objects.TryGetValue(new(x, 0), out var obj) || obj is not Chest saddleBag || !saddleBag.modData.ContainsKey($"{ModIntegrations.HorseOverhaulModUniqueId}/isSaddleBag"))
                {
                    continue;
                }

                if (player.mount.HorseId == stable.HorseId)
                {
                    yield return new(player, saddleBag, "SaddleBag");
                }
            }
        }
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var removedMods = this._modIds.Where(modId => !this.Helper.ModRegistry.IsLoaded(modId)).ToList();
        foreach (var modId in removedMods)
        {
            this._modIds.Remove(modId);
        }
    }
}