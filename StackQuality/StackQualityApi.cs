namespace StardewMods.StackQuality;

using System;
using System.Linq;
using System.Text;
using StardewMods.Common.Helpers.AtraBase.StringHandlers;
using StardewMods.Common.Integrations.StackQuality;
using StardewValley.Objects;
using StardewValley.Tools;

/// <inheritdoc />
public sealed class StackQualityApi : IStackQualityApi
{
    /// <inheritdoc />
    public bool AddToStacks(SObject obj, Item other, [NotNullWhen(true)] out int[]? remaining)
    {
        remaining = default;
        if (other is not SObject otherObj)
        {
            return false;
        }

        var maxStack = obj.maximumStackSize();
        if (maxStack == 1)
        {
            return false;
        }

        if (obj.IsSpawnedObject && !otherObj.IsSpawnedObject)
        {
            obj.IsSpawnedObject = false;
        }

        if (!this.GetStacks(obj, out var stacks))
        {
            stacks = new int[4];
        }

        if (!this.GetStacks(otherObj, out remaining))
        {
            remaining = new int[4];
        }

        var currentStack = stacks.Sum();
        if (currentStack >= maxStack)
        {
            return true;
        }

        for (var i = 3; i >= 0; --i)
        {
            var toAdd = Math.Min(remaining[i], Math.Min(maxStack - currentStack, maxStack - stacks[i]));
            currentStack += toAdd;
            stacks[i] += toAdd;
            remaining[i] -= toAdd;
            if (currentStack >= maxStack)
            {
                break;
            }
        }

        this.UpdateStacks(obj, stacks);
        return true;
    }

    /// <inheritdoc />
    public bool EquivalentObjects(ISalable salable, ISalable? other)
    {
        if (other is null || !salable.Name.Equals(other.Name))
        {
            return false;
        }

        return salable switch
        {
            ColoredObject coloredObj when other is not ColoredObject otherColoredObj
                                       || !coloredObj.color.Value.Equals(otherColoredObj.color.Value) => false,
            SObject obj when other is not SObject otherObj
                          || obj.ParentSheetIndex != otherObj.ParentSheetIndex
                          || obj.bigCraftable.Value != otherObj.bigCraftable.Value
                          || obj.orderData.Value != otherObj.orderData.Value
                          || obj.Type != otherObj.Type => false,
            Item item when other is not Item otherItem
                        || item.Category != otherItem.Category
                        || item.ParentSheetIndex != otherItem.ParentSheetIndex => false,
            Stackable stackable when other is not Stackable otherStackable || !stackable.canStackWith(otherStackable) =>
                false,
            Tool when other is not Tool => false,
            _ => true,
        };
    }

    /// <inheritdoc />
    public bool GetStacks(SObject obj, [NotNullWhen(true)] out int[]? stacks)
    {
        if (!obj.modData.TryGetValue("furyx639.StackQuality/qualities", out var qualities)
         || string.IsNullOrWhiteSpace(qualities))
        {
            stacks = new int[4];
            stacks[obj.Quality == 4 ? 3 : obj.Quality] = obj.Stack;
            return true;
        }

        stacks = new int[4];
        var span = new StreamSplit(qualities);
        var i = 0;
        foreach (var entry in span)
        {
            stacks[i++] = int.Parse(entry);
            if (i == 4)
            {
                break;
            }
        }

        return true;
    }


    /// <inheritdoc />
    public bool MoveStacks(SObject fromObj, [NotNullWhen(true)] ref Item? toItem, int[] amount)
    {
        if (amount.All(stack => stack == 0))
        {
            return toItem is not null;
        }

        if (!this.GetStacks(fromObj, out var fromStacks))
        {
            return false;
        }

        int[]? toStacks = null;
        switch (toItem)
        {
            case SObject toObj when this.EquivalentObjects(fromObj, toObj) && this.GetStacks(toObj, out toStacks):
                break;

            case null:
                toItem = (SObject)fromObj.getOne();
                break;

            default:
                return false;
        }

        toStacks ??= new int[4];
        var maxStack = fromObj.maximumStackSize();
        if (maxStack == 1)
        {
            return false;
        }

        var currentStack = toStacks.Sum();
        if (currentStack >= maxStack)
        {
            return false;
        }

        for (var i = 3; i >= 0; --i)
        {
            if (amount[i] > fromStacks[i] || toStacks[i] + amount[i] > maxStack)
            {
                return false;
            }

            fromStacks[i] -= amount[i];
            toStacks[i] += amount[i];
        }

        this.UpdateStacks(fromObj, fromStacks);
        this.UpdateStacks((SObject)toItem, toStacks);
        return true;
    }

    /// <inheritdoc />
    public bool SplitStacks(SObject obj, [NotNullWhen(true)] out SObject[]? items)
    {
        if (!this.GetStacks(obj, out var stacks))
        {
            items = default;
            return false;
        }

        items = new SObject[4];
        for (var i = 0; i < 4; ++i)
        {
            items[i] = (SObject)obj.getOne();
            items[i].modData.Remove("furyx639.StackQuality/qualities");
            items[i].Quality = i == 3 ? 4 : i;
            items[i].Stack = stacks[i];
        }

        return true;
    }

    /// <inheritdoc />
    public void UpdateStacks(SObject obj, int[] stacks)
    {
        var stack = 0;
        var quality = 0;
        var sb = new StringBuilder();
        for (var i = 0; i < 4; ++i)
        {
            sb.Append(stacks[i]);
            if (i < 3)
            {
                sb.Append(' ');
            }

            if (stacks[i] == 0)
            {
                continue;
            }

            stack += stacks[i];
            quality = i;
        }

        obj.modData["furyx639.StackQuality/qualities"] = sb.ToString();
        obj.Quality = quality == 3 ? 4 : quality;
        obj.Stack = stack;
    }
}