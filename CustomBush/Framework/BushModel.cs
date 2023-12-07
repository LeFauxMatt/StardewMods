namespace StardewMods.CustomBush.Framework;

using StardewValley.GameData;

/// <summary>Model used for custom tea saplings.</summary>
internal sealed class BushModel
{
    /// <summary>Gets or sets the age needed to produce.</summary>
    public int AgeToProduce { get; set; } = 20;

    /// <summary>Gets or sets the day of month to begin producing.</summary>
    public int DayToBeginProducing { get; set; } = 22;

    /// <summary>Gets or sets the items produced by this custom bush.</summary>
    public List<DropsModel> ItemsProduced { get; set; } = new();

    /// <summary>Gets or sets the season in which this bush will produce its drops.</summary>
    public List<Season> Seasons { get; set; } = new()
    {
        Season.Spring,
        Season.Summer,
        Season.Fall,
    };

    /// <summary>
    /// Gets or sets the rules which override the locations that custom bushes can be planted in.
    /// </summary>
    public List<PlantableRule> PlantableLocationRules { get; set; } = new();

    /// <summary>Gets or sets the texture of the tea bush.</summary>
    public string Texture { get; set; } = string.Empty;

    /// <summary>Gets or sets the row index for the custom bush's sprites.</summary>
    public int TextureSpriteRow { get; set; }
}
