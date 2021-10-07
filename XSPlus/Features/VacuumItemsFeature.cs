namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Threading.Tasks;
    using Common.Helpers;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <inheritdoc />
    internal class VacuumItemsFeature : BaseFeature
    {
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();
        private readonly FilterItemsFeature _filterItems;
        private MixInfo _collectPatch;

        private VacuumItemsFeature(ModConfigService modConfigService, FilterItemsFeature filterItems)
            : base("VacuumItems", modConfigService)
        {
            this._filterItems = filterItems;
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="VacuumItemsFeature" />.
        /// </summary>
        private static VacuumItemsFeature Instance { get; set; }

        private List<Chest> EnabledChests
        {
            get => this._cachedEnabledChests.Value ??= Game1.player.Items.OfType<Chest>()
                                                            .Where(this.IsEnabledForItem)
                                                            .ToList();
        }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="VacuumItemsFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="VacuumItemsFeature" /> class.</returns>
        public static async Task<VacuumItemsFeature> Create(ServiceManager serviceManager)
        {
            return VacuumItemsFeature.Instance ??= new(
                await serviceManager.Get<ModConfigService>(),
                await serviceManager.Get<FilterItemsFeature>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            Events.Player.InventoryChanged += this.OnInventoryChanged;

            // Patches
            this._collectPatch = Mixin.Transpiler(
                AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                typeof(VacuumItemsFeature),
                nameof(VacuumItemsFeature.Debris_collect_transpiler));
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            Events.Player.InventoryChanged -= this.OnInventoryChanged;

            // Patches
            Mixin.Unpatch(this._collectPatch);
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