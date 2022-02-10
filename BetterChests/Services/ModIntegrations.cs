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

    private const string AutomateModUniqueId = "Pathochild.Automate";
    private const string ExpandedStorageModUniqueId = "furyx639.ExpandedStorage";
    private const string HorseOverhaulModUniqueId = "Goldenrevolver.HorseOverhaul";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModIntegrations" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ModIntegrations(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        services.Lazy<AssetHandler>(assetHandler => { assetHandler.AddModDataKey($"{ModIntegrations.ExpandedStorageModUniqueId}/Storage"); });
    }

    private IModHelper Helper { get; }

    private IDictionary<string, string> Mods { get; } = new Dictionary<string, string>
    {
        { "Automate", ModIntegrations.AutomateModUniqueId },
        { "Expanded Storage", ModIntegrations.ExpandedStorageModUniqueId },
        { "Horse Overhaul", ModIntegrations.HorseOverhaulModUniqueId },
    };

    /// <summary>
    ///     Gets mod integrated placed storages.
    /// </summary>
    /// <param name="location">The location to get storages for.</param>
    /// <returns>An enumerable of location storages for integrated mods.</returns>
    public IEnumerable<LocationChest> GetLocationChests(GameLocation location)
    {
        if (location is Farm farm && this.IsLoaded("Horse Overhaul"))
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
        if (this.IsLoaded("Horse Overhaul") && player.mount is not null)
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

    /// <summary>
    ///     Checks if an integrated mod is loaded.
    /// </summary>
    /// <param name="name">The name of the mod to check.</param>
    /// <returns>True if the mod is loaded.</returns>
    public bool IsLoaded(string name)
    {
        return this.Mods.ContainsKey(name);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var removedMods = this.Mods.Where(mod => !this.Helper.ModRegistry.IsLoaded(mod.Value)).ToList();
        foreach (var (key, _) in removedMods)
        {
            this.Mods.Remove(key);
        }
    }
}