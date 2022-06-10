namespace StardewMods.BetterChests.Enums;

using NetEscapades.EnumGenerators;

/// <summary>
///     Indicates at what range a feature will be enabled.
/// </summary>
[EnumExtensions]
internal enum FeatureOptionRange
{
    /// <summary>Feature inherits from a parent config.</summary>
    Default = 0,

    /// <summary>Feature is disabled.</summary>
    Disabled = -1,

    /// <summary>Feature is enabled for storages in the player's inventory.</summary>
    Inventory = 1,

    /// <summary>Feature is enabled for storages in the player's current location.</summary>
    Location = 2,

    /// <summary>Feature is enabled for any storage in an accessible location to the player.</summary>
    World = 3,
}