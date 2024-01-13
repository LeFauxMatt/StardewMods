namespace StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Specifies the different draw methods for rendering.</summary>
public enum DrawMethod
{
    /// <summary>The method when an item is drawn in a menu.</summary>
    Menu,

    /// <summary>The method when an item is drawn while being held.</summary>
    Held,

    /// <summary>The method when an object is drawn in the world.</summary>
    World,

    /// <summary>The method for a building which has DrawInBackground enabled.</summary>
    Background,

    /// <summary>The method for a building in construction.</summary>
    Construction,

    /// <summary>The method for drawing a shadow.</summary>
    Shadow,
}