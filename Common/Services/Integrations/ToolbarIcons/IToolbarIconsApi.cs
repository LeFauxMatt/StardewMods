namespace StardewMods.Common.Services.Integrations.ToolbarIcons;

using Microsoft.Xna.Framework;
using StardewValley.Menus;

/// <summary>Public api to add icons above or below the toolbar.</summary>
public interface IToolbarIconsApi
{
    /// <summary>Event triggered when a toolbar icon is pressed.</summary>
    public event EventHandler<string> ToolbarIconPressed;

    /// <summary>Adds an icon next to the <see cref="Toolbar" />.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    /// <param name="texturePath">The path to the texture icon.</param>
    /// <param name="sourceRect">The source rectangle of the icon.</param>
    /// <param name="hoverText">Text to appear when hovering over the icon.</param>
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText);

    /// <summary>Removes an icon.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    public void RemoveToolbarIcon(string id);
}