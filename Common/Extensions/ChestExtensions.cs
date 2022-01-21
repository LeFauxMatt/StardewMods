namespace Common.Extensions;

using StardewValley.Objects;

/// <summary>
/// </summary>
internal static class ChestExtensions
{
    /// <summary>
    /// </summary>
    /// <param name="chest"></param>
    /// <returns></returns>
    public static bool IsPlayerChest(this Chest chest)
    {
        return chest is { playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin };
    }
}