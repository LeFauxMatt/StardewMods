namespace StardewMods.BetterChests.Framework.Features;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Handlers;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Objects;

/// <summary>
///     Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.
/// </summary>
internal sealed class CollectItems : IFeature
{
    private const string Id = "furyx639.BetterChests/CollectItems";

#nullable disable
    private static CollectItems Instance;
#nullable enable

    private readonly PerScreen<List<BaseStorage>> _eligible = new(() => new());
    private readonly IModHelper _helper;

    private bool _isActivated;

    private CollectItems(IModHelper helper)
    {
        this._helper = helper;
        HarmonyHelper.AddPatches(
            CollectItems.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                    typeof(CollectItems),
                    nameof(CollectItems.Debris_collect_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static List<BaseStorage> Eligible => CollectItems.Instance._eligible.Value;

    /// <summary>
    ///     Initializes <see cref="CollectItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="CollectItems" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return CollectItems.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void SetActivated(bool value)
    {
        if (this._isActivated == value)
        {
            return;
        }

        this._isActivated = value;
        if (this._isActivated)
        {
            HarmonyHelper.ApplyPatches(CollectItems.Id);
            Configurator.StorageEdited += CollectItems.OnStorageEdited;
            this._helper.Events.GameLoop.SaveLoaded += CollectItems.OnSaveLoaded;
            this._helper.Events.Player.InventoryChanged += CollectItems.OnInventoryChanged;
            return;
        }

        HarmonyHelper.UnapplyPatches(CollectItems.Id);
        Configurator.StorageEdited -= CollectItems.OnStorageEdited;
        this._helper.Events.GameLoop.SaveLoaded -= CollectItems.OnSaveLoaded;
        this._helper.Events.Player.InventoryChanged -= CollectItems.OnInventoryChanged;
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
            item.resetState();
            storage.ClearNulls();
            item = storage.StashItem(item, storage.StashToChestStacks is FeatureOption.Enabled);

            if (item is null)
            {
                break;
            }
        }

        return item is null || farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return instructions.MethodReplacer(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
            AccessTools.Method(typeof(CollectItems), nameof(CollectItems.AddItemToInventoryBool)));
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer && (e.Added.OfType<Chest>().Any() || e.Removed.OfType<Chest>().Any()))
        {
            CollectItems.RefreshEligible();
        }
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        CollectItems.RefreshEligible();
    }

    private static void OnStorageEdited(object? sender, BaseStorage storage)
    {
        CollectItems.RefreshEligible();
    }

    private static void RefreshEligible()
    {
        var storages = Storages.FromPlayer(Game1.player, limit: 12);
        CollectItems.Eligible.Clear();
        CollectItems.Eligible.AddRange(storages.Where(storage => storage.CollectItems is FeatureOption.Enabled));
    }
}