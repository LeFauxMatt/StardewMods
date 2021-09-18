namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CommonFeature : BaseFeature
    {
        // TODO: Collect common prefix/postfix patches into events
        private static readonly Type[] ItemGrabMenuConstructorParams = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };

        /// <summary>Initializes a new instance of the <see cref="CommonFeature"/> class.</summary>
        /// <param name="monitor">Encapsulates monitoring and logging for a given module.</param>
        internal CommonFeature()
            : base("Common")
        {
        }

        /// <summary>Gets or sets multicast delegate for highlighting items in chest inventory.</summary>
        public static InventoryMenu.highlightThisItem HighlightChestItems { get; internal set; }

        /// <summary>Gets or sets multicast delegate for highlighting items in player inventory.</summary>
        public static InventoryMenu.highlightThisItem HighlightPlayerItems { get; internal set; }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), CommonFeature.ItemGrabMenuConstructorParams),
                postfix: new HarmonyMethod(typeof(CommonFeature), nameof(CommonFeature.ItemGrabMenu_constructor_postfix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), CommonFeature.ItemGrabMenuConstructorParams),
                patch: AccessTools.Method(typeof(CommonFeature), nameof(CommonFeature.ItemGrabMenu_constructor_postfix)));
        }

        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.playerChest.Value)
            {
                return;
            }

            if (__instance.inventory.highlightMethod != CommonFeature.OnHighlightPlayerItems)
            {
                CommonFeature.HighlightPlayerItems = __instance.inventory.highlightMethod;
                __instance.inventory.highlightMethod = CommonFeature.OnHighlightPlayerItems;
            }

            if (__instance.ItemsToGrabMenu.highlightMethod == CommonFeature.OnHighlightChestItems)
            {
                return;
            }

            CommonFeature.HighlightChestItems = __instance.ItemsToGrabMenu.highlightMethod;
            __instance.ItemsToGrabMenu.highlightMethod = CommonFeature.OnHighlightChestItems;
        }

        private static bool OnHighlightChestItems(Item item)
        {
            return CommonFeature.HighlightChestItems.GetInvocationList().All(highlightMethod => (bool)highlightMethod.DynamicInvoke(item));
        }

        private static bool OnHighlightPlayerItems(Item item)
        {
            return CommonFeature.HighlightPlayerItems.GetInvocationList().All(highlightMethod => (bool)highlightMethod.DynamicInvoke(item));
        }
    }
}