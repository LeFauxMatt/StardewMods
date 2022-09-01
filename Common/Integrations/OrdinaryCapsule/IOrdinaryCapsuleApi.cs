namespace StardewMods.Common.Integrations.OrdinaryCapsule;

using System.Collections.Generic;

/// <summary>
///     API for Ordinary Capsule.
/// </summary>
public interface IOrdinaryCapsuleApi
{
    /// <summary>
    ///     Registers an item for use with Ordinary Capsule.
    /// </summary>
    /// <param name="contextTags">Tag(s) to identify the item.</param>
    /// <param name="productionTime">The time between each item duplication.</param>
    /// <param name="sound">The sound to play when item is loaded or collected.</param>
    public void RegisterItem(HashSet<string> contextTags, int productionTime, string? sound);
}