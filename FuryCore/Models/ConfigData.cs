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
    ///     Gets or sets a value indicating whether scrolling will be added to menus where items overflow the menu capacity.
    /// </summary>
    public bool ScrollMenuOverflow { get; set; } = true;

    /// <summary>
    ///     Copies data from one <see cref="ConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="ConfigData" /> to copy values to.</param>
    public void CopyTo(ConfigData other)
    {
        other.ScrollMenuOverflow = this.ScrollMenuOverflow;
        other.CustomTags = this.CustomTags;
    }
}