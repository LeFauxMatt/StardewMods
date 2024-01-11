namespace StardewMods.FindAnything.Framework.Models.Events;

using StardewMods.Common.Services.Integrations.FindAnything;

/// <inheritdoc cref="StardewMods.Common.Services.Integrations.FindAnything.ISearchSubmitted" />
internal sealed class SearchSubmittedEventArgs(string searchTerm, GameLocation location) : EventArgs, ISearchSubmitted
{
    /// <summary>Gets the search results.</summary>
    public List<ISearchResult> SearchResults { get; } = [];

    /// <inheritdoc />
    public string SearchTerm { get; } = searchTerm;

    /// <inheritdoc />
    public GameLocation Location { get; } = location;

    /// <inheritdoc />
    public void AddResult(ISearchResult result) => this.SearchResults.Add(result);
}