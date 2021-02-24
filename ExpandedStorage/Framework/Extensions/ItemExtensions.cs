using System;
using System.Linq;
using ImJustMatt.ExpandedStorage.Framework.Models;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace ImJustMatt.ExpandedStorage.Framework.Extensions
{
    public static class ItemExtensions
    {
        private const string CategoryFurniture = "category_furniture";
        private const string CategoryArtifact = "category_artifact";
        private const string DonateMuseum = "donate_museum";
        private const string DonateBundle = "donate_bundle";

        public static Chest ToChest(this Item item, Storage config = null)
        {
            // Get config for chest
            config ??= ExpandedStorage.GetConfig(item);

            // Create Chest from Item
            var chest = new Chest(true, Vector2.Zero, item.ParentSheetIndex)
            {
                name = item.Name,
                SpecialChestType = Enum.TryParse(config.SpecialChestType, out Chest.SpecialChestTypes specialChestType)
                    ? specialChestType
                    : Chest.SpecialChestTypes.None
            };
            chest.fridge.Value = config.IsFridge;

            if (string.IsNullOrWhiteSpace(config.Image))
                chest.lidFrameCount.Value = Math.Max(config.Frames, 1);
            else if (item.ParentSheetIndex == 216)
                chest.lidFrameCount.Value = 2;

            // Copy modData from original item
            foreach (var modData in item.modData)
                chest.modData.CopyFrom(modData);

            // Copy modData from config
            foreach (var modData in config.ModData)
            {
                if (!chest.modData.ContainsKey(modData.Key))
                    chest.modData.Add(modData.Key, modData.Value);
            }

            if (item is not Chest oldChest)
                return chest;

            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            if (oldChest.items.Any())
                chest.items.CopyFrom(oldChest.items);

            return chest;
        }

        public static bool MatchesTagExt(this Item item, string search)
        {
            return item.MatchesTagExt(search, true);
        }

        public static bool MatchesTagExt(this Item item, string search, bool exactMatch)
        {
            return item switch
            {
                Furniture when TagEquals(search, CategoryFurniture, exactMatch) => true,
                Object {Type: "Arch"} when TagEquals(search, CategoryArtifact, exactMatch) => true,
                Object {Type: "Arch"} when TagEquals(search, DonateMuseum, exactMatch) => CanDonateToMuseum(item),
                Object {Type: "Minerals"} when TagEquals(search, DonateMuseum, exactMatch) => CanDonateToMuseum(item),
                Object obj when TagEquals(search, DonateBundle, exactMatch) => CanDonateToBundle(obj),
                _ => item.GetContextTags().Any(tag => TagEquals(search, tag, exactMatch))
            };
        }

        private static bool TagEquals(string search, string match, bool exact)
        {
            return exact && search.Equals(match) || match.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        private static bool CanDonateToMuseum(Item item)
        {
            return Game1.locations
                       .OfType<LibraryMuseum>()
                       .FirstOrDefault()?.isItemSuitableForDonation(item)
                   ?? false;
        }

        private static bool CanDonateToBundle(Object obj)
        {
            return Game1.locations
                       .OfType<CommunityCenter>()
                       .FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
                   ?? false;
        }
    }
}