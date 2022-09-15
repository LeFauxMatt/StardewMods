namespace StardewMods.StackQuality.Framework;

using StardewMods.Common.Enums;

/// <summary>
///     Common helpers.
/// </summary>
internal static class Common
{
    private const string QualityGold = "quality_gold";
    private const string QualityIridium = "quality_iridium";
    private const string QualityNone = "quality_none";
    private const string QualitySilver = "quality_silver";

    /// <summary>
    ///     Gets the quality level from the index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Returns the quality level of the array index.</returns>
    public static string IndexToContextTag(int index)
    {
        return index switch
        {
            3 => Common.QualityIridium,
            2 => Common.QualityGold,
            1 => Common.QualitySilver,
            0 or _ => Common.QualityNone,
        };
    }

    /// <summary>
    ///     Gets the quality level from the index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>Returns the quality level of the array index.</returns>
    public static Quality IndexToQuality(int index)
    {
        return index switch
        {
            3 => Quality.Iridium,
            2 => Quality.Gold,
            1 => Quality.Silver,
            0 or _ => Quality.None,
        };
    }

    /// <summary>
    ///     Gets the quality tag from the quality level.
    /// </summary>
    /// <param name="quality">The quality level.</param>
    /// <returns>Returns the quality tag.</returns>
    public static string QualityToContextTag(Quality quality)
    {
        return quality switch
        {
            Quality.Iridium => Common.QualityIridium,
            Quality.Gold => Common.QualityGold,
            Quality.Silver => Common.QualitySilver,
            Quality.None or _ => Common.QualityNone,
        };
    }

    /// <summary>
    ///     Gets the index from the quality level.
    /// </summary>
    /// <param name="quality">The quality level.</param>
    /// <returns>Returns the array index of the quality stack.</returns>
    public static int QualityToIndex(Quality quality)
    {
        return quality switch
        {
            Quality.Iridium => 3,
            Quality.Gold => 2,
            Quality.Silver => 1,
            Quality.None or _ => 0,
        };
    }
}