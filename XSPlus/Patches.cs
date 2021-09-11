using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace XSPlus
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming convention defined by Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    internal class Patches
    {
        private static IModHelper Helper;
        private static IMonitor Monitor;

        public Patches(IModHelper helper, IMonitor monitor, Harmony harmony)
        {
            Helper = helper;
            Monitor = monitor;
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.ItemGrabMenu_constructor_postfix))
            );
        }
        /// <summary>Replace default highlight function for inventory menu</summary>
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.playerChest.Value)
                return;
            if (__instance.inventory.highlightMethod != CommonHelper.HighlightMethod_inventory)
            {
                CommonHelper.HighlightMethods_inventory = __instance.inventory.highlightMethod;
                __instance.inventory.highlightMethod = CommonHelper.HighlightMethod_inventory;
            }
            if (__instance.ItemsToGrabMenu.highlightMethod != CommonHelper.HighlightMethod_ItemsToGrabMenu)
            {
                CommonHelper.HighlightMethods_ItemsToGrabMenu = __instance.ItemsToGrabMenu.highlightMethod;
                __instance.ItemsToGrabMenu.highlightMethod = CommonHelper.HighlightMethod_ItemsToGrabMenu;
            }
        }
    }
}