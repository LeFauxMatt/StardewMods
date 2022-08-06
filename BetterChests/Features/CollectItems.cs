namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Objects;

/// <summary>
///     Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.
/// </summary>
internal class CollectItems : IFeature
{
    private const string Id = "furyx639.BetterChests/CollectItems";

    private static CollectItems? Instance;

    private readonly PerScreen<List<IStorageObject>> _eligiblePerScreen = new(() => new());

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

    private List<IStorageObject> CachedEligible => this._eligiblePerScreen.Value;

    /// <summary>
    ///     Initializes <see cref="CollectItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="CollectItems" /> class.</returns>
    public static CollectItems Init(IModHelper helper)
    {
        return CollectItems.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(CollectItems.Id);
        Configurator.StorageEdited += this.OnStorageEdited;
        this._helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        this._helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(CollectItems.Id);
        Configurator.StorageEdited -= this.OnStorageEdited;
        this._helper.Events.GameLoop.SaveLoaded -= this.OnSaveLoaded;
        this._helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null)
        {
            return true;
        }

        if (!CollectItems.Instance!.CachedEligible.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        foreach (var storage in CollectItems.Instance.CachedEligible)
        {
            item.resetState();
            storage.ClearNulls();
            item = storage.StashItem(item, storage.StashToChestStacks != FeatureOption.Disabled);

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

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer && (e.Added.OfType<Chest>().Any() || e.Removed.OfType<Chest>().Any()))
        {
            this.RefreshEligible();
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.RefreshEligible();
    }

    private void OnStorageEdited(object? sender, IStorageObject storage)
    {
        this.RefreshEligible();
    }

    private void RefreshEligible()
    {
        this.CachedEligible.Clear();
        this.CachedEligible.AddRange(
            Game1.player.Items.Take(12)
                 .Select(
                     item => StorageHelper.TryGetOne(item, out var storage)
                          && storage.CollectItems != FeatureOption.Disabled
                         ? storage
                         : null)
                 .OfType<IStorageObject>());
    }
}