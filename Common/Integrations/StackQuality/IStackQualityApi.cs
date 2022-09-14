namespace StardewMods.Common.Integrations.StackQuality;

/// <summary>
///     API for StackQuality.
/// </summary>
public interface IStackQualityApi
{
    /// <summary>
    ///     Gets an array of the stacks for each quality.
    /// </summary>
    /// <param name="obj">The object to get stacks for.</param>
    /// <returns>Returns the stacks.</returns>
    public int[] GetStacks(SObject obj);

    /// <summary>
    ///     Splits an item into two separate stacks.
    /// </summary>
    /// <param name="obj">The item to split.</param>
    /// <param name="other">Another item to stack the split into.</param>
    /// <param name="take">The amount of items to take from the first.</param>
    /// <returns>Returns true if the stack could be split.</returns>
    public bool SplitStacks(SObject obj, [NotNullWhen(true)] ref Item? other, int[] take);

    /// <summary>
    ///     Updates the quality of the item based on if it is holding multiple stacks.
    /// </summary>
    /// <param name="obj">The object to update.</param>
    /// <param name="stacks">The stacks to update the object with.</param>
    /// <param name="updateStack">Indicates whether to update the stack size of the object.</param>
    public void UpdateQuality(SObject obj, int[] stacks, bool updateStack = true);

    // Callback for item select
}