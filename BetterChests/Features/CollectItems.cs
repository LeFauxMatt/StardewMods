namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;

/// <inheritdoc />
internal class CollectItems : Feature
{
    private readonly PerScreen<IList<IManagedStorage>> _eligibleChests = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CollectItems" /> class.
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

    private IList<IManagedStorage> EligibleChests
    {
        get => this._eligibleChests.Value ??= (
            from inventoryStorage in this.ManagedStorages.InventoryStorages
            where inventoryStorage.Value.CollectItems == FeatureOption.Enabled
            select inventoryStorage.Value).ToList();
        set => this._eligibleChests.Value = value;
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
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

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (!e.IsLocalPlayer)
        {
            return;
        }

        if (e.Added.Any(item => this.ManagedStorages.TryGetManagedStorage(item, out _)))
        {
            this.EligibleChests = null;
            return;
        }

        if (e.Removed.Any(item => this.ManagedStorages.TryGetManagedStorage(item, out _)))
        {
            this.EligibleChests = null;
            return;
        }

        if (e.QuantityChanged.Any(stack => this.ManagedStorages.TryGetManagedStorage(stack.Item, out _)))
        {
            this.EligibleChests = null;
        }
    }
}