namespace Common.Helpers.PatternPatcher;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// </summary>
internal class Pattern<TItem> : IEnumerable<TItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Pattern{TRecord}" /> class.
    /// </summary>
    /// <param name="item"></param>
    public Pattern(IEnumerable<TItem> item)
    {
        this.Records = item.ToList();
    }

    private IList<TItem> Records { get; }

    /// <inheritdoc />
    public IEnumerator<TItem> GetEnumerator()
    {
        return new PatternEnum<TItem>(this.Records);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}