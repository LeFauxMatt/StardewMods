namespace StardewMods.Common.Services.Integrations.BetterChests.Enums;

using NetEscapades.EnumGenerators;

/// <summary>The method used to select items.</summary>
[EnumExtensions]
public enum FilterMethod
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