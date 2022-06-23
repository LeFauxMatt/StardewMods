namespace StardewMods.Common.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Models;

/// <summary>Common extension methods.</summary>
internal static class CommonExtensions
{
    /// <summary>
    ///     Maps a float value from one range to the same proportional value in another integer range.
    /// </summary>
    /// <param name="value">The float value to map.</param>
    /// <param name="sourceRange">The source range of the float value.</param>
    /// <param name="targetRange">The target range to map to.</param>
    /// <returns>The integer value.</returns>
    public static int Remap(this float value, Range<float> sourceRange, Range<int> targetRange)
    {
        return targetRange.Clamp((int)(targetRange.Minimum + (targetRange.Maximum - targetRange.Minimum) * ((value - sourceRange.Minimum) / (sourceRange.Maximum - sourceRange.Minimum))));
    }

    /// <summary>
    ///     Maps an integer value from one range to the same proportional value in another float range.
    /// </summary>
    /// <param name="value">The integer value to map.</param>
    /// <param name="sourceRange">The source range of the integer value.</param>
    /// <param name="targetRange">The target range to map to.</param>
    /// <returns>The float value.</returns>
    public static float Remap(this int value, Range<int> sourceRange, Range<float> targetRange)
    {
        return targetRange.Clamp(targetRange.Minimum + (targetRange.Maximum - targetRange.Minimum) * ((float)(value - sourceRange.Minimum) / (sourceRange.Maximum - sourceRange.Minimum)));
    }

    /// <summary>Rounds an int up to the next int by an interval.</summary>
    /// <param name="i">The integer to round up from.</param>
    /// <param name="d">The interval to round up to.</param>
    /// <returns>Returns the rounded value.</returns>
    public static int RoundUp(this int i, int d = 1)
    {
        return (int)(d * Math.Ceiling((float)i / d));
    }

    /// <summary>Shuffles a list randomly.</summary>
    /// <param name="source">The list to shuffle.</param>
    /// <typeparam name="T">The list type.</typeparam>
    /// <returns>Returns a shuffled list.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
    {
        return source.Shuffle(new());
    }

    private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (rng is null)
        {
            throw new ArgumentNullException(nameof(rng));
        }

        return source.ShuffleIterator(rng);
    }

    private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
    {
        var buffer = source.ToList();
        for (var i = 0; i < buffer.Count; i++)
        {
            var j = rng.Next(i, buffer.Count);
            yield return buffer[j];
            buffer[j] = buffer[i];
        }
    }
}