namespace Common.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers.ItemMatcher;

/// <summary>
///     Arguments for the TagChanged event from <see cref="ItemMatcher" />.
/// </summary>
internal class TagChangedEventArgs : EventArgs
{
    public TagChangedEventArgs(IEnumerable<string> added, IEnumerable<string> removed)
    {
        this.Added = added.ToList();
        this.Removed = removed.ToList();
    }

    public IList<string> Added { get; }

    public IList<string> Removed { get; }
}