namespace Common.Records;

using System.Linq;
using StardewValley;

/// <summary>
///     A record to represent the player and index of an item in a player's inventory.
/// </summary>
internal readonly record struct PlayerItem(Farmer Player, int Index)
{
    /// <summary>
    ///     Gets the Item referred to by this record.
    /// </summary>
    public Item Item
    {
        get => this.Player.Items.ElementAtOrDefault(this.Index);
    }
}