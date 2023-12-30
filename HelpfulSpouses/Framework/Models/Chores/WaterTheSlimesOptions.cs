namespace StardewMods.HelpfulSpouses.Framework.Models.Chores;

using StardewMods.HelpfulSpouses.Framework.Services.Chores;

/// <summary>Config data for <see cref="WaterTheSlimes" />.</summary>
internal sealed class WaterTheSlimesOptions
{
    /// <summary>Gets or sets the limit to the number of slimes that will be watered.</summary>
    public int SlimeLimit { get; set; }
}