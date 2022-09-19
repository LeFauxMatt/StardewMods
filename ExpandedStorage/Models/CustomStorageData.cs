namespace StardewMods.ExpandedStorage.Models;

using System.Collections.Generic;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewValley.Objects;

/// <inheritdoc />
internal class CustomStorageData : ICustomStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomStorageData" /> class.
    /// </summary>
    /// <param name="betterChestsData">Storage data for Better Chests integration.</param>
    [SuppressMessage(
        "ReSharper",
        "SuggestBaseTypeForParameterInConstructor",
        Justification = "Required to deserialize")]
    public CustomStorageData(BetterChestsData? betterChestsData)
    {
        this.BetterChestsData = betterChestsData;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CustomStorageData" /> class.
    /// </summary>
    protected CustomStorageData()
    {
        // None
    }

    /// <inheritdoc />
    public IStorageData? BetterChestsData { get; set; }

    /// <inheritdoc />
    public string CloseNearbySound { get; set; } = "doorCreakReverse";

    /// <inheritdoc />
    public int Depth { get; set; } = 16;

    /// <inheritdoc />
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public string DisplayName { get; set; } = string.Empty;

    /// <inheritdoc />
    public int Height { get; set; } = 32;

    /// <inheritdoc />
    public string Image { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool IsFridge { get; set; } = false;

    /// <inheritdoc />
    public bool IsPlaceable { get; set; }

    /// <inheritdoc />
    public IDictionary<string, string> ModData { get; set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public float OpenNearby { get; set; } = 0;

    /// <inheritdoc />
    public string OpenNearbySound { get; set; } = "doorCreak";

    /// <inheritdoc />
    public string OpenSound { get; set; } = "openChest";

    /// <inheritdoc />
    public bool PlayerColor { get; set; } = false;

    /// <inheritdoc />
    public Chest.SpecialChestTypes SpecialChestType { get; set; } = Chest.SpecialChestTypes.None;

    /// <inheritdoc />
    public int Width { get; set; } = 16;
}