namespace StardewMods.BetterChests.Services;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
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
        services.Lazy<AssetHandler>(assetHandler =>
        {
            assetHandler.AddModDataKey($"{BetterChests.ModUniqueId}/StorageName");
            assetHandler.AddModDataKey($"{ModIntegrations.ExpandedStorageModUniqueId}/Storage");
        });
        services.Lazy<IGameObjects>(gameObjects =>
        {
            gameObjects.AddInventoryItemsGetter(this.GetInventoryItems);
            gameObjects.AddLocationObjectsGetter(this.GetLocationObjects);
        });
    }

    private IModHelper Helper { get; }

    private IDictionary<string, string> Mods { get; } = new Dictionary<string, string>
    {
        { "Automate", ModIntegrations.AutomateModUniqueId },
        { "Expanded Storage", ModIntegrations.ExpandedStorageModUniqueId },
        { "Horse Overhaul", ModIntegrations.HorseOverhaulModUniqueId },
    };

    /// <summary>
    ///     Checks if an integrated mod is loaded.
    /// </summary>
    /// <param name="name">The name of the mod to check.</param>
    /// <returns>True if the mod is loaded.</returns>
    public bool IsLoaded(string name)
    {
        return this.Mods.ContainsKey(name);
    }

    private IEnumerable<(int Index, object Context)> GetInventoryItems(Farmer player)
    {
        if (player.mount is not null && this.TryGetSaddleBag(out _, out var saddleBag))
        {
            saddleBag.modData[$"{BetterChests.ModUniqueId}/StorageName"] = "Saddle Bag";
            yield return new(-1, saddleBag);
        }
    }

    private IEnumerable<(Vector2 Index, object Context)> GetLocationObjects(GameLocation location)
    {
        if (this.TryGetSaddleBag(out var horse, out var saddleBag) && horse.currentLocation.Equals(location))
        {
            saddleBag.modData[$"{BetterChests.ModUniqueId}/StorageName"] = "Saddle Bag";
            yield return new(horse.Position / 64f, saddleBag);
        }
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var removedMods = this.Mods.Where(mod => !this.Helper.ModRegistry.IsLoaded(mod.Value)).ToList();
        foreach (var (key, _) in removedMods)
        {
            this.Mods.Remove(key);
        }
    }

    private bool TryGetSaddleBag(out Horse horse, out Chest saddleBag)
    {
        if (!this.IsLoaded("Horse Overhaul"))
        {
            horse = null;
            saddleBag = null;
            return false;
        }

        // Attempt to load saddle bags
        var farm = Game1.getFarm();
        foreach (var stable in farm.buildings.OfType<Stable>())
        {
            if (!stable.modData.TryGetValue($"{ModIntegrations.HorseOverhaulModUniqueId}/stableID", out var stableId) || !int.TryParse(stableId, out var x))
            {
                continue;
            }

            if (!farm.Objects.TryGetValue(new(x, 0), out var obj) || obj is not Chest chest || !chest.modData.ContainsKey($"{ModIntegrations.HorseOverhaulModUniqueId}/isSaddleBag"))
            {
                continue;
            }

            if (Game1.player.mount?.HorseId == stable.HorseId)
            {
                horse = Game1.player.mount;
                saddleBag = chest;
                return true;
            }

            horse = stable.getStableHorse();
            if (horse?.getOwner() != Game1.player)
            {
                saddleBag = chest;
                return true;
            }
        }

        horse = null;
        saddleBag = null;
        return false;
    }
}