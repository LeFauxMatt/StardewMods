namespace StardewMods.Common.Services.Integrations.ExpandedStorage;

/// <summary>Data for an Expanded Storage chest.</summary>
public interface IStorageData
{
    /// <summary>Gets or sets the sound to play when the lid closing animation plays.</summary>
    public string CloseNearbySound { get; set; }

    /// <summary>Gets or sets the number of frames in the lid animation.</summary>
    public int Frames { get; set; }

    /// <summary>Gets or sets a value indicating whether the storage is a fridge.</summary>
    public bool IsFridge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the storage will play it's lid opening animation when the player is
    /// nearby.
    /// </summary>
    public bool OpenNearby { get; set; }

    /// <summary>Gets or sets the sound to play when the lid opening animation plays.</summary>
    public string OpenNearbySound { get; set; }

    /// <summary>Gets or sets the sound to play when the storage is opened.</summary>
    public string OpenSound { get; set; }

    /// <summary>Gets or sets the sound to play when storage is placed.</summary>
    public string PlaceSound { get; set; }

    /// <summary>Gets or sets a value indicating whether player color is enabled.</summary>
    public bool PlayerColor { get; set; }
}