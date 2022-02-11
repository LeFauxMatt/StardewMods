namespace StardewMods.FuryCore.Interfaces;

using System.Collections.Generic;
using StardewValley;

/// <summary>
///     Represents a game object that can store items.
/// </summary>
public interface IStorageContainer : IGameObject
{
    /// <summary>
    ///     Gets the items in the object's storage.
    /// </summary>
    IList<Item> Items { get; }
}