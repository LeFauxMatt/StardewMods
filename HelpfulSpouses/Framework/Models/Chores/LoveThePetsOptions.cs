namespace StardewMods.HelpfulSpouses.Framework.Models.Chores;

using StardewMods.HelpfulSpouses.Framework.Services.Chores;

/// <summary>Config data for <see cref="LoveThePets" />.</summary>
internal sealed class LoveThePetsOptions
{
    /// <summary>Gets or sets a value indicating whether petting will be enabled.</summary>
    public bool EnablePetting { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the water bowl will be filled.</summary>
    public bool FillWaterBowl { get; set; } = true;
}