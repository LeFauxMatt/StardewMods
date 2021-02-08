using System;
using System.Linq;
using ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ExpandedStorage.Framework.Extensions
{
    public static class ItemExtensions
    {
        private const string CategoryFurniture = "category_furniture";
        private const string CategoryArtifact = "category_artifact";
        public static Chest ToChest(this Item item, StorageContentData config = null)
        {
            // Get config for chest
            config ??= ExpandedStorage.GetConfig(item);
            
            // Create Chest from Item
            var chest = new Chest(true, Vector2.Zero, item.ParentSheetIndex)
            {
                name = item.Name,
                Stack = item.Stack,
                SpecialChestType = Enum.TryParse(config.SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                    ? specialChestType
                    : Chest.SpecialChestTypes.None
            };
            chest.fridge.Value = config.IsFridge;
            if (item.ParentSheetIndex == 216)
                chest.lidFrameCount.Value = 2;

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

        public static bool MatchesTagExt(this Item item, string search) => item.MatchesTagExt(search, true);
        public static bool MatchesTagExt(this Item item, string search, bool exactMatch) =>
            item switch
            {
                Furniture when TagEquals(search, CategoryFurniture, exactMatch) => true,
                Object {Type: "Arch"} when TagEquals(search, CategoryArtifact, exactMatch) => true,
                _ => item.GetContextTags().Any(tag => TagEquals(search, tag, exactMatch))
            };

        private static bool TagEquals(string search, string match, bool exact) =>
            exact && search.Equals(match) || match.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > -1;
    }
}