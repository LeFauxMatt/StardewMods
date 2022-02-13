namespace StardewMods.EasyAccess.Enums;

/// <summary>
///     Indicates at what range a feature will be enabled.
/// </summary>
public enum FeatureOptionRange
{
    /// <summary>Feature inherits from a parent config.</summary>
    Default = 0,

    /// <summary>Feature is disabled.</summary>
    Disabled = -1,

    /// <summary>Feature is enabled for producers in the player's current location.</summary>
    Location = 1,

    /// <summary>Feature is enabled for any producer in an accessible location to the player.</summary>
    World = 2,
}