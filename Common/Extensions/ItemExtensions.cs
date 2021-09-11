using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace Common.Extensions
{
    internal static class ItemExtensions
    {
        private const string CategoryFurniture = "category_furniture";
        private const string CategoryArtifact = "category_artifact";
        private const string DonateMuseum = "donate_museum";
        private const string DonateBundle = "donate_bundle";
        public static bool MatchesTagExt(this Item item, Dictionary<string, bool> filterItems)
        {
            if (filterItems.Count == 0)
                return true;
            var allowed = false;
            foreach (var filterItem in filterItems.Where(filterItem => item.MatchesTagExt(filterItem.Key)))
            {
                if (filterItem.Value)
                {
                    // Accepted due to matching at least one item from allowed list
                    allowed = true;
                }
                else
                {
                    // Rejected due to matching any item from block list
                    return false;
                }
            }
            return allowed;
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
        public static void RecursiveIterate(this Item item, Action<Item> action)
        {
            if (item is Chest { SpecialChestType: Chest.SpecialChestTypes.None } chest)
            {
                foreach (var chestItem in chest.items.Where(chestItem => chestItem is not null))
                {
                    RecursiveIterate(chestItem, action);
                }
            }
            action(item);
        }
    }
}