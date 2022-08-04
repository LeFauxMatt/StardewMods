namespace StardewMods.BetterChests.Models;

using System;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

/// <summary>
///     Parsed search text for <see cref="ItemMatcher" />.
/// </summary>
internal class SearchPhrase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchPhrase" /> class.
    /// </summary>
    /// <param name="value">The search text value.</param>
    /// <param name="tagMatch">Indicates whether to match by context tag.</param>
    /// <param name="exactMatch">Indicates whether to accept exact matches only.</param>
    /// <param name="translation">Allows for matching against translated tags.</param>
    public SearchPhrase(
        string value, bool tagMatch = true, bool exactMatch = false, ITranslationHelper? translation = null)
    {
        this.NotMatch = value[..1] == "!";
        this.TagMatch = tagMatch;
        this.ExactMatch = exactMatch;
        this.Value = this.NotMatch ? value[1..] : value;
        this.Translation = translation;
    }

    /// <summary>
    ///     Indicates if the search phrase is a negation match.
    /// </summary>
    public bool NotMatch { get; }

    private bool ExactMatch { get; }

    private bool TagMatch { get; }

    private ITranslationHelper? Translation { get; }

    private string Value { get; }

    /// <summary>
    ///     Checks if item matches this search phrase.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if item matches the search phrase.</returns>
    public bool Matches(Item item)
    {
        return (this.TagMatch
                   ? item.GetContextTags().Any(this.Matches)
                   : this.Matches(item.DisplayName) || this.Matches(item.Name))
            != this.NotMatch;
    }

    private bool Matches(string match)
    {
        if (this.Translation is not null && !this.ExactMatch)
        {
            var localMatch = this.Translation.Get($"tag.{match}").Default(string.Empty).ToString();
            if (!string.IsNullOrWhiteSpace(localMatch)
             && localMatch.IndexOf(this.Value, StringComparison.OrdinalIgnoreCase) != -1)
            {
                return true;
            }
        }

        return this.ExactMatch
            ? this.Value.Equals(match, StringComparison.OrdinalIgnoreCase)
            : match.IndexOf(this.Value, StringComparison.OrdinalIgnoreCase) != -1;
    }
}