namespace StardewMods.BetterChests.Framework.Services.Transient;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Extensions;

/// <summary>Matches item name/tags against a set of search phrases.</summary>
internal sealed class ItemMatcher : IItemFilter
{
    private readonly ModConfig config;
    private readonly Dictionary<string, ParsedTerm> parsedTerms = new();
    private readonly ITranslationHelper translation;
    private string searchText = string.Empty;

    private ParsedTerm[] terms = Array.Empty<ParsedTerm>();

    /// <summary>Initializes a new instance of the <see cref="ItemMatcher" /> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="translation">Dependency used for accessing translations.</param>
    public ItemMatcher(ModConfig config, ITranslationHelper translation)
    {
        this.config = config;
        this.translation = translation;
    }

    /// <summary>Gets or sets a string representation of all registered search texts.</summary>
    public string SearchText
    {
        get => this.searchText;
        set
        {
            this.searchText = value;
            var searchTerms = this.searchText.Split(' ');
            var addedAny = false;
            foreach (var searchTerm in searchTerms)
            {
                if (string.IsNullOrWhiteSpace(searchTerm) || this.parsedTerms.ContainsKey(searchTerm))
                {
                    continue;
                }

                var notMatch = searchTerm[0] == this.config.SearchNegationSymbol;
                var newValue = notMatch ? searchTerm[1..] : searchTerm;

                var tagMatch = this.OnlyTags || newValue[0] == this.config.SearchTagSymbol;
                newValue = tagMatch && searchTerm.StartsWith(this.config.SearchTagSymbol) ? newValue[1..] : newValue;
                if (string.IsNullOrWhiteSpace(newValue))
                {
                    continue;
                }

                var cachedTerm = new ParsedTerm(newValue, notMatch, tagMatch);
                this.parsedTerms.Add(searchTerm, cachedTerm);
                addedAny = true;
            }

            var removed = this.parsedTerms.RemoveWhere(pair => !searchTerms.Contains(pair.Key));
            if (!addedAny && removed == 0)
            {
                return;
            }

            this.terms = this.parsedTerms.Values.ToArray();
            Array.Sort(
                this.terms,
                (t1, t2) =>
                {
                    if (t1.TagMatch != t2.TagMatch)
                    {
                        return t1.TagMatch.CompareTo(t2.TagMatch);
                    }

                    if (t1.NotMatch != t2.NotMatch)
                    {
                        return t2.NotMatch.CompareTo(t1.NotMatch);
                    }

                    return 0;
                });
        }
    }

    /// <summary>Gets or sets a value indicating whether partial matches will be returned.</summary>
    public bool AllowPartial { get; set; }

    /// <summary>Gets or sets a value indicating whether all terms will be evaluated as context tags by default.</summary>
    public bool OnlyTags { get; set; } = true;

    /// <summary>Gets a value indicating whether the search text is empty.</summary>
    public bool IsEmpty => this.terms.Length == 0;

    /// <summary>Checks if an item matches the search phrases.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if item matches any search phrase unless a NotMatch search phrase was matched.</returns>
    public bool MatchesFilter(Item? item)
    {
        if (item is null)
        {
            return false;
        }

        if (this.terms.Length == 0)
        {
            return true;
        }

        foreach (var term in this.terms)
        {
            // Unable to determine if not a match
            if (!this.IsMatch(term, item))
            {
                continue;
            }

            // If it matches a not match, then instantly disqualify
            if (term.NotMatch)
            {
                return false;
            }

            // If it matches a match, then instantly qualify
            // (No not-matches are implied because of the sorting order)
            return true;
        }

        return false;
    }

    private static bool IsExactMatch(ParsedTerm term, Item item) =>
        term.TagMatch switch
        {
            // Exactly matches name or display name
            false when term.Value == item.DisplayName || term.Value == item.Name => true,

            // Exactly matches context tag
            true when item.HasContextTag(term.Value) => true,
            _ => false,
        };

    private bool IsMatch(ParsedTerm term, Item item) =>
        (this.AllowPartial ? this.IsPartialMatch(term, item) : ItemMatcher.IsExactMatch(term, item)) != term.NotMatch;

    private bool IsPartialMatch(ParsedTerm term, Item item)
    {
        // Partially matches name or display name
        if (!term.TagMatch)
        {
            return item.Name.Contains(term.Value, StringComparison.OrdinalIgnoreCase)
                || item.DisplayName.Contains(term.Value, StringComparison.OrdinalIgnoreCase);
        }

        // Partially matches context tag
        if (item.GetContextTags().Any(tag => tag.Contains(term.Value, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Partially matches translated tags
        foreach (var tag in item.GetContextTags())
        {
            var localMatch = this.translation.Get($"tag.{tag}");
            if (localMatch.HasValue() && localMatch.ToString().Contains(term.Value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private readonly struct ParsedTerm(string value, bool notMatch, bool tagMatch)
    {
        public string Value { get; } = value;

        public bool NotMatch { get; } = notMatch;

        public bool TagMatch { get; } = tagMatch;
    }
}