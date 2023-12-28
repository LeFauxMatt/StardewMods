namespace StardewMods.ToolbarIcons.Framework.Enums;

using NetEscapades.EnumGenerators;
using StardewValley.Menus;

/// <summary>The type of mod integration.</summary>
[EnumExtensions]
internal enum IntegrationType
{
    /// <summary>Opens an <see cref="IClickableMenu" /> from the mod..</summary>
    Menu,

    /// <summary>Invokes a method from the mod.</summary>
    Method,

    /// <summary>Issue a keybind.</summary>
    Keybind,
}