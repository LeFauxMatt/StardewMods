namespace StardewMods.SpritePatcher.Framework.Interfaces;

using StardewMods.SpritePatcher.Framework.Enums;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

/// <summary>The Helper class provides useful methods for performing common operations.</summary>
public interface IPatchHelper
{
    /// <summary>Logs a message with the specified information.</summary>
    /// <param name="message">The message to be logged.</param>
    void Log(string message);

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

    /// <summary>
    /// Sets the animation for the specified <paramref name="animate" /> with the given number of
    /// <paramref name="frames" />.
    /// </summary>
    /// <param name="animate">The animate object to set animation for.</param>
    /// <param name="frames">The number of frames in the animation.</param>
    void SetAnimation(Animate animate, int frames);

    /// <summary>Sets the texture of an object using the specified texture.</summary>
    /// <param name="data">The data.</param>
    /// <param name="scale">The scale of the texture.</param>
    void SetTexture(ParsedItemData data, float scale = -1f);

    /// <summary>Sets the texture of an object using the specified path.</summary>
    /// <param name="path">The path of the texture.</param>
    /// <param name="index">The index of the icon within the texture.</param>
    /// <param name="width">The width of each icon within the texture. Default value is 16.</param>
    /// <param name="height">The height of each icon within the texture. Default value is 16.</param>
    /// <param name="scale">The scale of the texture.</param>
    void SetTexture(string path, int index = 0, int width = 16, int height = 16, float scale = -1f);

    /// <summary>Invokes the specified action with the provided entity's heldObject value.</summary>
    /// <param name="entity">The entity with the heldObject.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="monitor">Whether to monitor the field for changes or not.</param>
    public void WithHeldObject(IHaveModData entity, Action<SObject, ParsedItemData> action, bool monitor = false);

    /// <summary>Invokes the specified action with the provided entity's lastInputItem value.</summary>
    /// <param name="entity">The entity with the lastInputItem.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="monitor">Whether to monitor the field for changes or not.</param>
    public void WithLastInputItem(IHaveModData entity, Action<Item, ParsedItemData> action, bool monitor = false);

    /// <summary>Invokes the specified action with the provided entity's neighbor values..</summary>
    /// <param name="entity">The entity to get the neighbor of.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="monitor">Whether to monitor the field for changes or not.</param>
    public void WithNeighbors(
        IHaveModData entity,
        Action<Dictionary<Direction, SObject?>> action,
        bool monitor = false);

    /// <summary>Invokes the specified action with the provided entity's preserve value.</summary>
    /// <param name="entity">The entity with the preserve.</param>
    /// <param name="action">The action to be invoked.</param>
    /// <param name="monitor">Whether to monitor the field for changes or not.</param>
    public void WithPreserve(IHaveModData entity, Action<ParsedItemData> action, bool monitor = false);
}