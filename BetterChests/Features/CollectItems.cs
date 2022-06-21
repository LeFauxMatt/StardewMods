namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Common.Enums;
using Common.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Objects;

/// <summary>
///     Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.
/// </summary>
internal class CollectItems : IFeature
{
    private const string Id = "BetterChests.CollectItems";

    private readonly PerScreen<List<EligibleChest>?> _cachedEligibleChests = new();

    private CollectItems(IModHelper helper)
    {
        this.Helper = helper;
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

    private static IEnumerable<EligibleChest> EligibleChests
    {
        get
        {
            foreach (var chest in Game1.player.Items.Take(12).OfType<Chest>())
            {
                // Disabled for object
                if (!StorageHelper.TryGetOne(chest, out var storage) || storage.CollectItems == FeatureOption.Disabled)
                {
                    continue;
                }

                // Try to stash
                if (storage.FilterItems != FeatureOption.Disabled && storage.FilterItemsList is not null)
                {
                    var itemMatcher = new ItemMatcher(true);
                    foreach (var filter in storage.FilterItemsList)
                    {
                        itemMatcher.Add(filter);
                    }

                    if (itemMatcher.Any() && !itemMatcher.All(filter => filter.StartsWith("!")))
                    {
                        yield return new(chest, itemMatcher);
                        continue;
                    }
                }

                yield return new(chest, null);
            }
        }
    }

    private static CollectItems? Instance { get; set; }

    private List<EligibleChest>? CachedEligibleChest
    {
        get => this._cachedEligibleChests.Value;
        set => this._cachedEligibleChests.Value = value;
    }

    private IModHelper Helper { get; }

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
        HarmonyHelper.ApplyPatches(CollectItems.Id);
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(CollectItems.Id);
        this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null)
        {
            return true;
        }

        CollectItems.Instance!.CachedEligibleChest ??= CollectItems.EligibleChests.ToList();

        if (!CollectItems.Instance.CachedEligibleChest.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        foreach (var (chest, itemMatcher) in CollectItems.Instance.CachedEligibleChest)
        {
            item.resetState();
            chest.clearNulls();

            // Add if categorized
            if (itemMatcher?.Matches(item) == true)
            {
                item = chest.addItem(item);
            }

            // Add if stackable
            if (item is not null && chest.items.Any(chestItem => chestItem.canStackWith(item)))
            {
                item = chest.addItem(item);
            }

            if (item is null)
            {
                break;
            }
        }

        return item is null || farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Callvirt && instruction.operand.Equals(AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool))))
            {
                yield return new(OpCodes.Call, AccessTools.Method(typeof(CollectItems), nameof(CollectItems.AddItemToInventoryBool)));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer && (e.Added.OfType<Chest>().Any() || e.Removed.OfType<Chest>().Any()))
        {
            this.CachedEligibleChest = null;
        }
    }

    private record EligibleChest(Chest Chest, ItemMatcher? Filter);
}