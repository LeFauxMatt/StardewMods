namespace StardewMods.BetterChests.Interfaces;

/// <inheritdoc />
internal interface IChestModel : IChestData
{
    /// <summary>
    /// Gets the name of the chest.
    /// </summary>
    public string Name { get; }
}