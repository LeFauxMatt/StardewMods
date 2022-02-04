namespace StardewMods.FuryCore.Models;

using System.Collections.Generic;
using StardewMods.FuryCore.Enums;

/// <summary>
///     Mod config data.
/// </summary>
internal class ConfigData
{
    /// <summary>
    ///     Gets or sets which custom tags can be added to items.
    /// </summary>
    public HashSet<CustomTag> CustomTags { get; set; } = new()
    {
        CustomTag.CategoryArtifact,
        CustomTag.CategoryFurniture,
        CustomTag.DonateBundle,
        CustomTag.DonateMuseum,
    };

    /// <summary>
    ///     Copies data from one <see cref="ConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="ConfigData" /> to copy values to.</param>
    public void CopyTo(ConfigData other)
    {
        other.CustomTags = this.CustomTags;
    }
}