namespace Common.Helpers.ItemMatcher
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using StardewValley;

    /// <summary>
    /// Matches item name/tags against a set of search phrases.
    /// </summary>
    internal class ItemMatcher
    {
        private readonly IDictionary<string, SearchPhrase> _searchPhrases = new Dictionary<string, SearchPhrase>();
        private readonly HashSet<string> _searchValues = new();
        private readonly string _searchTagSymbol;
        private readonly bool _exact;
        private string _search = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemMatcher"/> class.
        /// </summary>
        /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
        /// <param name="exact">Set true to disallow partial matches.</param>
        public ItemMatcher(string searchTagSymbol, bool exact = false)
        {
            this._searchTagSymbol = searchTagSymbol;
            this._exact = exact;
        }

        /// <summary>
        /// The current search expression.
        /// </summary>
        public string Search
        {
            get => this._search;
        }

        /// <summary>
        /// Checks if an item matches the search phrases.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if item matches any search phrase unless a NotMatch search phrase was matched.</returns>
        public bool Matches(Item item)
        {
            if (this._searchValues.Count == 0)
            {
                return true;
            }

            bool matchesAny = false;
            foreach (string searchValue in this._searchValues)
            {
                if (!this._searchPhrases.TryGetValue(searchValue, out SearchPhrase searchPhrase))
                {
                    searchPhrase = new SearchPhrase(searchValue, this._searchTagSymbol, this._exact);
                    this._searchPhrases.Add(searchValue, searchPhrase);
                }

                bool matched = searchPhrase.Matches(item);
                if (!matched && searchPhrase.NotMatch)
                {
                    return false;
                }

                matchesAny = matchesAny || matched;
            }

            return matchesAny;
        }

        /// <summary>
        /// Assign a new search expression.
        /// </summary>
        /// <param name="searchParts">The search expression represented as a list of parts.</param>
        public void SetSearch(IEnumerable<string> searchParts)
        {
            IList<string> searchValues = searchParts.ToList();
            string search = string.Join(" ", searchValues);
            if (this._search == search)
            {
                return;
            }

            this._search = search;
            this._searchValues.Clear();
            if (string.IsNullOrWhiteSpace(search))
            {
                return;
            }

            foreach (string searchValue in searchValues)
            {
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    this._searchValues.Add(searchValue);
                }
            }

            foreach (string searchValue in this._searchValues.Except(searchValues))
            {
                if (!this._searchValues.Contains(searchValue))
                {
                    this._searchPhrases.Remove(searchValue);
                }
            }
        }

        /// <summary>
        /// Assign a new search expression.
        /// </summary>
        /// <param name="search">The search expression.</param>
        public void SetSearch(string search)
        {
            if (this._search == search)
            {
                return;
            }

            IEnumerable<string> searchValues = Regex.Split(search, @"\s+").AsEnumerable();
            this.SetSearch(searchValues);
        }

        /// <summary>
        /// Assign a new search expression.
        /// </summary>
        /// <param name="searchParts">The search expression represented as a dictionary of parts.</param>
        public void SetSearch(IDictionary<string, bool> searchParts)
        {
            IEnumerable<string> searchValues = searchParts.Select(searchPart => searchPart.Value ? searchPart.Key : $"!{searchPart.Key}").AsEnumerable();
            this.SetSearch(searchValues);
        }
    }
}