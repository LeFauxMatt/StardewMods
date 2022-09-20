namespace StardewMods.Common.Integrations.ExpandedStorage;

using System.Collections.Generic;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Objects;

/// <summary>
///     Data for an ExpandedStorage chest.
/// </summary>
public interface ICustomStorage
{
    /// <summary>
    ///     Gets or sets data for integration with Better Chests.
    /// </summary>
    IStorageData? BetterChestsData { get; set; }

    /// <summary>
    ///     Gets or sets the sound to play when the lid closing animation plays.
    /// </summary>
    string CloseNearbySound { get; set; }

    /// <summary>
    ///     Gets or sets the pixel depth.
    /// </summary>
    int Depth { get; set; }

    /// <summary>
    ///     Gets or sets the description.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    ///     Gets or sets the display name.
    /// </summary>
    string DisplayName { get; set; }

    /// <summary>
    ///     Gets or sets the pixel height.
    /// </summary>
    int Height { get; set; }

    /// <summary>
    ///     Gets or sets the texture path.
    /// </summary>
    string Image { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the storage is a fridge.
    /// </summary>
    bool IsFridge { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the storage can be placed.
    /// </summary>
    bool IsPlaceable { get; set; }

    /// <summary>
    ///     Gets or sets additional mod data that will be added to storage when it is created.
    /// </summary>
    IDictionary<string, string> ModData { get; set; }

    /// <summary>
    ///     Gets or sets the distance from the player that the storage will play it's lid opening animation.
    /// </summary>
    float OpenNearby { get; set; }

    /// <summary>
    ///     Gets or sets the sound to play when the lid opening animation plays.
    /// </summary>
    string OpenNearbySound { get; set; }

    /// <summary>
    ///     Gets or sets the sound to play when the storage is opened.
    /// </summary>
    string OpenSound { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether player color is enabled.
    /// </summary>
    bool PlayerColor { get; set; }

    /// <summary>
    ///     Gets or sets the special chest type.
    /// </summary>
    Chest.SpecialChestTypes SpecialChestType { get; set; }

    /// <summary>
    ///     Gets or sets the pixel width.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    ///     Copies all properties to another instance of <see cref="ICustomStorage" />.
    /// </summary>
    /// <param name="other">The other <see cref="ICustomStorage" /> to copy properties to.</param>
    public void CopyTo(ICustomStorage other)
    {
        other.BetterChestsData = this.BetterChestsData;
        other.Depth = this.Depth;
        other.Description = this.Description;
        other.DisplayName = this.DisplayName;
        other.Height = this.Height;
        other.Image = this.Image;
        other.IsFridge = this.IsFridge;
        other.IsPlaceable = this.IsPlaceable;
        other.OpenSound = this.OpenSound;
        other.OpenNearby = this.OpenNearby;
        other.OpenNearbySound = this.OpenNearbySound;
        other.CloseNearbySound = this.CloseNearbySound;
        other.ModData = this.ModData;
        other.PlayerColor = this.PlayerColor;
        other.SpecialChestType = this.SpecialChestType;
        other.Width = this.Width;
    }
}