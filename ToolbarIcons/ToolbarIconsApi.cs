namespace StardewMods.ToolbarIcons;

using System;
using System.Collections.Generic;
using Common.Integrations.ToolbarIcons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;

/// <inheritdoc />
public class ToolbarIconsApi : IToolbarIconsApi
{
    private EventHandler<string>? _toolbarIconPressed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolbarIconsApi" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="icons">List containing toolbar icons.</param>
    public ToolbarIconsApi(IGameContentHelper helper, Dictionary<string, ClickableTextureComponent> icons)
    {
        this.Helper = helper;
        this.Icons = icons;
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add => this._toolbarIconPressed += value;
        remove => this._toolbarIconPressed -= value;
    }

    private IGameContentHelper Helper { get; }

    private Dictionary<string, ClickableTextureComponent> Icons { get; }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText)
    {
        var icon = new ClickableTextureComponent(
            new(0, 0, 32, 32),
            this.Helper.Load<Texture2D>(texturePath),
            sourceRect ?? new(0, 0, 16, 16),
            2f)
        {
            hoverText = hoverText,
            name = id,
        };

        this.Icons.Add(id, icon);
    }

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id)
    {
        this.Icons.Remove(id);
    }

    /// <summary>
    ///     Invokes all ToolbarIconPressed event handlers.
    /// </summary>
    /// <param name="id">The id of the toolbar icon pressed.</param>
    internal void Invoke(string id)
    {
        if (this._toolbarIconPressed is null)
        {
            return;
        }

        foreach (var handler in this._toolbarIconPressed.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(this, id);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}