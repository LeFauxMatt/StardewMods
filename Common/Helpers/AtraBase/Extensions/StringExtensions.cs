namespace StardewMods.Common.Helpers.AtraBase.Extensions;

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

/// <summary>
/// Extension methods on strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets the Nth occurance of a specific unicode char in a string.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="item">Char to search for.</param>
    /// <param name="count">N.</param>
    /// <returns>Index of the char, or -1 if not found.</returns>
    [Pure]
    public static int NthOccuranceOf(this string str, char item, int count = 1)
    {
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == item && --count <= 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Gets the Nth occurance from the end of a specific unicode char in a string.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="item">Char to search for.</param>
    /// <param name="count">N.</param>
    /// <returns>Index of the char, or -1 if not found.</returns>
    [Pure]
    public static int NthOccuranceFromEnd(this string str, char item, int count = 1)
    {
        for (int i = str.Length - 1; i >= 0; i--)
        {
            if (str[i] == item && --count <= 0)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminator">deliminator to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    [Pure]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char deliminator, int index = 0)
        => str.GetNthChunk(new[] { deliminator }, index);

    /// <summary>
    /// Faster replacement for str.Split()[index];.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <param name="deliminators">deliminators to use.</param>
    /// <param name="index">index of the chunk to get.</param>
    /// <returns>a readonlyspan char with the chunk, or an empty readonlyspan for failure.</returns>
    /// <remarks>Inspired by the lovely Wren.</remarks>
    [Pure]
    public static ReadOnlySpan<char> GetNthChunk(this string str, char[] deliminators, int index = 0)
    {
        //Guard.IsBetweenOrEqualTo(index, 0, str.Length + 1, nameof(index));

        int start = 0;
        int ind = 0;
        while (index-- >= 0)
        {
            ind = str.IndexOfAny(deliminators, start);
            if (ind == -1)
            {
                // since we've previously decremented index, check against -1;
                // this means we're done.
                if (index == -1)
                {
                    return str.AsSpan()[start..];
                }

                // else, we've run out of entries
                // and return an empty span to mark as failure.
                return ReadOnlySpan<char>.Empty;
            }

            if (index > -1)
            {
                start = ind + 1;
            }
        }
        return str.AsSpan()[start..ind];
    }

    /// <summary>
    /// Gets the index of the next whitespace character.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    [MethodImpl(TKConstants.Hot)]
    public static int GetIndexOfWhiteSpace(this string str)
        => str.AsSpan().GetIndexOfWhiteSpace();

    /// <summary>
    /// Gets the index of the next whitespace character.
    /// </summary>
    /// <param name="chars">ReadOnlySpan to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    [MethodImpl(TKConstants.Hot)]
    public static int GetIndexOfWhiteSpace(this ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (char.IsWhiteSpace(chars[i]))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Gets the index of the last whitespace character.
    /// </summary>
    /// <param name="str">String to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    [MethodImpl(TKConstants.Hot)]
    public static int GetLastIndexOfWhiteSpace(this string str)
        => str.AsSpan().GetLastIndexOfWhiteSpace();

    /// <summary>
    /// Gets the index of the last whitespace character.
    /// </summary>
    /// <param name="chars">ReadOnlySpan to search in.</param>
    /// <returns>Index of the whitespace character, or -1 if not found.</returns>
    [Pure]
    [MethodImpl(TKConstants.Hot)]
    public static int GetLastIndexOfWhiteSpace(this ReadOnlySpan<char> chars)
    {
        for (int i = chars.Length - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(chars[i]))
            {
                return i;
            }
        }
        return -1;
    }
}