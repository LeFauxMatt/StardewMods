namespace StardewMods.ToolbarIcons.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewValley.Menus;

/// <inheritdoc />
public sealed class Api : IToolbarIconsApi
{
    private readonly Dictionary<string, ClickableTextureComponent> _components;
    private readonly IModHelper _helper;
    private readonly List<ToolbarIcon> _icons;

    private EventHandler<string>? _toolbarIconPressed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Api" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="icons">List containing toolbar icons.</param>
    /// <param name="components">Dictionary containing the textures.</param>
    public Api(IModHelper helper, List<ToolbarIcon> icons, Dictionary<string, ClickableTextureComponent> components)
    {
        this._helper = helper;
        this._icons = icons;
        this._components = components;
    }

    /// <summary>
    ///     Raised after a toolbar icon is pressed.
    /// </summary>
    public event EventHandler<string> ToolbarIconPressed
    {
        add => this._toolbarIconPressed += value;
        remove => this._toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText)
    {
        var icon = this._icons.FirstOrDefault(icon => icon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (icon is null)
        {
            icon = new(id);
            this._icons.Add(icon);
        }

        if (this._components.ContainsKey(id))
        {
            return;
        }

        Log.Trace($"Adding icon: {id}");
        this._components.Add(
            id,
            new(
                new(0, 0, 32, 32),
                this._helper.GameContent.Load<Texture2D>(texturePath),
                sourceRect ?? new(0, 0, 16, 16),
                2f)
            {
                hoverText = hoverText,
                name = id,
                visible = icon.Enabled,
            });
    }

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id)
    {
        var toolbarIcon =
            this._icons.FirstOrDefault(toolbarIcon => toolbarIcon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (toolbarIcon is null)
        {
            return;
        }

        Log.Trace($"Removing icon: {id}");
        this._icons.Remove(toolbarIcon);
        this._components.Remove(id);
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