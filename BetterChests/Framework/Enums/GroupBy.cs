namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Grouping type for items.</summary>
[EnumExtensions]
internal enum GroupBy
{
    /// <summary>Default grouping (Ungrouped).</summary>
    Default = 0,

    /// <summary>Group by category.</summary>
    Category = 1,

    /// <summary>Group by color.</summary>
    Color = 2,

    /// <summary>Group by name.</summary>
    Name = 3,
}
