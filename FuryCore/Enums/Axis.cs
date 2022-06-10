﻿namespace StardewMods.FuryCore.Enums;

using NetEscapades.EnumGenerators;

/// <summary>
///     Describes an axis in a 2-dimensional coordinate system.
/// </summary>
[EnumExtensions]
public enum Axis
{
    /// <summary>An axis going from top to bottom.</summary>
    Vertical,

    /// <summary>An axis going from left to right.</summary>
    Horizontal,
}