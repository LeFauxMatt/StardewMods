using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace XSPlus.Features
{
    internal class VacuumItems : BaseFeature
    {
        private static VacuumItems _feature;
        private IList<Chest> VacuumChests => _vacuumChests.Value ??= Game1.player.Items.Take(12).OfType<Chest>().Where(IsEnabled).ToList();
        private readonly PerScreen<IList<Chest>> _vacuumChests = new();
        private FilterItems _filterItems;
        public VacuumItems(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Events
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Player.InventoryChanged += OnInventoryChanged;
            
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                prefix: new HarmonyMethod(typeof(VacuumItems), nameof(VacuumItems.Debris_collect_prefix))
            );
        }
        protected override void DisableFeature()
        {
            // Events
            Helper.Events.GameLoop.GameLaunched -= OnGameLaunched;
            Helper.Events.Player.InventoryChanged -= OnInventoryChanged;
            
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                patch: AccessTools.Method(typeof(VacuumItems), nameof(VacuumItems.Debris_collect_prefix))
            );
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _filterItems = XSPlus.Features["FilterItems"] as FilterItems;
        }
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;
            _vacuumChests.Value = null;
        }
        private IEnumerable<Chest> GetVacuumChestsForItem(Item item = null)
        {
            return item is null
                ? VacuumChests
                : VacuumChests.Where(vacuumChest => _filterItems.TakesItem(vacuumChest, item)).ToList();
        }
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static bool Debris_collect_prefix(Debris __instance, ref bool __result, Farmer farmer, Chunk chunk)
        {
            chunk ??= __instance.Chunks.FirstOrDefault();
            if (chunk == null || !_feature.VacuumChests.Any())
                return true;
            var switcher = __instance.debrisType.Value.Equals(Debris.DebrisType.ARCHAEOLOGY) || __instance.debrisType.Value.Equals(Debris.DebrisType.OBJECT)
                ? chunk.debrisType
                : chunk.debrisType - chunk.debrisType % 2;
            if (__instance.item == null && __instance.debrisType.Value == 0)
                return true;
            if (__instance.item != null)
            {
                // Golden Walnuts
                if (Utility.IsNormalObjectAtParentSheetIndex(__instance.item, 73))
                    return true;
                // Lost Book
                if (Utility.IsNormalObjectAtParentSheetIndex(__instance.item, 102))
                    return true;
                // Qi Gems
                if (Utility.IsNormalObjectAtParentSheetIndex(__instance.item, 858))
                    return true;
                if (Utility.IsNormalObjectAtParentSheetIndex(__instance.item, 930))
                    return true;
                __instance.item = farmer.AddItemToInventory(__instance.item, _feature.GetVacuumChestsForItem(__instance.item));
                __result = __instance.item == null;
                return !__result;
            }
            Item item = __instance.debrisType.Value switch
            {
                Debris.DebrisType.ARCHAEOLOGY => new Object(chunk.debrisType, 1),
                _ when switcher <= -10000 => new MeleeWeapon(switcher),
                _ when switcher <= 0 => new Object(Vector2.Zero, -switcher),
                _ when switcher is 93 or 94 => new Torch(Vector2.Zero, 1, switcher)
                {
                    Quality = __instance.itemQuality
                },
                _ => new Object(Vector2.Zero, switcher, 1) {Quality = __instance.itemQuality}
            };
            item = farmer.AddItemToInventory(item, _feature.GetVacuumChestsForItem(item));
            __result = item == null;
            return !__result;
        }
    }
}