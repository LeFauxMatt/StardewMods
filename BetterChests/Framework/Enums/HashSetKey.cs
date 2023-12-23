namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Keys used for HashSet fields in DictionaryStorageOptions.</summary>
[EnumExtensions]
internal enum HashSetKey
{
    /// <summary>The locations in which craft from chest will be disabled. </summary>
    CraftFromChestDisableLocations,

    /// <summary>The filter rules for what items are allowed into the container.</summary>
    FilterItemsList,

    /// <summary>The list of tabs to show in the inventory menu.</summary>
    InventoryTabList,

    /// <summary>The locations in which stash to chest will be disabled.</summary>
    StashToChestDisableLocations,
}