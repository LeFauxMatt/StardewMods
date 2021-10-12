namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Services;
    using CommonHarmony.Enums;
    using CommonHarmony.Services;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <inheritdoc />
    internal class VacuumItemsFeature : BaseFeature
    {
        private static VacuumItemsFeature Instance;
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();
        private FilterItemsFeature _filterItems;
        private HarmonyService _harmony;

        private VacuumItemsFeature(ServiceManager serviceManager)
            : base("VacuumItems", serviceManager)
        {
            // Init
            VacuumItemsFeature.Instance ??= this;

            // Dependencies
            this.AddDependency<FilterItemsFeature>(service => this._filterItems = service as FilterItemsFeature);
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                        typeof(VacuumItemsFeature),
                        nameof(VacuumItemsFeature.Debris_collect_transpiler),
                        PatchType.Transpiler);
                });
        }

        private List<Chest> EnabledChests
        {
            get => this._cachedEnabledChests.Value ??= Game1.player.Items.OfType<Chest>()
                                                            .Where(this.IsEnabledForItem)
                                                            .ToList();
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        internal override bool IsEnabledForItem(Item item)
        {
            return base.IsEnabledForItem(item) && item.Stack == 1 && item is Chest chest && chest.playerChest.Value;
        }

        private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand.Equals(AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool))))
                {
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(VacuumItemsFeature), nameof(VacuumItemsFeature.AddItemToInventoryBool)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        private static bool AddItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
        {
            if (!VacuumItemsFeature.Instance.EnabledChests.Any())
            {
                return farmer.addItemToInventoryBool(item, makeActiveObject);
            }

            foreach (var chest in VacuumItemsFeature.Instance.EnabledChests.Where(chest => VacuumItemsFeature.Instance._filterItems.HasFilterItems(chest)))
            {
                item = chest.addItem(item);
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
                this._cachedEnabledChests.Value = null;
            }
        }
    }
}