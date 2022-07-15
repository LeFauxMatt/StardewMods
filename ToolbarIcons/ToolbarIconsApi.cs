namespace StardewMods.ToolbarIcons;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Models;
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
    /// <param name="components">Dictionary containing the textures.</param>
    public ToolbarIconsApi(IModHelper helper, List<ToolbarIcon> icons, Dictionary<string, ClickableTextureComponent> components)
    {
        this.Helper = helper;
        this.Icons = icons;
        this.Components = components;
    }

    /// <summary>
    ///     Raised after a toolbar icon is pressed.
    /// </summary>
    public event EventHandler<string> ToolbarIconPressed
    {
        add => this._toolbarIconPressed += value;
        remove => this._toolbarIconPressed -= value;
    }

    private Dictionary<string, ClickableTextureComponent> Components { get; }

    private IModHelper Helper { get; }

    private List<ToolbarIcon> Icons { get; }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText)
    {
        var icon = this.Icons.FirstOrDefault(icon => icon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (icon is null)
        {
            icon = new(id);
            this.Icons.Add(icon);
        }

        if (!this.Components.ContainsKey(id))
        {
            Log.Trace($"Adding icon: {id}");
            this.Components.Add(
                id,
                new(
                    new(0, 0, 32, 32),
                    this.Helper.GameContent.Load<Texture2D>(texturePath),
                    sourceRect ?? new(0, 0, 16, 16),
                    2f)
                {
                    hoverText = hoverText,
                    name = id,
                    visible = icon.Enabled,
                });
        }
    }

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id)
    {
        var toolbarIcon = this.Icons.FirstOrDefault(toolbarIcon => toolbarIcon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (toolbarIcon is not null)
        {
            Log.Trace($"Removing icon: {id}");
            this.Icons.Remove(toolbarIcon);
            this.Components.Remove(id);
        }
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