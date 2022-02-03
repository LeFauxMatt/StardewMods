namespace Common.Extensions;

using StardewValley.Objects;

/// <summary>
///     Extension methods for StardewValley Chest.
/// </summary>
internal static class ChestExtensions
{
    /// <summary>
    ///     Determines if a Chest is a player chest.
    /// </summary>
    /// <param name="chest">The chest to check.</param>
    /// <returns>The if the chest is a player chest.</returns>
    public static bool IsPlayerChest(this Chest chest)
    {
        return chest is { playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin };
    }
}