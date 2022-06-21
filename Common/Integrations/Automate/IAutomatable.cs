namespace StardewMods.Common.Integrations.Automate;

using Microsoft.Xna.Framework;
using StardewValley;

/// <summary>
///     An automatable entity, which can implement a more specific type like <see cref="IMachine"/> or <see cref="IContainer"/>.
///     If it doesn't implement a more specific type, it's treated as a connector with no additional logic.
/// </summary>
public interface IAutomatable
{
    /*********
    ** Accessors
    *********/

    /// <summary>Gets the location which contains the machine.</summary>
    GameLocation Location { get; }

    /// <summary>Gets the tile area covered by the machine.</summary>
    Rectangle TileArea { get; }
}