#nullable disable

namespace StardewMods.BetterChests.Interfaces.ManagedObjects;

using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IGameObject" />
internal interface IManagedObject : IGameObject, IStorageData
{
    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> that is assigned to each storage type.
    /// </summary>
    public ItemMatcher ItemMatcher { get; }

    /// <summary>
    ///     Gets the Qualified Item Id of the storage object.
    /// </summary>
    public string QualifiedItemId { get; }
}