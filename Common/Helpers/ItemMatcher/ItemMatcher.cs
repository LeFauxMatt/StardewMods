namespace Common.Helpers.ItemMatcher
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using StardewValley;

    /// <summary>
    ///     Matches item name/tags against a set of search phrases.
    /// </summary>
    internal class ItemMatcher
    {
        private readonly bool _exact;
        private readonly IDictionary<string, SearchPhrase> _searchPhrases = new Dictionary<string, SearchPhrase>();
        private readonly string _searchTagSymbol;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ItemMatcher" /> class.
        /// </summary>
        /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
        /// <param name="exact">Set true to disallow partial matches.</param>
        public ItemMatcher(string searchTagSymbol, bool exact = false)
        {
            this._searchTagSymbol = searchTagSymbol;
            this._exact = exact;
        }

        /// <summary>
        ///     The current search expression as a single search expression.
        /// </summary>
        public string Search { get; private set; } = string.Empty;

        /// <summary>
        ///     The current search expression as a list of search values.
        /// </summary>
        public IList<string> SearchValues { get; } = new List<string>();

        /// <summary>
        ///     Checks if an item matches the search phrases.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if item matches any search phrase unless a NotMatch search phrase was matched.</returns>
        public bool Matches(Item item)
        {
            if (this.SearchValues.Count == 0)
            {
                return true;
            }

            var matchesAny = false;
            foreach (var searchValue in this.SearchValues)
            {
                if (!this._searchPhrases.TryGetValue(searchValue, out var searchPhrase))
                {
                    searchPhrase = new(searchValue, this._searchTagSymbol, this._exact);
                    this._searchPhrases.Add(searchValue, searchPhrase);
                }

                if (searchPhrase.Matches(item))
                {
                    if (!searchPhrase.NotMatch || this._searchPhrases.All(p => p.Value.NotMatch))
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

        /// <summary>
        ///     Adds a new search expression to the current expression.
        /// </summary>
        /// <param name="searchParts">The search expression represented as a list of parts.</param>
        public void AddSearch(IEnumerable<string> searchParts)
        {
            foreach (var searchPart in searchParts)
            {
                if (string.IsNullOrWhiteSpace(searchPart) || this.SearchValues.Contains(searchPart))
                {
                    continue;
                }

                this.SearchValues.Add(searchPart);
            }

            this.Search = string.Join(" ", this.SearchValues);
        }

        /// <summary>
        ///     Adds a new search expression to the current expression.
        /// </summary>
        /// <param name="search">The search expression to add.</param>
        public void AddSearch(string search)
        {
            var searchValues = Regex.Split(search, @"\s+").AsEnumerable();
            this.AddSearch(searchValues);
        }

        /// <summary>
        ///     Removes a search expression from the current expression.
        /// </summary>
        /// <param name="searchParts">The search expressions to remove as a list of parts.</param>
        public void RemoveSearch(IEnumerable<string> searchParts)
        {
            foreach (var searchPart in searchParts)
            {
                this.SearchValues.Remove(searchPart);
            }

            this.Search = string.Join(" ", this.SearchValues);
        }

        /// <summary>
        ///     Removes a search expression from the current expression.
        /// </summary>
        /// <param name="search">The search expression to remove.</param>
        public void RemoveSearch(string search)
        {
            var searchValues = Regex.Split(search, @"\s+").AsEnumerable();
            this.RemoveSearch(searchValues);
        }

        /// <summary>
        ///     Assign a new search expression.
        /// </summary>
        /// <param name="searchParts">The search expression represented as a list of parts.</param>
        public void SetSearch(IEnumerable<string> searchParts)
        {
            IList<string> searchValues = searchParts.ToList();
            var search = string.Join(" ", searchValues);
            if (this.Search == search)
            {
                return;
            }

            this.Search = search;
            this.SearchValues.Clear();
            if (string.IsNullOrWhiteSpace(search))
            {
                return;
            }

            foreach (var searchValue in searchValues)
            {
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    this.SearchValues.Add(searchValue);
                }
            }

            searchValues = this._searchPhrases.Keys.ToList();
            foreach (var searchValue in searchValues)
            {
                if (!this.SearchValues.Contains(searchValue))
                {
                    this._searchPhrases.Remove(searchValue);
                }
            }
        }

        /// <summary>
        ///     Assign a new search expression.
        /// </summary>
        /// <param name="search">The search expression.</param>
        public void SetSearch(string search)
        {
            if (this.Search == search)
            {
                return;
            }

            var searchValues = Regex.Split(search, @"\s+").AsEnumerable();
            this.SetSearch(searchValues);
        }

        /// <summary>
        ///     Assign a new search expression.
        /// </summary>
        /// <param name="searchParts">The search expression represented as a dictionary of parts.</param>
        public void SetSearch(IDictionary<string, bool> searchParts)
        {
            var searchValues = searchParts.Select(searchPart => searchPart.Value ? searchPart.Key : $"!{searchPart.Key}").AsEnumerable();
            this.SetSearch(searchValues);
        }
    }
}