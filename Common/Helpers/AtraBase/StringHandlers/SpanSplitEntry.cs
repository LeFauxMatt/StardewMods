namespace StardewMods.Common.Helpers.AtraBase.StringHandlers;

using System;

/// <summary>
///     A split entry. Consists of the word + the character split by.
///     (The end of the string is marked with string.Empty).
/// </summary>
public readonly ref struct SpanSplitEntry
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SpanSplitEntry" /> struct.
    /// </summary>
    /// <param name="word">Word.</param>
    /// <param name="seperator">Seperator.</param>
    public SpanSplitEntry(ReadOnlySpan<char> word, ReadOnlySpan<char> seperator)
    {
        this.Word = word;
        this.Seperator = seperator;
    }

    /// <summary>
    ///     Gets the seperator after the word. (String.Empty denotes the end).
    /// </summary>
    public ReadOnlySpan<char> Seperator { get; }

    /// <summary>
    ///     Gets the word.
    /// </summary>
    public ReadOnlySpan<char> Word { get; }

    public static implicit operator ReadOnlySpan<char>(SpanSplitEntry entry)
    {
        return entry.Word;
    }

    public static implicit operator string(SpanSplitEntry entry)
    {
        return entry.Word.ToString();
    }

    /// <summary>
    ///     Whether this entry contains the string.
    /// </summary>
    /// <param name="str">Substring.</param>
    /// <param name="comparison">The comparsion method - defaults to ordinal.</param>
    /// <returns>True if this entry contains that string.</returns>
    public bool Contains(ReadOnlySpan<char> str, StringComparison comparison = StringComparison.Ordinal)
    {
        return this.Word.Contains(str, comparison);
    }

    /// <summary>
    ///     Deconstructs the entry.
    /// </summary>
    /// <param name="word">Word.</param>
    /// <param name="seperator">Seperator.</param>
    public void Deconstruct(out ReadOnlySpan<char> word, out ReadOnlySpan<char> seperator)
    {
        word = this.Word;
        seperator = this.Seperator;
    }

    /// <summary>
    ///     Whether or not this entry ends with a specfic string.
    /// </summary>
    /// <param name="str">Substring.</param>
    /// <param name="comparison">The comparsion method - defaults to ordinal.</param>
    /// <returns>True if this entry ends with that string.</returns>
    public bool EndsWith(ReadOnlySpan<char> str, StringComparison comparison = StringComparison.Ordinal)
    {
        return this.Word.EndsWith(str, comparison);
    }

    /// <summary>
    ///     Whether this entry starts with the string.
    /// </summary>
    /// <param name="str">Substring.</param>
    /// <param name="comparison">The comparison method - defaults to ordinal.</param>
    /// <returns>True if this entry starts with that string.</returns>
    public bool StartsWith(ReadOnlySpan<char> str, StringComparison comparison = StringComparison.Ordinal)
    {
        return this.Word.StartsWith(str, comparison);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.Word.ToString();
    }
}