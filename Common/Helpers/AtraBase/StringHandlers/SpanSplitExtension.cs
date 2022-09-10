namespace StardewMods.Common.Helpers.AtraBase.StringHandlers;

using System;

/// <summary>
/// Holds the extension methods for SpanSplit.
/// </summary>
public static class SpanSplitExtension
{
    /// <summary>
    /// Creates a new instance of SpanSplit.
    /// </summary>
    /// <param name="str">String to split.</param>
    /// <param name="splitchars">Characters to split by. (leave null for whitespace).</param>
    /// <param name="options">String split options.</param>
    /// <param name="expectedCount">The expected number of splits.</param>
    /// <returns>SpanSplit instance.</returns>
    public static SpanSplit SpanSplit(this string str, char[]? splitchars = null, StringSplitOptions options = StringSplitOptions.None, int? expectedCount = null)
        => new(str, splitchars, options, expectedCount);

    /// <summary>
    /// Creates a new instance of SpanSplit.
    /// </summary>
    /// <param name="str">String to split.</param>
    /// <param name="splitchars">Characters to split by. (leave null for whitespace).</param>
    /// <param name="options">String split options.</param>
    /// <param name="expectedCount">The expected number of splits.</param>
    /// <returns>SpanSplit instance.</returns>
    public static SpanSplit SpanSplit(this ReadOnlySpan<char> str, char[]? splitchars = null, StringSplitOptions options = StringSplitOptions.None, int? expectedCount = null)
        => new(str, splitchars, options, expectedCount);

    /// <summary>
    /// Creates a new instance of SpanSplit.
    /// </summary>
    /// <param name="str">String to split.</param>
    /// <param name="splitchars">Characters to split by. (leave null for whitespace).</param>
    /// <param name="options">String split options.</param>
    /// <param name="expectedCount">The expected number of splits.</param>
    /// <returns>SpanSplit instance.</returns>
    public static SpanSplit SpanSplit(this SpanSplitEntry str, char[]? splitchars = null, StringSplitOptions options = StringSplitOptions.None, int? expectedCount = null)
        => new(str.Word, splitchars, options, expectedCount);

    /// <summary>
    /// Creates a new instance of SpanSplit.
    /// </summary>
    /// <param name="str">String to split.</param>
    /// <param name="splitchar">Character to split by.</param>
    /// <param name="options">String split options.</param>
    /// <param name="expectedCount">The expected number of splits.</param>
    /// <returns>SpanSplit instance.</returns>
    public static SpanSplit SpanSplit(this string str, char splitchar, StringSplitOptions options = StringSplitOptions.None, int? expectedCount = null)
        => new(str, splitchar, options, expectedCount);

    /// <summary>
    /// Creates a new instance of SpanSplit.
    /// </summary>
    /// <param name="str">String to split.</param>
    /// <param name="splitchar">Characters to split by.</param>
    /// <param name="options">String split options.</param>
    /// <param name="expectedCount">The expected number of splits.</param>
    /// <returns>SpanSplit instance.</returns>
    public static SpanSplit SpanSplit(this ReadOnlySpan<char> str, char splitchar, StringSplitOptions options = StringSplitOptions.None, int? expectedCount = null)
        => new(str, splitchar, options, expectedCount);

    /// <summary>
    /// Creates a new instance of SpanSplit.
    /// </summary>
    /// <param name="str">String to split.</param>
    /// <param name="splitchar">Characters to split by.</param>
    /// <param name="options">String split options.</param>
    /// <param name="expectedCount">The expected number of splits.</param>
    /// <returns>SpanSplit instance.</returns>
    public static SpanSplit SpanSplit(this SpanSplitEntry str, char splitchar, StringSplitOptions options = StringSplitOptions.None, int? expectedCount = null)
        => new(str.Word, splitchar, options, expectedCount);
}