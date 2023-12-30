namespace StardewMods.HelpfulSpouses.Framework.Models.Chores;

using StardewMods.HelpfulSpouses.Framework.Services.Chores;

/// <summary>Config data for <see cref="WaterTheCrops" />.</summary>
internal sealed class WaterTheCropsOptions
{
    /// <summary>Gets or sets the limit to the number of crops that will be watered.</summary>
    public int CropLimit { get; set; }
}