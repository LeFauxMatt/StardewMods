namespace StardewMods.CustomBush.Framework.Models;

using StardewValley.GameData;

/// <summary>Model used for custom tea saplings.</summary>
internal sealed class BushModel
{
    /// <summary>Gets or sets the age needed to produce.</summary>
    public int AgeToProduce { get; set; } = 20;

    /// <summary>Gets or sets the day of month to begin producing.</summary>
    public int DayToBeginProducing { get; set; } = 22;

    /// <summary>Gets or sets the description of the bush.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the bush.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the default texture used when planted indoors.</summary>
    public string IndoorTexture { get; set; } = string.Empty;

    /// <summary>Gets or sets the items produced by this custom bush.</summary>
    public List<DropsModel> ItemsProduced { get; set; } = [];

    /// <summary>Gets or sets the season in which this bush will produce its drops.</summary>
    public List<Season> Seasons { get; set; } = [];

    /// <summary>Gets or sets the rules which override the locations that custom bushes can be planted in.</summary>
    public List<PlantableRule> PlantableLocationRules { get; set; } = [];

    /// <summary>Gets or sets the texture of the tea bush.</summary>
    public string Texture { get; set; } = string.Empty;

    /// <summary>Gets or sets the row index for the custom bush's sprites.</summary>
    public int TextureSpriteRow { get; set; }
}