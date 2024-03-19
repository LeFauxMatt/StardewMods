namespace StardewMods.SpritePatcher.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Specifies the different draw methods for rendering.</summary>
[EnumExtensions]
public enum DrawMethod
{
    /// <summary>The method for a building which has DrawInBackground enabled.</summary>
    Background,

    /// <summary>The method for a building in construction.</summary>
    Construction,

    /// <summary>The method when an item is drawn while being held.</summary>
    Held,

    /// <summary>The method when an item is drawn in a menu.</summary>
    Menu,

    /// <summary>The method for drawing a shadow.</summary>
    Shadow,

    /// <summary>The method when an object is drawn in the world.</summary>
    World,
}