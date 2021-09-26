namespace Common.Extensions
{
    using System;
    using System.Linq;
    using StardewValley;
    using StardewValley.Objects;

    /// <summary>Extension methods for the <see cref="StardewValley.Item">StardewValley.Item</see> class.</summary>
    internal static class ItemExtensions
    {
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
    }
}