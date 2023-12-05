namespace StardewMods.BetterChests.Framework.Models;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;

/// <summary>Matches item name/tags against a set of search phrases.</summary>
internal sealed class ItemMatcher : ObservableCollection<string>
{
    private readonly IDictionary<string, SearchPhrase> clean = new Dictionary<string, SearchPhrase>();

    /// <summary>Initializes a new instance of the <see cref="ItemMatcher" /> class.</summary>
    /// <param name="exactMatch">Set to true to disallow partial matches.</param>
    /// <param name="searchTagSymbol">Prefix to denote search is based on an item's context tags.</param>
    /// <param name="translation">Translations from the i18n folder.</param>
    public ItemMatcher(bool exactMatch = false, string? searchTagSymbol = null, ITranslationHelper? translation = null)
    {
        this.Translation = translation;
        this.ExactMatch = exactMatch;
        this.SearchTagSymbol = searchTagSymbol ?? string.Empty;
    }

    /// <summary>Gets or sets a string representation of all registered search texts.</summary>
    public string StringValue
    {
        get => string.Join(" ", this);
        set
        {
            this.Clear();
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var split = value.Split(' ');
            foreach (var text in split)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    this.Add(text);
                }
            }
        }
    }

    private bool ExactMatch { get; }

    private string SearchTagSymbol { get; }

    private ITranslationHelper? Translation { get; }

    /// <summary>Checks if an item matches the search phrases.</summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if item matches any search phrase unless a NotMatch search phrase was matched.</returns>
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
        foreach (var searchPhrase in this.clean.Values)
        {
            if (searchPhrase.Matches(item))
            {
                if (!searchPhrase.NotMatch || this.clean.Values.All(p => p.NotMatch))
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
        var added = this.Except(this.clean.Keys);
        var removed = this.clean.Keys.Except(this);

        foreach (var item in removed)
        {
            this.clean.Remove(item);
        }

        foreach (var item in added)
        {
            if (this.TryParse(item, out var searchPhrase))
            {
                this.clean.Add(item, searchPhrase);
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

    private bool TryParse(string value, [NotNullWhen(true)] out SearchPhrase? searchPhrase)
    {
        var stringBuilder = new StringBuilder(value.Trim());
        var tagMatch = string.IsNullOrWhiteSpace(this.SearchTagSymbol) || value[..1] == this.SearchTagSymbol;
        if (tagMatch && !string.IsNullOrWhiteSpace(this.SearchTagSymbol))
        {
            stringBuilder.Remove(0, this.SearchTagSymbol.Length);
        }

        var newValue = stringBuilder.ToString();
        if (string.IsNullOrWhiteSpace(newValue))
        {
            searchPhrase = null;
            return false;
        }

        searchPhrase = new(newValue, tagMatch, this.ExactMatch, this.Translation);
        return true;
    }
}
