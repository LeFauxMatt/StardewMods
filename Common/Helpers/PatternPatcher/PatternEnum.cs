namespace Common.Helpers.PatternPatcher;

using System.Collections.Generic;

/// <inheritdoc />
internal class PatternEnum<TItem> : IEnumerator<TItem>
{
    private TItem _currentItem;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PatternEnum{TRecord}" /> class.
    /// </summary>
    /// <param name="items"></param>
    public PatternEnum(IList<TItem> items)
    {
        this.Items = items;
    }

    /// <inheritdoc />
    public object Current
    {
        get => this._currentItem;
    }

    /// <inheritdoc />
    TItem IEnumerator<TItem>.Current
    {
        get => this._currentItem;
    }

    private IList<TItem> Items { get; }

    private int Index { get; set; } = -1;

    /// <inheritdoc />
    public bool MoveNext()
    {
        // Loop
        if (++this.Index >= this.Items.Count)
        {
            this.Index = -1;
            return false;
        }

        this._currentItem = this.Items[this.Index];
        return true;
    }

    /// <inheritdoc />
    public void Reset()
    {
        this.Index = -1;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing
    }
}