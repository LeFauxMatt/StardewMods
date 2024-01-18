namespace StardewMods.CrystallineJunimoChests.Models;

/// <summary>The data model for the cost, sound, and colors.</summary>
internal sealed class DataModel(int cost, string sound, ColorData[] colors)
{
    /// <summary>Gets or sets the cost.</summary>
    public int Cost { get; set; } = cost;

    /// <summary>Gets or sets the sound.</summary>
    public string Sound { get; set; } = sound;

    /// <summary>Gets or sets the colors.</summary>
    public ColorData[] Colors { get; set; } = colors;
}