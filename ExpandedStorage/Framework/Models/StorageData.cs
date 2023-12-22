namespace StardewMods.ExpandedStorage.Framework.Models;

/// <summary>Data for an Expanded Storage chest.</summary>
internal sealed class StorageData
{
    /// <summary>Gets or sets the sound to play when the lid closing animation plays.</summary>
    public string CloseNearbySound { get; set; } = "doorCreakReverse";

    /// <summary>Gets or sets the number of frames in the lid animation.</summary>
    public int Frames { get; set; } = 1;

    /// <summary>Gets or sets a value indicating whether the storage is a fridge.</summary>
    public bool IsFridge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the storage will play it's lid opening animation when the player is
    /// nearby.
    /// </summary>
    public bool OpenNearby { get; set; }

    /// <summary>Gets or sets the sound to play when the lid opening animation plays.</summary>
    public string OpenNearbySound { get; set; } = "doorCreak";

    /// <summary>Gets or sets the sound to play when the storage is opened.</summary>
    public string OpenSound { get; set; } = "openChest";

    /// <summary>Gets or sets the sound to play when storage is placed.</summary>
    public string PlaceSound { get; set; } = "axe";

    /// <summary>Gets or sets a value indicating whether player color is enabled.</summary>
    public bool PlayerColor { get; set; }
}
