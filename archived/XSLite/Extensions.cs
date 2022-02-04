namespace XSLite;

using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

internal static class Extensions
{
    /// <summary>Retrieves a storage model for an item.</summary>
    /// <param name="item">The item to check.</param>
    /// <param name="storage">The storage model to for the item.</param>
    /// <returns>Returns true if a storage model exists for this item.</returns>
    public static bool TryGetStorage(this Item item, out Storage storage)
    {
        if (!item.modData.TryGetValue($"{XSLite.ModPrefix}/Storage", out var storageName))
        {
            storageName = item.Name;
        }

        return XSLite.Storages.TryGetValue(storageName, out storage) && item.Category == -9;
    }

    /// <summary>Converts a vanilla item with an expanded storage.</summary>
    /// <param name="item">The item to replace.</param>
    /// <param name="storage">The storage model to use.</param>
    public static Chest ToChest(this Item item, Storage storage)
    {
        var chest = new Chest(true, Vector2.Zero, storage.Format == Storage.AssetFormat.Vanilla ? item.ParentSheetIndex : 232)
        {
            Name = storage.Name,
            SpecialChestType = storage.SpecialChestType,
            fridge =
            {
                Value = storage.IsFridge,
            },
            lidFrameCount =
            {
                Value = storage.Frames,
            },
            modData =
            {
                [$"{XSLite.ModPrefix}/Storage"] = storage.Name,
            },
        };

        if (item is Chest oldChest)
        {
            if (oldChest.items.Any())
            {
                chest.items.CopyFrom(oldChest.items);
            }

            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
        }

        if (storage.HeldStorage)
        {
            var heldChest = new Chest(true, Vector2.Zero)
            {
                modData =
                {
                    [$"{XSLite.ModPrefix}/Storage"] = storage.Name,
                },
            };

            if (item is SObject {heldObject.Value: Chest oldHeldChest} && oldHeldChest.items.Any())
            {
                heldChest.items.CopyFrom(oldHeldChest.items);
            }

            chest.heldObject.Value = heldChest;
        }

        // Copy modData from original item
        var excludedKeys = new[]
        {
            $"{XSLite.ModPrefix}/X", $"{XSLite.ModPrefix}/Y",
        };

        foreach (var modData in item.modData.Pairs.Where(modData => !excludedKeys.Contains(modData.Key)))
        {
            chest.modData[modData.Key] = modData.Value;
        }

        // Copy modData from config
        foreach (var modData in storage.ModData)
        {
            if (!chest.modData.ContainsKey(modData.Key))
            {
                chest.modData.Add(modData.Key, modData.Value);
            }
        }

        return chest;
    }
}