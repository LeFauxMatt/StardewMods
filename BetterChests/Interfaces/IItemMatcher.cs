namespace StardewMods.BetterChests.Interfaces;

using System.Collections.Generic;
using System.Collections.Specialized;

/// <summary>
///     Matches item name/tags against a set of search phrases.
/// </summary>
internal interface IItemMatcher : IList<string>, INotifyCollectionChanged
{
    /// <summary>
    ///     Gets or sets a string representation of all registered search texts.
    /// </summary>
    public string StringValue { get; set; }

    /// <summary>
    ///     Checks if an item matches the search phrases.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if item matches any search phrase unless a NotMatch search phrase was matched.</returns>
    public bool Matches(Item? item);
}