#nullable disable

namespace StardewMods.EasyAccess.Interfaces.ManagedObjects;

using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IGameObject" />
internal interface IManagedObject : IGameObject, IProducerData
{
    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> for items allowed into this producer type.
    /// </summary>
    public ItemMatcher ItemMatcherIn { get; }

    /// <summary>
    ///     Gets an <see cref="FuryCore.Helpers.ItemMatcher" /> for items allowed out of this producer type.
    /// </summary>
    public ItemMatcher ItemMatcherOut { get; }

    /// <summary>
    ///     Gets the Qualified Item Id of the storage object.
    /// </summary>
    public string QualifiedItemId { get; }
}