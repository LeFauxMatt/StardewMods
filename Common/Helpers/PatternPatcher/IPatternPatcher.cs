namespace StardewMods.Common.Helpers.PatternPatcher;

/// <summary>Edits an enumerable list of items at points identified by patterns or sequences.</summary>
/// <typeparam name="TItem">The entry type of the item.</typeparam>
public interface IPatternPatcher<TItem>
{
    /// <summary>Gets the number of patches that were applied.</summary>
    public int AppliedPatches { get; }

    /// <summary>Gets the total number of patches that were registered.</summary>
    public int TotalPatches { get; }

    /// <summary>Allows patching a list in place after a specific pattern block is matched.</summary>
    /// <param name="patch">The patch to apply.</param>
    /// <param name="patternBlock">The pattern block to match.</param>
    /// <returns>The same instance of PatternPatcher.</returns>
    public IPatternPatcher<TItem> AddPatch(Action<IList<TItem>> patch, params TItem[] patternBlock);

    /// <summary>Allows patching a list in place after a specific pattern block is matched.</summary>
    /// <param name="patch">The patch to apply.</param>
    /// <param name="patternBlock">The pattern block to match.</param>
    public void AddPatchLoop(Action<IList<TItem>> patch, params TItem[] patternBlock);

    /// <summary>Empty patch that will skip passed the pattern block.</summary>
    /// <param name="patternBlock">The pattern block to match.</param>
    /// <returns>The same instance of PatternPatcher.</returns>
    public IPatternPatcher<TItem> AddSeek(params TItem[] patternBlock);

    /// <summary>Returns the remaining buffer of pattern items.</summary>
    /// <returns>The remaining items in buffer.</returns>
    public IEnumerable<TItem> FlushBuffer();

    /// <summary>Matches the incoming items against patterns in sequence, and return the patched sequence.</summary>
    /// <param name="item">The next incoming item from the original list.</param>
    /// <returns>The patched sequence.</returns>
    public IEnumerable<TItem> From(TItem item);

    /// <summary>Repeats the last patch the specified number of times.</summary>
    /// <param name="repeat">The number of times to repeat the patch.</param>
    /// <returns>The same instance of PatternPatcher.</returns>
    public IPatternPatcher<TItem> Repeat(int repeat);
}
