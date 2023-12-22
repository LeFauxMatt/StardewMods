namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Determines the in-game config menu.</summary>
[EnumExtensions]
internal enum InGameMenu
{
    /// <summary>Inherit option from parent.</summary>
    Default = 0,

    /// <summary>Only the Categorize menu will be available.</summary>
    Categorize = 1,

    /// <summary>Only show ChestLabel, Categorize, and Priority.</summary>
    Simple = 2,

    /// <summary>Show all options.</summary>
    Full = 3,

    /// <summary>Show all options and replaces some options with open fields..</summary>
    Advanced = 4,
}
