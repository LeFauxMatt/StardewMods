namespace StardewMods.Common.Integrations.ExpandedStorage;

using Microsoft.Xna.Framework.Graphics;

/// <summary>
///     Interface to a chest managed by Expanded Storage.
/// </summary>
public interface IManagedStorage : ICustomStorage
{
    /// <summary>
    ///     Gets the sprite sheet texture.
    /// </summary>
    public Texture2D Texture { get; }
}