namespace StardewMods.StackQuality.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;

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
            var quality = qualities.Split(' ');
            for (var i = 0; i < 4; ++i)
            {
                stacks[i] = Convert.ToInt32(quality[i]);
            }
        }
        else
        {
            stacks[obj.Quality == 4 ? 3 : obj.Quality] = obj.Stack;
        }

        return stacks;
    }

    /// <summary>
    ///     Updates the quality of the item based on if it is holding multiple stacks.
    /// </summary>
    /// <param name="obj">The object to update.</param>
    /// <param name="stacks">The stacks to update the object with.</param>
    public static void UpdateQuality(this SObject obj, int[] stacks)
    {
        obj.Stack = stacks.Sum();
        obj.modData["furyx639.StackQuality/qualities"] = stacks.ToModData();
        for (var index = 3; index >= 0; index--)
        {
            if (stacks[index] <= 0)
            {
                continue;
            }

            obj.Quality = index == 3 ? 4 : index;
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