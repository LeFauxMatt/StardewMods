namespace StardewMods.ShoppingCart;

using System.Globalization;
using System.Text;

/// <summary>
///     Mod config data.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets the amount of an item to purchase when holding shift.
    /// </summary>
    public int ShiftClickQuantity { get; set; } = 5;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"ShiftClickQuantity: {this.ShiftClickQuantity.ToString(CultureInfo.InvariantCulture)}");
        return sb.ToString();
    }
}