namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Indicates if a feature is enabled, disabled, or will inherit from a parent config.</summary>
[EnumExtensions]
internal enum Option
{
    /// <summary>Feature inherits from a parent config.</summary>
    Default = 0,

    /// <summary>Feature is disabled.</summary>
    Disabled = 1,

    /// <summary>Feature is enabled.</summary>
    Enabled = 2,
}