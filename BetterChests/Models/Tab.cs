namespace BetterChests.Models
{
    using System.Collections.Generic;
    using StardewValley.Menus;

    /// <summary>
    ///     A tab representing a group of items.
    /// </summary>
    internal class Tab
    {
        /// <summary>
        ///     Gets or sets the name of the tab.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the context tags of items belonging to this tab.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        ///     Gets or sets the visual representation fo the tab.
        /// </summary>
        public ClickableTextureComponent Component { get; set; }
    }
}