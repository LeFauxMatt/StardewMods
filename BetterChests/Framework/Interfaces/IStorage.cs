namespace StardewMods.BetterChests.Framework.Interfaces;

using Microsoft.Xna.Framework;
using StardewValley.Mods;
using StardewValley.Network;

/// <summary>Represents a storage interface for a game object.</summary>
internal interface IStorage
{
    /// <summary>Gets the location of the game.</summary>
    GameLocation Location { get; }

    /// <summary>Gets the tile location of an object.</summary>
    Vector2 TileLocation { get; }

    /// <summary>Gets the mod data dictionary.</summary>
    ModDataDictionary ModData { get; }

    /// <summary>Gets the actual capacity of the storage.</summary>
    Dictionary<string, string>? CustomFields { get; }

    /// <summary>Gets the collection of items.</summary>
    IEnumerable<Item> Items { get; }

    /// <summary>Gets the mutex for the storage.</summary>
    NetMutex Mutex { get; }
}
