namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BetterChests.Enums;
using BetterChests.Models;
using FuryCore.Enums;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class VacuumItems : Feature
{
    private readonly PerScreen<IList<ManagedChest>> _eligibleChests = new();
    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="VacuumItems"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public VacuumItems(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        VacuumItems.Instance ??= this;
        this._harmony = services.Lazy<HarmonyHelper>(VacuumItems.AddPatches);
    }

    private static VacuumItems Instance { get; set; }

    private HarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private IList<ManagedChest> EligibleChests
    {
        get => this._eligibleChests.Value ??= (
            from item in this.ManagedChests.AccessibleChests
            where item.Value.Config.VacuumItems == FeatureOption.Enabled
                  && ReferenceEquals(item.Key.Player, Game1.player)
                  && item.Value.Chest.Stack == 1
            select item.Value).ToList();
        set => this._eligibleChests.Value = value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(nameof(VacuumItems));
        this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc/>
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(nameof(VacuumItems));
        this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
    }

    private static void AddPatches(HarmonyHelper harmony)
    {
        harmony.AddPatches(
            nameof(VacuumItems),
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                    typeof(VacuumItems),
                    nameof(VacuumItems.Debris_collect_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Callvirt && instruction.operand.Equals(AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool))))
            {
                yield return new(OpCodes.Call, AccessTools.Method(typeof(VacuumItems), nameof(VacuumItems.AddItemToInventoryBool)));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
    {
        if (!VacuumItems.Instance.EligibleChests.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        foreach (var managedChest in VacuumItems.Instance.EligibleChests)
        {
            item = managedChest.StashItem(item, true);
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