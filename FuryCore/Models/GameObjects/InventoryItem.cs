namespace StardewMods.FuryCore.Models.GameObjects;

using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;

/// <summary>
///     Represents a <see cref="IGameObject" /> that is in a player's inventory.
/// </summary>
/// <param name="Player">The player whose inventory has the object.</param>
/// <param name="Index">The item slot where the object is held.</param>
public readonly record struct InventoryItem(Farmer Player, int Index) : IGameObjectType;