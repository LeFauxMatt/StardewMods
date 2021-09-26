namespace XSPlus.Models
{
    using System.Collections.Generic;
    using StardewModdingAPI;
    using StardewModdingAPI.Utilities;

    /// <summary>Default values and config options for features.</summary>
    internal class ModConfig
    {
        /// <summary>Gets or sets default slots that a <see cref="StardewValley.Objects.Chest"/> can store.</summary>
        public int Capacity { get; set; }

        /// <summary>Gets or sets maximum number of rows to show in the <see cref="StardewValley.Menus.ItemGrabMenu"/>.</summary>
        public int MenuRows { get; set; } = 3;

        /// <summary>Gets or sets default maximum range that a <see cref="StardewValley.Objects.Chest"/> can be crafted from.</summary>
        public string CraftingRange { get; set; } = "Location";

        /// <summary>Gets or sets default maximum range that a <see cref="StardewValley.Objects.Chest"/> can be stashed into.</summary>
        public string StashingRange { get; set; } = "Location";

        /// <summary>Gets or sets controls to open <see cref="StardewValley.Menus.CraftingPage"/>.</summary>
        public KeybindList OpenCrafting { get; set; } = new(SButton.K);

        /// <summary>Gets or sets controls to stash player items into <see cref="StardewValley.Objects.Chest"/>.</summary>
        public KeybindList StashItems { get; set; } = new(SButton.Z);

        /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu"/> up.</summary>
        public KeybindList ScrollUp { get; set; } = new(SButton.DPadUp);

        /// <summary>Gets or sets controls to scroll <see cref="StardewValley.Menus.ItemGrabMenu"/> down.</summary>
        public KeybindList ScrollDown { get; set; } = new(SButton.DPadDown);

        /// <summary>Gets or sets controls to switch to previous tab.</summary>
        public KeybindList PreviousTab { get; set; } = new(SButton.DPadLeft);

        /// <summary>Gets or sets controls to switch to next tab.</summary>
        public KeybindList NextTab { get; set; } = new(SButton.DPadRight);

        /// <summary>Gets or sets character that will be used to denote tags in search.</summary>
        public string SearchTagSymbol { get; set; } = "#";

        /// <summary>Gets or sets globally enabled/disabled features.</summary>
        public IDictionary<string, bool> Global { get; set; } = new Dictionary<string, bool>
        {
            { "InventoryTabs", true },
            { "SearchItems", true },
        };
    }
}