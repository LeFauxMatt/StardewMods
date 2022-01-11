namespace BetterChests.Extensions;

using StardewValley.Objects;

internal static class ChestExtensions
{
    public static bool IsPlayerChest(this Chest chest)
    {
        return chest is {playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin};
    }
}