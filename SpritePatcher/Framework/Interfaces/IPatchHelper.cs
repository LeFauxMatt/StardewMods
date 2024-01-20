namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework.Graphics;

/// <summary>The Helper class provides useful methods for performing common operations.</summary>
public interface IPatchHelper
{
    /// <summary>Invalidates the cached texture of the target sprite sheet.</summary>
    /// <param name="field">The field to monitor.</param>
    /// <param name="eventName">The name of the event..</param>
    void InvalidateCacheOnChanged(object field, string eventName);

    /// <summary>Returns the index of the first occurrence of the specified value in the given array of strings.</summary>
    /// <param name="input">The input string to split.</param>
    /// <param name="value">The value to locate.</param>
    /// <param name="separator">The character used to separate the substrings. The default value is ','.</param>
    /// <returns>The index of the first occurrence of the specified value in the array, if found; otherwise, -1.</returns>
    int GetIndexFromString(string input, string value, char separator = ',');

    /// <summary>Sets the texture of an object using the specified texture.</summary>
    /// <param name="texture">The texture.</param>
    void SetTexture(Texture2D texture);

    /// <summary>Sets the texture of an object using the specified path.</summary>
    /// <param name="path">The path of the texture.</param>
    /// <param name="index">The index of the icon within the texture.</param>
    /// <param name="width">The width of each icon within the texture. Default value is 16.</param>
    /// <param name="height">The height of each icon within the texture. Default value is 16.</param>
    void SetTexture(string path, int index = 0, int width = 16, int height = 16);

    /// <summary>Logs a message with the specified information.</summary>
    /// <param name="message">The message to be logged.</param>
    void Log(string message);
}