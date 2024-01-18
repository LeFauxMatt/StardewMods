namespace StardewMods.Common.Extensions;

using Microsoft.Xna.Framework;
using StardewMods.Common.Models;
using StardewValley.Mods;

/// <summary>Common extension methods.</summary>
internal static class CommonExtensions
{
    /// <summary>Generate a box of coordinates centered at a specified point with a given radius.</summary>
    /// <param name="center">The center point of the box.</param>
    /// <param name="radius">The radius of the box.</param>
    /// <returns>An enumerable collection of Vector2 coordinates representing the points in the box.</returns>
    public static IEnumerable<Vector2> Box(this Vector2 center, int radius)
    {
        for (var x = (int)(center.X - radius); x <= center.X + radius; ++x)
        {
            for (var y = (int)(center.Y - radius); y <= center.Y + radius; ++y)
            {
                yield return new Vector2(x, y);
            }
        }
    }

    /// <summary>Invokes all event handlers for an event.</summary>
    /// <param name="eventHandler">The event.</param>
    /// <param name="source">The source.</param>
    public static void InvokeAll(this EventHandler? eventHandler, object source)
    {
        if (eventHandler is null)
        {
            return;
        }

        foreach (var handler in eventHandler.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(source, null);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    /// <summary>Invokes all event handlers for an event.</summary>
    /// <param name="eventHandler">The event.</param>
    /// <param name="source">The source.</param>
    /// <param name="param">The event parameters.</param>
    /// <typeparam name="T">The event handler type.</typeparam>
    public static void InvokeAll<T>(this EventHandler<T>? eventHandler, object source, T param)
    {
        if (eventHandler is null)
        {
            return;
        }

        foreach (var @delegate in eventHandler.GetInvocationList())
        {
            var handler = (EventHandler<T>)@delegate;
            try
            {
                handler(source, param);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    /// <summary>Maps a float value from one range to the same proportional value in another integer range.</summary>
    /// <param name="value">The float value to map.</param>
    /// <param name="sourceRange">The source range of the float value.</param>
    /// <param name="targetRange">The target range to map to.</param>
    /// <returns>The integer value.</returns>
    public static int Remap(this float value, Range<float> sourceRange, Range<int> targetRange) =>
        targetRange.Clamp(
            (int)(targetRange.Minimum
                + ((targetRange.Maximum - targetRange.Minimum)
                    * ((value - sourceRange.Minimum) / (sourceRange.Maximum - sourceRange.Minimum)))));

    /// <summary>Maps an integer value from one range to the same proportional value in another float range.</summary>
    /// <param name="value">The integer value to map.</param>
    /// <param name="sourceRange">The source range of the integer value.</param>
    /// <param name="targetRange">The target range to map to.</param>
    /// <returns>The float value.</returns>
    public static float Remap(this int value, Range<int> sourceRange, Range<float> targetRange) =>
        targetRange.Clamp(
            targetRange.Minimum
            + ((targetRange.Maximum - targetRange.Minimum)
                * ((float)(value - sourceRange.Minimum) / (sourceRange.Maximum - sourceRange.Minimum))));

    /// <summary>Rounds an int up to the next int by an interval.</summary>
    /// <param name="i">The integer to round up from.</param>
    /// <param name="d">The interval to round up to.</param>
    /// <returns>Returns the rounded value.</returns>
    public static int RoundUp(this int i, int d = 1) => (int)(d * Math.Ceiling((float)i / d));

    /// <summary>Shuffles a list randomly.</summary>
    /// <param name="source">The list to shuffle.</param>
    /// <typeparam name="T">The list type.</typeparam>
    /// <returns>Returns a shuffled list.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source) => source.Shuffle(new Random());

    /// <summary>Tries to parse the specified string value as a boolean and returns the result.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="defaultValue">The default value to return if the value cannot be parsed as a boolean.</param>
    /// <returns>The boolean value, or the default value if the value is not a valid boolean.</returns>
    public static bool GetBool(this string value, bool defaultValue = false) =>
        !string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out var boolValue) ? boolValue : defaultValue;

    /// <summary>Retrieves a boolean value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="dictionary">The dictionary to retrieve the boolean value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid boolean. </param>
    /// <returns>The boolean value associated with the key, or the default value.</returns>
    public static bool GetBool(this IDictionary<string, string> dictionary, string key, bool defaultValue = false) =>
        dictionary.TryGetValue(key, out var value) ? value.GetBool(defaultValue) : defaultValue;

    /// <summary>Retrieves a boolean value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="modData">The mod data dictionary to retrieve the boolean value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid boolean. </param>
    /// <returns>The boolean value associated with the key, or the default value.</returns>
    public static bool GetBool(this ModDataDictionary modData, string key, bool defaultValue = false) =>
        modData.TryGetValue(key, out var value) ? value.GetBool(defaultValue) : defaultValue;

    /// <summary>Tries to parse the specified string value as an integer and returns the result.</summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="defaultValue">The default value to return if the value cannot be parsed as an integer.</param>
    /// <returns>The integer value, or the default value if the value is not a valid integer.</returns>
    public static int GetInt(this string value, int defaultValue = 0) =>
        !string.IsNullOrWhiteSpace(value) && int.TryParse(value, out var intValue) ? intValue : defaultValue;

    /// <summary>Retrieves an integer value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="modData">The mod data dictionary to retrieve the integer value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid integer.</param>
    /// <returns>The integer value associated with the key, or the default value.</returns>
    public static int GetInt(this ModDataDictionary modData, string key, int defaultValue = 0) =>
        modData.TryGetValue(key, out var value) ? value.GetInt(defaultValue) : defaultValue;

    /// <summary>Retrieves an integer value from the specified dictionary based on the given key and optional default value.</summary>
    /// <param name="dictionary">The dictionary to retrieve the integer value from.</param>
    /// <param name="key">The key used to look up the value.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value is not a valid integer.</param>
    /// <returns>The integer value associated with the key, or the default value.</returns>
    public static int GetInt(this IDictionary<string, string> dictionary, string key, int defaultValue = 0) =>
        dictionary.TryGetValue(key, out var value) ? value.GetInt(defaultValue) : defaultValue;

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
        for (var i = 0; i < buffer.Count; ++i)
        {
            var j = rng.Next(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }
}