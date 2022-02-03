namespace StardewMods.FuryCore.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <summary>
///     Matches item name/tags against a set of search phrases.
/// </summary>
public class ItemMatcher : ObservableCollection<string>
{
    private readonly IDictionary<string, SearchPhrase> _clean = new Dictionary<string, SearchPhrase>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemMatcher" /> class.
    /// </summary>
    /// <param name="exactMatch">Set to true to disallow partial matches.</param>
    /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
    public ItemMatcher(bool exactMatch = false, string searchTagSymbol = null)
    {
        this.ExactMatch = exactMatch;
        this.SearchTagSymbol = searchTagSymbol ?? string.Empty;
    }

    /// <summary>
    ///     Gets or sets a string representation of all registered search texts.
    /// </summary>
    public string StringValue
    {
        get => string.Join(" ", this);
        set
        {
            this.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                foreach (var item in Regex.Split(value, @"\s+"))
                {
                    this.Add(item);
                }
            }
        }
    }

    private bool ExactMatch { get; }

    private string SearchTagSymbol { get; }

    /// <summary>
    ///     Gets all context tags for an item including custom ones.
    /// </summary>
    /// <param name="item">The item to get context tags for.</param>
    /// <returns>A list of context tags from the item.</returns>
    public static IEnumerable<string> GetContextTags(Item item)
    {
        foreach (var contextTag in item.GetContextTags().Where(contextTag => !contextTag.StartsWith("id_")))
        {
            yield return contextTag;
        }

        if (item is Furniture)
        {
            yield return SearchPhrase.CategoryFurniture;
        }

        if (item is SObject { Type: "Arch" })
        {
            yield return SearchPhrase.CategoryArtifact;
        }

        if (item is SObject obj && SearchPhrase.CanDonateToBundle(obj))
        {
            yield return SearchPhrase.DonateBundle;
        }

        if (SearchPhrase.CanDonateToMuseum(item))
        {
            yield return SearchPhrase.DonateMuseum;
        }
    }

    /// <summary>
    ///     Checks if an item matches the search phrases.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if item matches any search phrase unless a NotMatch search phrase was matched.</returns>
    public bool Matches(Item item)
    {
        if (this.Count == 0)
        {
            return true;
        }

        var matchesAny = false;
        foreach (var searchPhrase in this._clean.Values)
        {
            if (searchPhrase.Matches(item))
            {
                if (!searchPhrase.NotMatch || this._clean.Values.All(p => p.NotMatch))
                {
                    matchesAny = true;
                }
            }
            else if (searchPhrase.NotMatch)
            {
                return false;
            }
        }

        return matchesAny;
    }

    /// <inheritdoc />
    protected override void InsertItem(int index, string item)
    {
        if (!this.Contains(item))
        {
            base.InsertItem(index, item);
        }
    }

    /// <inheritdoc />
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        var added = this.Except(this._clean.Keys);
        var removed = this._clean.Keys.Except(this);

        foreach (var item in removed)
        {
            this._clean.Remove(item);
        }

        foreach (var item in added)
        {
            this._clean.Add(item, this.ParseString(item));
        }

        base.OnCollectionChanged(e);
    }

    /// <inheritdoc />
    protected override void SetItem(int index, string item)
    {
        if (this.IndexOf(item) == -1)
        {
            base.SetItem(index, item);
        }
    }

    private SearchPhrase ParseString(string value)
    {
        var stringBuilder = new StringBuilder(value.Trim());
        var tagMatch = string.IsNullOrWhiteSpace(this.SearchTagSymbol) || value[..1] == this.SearchTagSymbol;
        if (tagMatch && !string.IsNullOrWhiteSpace(this.SearchTagSymbol))
        {
            stringBuilder.Remove(0, this.SearchTagSymbol.Length);
        }

        return new(stringBuilder.ToString(), tagMatch, this.ExactMatch);
    }

    private record SearchPhrase
    {
        public const string CategoryFurniture = "category_furniture";
        public const string CategoryArtifact = "category_artifact";
        public const string DonateMuseum = "donate_museum";
        public const string DonateBundle = "donate_bundle";

        public SearchPhrase(string value, bool tagMatch = true, bool exactMatch = false)
        {
            this.NotMatch = value[..1] == "!";
            this.ExactMatch = exactMatch;
            this.TagMatch = tagMatch;
            this.Value = this.NotMatch ? value[1..] : value;
        }

        public bool NotMatch { get; }

        private bool ExactMatch { get; }

        private bool TagMatch { get; }

        private string Value { get; }

        /// <summary>
        ///     Checks if item matches this search phrase.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if item matches the search phrase.</returns>
        public bool Matches(Item item)
        {
            if (!this.TagMatch)
            {
                return this.Matches(item.Name) != this.NotMatch;
            }

            return item switch
            {
                Furniture when this.Matches(SearchPhrase.CategoryFurniture) => true,
                SObject { Type: "Arch" } when this.Matches(SearchPhrase.CategoryArtifact) => true,
                SObject { Type: "Arch" } when this.Matches(SearchPhrase.DonateMuseum) => SearchPhrase.CanDonateToMuseum(item),
                SObject { Type: "Minerals" } when this.Matches(SearchPhrase.DonateMuseum) => SearchPhrase.CanDonateToMuseum(item),
                SObject obj when this.Matches(SearchPhrase.DonateBundle) => SearchPhrase.CanDonateToBundle(obj),
                _ => item.GetContextTags().Any(this.Matches) != this.NotMatch,
            };
        }

        public static bool CanDonateToMuseum(Item item)
        {
            return Game1.locations
                        .OfType<LibraryMuseum>()
                        .FirstOrDefault()?.isItemSuitableForDonation(item)
                   ?? false;
        }

        public static bool CanDonateToBundle(SObject obj)
        {
            return Game1.locations
                        .OfType<CommunityCenter>()
                        .FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
                   ?? false;
        }

        private bool Matches(string match)
        {
            return this.ExactMatch
                ? this.Value.Equals(match, StringComparison.OrdinalIgnoreCase)
                : match.IndexOf(this.Value, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}