namespace StardewMods.Common.Integrations.BetterChests;

using System.Collections.Generic;
using System.Collections.Specialized;
using StardewValley;

/// <summary>
///     Matches item name/tags against a set of search phrases.
/// </summary>
public interface IItemMatcher : IList<string>, INotifyCollectionChanged
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