namespace BetterChests.Extensions;

using StardewValley.Menus;
using StardewValley.Objects;

internal static class MenuWithInventoryExtensions
{
    public static bool IsPlayerChestMenu(this MenuWithInventory menu, out Chest chest)
    {
        chest = menu is ItemGrabMenu {shippingBin: false, context: Chest outChest} && outChest.IsPlayerChest()
            ? outChest
            : null;

        return chest is not null;
    }
}