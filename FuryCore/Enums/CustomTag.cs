namespace StardewMods.FuryCore.Enums;

using NetEscapades.EnumGenerators;
using StardewMods.FuryCore.Interfaces;

/// <summary>
///     Custom Item Context Tags that can be added by <see cref="ICustomTags" />.
/// </summary>
[EnumExtensions]
public enum CustomTag
{
    /// <summary>Context tag for Artifacts.</summary>
    CategoryArtifact,

    /// <summary>Context tag for Furniture.</summary>
    CategoryFurniture,

    /// <summary>Context tag for items that can be donated to the Community Center.</summary>
    DonateBundle,

    /// <summary>Context tag for items that can be donated to the Museum.</summary>
    DonateMuseum,
}