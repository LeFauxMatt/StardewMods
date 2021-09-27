namespace Common.Helpers.ItemMatcher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <summary>
    /// A search phrase for an Item name or tags.
    /// </summary>
    public class SearchPhrase
    {
        private const string CategoryFurniture = "category_furniture";
        private const string CategoryArtifact = "category_artifact";
        private const string DonateMuseum = "donate_museum";
        private const string DonateBundle = "donate_bundle";

        private readonly string _search;
        private readonly bool _tag;
        private readonly bool _exact;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchPhrase"/> class.
        /// </summary>
        /// <param name="searchPhrase">The phrase to search.</param>
        /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
        /// <param name="exact">Set true to disallow partial matches.</param>
        public SearchPhrase(string searchPhrase, string searchTagSymbol, bool exact = false)
        {
            this.NotMatch = searchPhrase.StartsWith("!");
            if (this.NotMatch)
            {
                searchPhrase = searchPhrase.Substring(1);
            }

            this._tag = string.IsNullOrWhiteSpace(searchTagSymbol) || searchPhrase.StartsWith(searchTagSymbol);
            if (this._tag && !string.IsNullOrWhiteSpace(searchTagSymbol))
            {
                searchPhrase = searchPhrase.Substring(1);
            }

            this._search = searchPhrase;
            this._exact = exact;
        }

        /// <summary>
        /// Gets a value indicating whether this is a "Not" search phrase.
        /// </summary>
        public bool NotMatch { get; }

        public static HashSet<string> GetContextTags(Item item)
        {
            HashSet<string> contextTags = item.GetContextTags();
            if (item is SObject obj && SearchPhrase.CanDonateToBundle(obj))
            {
                contextTags.Add(SearchPhrase.DonateBundle);
            }

            if (SearchPhrase.CanDonateToMuseum(item))
            {
                contextTags.Add(SearchPhrase.DonateMuseum);
            }

            return contextTags;
        }

        /// <summary>
        /// Checks if item matches this search phrase.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if item matches the search phrase.</returns>
        public bool Matches(Item item)
        {
            if (!this._tag)
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

        private static bool CanDonateToMuseum(Item item)
        {
            return Game1.locations
                        .OfType<LibraryMuseum>()
                        .FirstOrDefault()?.isItemSuitableForDonation(item)
                   ?? false;
        }

        private static bool CanDonateToBundle(SObject obj)
        {
            return Game1.locations
                        .OfType<CommunityCenter>()
                        .FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
                   ?? false;
        }

        private bool Matches(string match)
        {
            return this._exact ? this._search == match : match.IndexOf(this._search, StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}