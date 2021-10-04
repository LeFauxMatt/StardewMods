namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Services;
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
        private static readonly PerScreen<bool> IsVacuuming = new();
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();
        private MixInfo _addItemToInventoryPatch;
        private MixInfo _collectPatch;

        private VacuumItemsFeature(ModConfigService modConfigService)
            : base("VacuumItems", modConfigService)
        {
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
            return VacuumItemsFeature.Instance ??= new(await serviceManager.Get<ModConfigService>());
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

            this._addItemToInventoryPatch = Mixin.Prefix(
                AccessTools.Method(
                    typeof(Farmer),
                    nameof(Farmer.addItemToInventory),
                    new[]
                    {
                        typeof(Item), typeof(List<Item>),
                    }),
                typeof(VacuumItemsFeature),
                nameof(VacuumItemsFeature.Farmer_addItemToInventory_prefix));
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            Events.Player.InventoryChanged -= this.OnInventoryChanged;

            // Patches
            Mixin.Unpatch(this._collectPatch);
            Mixin.Unpatch(this._addItemToInventoryPatch);
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
            var stack = item.Stack;
            foreach (var chest in VacuumItemsFeature.Instance.EnabledChests)
            {
                remaining = chest.addItem(item);
                if (remaining is null)
                {
                    __result = null!;
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
            var success = farmer.addItemToInventoryBool(item, makeActiveObject);
            VacuumItemsFeature.IsVacuuming.Value = false;
            return success;
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer || !e.Added.OfType<Chest>().Any() && !e.Removed.OfType<Chest>().Any())
            {
                return;
            }

            this._cachedEnabledChests.Value = null;
        }
    }
}