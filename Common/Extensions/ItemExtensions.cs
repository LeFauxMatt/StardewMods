namespace Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Objects;
    using Object = StardewValley.Object;

    /// <summary>Extension methods for the <see cref="StardewValley.Item">StardewValley.Item</see> class.</summary>
    internal static class ItemExtensions
    {
        private const string CategoryFurniture = "category_furniture";
        private const string CategoryArtifact = "category_artifact";
        private const string DonateMuseum = "donate_museum";
        private const string DonateBundle = "donate_bundle";

        /// <summary>Tests if an item has a tag matching one from a list.</summary>
        /// <param name="item">The item to test against.</param>
        /// <param name="tags">The list of tags.</param>
        /// <returns>Returns true when item matches one of the listed tags.</returns>
        public static bool MatchesTagExt(this Item item, IList<string> tags)
        {
            return tags.Count == 0 || tags.Any(item.MatchesTagExt);
        }

        /// <summary>Tests if an item's tags meet allow/block filter conditions.</summary>
        /// <param name="item">The item to test against.</param>
        /// <param name="filterItems">A dictionary of tags that are allowed or blocked.</param>
        /// <returns>Returns true when item matches any true tag and does not match any false tags.</returns>
        public static bool MatchesTagExt(this Item item, Dictionary<string, bool> filterItems)
        {
            if (filterItems.Count == 0)
            {
                return true;
            }

            bool anyAllowed = false;
            bool allowed = true;
            foreach (KeyValuePair<string, bool> filterItem in filterItems)
            {
                bool match = item.MatchesTagExt(filterItem.Key, true);

                if (filterItem.Value)
                {
                    if (!anyAllowed)
                    {
                        anyAllowed = true;
                        allowed = match;
                    }
                    else if (match)
                    {
                        // Accepted due to matching at least one item from allowed list
                        allowed = true;
                    }
                }
                else if (match)
                {
                    // Rejected due to matching any item from block list
                    return false;
                }
            }

            return allowed;
        }

        /// <summary>Searches an item's context tags for a search phrase.</summary>
        /// <param name="item">The item to search.</param>
        /// <param name="search">The search phrase.</param>
        /// <param name="exactMatch">Whether to reject partial matches.</param>
        /// <returns>Returns true if the search phrase was found.</returns>
        public static bool MatchesTagExt(this Item item, string search, bool exactMatch)
        {
            return item switch
            {
                Furniture when ItemExtensions.TagEquals(search, ItemExtensions.CategoryFurniture, exactMatch) => true,
                Object { Type: "Arch" } when ItemExtensions.TagEquals(search, ItemExtensions.CategoryArtifact, exactMatch) => true,
                Object { Type: "Arch" } when ItemExtensions.TagEquals(search, ItemExtensions.DonateMuseum, exactMatch) => ItemExtensions.CanDonateToMuseum(item),
                Object { Type: "Minerals" } when ItemExtensions.TagEquals(search, ItemExtensions.DonateMuseum, exactMatch) => ItemExtensions.CanDonateToMuseum(item),
                Object obj when ItemExtensions.TagEquals(search, ItemExtensions.DonateBundle, exactMatch) => ItemExtensions.CanDonateToBundle(obj),
                _ => item.GetContextTags().Any(tag => ItemExtensions.TagEquals(search, tag, exactMatch)),
            };
        }

        /// <summary>Test an item for multiple search phrases.</summary>
        /// <param name="item">The item to search.</param>
        /// <param name="searchItems">The search phrases.</param>
        /// <param name="searchTagSymbol">Prefix for search phrases that will be tested against an item's context tags.</param>
        /// <returns>Returns true if item matches all search phrases.</returns>
        public static bool SearchTags(this Item item, IList<string> searchItems, string searchTagSymbol)
        {
            return searchItems.Count == 0 || searchItems.All(searchItem => item.SearchTag(searchItem, searchTagSymbol));
        }

        /// <summary>Recursively iterates chests held within chests.</summary>
        /// <param name="item">The originating item to search.</param>
        /// <param name="action">The action to perform on items within chests.</param>
        public static void RecursiveIterate(this Item item, Action<Item> action)
        {
            if (item is Chest { SpecialChestType: Chest.SpecialChestTypes.None } chest)
            {
                foreach (Item chestItem in chest.items.Where(chestItem => chestItem is not null))
                {
                    chestItem.RecursiveIterate(action);
                }
            }

            action(item);
        }

        private static bool SearchTag(this Item item, string searchItem, string searchTagSymbol)
        {
            bool matchCondition = !searchItem.StartsWith("!");
            string searchPhrase = matchCondition ? searchItem : searchItem.Substring(1);
            if (string.IsNullOrWhiteSpace(searchPhrase))
            {
                return true;
            }

            if (searchPhrase.StartsWith(searchTagSymbol))
            {
                if (item.MatchesTagExt(searchPhrase.Substring(1), false) != matchCondition)
                {
                    return false;
                }
            }
            else if ((!item.Name.Contains(searchPhrase) &&
                      !item.DisplayName.Contains(searchPhrase)) == matchCondition)
            {
                return false;
            }

            return true;
        }

        private static bool MatchesTagExt(this Item item, string search)
        {
            return item.MatchesTagExt(search, true);
        }

        private static bool TagEquals(string search, string match, bool exact)
        {
            return (exact && search.Equals(match)) || match.IndexOf(search, StringComparison.InvariantCultureIgnoreCase) > -1;
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