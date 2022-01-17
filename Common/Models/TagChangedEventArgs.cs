namespace Common.Models;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
///     Arguments for the TagChanged event from <see cref="ItemMatcher" />.
/// </summary>
public class TagChangedEventArgs : EventArgs
{
    public TagChangedEventArgs(IEnumerable<string> added, IEnumerable<string> removed)
    {
        this.Added = added.ToList();
        this.Removed = removed.ToList();
    }

    public IList<string> Added { get; }

    public IList<string> Removed { get; }
}