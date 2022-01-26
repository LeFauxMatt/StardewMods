namespace BetterChests.Interfaces;

using FuryCore.Helpers;

/// <inheritdoc />
internal interface IChestModel : IChestData
{
    /// <summary>
    /// Gets an <see cref="ItemMatcher" /> that is configured for each <see cref="IChestData" />.
    /// </summary>
    public ItemMatcher ItemMatcherByType { get; }
}