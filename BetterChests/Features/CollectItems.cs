namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class CollectItems : Feature
{
    private readonly PerScreen<IList<ManagedChest>> _eligibleChests = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectItems"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CollectItems(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        CollectItems.Instance ??= this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatches(
                    this.Id,
                    new SavedPatch[]
                    {
                        new(
                            AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                            typeof(CollectItems),
                            nameof(CollectItems.Debris_collect_transpiler),
                            PatchType.Transpiler),
                    });
            });
    }

    private static CollectItems Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private IList<ManagedChest> EligibleChests
    {
        get => this._eligibleChests.Value ??= (
            from managedChest in this.ManagedChests.AccessibleChests
            where managedChest.CollectItems == FeatureOption.Enabled
                  && managedChest.CollectionType == ItemCollectionType.PlayerInventory
                  && ReferenceEquals(managedChest.Player, Game1.player)
                  && managedChest.Chest.Stack == 1
            select managedChest).ToList();
        set => this._eligibleChests.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc/>
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
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

    private static bool AddItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
    {
        if (!CollectItems.Instance.EligibleChests.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        foreach (var managedChest in CollectItems.Instance.EligibleChests)
        {
            item = managedChest.StashItem(item);
            if (item is null)
            {
                break;
            }
        }

        return item is null || farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer && (e.Added.OfType<Chest>().Any() || e.Removed.OfType<Chest>().Any() || e.QuantityChanged.Any(stack => stack.Item is Chest && stack.NewSize == 1)))
        {
            this.EligibleChests = null;
        }
    }
}