namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Keys used for int fields in DictionaryStorageOptions.</summary>
[EnumExtensions]
internal enum IntegerKey
{
    /// <summary>The tiles from a player that craft from chest will be enabled.</summary>
    CraftFromChestDistance,

    /// <summary>The quantity of item stacks that a chest can hold.</summary>
    ResizeChestCapacity,

    /// <summary>The tiles from a player that stash to chest will be enabled.</summary>
    StashToChestDistance,

    /// <summary>Determines which chests will be stashed into before others.</summary>
    StashToChestPriority,
}