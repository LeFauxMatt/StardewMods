namespace XSPlus.Models
{
    using Newtonsoft.Json;
    using StardewValley.Menus;

    /// <summary>
    /// A tab representing a group of items.
    /// </summary>
    internal class Tab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tab"/> class.
        /// </summary>
        /// <param name="name">The name of this tab.</param>
        /// <param name="tags">The context tags of items belonging to this tab.</param>
        [JsonConstructor]
        public Tab(string name, string[] tags)
        {
            this.Name = name;
            this.Tags = tags;
        }

        /// <summary>
        /// The name of the tab.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The context tags of items belonging to this tab.
        /// </summary>
        public string[] Tags { get; }

        /// <summary>
        /// The visual representation fo the tab.
        /// </summary>
        public ClickableTextureComponent? Component { get; set; }
    }
}