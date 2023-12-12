namespace StardewMods.ExpandedStorage.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Custom field keys supported by Expanded Storage.</summary>
[EnumExtensions]
internal enum CustomFieldKeys
{
    /// <summary>The sound to play when the lid closing animation plays.</summary>
    CloseNearbySound,

    /// <summary>The number of frames in the lid animation.</summary>
    Frames,

    /// <summary>Indicates whether the storage is a fridge.</summary>
    IsFridge,

    /// <summary>The distance from the player that the storage will play it's lid opening animation.</summary>
    OpenNearby,

    /// <summary>The sound to play when the lid opening animation plays.</summary>
    OpenNearbySound,

    /// <summary>The sound to play when the storage is opened.</summary>
    OpenSound,

    /// <summary>The sound to play when storage is placed.</summary>
    PlaceSound,

    /// <summary>Indicates whether player color is enabled.</summary>
    PlayerColor,
}
