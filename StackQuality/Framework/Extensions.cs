namespace StardewMods.StackQuality.Framework;

using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Helpers.AtraBase.StringHandlers;

/// <summary>
///     Extension methods.
/// </summary>
internal static class Extensions
{
    /// <summary>
    ///     Gets an array of the stacks for each quality.
    /// </summary>
    /// <param name="obj">The object to get stacks for.</param>
    /// <returns>Returns the stacks.</returns>
    public static int[] GetStacks(this SObject obj)
    {
        var stacks = new int[4];

        if (obj.modData.TryGetValue("furyx639.StackQuality/qualities", out var qualities)
         && !string.IsNullOrWhiteSpace(qualities))
        {
            var qualitiesSpan = new StreamSplit(qualities);
            var quality = qualitiesSpan.GetEnumerator();
            for (var i = 0; i < 4; ++i)
            {
                if (quality.MoveNext())
                {
                    stacks[i] = int.Parse(quality.Current);
                }
            }
        }
        else
        {
            stacks[obj.Quality == 4 ? 3 : obj.Quality] = obj.Stack;
        }

        return stacks;
    }

    /// <summary>
    ///     Splits an item into two separate stacks.
    /// </summary>
    /// <param name="obj">The item to split.</param>
    /// <param name="other">Another item to stack the split into.</param>
    /// <param name="take">The amount of items to take from the first.</param>
    /// <returns>Returns true if the stack could be split.</returns>
    public static bool SplitStacks(this SObject obj, [NotNullWhen(true)] ref Item? other, int[] take)
    {
        if (other is not (SObject or null) || other?.canStackWith(obj) == false || take.All(stack => stack == 0))
        {
            return false;
        }

        var stacks = obj.GetStacks();
        var existingStacks = (other as SObject)?.GetStacks() ?? new int[4];
        other ??= (SObject)obj.getOne();
        for (var i = 0; i < 4; ++i)
        {
            if (stacks[i] == 0)
            {
                continue;
            }

            if (stacks[i] >= take[i])
            {
                stacks[i] -= take[i];
                existingStacks[i] += take[i];
                continue;
            }

            take[i] = stacks[i];
            existingStacks[i] += stacks[i];
            stacks[i] = 0;
        }

        obj.UpdateQuality(stacks);
        ((SObject)other).UpdateQuality(existingStacks);
        return true;
    }

    /// <summary>
    ///     Updates the quality of the item based on if it is holding multiple stacks.
    /// </summary>
    /// <param name="obj">The object to update.</param>
    /// <param name="stacks">The stacks to update the object with.</param>
    /// <param name="updateStack">Indicates whether to update the stack size of the object.</param>
    public static void UpdateQuality(this SObject obj, int[] stacks, bool updateStack = true)
    {
        obj.modData["furyx639.StackQuality/qualities"] = stacks.ToModData();
        if (updateStack)
        {
            obj.Stack = stacks.Sum();
            return;
        }

        for (var index = 3; index >= 0; index--)
        {
            if (stacks[index] <= 0)
            {
                continue;
            }

            obj.Quality = (int)Common.IndexToQuality(index);
            return;
        }

        obj.Quality = 0;
    }

    /// <summary>
    ///     Gets string representation of stack sizes.
    /// </summary>
    /// <param name="stacks">The stacks to serialize.</param>
    /// <returns>Returns a the string value of stack sizes.</returns>
    private static string ToModData(this IEnumerable<int> stacks)
    {
        return string.Join(" ", stacks.Select(stack => stack.ToString()).ToArray());
    }
}