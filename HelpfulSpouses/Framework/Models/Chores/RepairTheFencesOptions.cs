namespace StardewMods.HelpfulSpouses.Framework.Models.Chores;

using StardewMods.HelpfulSpouses.Framework.Services.Chores;

/// <summary>Config data for <see cref="RepairTheFences" />.</summary>
internal sealed class RepairTheFencesOptions
{
    /// <summary>Gets or sets the limit to the number of fences that will be repaired.</summary>
    public int FenceLimit { get; set; }
}