namespace StardewMods.StackQuality;

using StardewMods.Common.Integrations.StackQuality;
using StardewMods.StackQuality.Helpers;

/// <inheritdoc />
public sealed class StackQualityApi : IStackQualityApi
{
    /// <inheritdoc/>
    public int[] GetStacks(SObject obj)
    {
        return obj.GetStacks();
    }

    /// <inheritdoc/>
    public bool SplitStacks(SObject obj, [NotNullWhen(true)] ref Item? other, int take)
    {
        return obj.SplitStacks(ref other, take);
    }

    /// <inheritdoc/>
    public void UpdateQuality(SObject obj, int[] stacks, bool updateStack = true)
    {
        obj.UpdateQuality(stacks, updateStack);
    }
}