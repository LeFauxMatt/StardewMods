namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>The method used to select items.</summary>
[EnumExtensions]
internal enum Method
{
    /// <summary>no transformation will be applied.</summary>
    Default = 0,

    /// <summary>Selected items will be sorted first.</summary>
    Sorted = 1,

    /// <summary>Gray out unselected items.</summary>
    GrayedOut = 2,

    /// <summary>Hide unselected items.</summary>
    Hidden = 3,
}