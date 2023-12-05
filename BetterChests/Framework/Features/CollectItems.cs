namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Objects;

/// <summary>Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.</summary>
internal sealed class CollectItems : BaseFeature
{
    private static readonly MethodBase DebrisCollect = AccessTools.Method(typeof(Debris), nameof(Debris.collect));

#nullable disable
    private static CollectItems instance;
#nullable enable

    private readonly PerScreen<List<StorageNode>> eligible = new(() => new());
    private readonly IModEvents events;
    private readonly Harmony harmony;

    /// <summary>Initializes a new instance of the <see cref="CollectItems" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    public CollectItems(IMonitor monitor, ModConfig config, IModEvents events, Harmony harmony)
        : base(monitor, nameof(CollectItems), () => config.CollectItems is not FeatureOption.Disabled)
    {
        CollectItems.instance = this;
        this.events = events;
        this.harmony = harmony;
    }

    private static List<StorageNode> Eligible => CollectItems.instance.eligible.Value;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        Configurator.StorageEdited += CollectItems.OnStorageEdited;
        this.events.GameLoop.SaveLoaded += CollectItems.OnSaveLoaded;
        this.events.Player.InventoryChanged += CollectItems.OnInventoryChanged;

        // Patches
        this.harmony.Patch(
            CollectItems.DebrisCollect,
            transpiler: new(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        Configurator.StorageEdited -= CollectItems.OnStorageEdited;
        this.events.GameLoop.SaveLoaded -= CollectItems.OnSaveLoaded;
        this.events.Player.InventoryChanged -= CollectItems.OnInventoryChanged;

        // Patches
        this.harmony.Unpatch(
            CollectItems.DebrisCollect,
            AccessTools.Method(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)));
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null)
        {
            return true;
        }

        if (!CollectItems.Eligible.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        foreach (var storage in CollectItems.Eligible)
        {
            if (storage is not
                {
                    Data: Storage storageObject,
                })
            {
                continue;
            }

            item.resetState();
            storageObject.ClearNulls();
            item = storage.StashItem(
                CollectItems.instance.Monitor,
                item,
                storage.StashToChestStacks is FeatureOption.Enabled);

            if (item is null)
            {
                break;
            }
        }

        return item is null || farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions) =>
        instructions.MethodReplacer(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
            AccessTools.Method(typeof(CollectItems), nameof(CollectItems.AddItemToInventoryBool)));

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer && (e.Added.OfType<Chest>().Any() || e.Removed.OfType<Chest>().Any()))
        {
            CollectItems.RefreshEligible();
        }
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) => CollectItems.RefreshEligible();

    private static void OnStorageEdited(object? sender, StorageNode storage) => CollectItems.RefreshEligible();

    private static void RefreshEligible()
    {
        CollectItems.Eligible.Clear();
        foreach (var storage in StorageService.FromPlayer(Game1.player, limit: 12))
        {
            if (storage is not
                {
                    CollectItems: FeatureOption.Enabled,
                })
            {
                continue;
            }

            CollectItems.Eligible.Add(storage);
        }
    }
}
