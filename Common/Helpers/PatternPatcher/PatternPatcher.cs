#nullable disable

namespace Common.Helpers.PatternPatcher;

using System;
using System.Collections.Generic;
using System.Linq;

/// <inheritdoc />
internal class PatternPatcher<TItem> : IPatternPatcher<TItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PatternPatcher{TItem}" /> class.
    /// </summary>
    /// <param name="comparer">A function that determines if an item in the list matches a item in the pattern.</param>
    public PatternPatcher(Func<TItem, TItem, bool> comparer)
    {
        this.Comparer = comparer;
    }

    /// <inheritdoc />
    public int AppliedPatches { get; private set; }

    /// <inheritdoc />
    public int TotalPatches { get; private set; }

    private IEnumerator<TItem> CodeEnum { get; set; }

    private Func<TItem, TItem, bool> Comparer { get; }

    private PatternPatch CurrentPatch { get; set; }

    private bool Done { get; set; }

    private IList<TItem> ItemBuffer { get; } = new List<TItem>();

    private PatternPatch LastPatch { get; set; }

    private Queue<PatternPatch> Patches { get; } = new();

    /// <inheritdoc />
    public IPatternPatcher<TItem> AddPatch(Action<IList<TItem>> patch, params TItem[] patternBlock)
    {
        this.LastPatch = new(patternBlock, patch, false);
        this.Patches.Enqueue(this.LastPatch);
        this.TotalPatches++;

        return this;
    }

    /// <inheritdoc />
    public void AddPatchLoop(Action<IList<TItem>> patch, params TItem[] patternBlock)
    {
        this.LastPatch = new(patternBlock, patch, true);
        this.Patches.Enqueue(this.LastPatch);
        this.TotalPatches++;
    }

    /// <inheritdoc />
    public IPatternPatcher<TItem> AddSeek(params TItem[] patternBlock)
    {
        return this.AddPatch(null, patternBlock);
    }

    /// <inheritdoc />
    public IEnumerable<TItem> FlushBuffer()
    {
        foreach (var item in this.ItemBuffer)
        {
            yield return item;
        }

        this.ItemBuffer.Clear();
    }

    /// <inheritdoc />
    public IEnumerable<TItem> From(TItem item)
    {
        // Add incoming item to buffer
        this.ItemBuffer.Add(item);

        // No more patches to apply
        if (this.Done)
        {
            return Enumerable.Empty<TItem>();
        }

        // Initialize Patch
        if (this.CurrentPatch is null && this.Patches.TryDequeue(out var patch))
        {
            this.CurrentPatch = patch;
            this.CodeEnum = this.CurrentPatch.Pattern.GetEnumerator();
            this.CodeEnum.MoveNext();
        }

        // No more patches
        if (this.CurrentPatch is null)
        {
            this.Done = true;
            return Enumerable.Empty<TItem>();
        }

        // Does not match current pattern
        if (!this.Comparer(this.CodeEnum.Current, item))
        {
            this.CodeEnum.Reset();
            this.CodeEnum.MoveNext();
            return Enumerable.Empty<TItem>();
        }

        // Matches pattern incompletely
        if (this.CodeEnum.MoveNext())
        {
            return Enumerable.Empty<TItem>();
        }

        // Complete match so apply patch
        this.CurrentPatch.Patch?.Invoke(this.ItemBuffer);
        this.AppliedPatches++;

        // Reset code position to allow looping
        if (this.CurrentPatch.Loop)
        {
            this.CodeEnum.Reset();
            this.CodeEnum.MoveNext();
        }
        else
        {
            // Next patch will be dequeued
            this.CurrentPatch = null;
        }

        // Flush outgoing item buffer
        return this.FlushBuffer();
    }

    /// <inheritdoc />
    public IPatternPatcher<TItem> Repeat(int repeat)
    {
        // Add extra copies for repeat-N times patches
        while (--repeat >= 0)
        {
            this.Patches.Enqueue(new(this.LastPatch.Pattern, this.LastPatch.Patch, false));
            this.TotalPatches++;
        }

        return this;
    }

    private record PatternPatch(IEnumerable<TItem> Pattern, Action<IList<TItem>> Patch, bool Loop);
}