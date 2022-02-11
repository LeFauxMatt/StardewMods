namespace StardewMods.EasyAccess.Interfaces.ManagedObjects;

using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IGameObject" />
internal interface IManagedObject : IGameObject, IProducerData
{
    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> that is assigned to each producer type.
    /// </summary>
    public ItemMatcher ItemMatcher { get; }

    /// <summary>
    ///     Gets the Qualified Item Id of the storage object.
    /// </summary>
    public string QualifiedItemId { get; }
}