namespace StardewMods.FuryCore.Enums;

using NetEscapades.EnumGenerators;

/// <summary>
///     Designates certain components to automatically align to an area around the menu.
/// </summary>
[EnumExtensions]
public enum ComponentArea
{
    /// <summary>Above the ItemsToGrabMenu.</summary>
    Top = 0,

    /// <summary>To the right of the ItemsToGrabMenu.</summary>
    Right = 1,

    /// <summary>Below the ItemsToGrabMenu.</summary>
    Bottom = 2,

    /// <summary>To the left of the ItemsToGrabMenu.</summary>
    Left = 3,

    /// <summary>A Custom area.</summary>
    Custom = -1,
}