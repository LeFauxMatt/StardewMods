namespace Mod.BetterChests.Enums;

/// <summary>
/// Indicates if a feature is enabled, disabled, or will inherit from a parent config.
/// </summary>
public enum FeatureOption
{
    /// <summary>Feature is disabled.</summary>
    Disabled = 0,

    /// <summary>Feature inherits from a parent config.</summary>
    Default = 1,

    /// <summary>Feature is enabled.</summary>
    Enabled = 2,
}