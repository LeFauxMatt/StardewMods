namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Models;

/// <summary>Build and cache textures from patch layers.</summary>
public interface ITextureManager
{
    /// <summary>Tries to get the texture data from the specified game path.</summary>
    /// <param name="path">The path of the texture.</param>
    /// <param name="texture">
    /// When this method returns, contains the texture data if successful, or null if the texture could
    /// not be found. This parameter is passed uninitialized.
    /// </param>
    /// <returns><c>true</c> if the texture data is successfully retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetTexture(string path, [NotNullWhen(true)] out IRawTextureData? texture);

    /// <summary>Tries to build a texture by combining multiple texture layers.</summary>
    /// <param name="key">A key for the original texture method.</param>
    /// <param name="baseTexture">The base texture to use as a background.</param>
    /// <param name="layers">The list of texture layers to combine.</param>
    /// <param name="managedTexture">When this method returns, contains the built texture if successful, otherwise null.</param>
    /// <returns>True if the texture was successfully built, otherwise false.</returns>
    public bool TryBuildTexture(
        TextureKey key,
        Texture2D baseTexture,
        List<IPatchModel> layers,
        [NotNullWhen(true)] out IManagedTexture? managedTexture);
}