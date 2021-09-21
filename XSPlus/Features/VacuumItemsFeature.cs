namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <inheritdoc />
    internal class VacuumItemsFeature : BaseFeature
    {
        private static readonly PerScreen<bool> IsVacuuming = new();
        private static VacuumItemsFeature Instance;
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();

        /// <summary>Initializes a new instance of the <see cref="VacuumItemsFeature"/> class.</summary>
        public VacuumItemsFeature()
            : base("VacuumItems")
        {
            VacuumItemsFeature.Instance = this;
        }

        private List<Chest> EnabledChests
        {
            get => this._cachedEnabledChests.Value ??= Game1.player.Items.OfType<Chest>()
                .Where(this.IsEnabledForItem)
                .ToList();
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Player.InventoryChanged += this.OnInventoryChanged;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                transpiler: new HarmonyMethod(typeof(VacuumItemsFeature), nameof(VacuumItemsFeature.Debris_collect_transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventory), new[] { typeof(Item), typeof(List<Item>) }),
                prefix: new HarmonyMethod(typeof(VacuumItemsFeature), nameof(VacuumItemsFeature.Farmer_addItemToInventory_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Player.InventoryChanged -= this.OnInventoryChanged;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                patch: AccessTools.Method(typeof(VacuumItemsFeature), nameof(VacuumItemsFeature.Debris_collect_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventory), new[] { typeof(Item), typeof(List<Item>) }),
                patch: AccessTools.Method(typeof(VacuumItemsFeature), nameof(VacuumItemsFeature.Farmer_addItemToInventory_prefix)));
        }

        private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand.Equals(AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool))))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(VacuumItemsFeature), nameof(VacuumItemsFeature.AddItemToInventoryBool)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static bool Farmer_addItemToInventory_prefix(ref Item __result, ref Item item)
        {
            if (!VacuumItemsFeature.IsVacuuming.Value)
            {
                return true;
            }

            Item remaining = null;
            int stack = item.Stack;
            foreach (Chest chest in VacuumItemsFeature.Instance.EnabledChests)
            {
                remaining = chest.addItem(item);
                if (remaining == null)
                {
                    __result = null;
                    return false;
                }
            }

            if (remaining is not null && remaining.Stack != stack)
            {
                item = remaining;
            }

            return true;
        }

        private static bool AddItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
        {
            if (!VacuumItemsFeature.Instance.EnabledChests.Any())
            {
                return farmer.addItemToInventoryBool(item, makeActiveObject);
            }

            VacuumItemsFeature.IsVacuuming.Value = true;
            bool success = farmer.addItemToInventoryBool(item, makeActiveObject);
            VacuumItemsFeature.IsVacuuming.Value = false;
            return success;
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer || (!e.Added.OfType<Chest>().Any() && !e.Removed.OfType<Chest>().Any()))
            {
                return;
            }

            this._cachedEnabledChests.Value = null;
        }
    }
}