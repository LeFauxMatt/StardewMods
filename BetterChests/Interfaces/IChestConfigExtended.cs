namespace BetterChests.Interfaces;

using FuryCore.Helpers;

/// <summary>
///
/// </summary>
internal interface IChestConfigExtended : IChestConfig
{
    /// <summary>
    ///
    /// </summary>
    public ItemMatcher ItemMatcher { get; }
}