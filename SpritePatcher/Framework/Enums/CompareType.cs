namespace StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents the type of comparison between two values.</summary>
public enum CompareType
{
    /// <summary>Matches any non-null value.</summary>
    WildCard,

    /// <summary>Test if two values are equal.</summary>
    EqualTo,

    /// <summary>Test if two values are not equal.</summary>
    NotEqualTo,

    /// <summary>Test if the one value is less than the other value.</summary>
    LessThan,

    /// <summary>Test if the one value is less than or equal to the other value.</summary>
    LessThanOrEqualTo,

    /// <summary>Test if the one value is greater than the other value.</summary>
    GreaterThan,

    /// <summary>Test if the one value is greater than or equal to the other value.</summary>
    GreaterThanOrEqualTo,
}