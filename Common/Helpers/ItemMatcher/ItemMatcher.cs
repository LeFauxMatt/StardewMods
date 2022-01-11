namespace Common.Helpers.ItemMatcher;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Common.Models;
using StardewValley;

/// <summary>
///     Matches item name/tags against a set of search phrases.
/// </summary>
internal class ItemMatcher : HashSet<string>
{
    private readonly Dictionary<string, SearchPhrase> _clean = new();
    private readonly HashSet<string> _dirty = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemMatcher" /> class.
    /// </summary>
    /// <param name="exactMatch">Whether </param>
    /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
    public ItemMatcher(bool exactMatch = false, string searchTagSymbol = null)
    {
        this.ExactMatch = exactMatch;
        this.SearchTagSymbol = searchTagSymbol ?? string.Empty;
    }

    public event EventHandler<TagChangedEventArgs> TagChanged;

    public bool ExactMatch { get; }

    public string SearchTagSymbol { get; }

    public string StringValue
    {
        get => string.Join(" ", this);
        set
        {
            this.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                this.UnionWith(Regex.Split(value, @"\s+"));
            }

            this.Update();
        }
    }

    /// <summary>
    /// Adds the specified element to the <see cref="ItemMatcher"/>.
    /// </summary>
    /// <param name="item">The element to add to the set.</param>
    /// <returns>true if the element is added to the <see cref="ItemMatcher"/> object; false if the element is already present.</returns>
    public new bool Add(string item)
    {
        var added = base.Add(item);
        this.Update();
        return added;
    }

    /// <summary>
    /// Removes the specified element to the <see cref="ItemMatcher"/>.
    /// </summary>
    /// <param name="item">The element to remove from the set.</param>
    /// <returns>true if the element is added to the <see cref="ItemMatcher"/> object; false if the element is already present.</returns>
    public new bool Remove(string item)
    {
        var removed = base.Remove(item);
        this.Update();
        return removed;
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

        this.Update();
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

    private void Update()
    {
        if (!this._dirty.SetEquals(this))
        {
            var added = this.Except(this._dirty).ToList();
            var removed = this._dirty.Except(this).ToList();

            foreach (var key in added.Where(key => !string.IsNullOrWhiteSpace(key)))
            {
                this._clean.Add(key, this.ParseString(key));
                this._dirty.Add(key);
            }

            foreach (var key in removed)
            {
                this._clean.Remove(key);
                this._dirty.Remove(key);
            }

            this.TagChanged?.Invoke(this, new(added, removed));
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
}