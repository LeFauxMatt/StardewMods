namespace Common.Extensions;

using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

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
        chest = menu switch
        {
            ItemGrabMenu { shippingBin: false, context: Chest outChest } when outChest.IsPlayerChest() => outChest,
            ItemGrabMenu { context: JunimoHut { output.Value: { } junimoHutChest } } when junimoHutChest.IsPlayerChest() => junimoHutChest,
            ItemGrabMenu { context: SObject { ParentSheetIndex: 165, heldObject.Value: Chest heldChest } } when heldChest.IsPlayerChest() => heldChest,
            _ => null,
        };

        return chest is not null;
    }
}