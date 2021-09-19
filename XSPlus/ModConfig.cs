namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using StardewModdingAPI;
    using StardewModdingAPI.Utilities;

    /// <summary>Default values and config options for features.</summary>
    internal class ModConfig
    {
        private int _capacity;
        private string _craftingRange = "Location";
        private string _stashingRange = "Location";

        /// <summary>Initializes a new instance of the <see cref="ModConfig"/> class.</summary>
        /// <param name="global">Globally enabled/disabled features.</param>
        [JsonConstructor]
        public ModConfig(IDictionary<string, bool> global)
        {
            this.Global = global ?? new Dictionary<string, bool>
            {
                { "InventoryTabs", true },
                { "SearchItems", true },
            };
        }

        /// <summary>Initializes a new instance of the <see cref="ModConfig"/> class.</summary>
        public ModConfig()
            : this(
                new Dictionary<string, bool>
                {
                    { "InventoryTabs", true },
                    { "SearchItems", true },
                })
        {
        }

        /// <summary>Gets or sets default slots that a <see cref="StardewValley.Objects.Chest"/> can store.</summary>
        public int Capacity
        {
            get => this._capacity;
            set
            {
                this._capacity = value;
                if (value == 0)
                {
                    this.Global.Remove("Capacity");
                }
                else
                {
                    this.Global["Capacity"] = true;
                }
            }
        }

        /// <summary>Gets or sets maximum number of rows to show in the <see cref="StardewValley.Menus.ItemGrabMenu"/>.</summary>
        public int MenuRows { get; set; } = 6;

        /// <summary>Gets or sets default maximum range that a <see cref="StardewValley.Objects.Chest"/> can be crafted from.</summary>
        public string CraftingRange
        {
            get => this._craftingRange;
            set
            {
                this._craftingRange = value;
                switch (value)
                {
                    case "Default":
                        this.Global.Remove("CraftFromChest");
                        break;
                    case "Disabled":
                        this.Global["CraftFromChest"] = false;
                        break;
                    default:
                        this.Global["CraftFromChest"] = true;
                        break;
                }
            }
        }

        /// <summary>Gets or sets default maximum range that a <see cref="StardewValley.Objects.Chest"/> can be stashed into.</summary>
        public string StashingRange
        {
            get => this._stashingRange;
            set
            {
                this._stashingRange = value;
                switch (value)
                {
                    case "Default":
                        this.Global.Remove("StashToChest");
                        break;
                    case "Disabled":
                        this.Global["StashToChest"] = false;
                        break;
                    default:
                        this.Global["StashToChest"] = true;
                        break;
                }
            }
        }

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
        public IDictionary<string, bool> Global { get; set; }

        /// <summary>Gets the global config value for feature by name.</summary>
        /// <param name="featureName">The feature to get global value for.</param>
        /// <returns>Returns a getter method for the global config value.</returns>
        public Func<string> GetConfig(string featureName)
        {
            return () => this.Global.TryGetValue(featureName, out bool global)
                ? (global ? "Enable" : "Disable")
                : "Default";
        }

        /// <summary>Sets the global config value for feature by name.</summary>
        /// <param name="featureName">The feature to set global value for.</param>
        /// <returns>Returns a setter method for assigning the global config value.</returns>
        public Action<string> SetConfig(string featureName)
        {
            return value =>
            {
                switch (value)
                {
                    case "Enable":
                        this.Global[featureName] = true;
                        FeatureManager.ActivateFeature(featureName);
                        break;
                    case "Disable":
                        this.Global[featureName] = false;
                        FeatureManager.DeactivateFeature(featureName);
                        break;
                    default:
                        this.Global.Remove(featureName);
                        FeatureManager.ActivateFeature(featureName);
                        break;
                }
            };
        }
    }
}