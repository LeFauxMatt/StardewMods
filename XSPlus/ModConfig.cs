using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace XSPlus
{
    internal class ModConfig
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(0)]
        public int Capacity { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(6)]
        public int MenuRows { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("Inventory")]
        public string CraftingRange { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("Inventory")]
        public string StashingRange { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SButton.K)]
        public KeybindList OpenCrafting { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SButton.Z)]
        public KeybindList StashItems { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SButton.DPadUp)]
        public KeybindList ScrollUp { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SButton.DPadDown)]
        public KeybindList ScrollDown { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SButton.DPadLeft)]
        public KeybindList PreviousTab { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(SButton.DPadRight)]
        public KeybindList NextTab { get; set; }
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("#")]
        public string SearchTagSymbol { get; set; }
        public IDictionary<string, bool> Global { get; set; }
        [JsonConstructor]
        public ModConfig(
            int capacity,
            int menuRows,
            string craftingRange,
            string stashingRange,
            KeybindList openCrafting,
            KeybindList stashItems,
            KeybindList scrollUp,
            KeybindList scrollDown,
            KeybindList previousTab,
            KeybindList nextTab,
            string searchTagSymbol,
            IDictionary<string, bool> global
        )
        {
            Capacity = capacity;
            MenuRows = menuRows;
            CraftingRange = craftingRange;
            StashingRange = stashingRange;
            OpenCrafting = openCrafting;
            StashItems = stashItems;
            ScrollUp = scrollUp;
            ScrollDown = scrollDown;
            PreviousTab = previousTab;
            NextTab = nextTab;
            SearchTagSymbol = searchTagSymbol;
            Global = global ?? new Dictionary<string, bool>
            {
                { "InventoryTabs", true },
                { "SearchItems", true }
            };
        }
        public ModConfig() : this(
            capacity: 0,
            menuRows: 6,
            craftingRange: "Inventory",
            stashingRange: "Inventory",
            openCrafting: new KeybindList(SButton.K),
            stashItems: new KeybindList(SButton.Z),
            scrollUp: new KeybindList(SButton.DPadUp),
            scrollDown: new KeybindList(SButton.DPadDown),
            previousTab: new KeybindList(SButton.DPadLeft),
            nextTab: new KeybindList(SButton.DPadRight),
            searchTagSymbol: "#",
            global: new Dictionary<string, bool>
            {
                { "InventoryTabs", true },
                { "SearchItems", true }
            })
        {
        }
    }
}