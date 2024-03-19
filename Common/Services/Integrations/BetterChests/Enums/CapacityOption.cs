namespace StardewMods.Common.Services.Integrations.BetterChests.Enums;

using NetEscapades.EnumGenerators;

/// <summary>The possible values for Chest capacity.</summary>
[EnumExtensions]
public enum CapacityOption
{
    /// <summary>Capacity is inherited by a parent config.</summary>
    Default = 0,

    /// <summary>Vanilla capacity.</summary>
    Disabled = 1,

    /// <summary>Resize to 9 item slots.</summary>
    Small = 2,

    /// <summary>Resize to 36 item slots.</summary>
    Medium = 3,

    /// <summary>Resize to 70 item slots.</summary>
    Large = 4,

    /// <summary>Resize to unlimited item slots.</summary>
    Unlimited = 5,
}