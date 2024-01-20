namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents a key used to identify a texture and its drawing parameters.</summary>
public readonly record struct TextureKey(string Target, Rectangle Area, DrawMethod DrawMethod);