using System.Collections.Generic;
using Common.PatternPatches;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.UI;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

// ReSharper disable UnusedParameter.Global

// ReSharper disable InconsistentNaming

namespace ExpandedStorage.Framework.Patches
{
    internal class ChestPatch : Patch<ModConfig>
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new();

        internal ChestPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config)
        {
        }

        internal static void AddExclusion(string modDataKey)
        {
            if (!ExcludeModDataKeys.Contains(modDataKey))
                ExcludeModDataKeys.Add(modDataKey);
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                new HarmonyMethod(GetType(), nameof(CheckForActionPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.grabItemFromChest)),
                postfix: new HarmonyMethod(GetType(), nameof(GrabItemFromChestPostfix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.grabItemFromInventory)),
                postfix: new HarmonyMethod(GetType(), nameof(GrabItemFromInventoryPostfix))
            );

            if (Config.AllowRestrictedStorage)
                harmony.Patch(
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem), new[] {typeof(Item)}),
                    new HarmonyMethod(GetType(), nameof(AddItemPrefix))
                );

            if (Config.AllowModdedCapacity)
                harmony.Patch(
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    new HarmonyMethod(GetType(), nameof(GetActualCapacity_Prefix))
                );
        }

        public static bool CheckForActionPrefix(Chest __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity
                || !__instance.playerChest.Value
                || !Game1.didPlayerJustRightClick(true))
                return true;

            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null || config.SourceType != SourceType.MoreCraftables)
                return true;
            __instance.GetMutex().RequestLock(delegate
            {
                __instance.frameCounter.Value = 5;
                Game1.playSound(config.OpenSound);
                Game1.player.Halt();
                Game1.player.freezePause = 1000;
            });
            __result = true;
            return false;
        }

        /// <summary>Prevent adding item if filtered.</summary>
        public static bool AddItemPrefix(Chest __instance, ref Item __result, Item item)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (!ReferenceEquals(__instance, item) && (config == null || config.Filter(item)))
                return true;

            __result = item;
            return false;
        }

        /// <summary>Refresh inventory after item grabbed from chest.</summary>
        public static void GrabItemFromChestPostfix()
        {
            MenuViewModel.RefreshItems();
        }

        /// <summary>Refresh inventory after item grabbed from inventory.</summary>
        public static void GrabItemFromInventoryPostfix()
        {
            MenuViewModel.RefreshItems();
        }

        /// <summary>Returns modded capacity for storage.</summary>
        public static bool GetActualCapacity_Prefix(Chest __instance, ref int __result)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null || config.Capacity == 0)
                return true;

            __result = config.Capacity == -1
                ? int.MaxValue
                : config.Capacity;
            return false;
        }
    }
}