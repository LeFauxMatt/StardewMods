namespace Common.Extensions;

using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Extension methods for MenuWithInventory.
/// </summary>
internal static class ClickableMenuExtensions
{
    /// <summary>
    ///     Determines if an IClickableMenu is an ItemGrabMenu for a Player Chest.
    /// </summary>
    /// <param name="menu">The IClickableMenu to check.</param>
    /// <param name="chest">Outputs the Chest associated with the menu if true.</param>
    /// <returns>True if the menu is an ItemGrabMenu for a Player Chest.</returns>
    public static bool IsPlayerChestMenu(this IClickableMenu menu, out Chest chest)
    {
        chest = menu is ItemGrabMenu { shippingBin: false, context: Chest outChest } && outChest.IsPlayerChest()
            ? outChest
            : null;

        return chest is not null;
    }
}