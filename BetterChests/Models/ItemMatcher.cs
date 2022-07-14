namespace StardewMods.BetterChests.Models;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StardewModdingAPI;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;

/// <inheritdoc cref="StardewMods.Common.Integrations.BetterChests.IItemMatcher" />
internal class ItemMatcher : ObservableCollection<string>, IItemMatcher
{
    private readonly IDictionary<string, SearchPhrase> _clean = new Dictionary<string, SearchPhrase>();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemMatcher" /> class.
    /// </summary>
    /// <param name="exactMatch">Set to true to disallow partial matches.</param>
    /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
    /// <param name="translation">Translations from the i18n folder.</param>
    public ItemMatcher(bool exactMatch = false, string? searchTagSymbol = null, ITranslationHelper? translation = null)
    {
        this.Translation = translation;
        this.ExactMatch = exactMatch;
        this.SearchTagSymbol = searchTagSymbol ?? string.Empty;
    }

    /// <inheritdoc />
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

    private ITranslationHelper? Translation { get; }

    /// <inheritdoc />
    public bool Matches(Item? item)
    {
        if (item is null)
        {
            return false;
        }

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
        if (!string.IsNullOrWhiteSpace(item) && !this.Contains(item))
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
            var clean = this.ParseString(item);
            if (clean is not null)
            {
                this._clean.Add(item, clean);
            }
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

    private SearchPhrase? ParseString(string value)
    {
        var stringBuilder = new StringBuilder(value.Trim());
        var tagMatch = string.IsNullOrWhiteSpace(this.SearchTagSymbol) || value[..1] == this.SearchTagSymbol;
        if (tagMatch && !string.IsNullOrWhiteSpace(this.SearchTagSymbol))
        {
            stringBuilder.Remove(0, this.SearchTagSymbol.Length);
        }

        var newValue = stringBuilder.ToString();
        return string.IsNullOrWhiteSpace(newValue) ? null : new(newValue, tagMatch, this.ExactMatch, this.Translation);
    }
}