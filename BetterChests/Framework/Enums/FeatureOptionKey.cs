namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Keys used for FeatureOption fields in DictionaryStorageOptions.</summary>
[EnumExtensions]
internal enum FeatureOptionKey
{
    /// <summary>Automatically organizes chests.</summary>
    AutoOrganize,

    /// <summary>Allows chests to be carried by the player.</summary>
    CarryChest,

    /// <summary>Applies a slowness effect while carrying chests.</summary>
    CarryChestSlow,

    /// <summary>Search for a chest in the current location.</summary>
    ChestFinder,

    /// <summary>Displays info about the chest from the chest menu.</summary>
    ChestInfo,

    /// <summary>Dropped items will be collected into the chest while held.</summary>
    CollectItems,

    /// <summary>Adds a configuration button to the chest menu.</summary>
    ConfigureChest,

    /// <summary>Prevents disallowed items from being added to the chest.</summary>
    FilterItems,

    /// <summary>When searching or using tabs, hide the deselected items instead of graying them out.</summary>
    HideUnselectedItems,

    /// <summary>Replaces the color picker with one that supports hue, saturation, and lightness sliders.</summary>
    HslColorPicker,

    /// <summary>Adds tabs to the chest menu.</summary>
    InventoryTabs,

    /// <summary>Allows labeling chest with a name.</summary>
    LabelChest,

    /// <summary>Allows chests to be opened while held.</summary>
    OpenHeldChest,

    /// <summary>Overrides the sort function to group and sort by a custom field.</summary>
    OrganizeChest,

    /// <summary>Customize the carrying capacity of a chest.</summary>
    ResizeChest,

    /// <summary>Adds a search bar to the chest menu.</summary>
    SearchItems,

    /// <summary>Allows items to be locked in their slot so they will not be transferred automatically.</summary>
    SlotLock,

    /// <summary>Allows StashToChest to fill existing stacks of items even if the chest is not categorized for that item.</summary>
    StashToChestStacks,

    /// <summary>Adds buttons to quickly transfer items into or out of a chest.</summary>
    TransferItems,

    /// <summary>Allows a chests contents to be unloaded into another chest.</summary>
    UnloadChest,

    /// <summary>When unloading a chest, the chest being unloaded will be absorbed by the chest receiving items.</summary>
    UnloadChestCombine,
}