namespace StardewMods.Common.Integrations.OrdinaryCapsule;

using System.Collections.Generic;

/// <summary>
///     Represents an item that can be duplicated in an Ordinary Capsule.
/// </summary>
public interface ICapsuleItem
{
    /// <summary>
    ///     Gets or sets the tag to identify supported item(s).
    /// </summary>
    public HashSet<string> ContextTags { get; set; }

    /// <summary>
    ///     Gets or sets the time between duplicating the item.
    /// </summary>
    public int ProductionTime { get; set; }

    /// <summary>
    ///     Gets or sets the sound to play when the item is added.
    /// </summary>
    public string? Sound { get; set; }
}