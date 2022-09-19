namespace StardewMods.ExpandedStorage.Models;

/// <summary>
///     Represents a shop entry for an Expanded Storage chest or recipe.
/// </summary>
internal sealed class ShopEntry
{
    /// <summary>
    ///     Gets or sets a value indicating whether this shop entry is for a recipe.
    /// </summary>
    public bool IsRecipe { get; set; }

    /// <summary>
    ///     Gets or sets the price of this shop entry.
    /// </summary>
    public int Price { get; set; }

    /// <summary>
    ///     Gets or sets the shop id where this will be sold.
    /// </summary>
    public string ShopId { get; set; } = string.Empty;
}