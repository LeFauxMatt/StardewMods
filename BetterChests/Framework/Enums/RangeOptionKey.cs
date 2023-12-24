namespace StardewMods.BetterChests.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Keys used for FeatureOptionRange fields in DictionaryStorageOptions.</summary>
[EnumExtensions]
internal enum RangeOptionKey
{
    /// <summary>Allows chests to be remotely crafted from.</summary>
    CraftFromChest,

    /// <summary>Allows chests to be remotely stashed into.</summary>
    StashToChest,
}