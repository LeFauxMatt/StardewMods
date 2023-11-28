namespace StardewMods.BetterChests.Framework;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.Automate;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
    private const string ExpandedFridgeId = "Uwazouri.ExpandedFridge";
    private const string HorseOverhaulId = "Goldenrevolver.HorseOverhaul";
    private const string WearMoreRingsId = "bcmpinc.WearMoreRings";

#nullable disable
    private static Integrations instance;
#nullable enable

    private readonly AutomateIntegration automate;
    private readonly BetterCraftingIntegration betterCrafting;
    private readonly ModConfig config;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IModHelper helper;
    private readonly Dictionary<string, HashSet<string>> incompatibilities;
    private readonly ToolbarIconsIntegration toolbarIcons;

    private Integrations(IModHelper helper, ModConfig config)
    {
        this.helper = helper;
        this.config = config;
        this.automate = new(helper.ModRegistry);
        this.betterCrafting = new(helper.ModRegistry);
        this.gmcm = new(helper.ModRegistry);
        this.toolbarIcons = new(helper.ModRegistry);
        this.incompatibilities =
            this.helper.ModContent.Load<Dictionary<string, HashSet<string>>>("assets/incompatibilities.json");
        this.helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <summary>
    ///     Gets Automate integration.
    /// </summary>
    public static AutomateIntegration Automate => Integrations.instance.automate;

    /// <summary>
    ///     Gets Better Craft integration.
    /// </summary>
    public static BetterCraftingIntegration BetterCrafting => Integrations.instance.betterCrafting;

    /// <summary>
    ///     Gets Generic Mod Config Menu integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GMCM => Integrations.instance.gmcm;

    /// <summary>
    ///     Gets Toolbar Icons integration.
    /// </summary>
    public static ToolbarIconsIntegration ToolbarIcons => Integrations.instance.toolbarIcons;

    private static Dictionary<string, HashSet<string>> Incompatibilities =>
        Integrations.instance.incompatibilities;

    private static IModRegistry ModRegistry => Integrations.instance.helper.ModRegistry;

    /// <summary>
    ///     Gets all storages placed in a particular location.
    /// </summary>
    /// <param name="location">The location to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all placed storages at the location.</returns>
    public static IEnumerable<Storage> FromLocation(GameLocation location, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();

        foreach (var storage in Integrations.ExpandedFridge_FromLocation(location, excluded))
        {
            yield return storage;
        }

        foreach (var storage in Integrations.HorseOverhaul_FromLocation(location, excluded))
        {
            yield return storage;
        }

        if (Integrations.ModRegistry.IsLoaded(Integrations.WearMoreRingsId)
            && location is Farm
            && location.Objects.TryGetValue(new(0, -50), out var obj))
        {
            excluded.Add(obj);
        }
    }

    /// <summary>
    ///     Gets all storages placed in a particular farmer's inventory.
    /// </summary>
    /// <param name="player">The farmer to get storages from.</param>
    /// <param name="excluded">A list of storage contexts to exclude to prevent iterating over the same object.</param>
    /// <returns>An enumerable of all held storages in the farmer's inventory.</returns>
    public static IEnumerable<Storage> FromPlayer(Farmer player, ISet<object>? excluded = null)
    {
        excluded ??= new HashSet<object>();

        foreach (var storage in Integrations.HorseOverhaul_FromPlayer(player, excluded))
        {
            yield return storage;
        }
    }

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModHelper helper, ModConfig config)
    {
        return Integrations.instance ??= new(helper, config);
    }

    /// <summary>
    ///     Checks if any known incompatibilities.
    /// </summary>
    /// <param name="featureName">The feature to check.</param>
    /// <param name="mods">The list of incompatible mods.</param>
    /// <returns>Returns true if there is an incompatibility.</returns>
    public static bool TestConflicts(string featureName, [NotNullWhen(true)] out List<IModInfo?>? mods)
    {
        if (!Integrations.Incompatibilities.TryGetValue(featureName, out var modIds))
        {
            mods = null;
            return false;
        }

        mods = modIds.Where(Integrations.ModRegistry.IsLoaded).Select(Integrations.ModRegistry.Get).ToList();
        return mods.Any();
    }

    /// <summary>
    ///     Attempts to retrieve a storage based on a context object.
    /// </summary>
    /// <param name="context">The context object.</param>
    /// <param name="storage">The storage object.</param>
    /// <returns>Returns true if a storage could be found for the context object.</returns>
    public static bool TryGetOne(object? context, [NotNullWhen(true)] out Storage? storage)
    {
        if (Integrations.HorseOverhaul_TryGetOne(context, out storage))
        {
            return true;
        }

        storage = default;
        return false;
    }

    private static IEnumerable<Storage> ExpandedFridge_FromLocation(GameLocation location, ISet<object> excluded)
    {
        if (!Integrations.ModRegistry.IsLoaded(Integrations.ExpandedFridgeId)
            || location is not FarmHouse { upgradeLevel: > 0 })
        {
            yield break;
        }

        foreach (var (pos, obj) in location.Objects.Pairs)
        {
            if ((int)pos.Y == -300 && obj is Chest { bigCraftable.Value: true, ParentSheetIndex: 216 } chest)
            {
                excluded.Add(chest);
            }
        }
    }

    private static IEnumerable<Storage> HorseOverhaul_FromLocation(GameLocation location, ISet<object> excluded)
    {
        if (!Integrations.ModRegistry.IsLoaded(Integrations.HorseOverhaulId))
        {
            yield break;
        }

        var farm = Game1.getFarm();
        foreach (var stable in farm.buildings.OfType<Stable>())
        {
            if (!stable.modData.TryGetValue($"{Integrations.HorseOverhaulId}/stableID", out var stableId)
                || !int.TryParse(stableId, out var x)
                || !farm.Objects.TryGetValue(new(x, 0), out var obj)
                || obj is not Chest chest
                || !chest.modData.ContainsKey($"{Integrations.HorseOverhaulId}/isSaddleBag"))
            {
                continue;
            }

            var horse = Game1.player.mount;
            if (horse?.HorseId == stable.HorseId && Game1.player.currentLocation.Equals(location))
            {
                excluded.Add(chest);
                yield return new ChestStorage(chest, horse, Game1.player.Tile);
            }

            horse = stable.getStableHorse();
            if (horse?.getOwner() != Game1.player || !horse.currentLocation.Equals(location))
            {
                continue;
            }

            excluded.Add(chest);
            yield return new ChestStorage(chest, horse, horse.Tile);
        }
    }

    private static IEnumerable<Storage> HorseOverhaul_FromPlayer(Farmer player, ISet<object> excluded)
    {
        if (!Integrations.ModRegistry.IsLoaded(Integrations.HorseOverhaulId))
        {
            yield break;
        }

        if (player.mount is null)
        {
            yield break;
        }

        var farm = Game1.getFarm();
        var stable = farm.buildings.OfType<Stable>().FirstOrDefault(stable => stable.HorseId == player.mount.HorseId);
        if (stable is null
            || !stable.modData.TryGetValue($"{Integrations.HorseOverhaulId}/stableID", out var stableId)
            || !int.TryParse(stableId, out var x)
            || !farm.Objects.TryGetValue(new(x, 0), out var obj)
            || obj is not Chest chest
            || !chest.modData.ContainsKey($"{Integrations.HorseOverhaulId}/isSaddleBag"))
        {
            yield break;
        }

        excluded.Add(chest);
        yield return new ChestStorage(chest, Game1.player, player.Tile);
    }

    private static bool HorseOverhaul_TryGetOne(object? context, [NotNullWhen(true)] out Storage? storage)
    {
        if (!Integrations.ModRegistry.IsLoaded(Integrations.HorseOverhaulId)
            || context is not Chest chest
            || !chest.modData.ContainsKey($"{Integrations.HorseOverhaulId}/isSaddleBag"))
        {
            storage = default;
            return false;
        }

        var farm = Game1.getFarm();
        foreach (var stable in farm.buildings.OfType<Stable>())
        {
            if (!stable.modData.TryGetValue($"{Integrations.HorseOverhaulId}/stableID", out var stableId)
                || !int.TryParse(stableId, out var x)
                || !farm.Objects.TryGetValue(new(x, 0), out var obj)
                || !ReferenceEquals(chest, obj))
            {
                continue;
            }

            var horse = Game1.player.mount;
            if (horse?.HorseId == stable.HorseId)
            {
                storage = new ChestStorage(chest, Game1.player, horse.Tile);
                return true;
            }

            horse = stable.getStableHorse();
            if (horse?.getOwner() != Game1.player)
            {
                continue;
            }

            storage = new ChestStorage(chest, horse, horse.Tile);
            return true;
        }

        storage = default;
        return false;
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        Storages.StorageTypeRequested += this.OnStorageTypeRequested;

        if (Integrations.ModRegistry.IsLoaded(Integrations.HorseOverhaulId)
            && !this.config.VanillaStorages.ContainsKey("SaddleBag"))
        {
            this.config.VanillaStorages.Add(
                "SaddleBag",
                new()
                {
                    CustomColorPicker = FeatureOption.Disabled,
                });
        }
    }

    private void OnStorageTypeRequested(object? sender, IStorageTypeRequestedEventArgs e)
    {
        switch (e.Context)
        {
            case Chest chest when Integrations.ModRegistry.IsLoaded(Integrations.HorseOverhaulId)
                && chest.modData.ContainsKey($"{Integrations.HorseOverhaulId}/isSaddleBag")
                && this.config.VanillaStorages.TryGetValue("SaddleBag", out var saddleBagData):
                e.Load(saddleBagData, -1);
                return;
        }
    }
}