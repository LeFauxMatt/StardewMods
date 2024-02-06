namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewValley.ItemTypeDefinitions;

/// <summary>The Helper class provides useful methods for performing common operations.</summary>
public interface IPatchHelper
{
    // Get access to current RawTexture data
    // Handle working canvas as a RawTexture
    // Cache each texture based on signature of layers
    // Commands/property setters are treated as layers

    /// <summary>Sets the texture of an object using the specified texture.</summary>
    /// <param name="texture">The texture data.</param>
    /// <param name="sourceArea">The area within the source texture.</param>
    /// <param name="scale">The scale of the texture.</param>
    /// <param name="alpha">The alpha of the texture.</param>
    void ApplyTexture(
        Texture2D texture,
        Rectangle sourceArea,
        float scale = -1f,
        float alpha = -1f);

    /// <summary>Sets the texture of an object using the specified path.</summary>
    /// <param name="path">The path of the texture.</param>
    /// <param name="sourceArea">The area within the source texture.</param>
    /// <param name="scale">The scale of the texture.</param>
    /// <param name="alpha">The alpha of the texture.</param>
    void ApplyTexture(
        string? path,
        Rectangle sourceArea,
        float scale = -1f,
        float alpha = -1f);

    /// <summary>Sets the texture of an object using the specified texture.</summary>
    /// <param name="item">The item.</param>
    /// <param name="scale">The scale of the texture.</param>
    /// <param name="alpha">The alpha of the texture.</param>
    void ApplyTexture(Item? item, float scale = -1f, float alpha = -1f);

    /// <summary>Sets the texture of an object using the specified texture.</summary>
    /// <param name="data">The data.</param>
    /// <param name="scale">The scale of the texture.</param>
    /// <param name="alpha">The alpha of the texture.</param>
    void ApplyTexture(ParsedItemData data, float scale = -1f, float alpha = -1f);

    /// <summary>Returns the source area of a texture from the specified index.</summary>
    /// <param name="index">The index of the sprite within the texture.</param>
    /// <param name="width">The width of each sprite within the texture.</param>
    /// <param name="height">The height of each sprite within the texture.</param>
    /// <returns>The area corresponding to the specified index, width, and height.</returns>
    Rectangle GetAreaFromIndex(int index, int width = -1, int height = -1);

    /// <summary>Returns the index of the first occurrence of the specified value in the given array of strings.</summary>
    /// <param name="input">The input string to split.</param>
    /// <param name="value">The value to locate.</param>
    /// <param name="separator">The character used to separate the substrings. The default value is ','.</param>
    /// <returns>The index of the first occurrence of the specified value in the array, if found; otherwise, -1.</returns>
    int GetIndexFromString(string input, string value, char separator = ',');

    /// <summary>Gets the value with the specified key or add if it does not exist.</summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <param name="value">The value to set if the key does not exist.</param>
    /// <typeparam name="T">The type of value to return.</typeparam>
    /// <returns>The value associated with the specified key if the key.</returns>
    T GetOrSetData<T>(string key, T value);

    /// <summary>Get and monitor the provided entity's heldObject value.</summary>
    /// <param name="entity">The entity with the heldObject.</param>
    /// <returns>The entity's heldObject value.</returns>
    (SObject Object, ParsedItemData Data) GetHeldObject(IHaveModData? entity = null);

    /// <summary>Get and monitor the provided entity's lastInputItem value.</summary>
    /// <param name="entity">The entity with the lastInputItem.</param>
    /// <returns>The entity's lastInputItem value.</returns>
    (Item Item, ParsedItemData Data) GetLastInputItem(IHaveModData? entity = null);

    /// <summary>Get and monitor the provided entity's neighbor values..</summary>
    /// <param name="entity">The entity to get the neighbor of.</param>
    /// <returns>The entity's neighbor values.</returns>
    Dictionary<Direction, SObject?> GetNeighbors(IHaveModData? entity = null);

    /// <summary>Get and monitor the provided entity's preserve value.</summary>
    /// <param name="entity">The entity with the preserve.</param>
    /// <returns>The entity's preserve value.</returns>
    ParsedItemData GetPreserve(IHaveModData? entity = null);

    /// <summary>Invalidates the cached texture of the target sprite sheet.</summary>
    /// <param name="field">The field to monitor.</param>
    /// <param name="eventName">The name of the event..</param>
    void InvalidateCacheOnChanged(object field, string eventName);

    /// <summary>Logs a message with the specified information.</summary>
    /// <param name="message">The message to be logged.</param>
    void Log(string message);

    /// <summary>
    /// Sets the animation for the specified <paramref name="animate" /> with the given number of
    /// <paramref name="frames" />.
    /// </summary>
    /// <param name="animate">The animate object to set animation for.</param>
    /// <param name="frames">The number of frames in the animation.</param>
    void SetAnimation(Animate animate, int frames);
}