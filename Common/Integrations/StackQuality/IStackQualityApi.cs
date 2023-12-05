namespace StardewMods.Common.Integrations.StackQuality;

/// <summary>API for StackQuality.</summary>
public interface IStackQualityApi
{
    /// <summary>Stacks one item on to another.</summary>
    /// <param name="obj">The item to stack on to.</param>
    /// <param name="other">The item to stack from.</param>
    /// <param name="remaining">The remaining stacks.</param>
    /// <returns>Returns true if the item could be stacked.</returns>
    public bool AddToStacks(SObject obj, Item other, [NotNullWhen(true)] out int[]? remaining);

    /// <summary>Tests if two <see cref="ISalable" /> are equivalent.</summary>
    /// <param name="salable">The first object to check.</param>
    /// <param name="other">The second object to check.</param>
    /// <returns>Returns true if the objects are equivalent.</returns>
    public bool EquivalentObjects(ISalable salable, ISalable? other);

    /// <summary>Gets an array of the stacks for each quality.</summary>
    /// <param name="obj">The object to get stacks for.</param>
    /// <param name="stacks">The stack size for each quality.</param>
    /// <returns>Returns true if the object has multiple stacks.</returns>
    public bool GetStacks(SObject obj, [NotNullWhen(true)] out int[]? stacks);

    /// <summary>Moves stacks from one to another by a given amount.</summary>
    /// <param name="fromObj">The object to take stacks from.</param>
    /// <param name="toItem">The item to add stacks to.</param>
    /// <param name="amount">The amount to take from the first stack.</param>
    /// <returns>Returns true if the stacks could be moved.</returns>
    public bool MoveStacks(SObject fromObj, [NotNullWhen(true)] ref Item? toItem, int[] amount);

    /// <summary>Splits a stacked object into individual items for each stack quality.</summary>
    /// <param name="obj">The object to split.</param>
    /// <param name="items">An object array containing a stack for each quality.</param>
    /// <returns>Returns true if the stack could be split.</returns>
    public bool SplitStacks(SObject obj, [NotNullWhen(true)] out SObject[]? items);

    /// <summary>Updates the quality of the item based on if it is holding multiple stacks.</summary>
    /// <param name="obj">The object to update.</param>
    /// <param name="stacks">The stacks to update the object with.</param>
    public void UpdateStacks(SObject obj, int[] stacks);
}
