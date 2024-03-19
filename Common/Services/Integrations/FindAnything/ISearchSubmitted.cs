namespace StardewMods.Common.Services.Integrations.FindAnything;

/// <summary>The event arguments after a search is submitted.</summary>
public interface ISearchSubmitted
{
    /// <summary>Gets the search term.</summary>
    string SearchTerm { get; }

    /// <summary>Gets the location where the search was performed.</summary>
    GameLocation Location { get; }

    /// <summary>Adds a search result to the collection.</summary>
    /// <param name="result">The search result to add.</param>
    public void AddResult(ISearchResult result);
}