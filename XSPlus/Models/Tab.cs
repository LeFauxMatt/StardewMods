namespace XSPlus.Models
{
    using System.Collections.Generic;
    using StardewValley.Menus;

    /// <summary>
    /// A tab representing a group of items.
    /// </summary>
    internal class Tab
    {
        /// <summary>
        /// The name of the tab.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The context tags of items belonging to this tab.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// The visual representation fo the tab.
        /// </summary>
        public ClickableTextureComponent Component { get; set; }
    }
}