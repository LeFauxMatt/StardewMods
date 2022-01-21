namespace Common.Extensions;

using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
/// </summary>
internal static class MenuWithInventoryExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="chest"></param>
    /// <returns></returns>
    public static bool IsPlayerChestMenu(this MenuWithInventory menu, out Chest chest)
    {
        chest = menu is ItemGrabMenu { shippingBin: false, context: Chest outChest } && outChest.IsPlayerChest()
            ? outChest
            : null;

        return chest is not null;
    }
}