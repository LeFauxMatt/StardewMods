namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Sorting type for items.</summary>
[EnumExtensions]
internal enum SortBy
{
    /// <summary>Default sorting.</summary>
    Default = 0,

    /// <summary>Sort by type.</summary>
    Type = 1,

    /// <summary>Sort by quality.</summary>
    Quality = 2,

    /// <summary>Sort by quantity.</summary>
    Quantity = 3,
}