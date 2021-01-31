using System;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Extensions
{
    public static class ItemExtensions
    {
        public static Chest ToChest(this Item item, StorageContentData config = null)
        {
            // Get config for chest
            config ??= ExpandedStorage.GetConfig(item);
            
            // Create Chest from Item
            if (item is not Chest chest)
            {
                chest = new Chest(true, Vector2.Zero, item.ParentSheetIndex)
                {
                    name = item.Name,
                    Stack = item.Stack,
                    SpecialChestType = Enum.TryParse(config.SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                        ? specialChestType
                        : Chest.SpecialChestTypes.None
                };
                if (item.ParentSheetIndex == 216)
                    chest.lidFrameCount.Value = 2;
                chest.fixLidFrame();
            }

            // Copy modData
            foreach (var modData in item.modData)
                chest.modData.CopyFrom(modData);
            
            if (item is not Chest oldChest)
                return chest;

            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            if (oldChest.items.Any())
                chest.items.CopyFrom(oldChest.items);
            return chest;
        }
    }
}