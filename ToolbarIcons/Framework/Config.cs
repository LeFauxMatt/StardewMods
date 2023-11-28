namespace StardewMods.ToolbarIcons.Framework;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Extensions;
using StardewMods.ToolbarIcons.Framework.UI;
using StardewValley.Menus;

/// <summary>
///     Handles config options.
/// </summary>
internal sealed class Config
{
#nullable disable
    private static Config instance;
#nullable enable

    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly ModConfig config;
    private readonly IModHelper helper;
    private readonly IManifest manifest;

    private EventHandler? toolbarIconsChanged;

    private Config(
        IModHelper helper,
        IManifest manifest,
        ModConfig config,
        Dictionary<string, ClickableTextureComponent> components)
    {
        this.helper = helper;
        this.manifest = manifest;
        this.config = config;
        this.components = components;

        Integrations.ToolbarIconsLoaded += this.OnToolbarIconsLoaded;
    }

    /// <summary>
    ///     Raised after Toolbar Icons have changed.
    /// </summary>
    public static event EventHandler ToolbarIconsChanged
    {
        add => Config.instance.toolbarIconsChanged += value;
        remove => Config.instance.toolbarIconsChanged -= value;
    }

    /// <summary>
    ///     Initializes <see cref="Config" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="config">Mod config data.</param>
    /// <param name="components">Dictionary containing the textures.</param>
    /// <returns>Returns an instance of the <see cref="Config" /> class.</returns>
    public static Config Init(
        IModHelper helper,
        IManifest manifest,
        ModConfig config,
        Dictionary<string, ClickableTextureComponent> components)
    {
        return Config.instance ??= new(helper, manifest, config, components);
    }

    private void DrawButton(SpriteBatch b, Vector2 pos)
    {
        var label = I18n.Config_OpenMenu_Name();
        var dims = Game1.dialogueFont.MeasureString(I18n.Config_OpenMenu_Name());
        var bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X + Game1.tileSize, Game1.tileSize);
        if (Game1.activeClickableMenu.GetChildMenu() is null)
        {
            var point = Game1.getMousePosition();
            if (Game1.oldMouseState.LeftButton == ButtonState.Released
                && Mouse.GetState().LeftButton == ButtonState.Pressed
                && bounds.Contains(point))
            {
                Game1.activeClickableMenu.SetChildMenu(new ToolbarIconsMenu(this.config.Icons, this.components));
                return;
            }
        }

        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new(432, 439, 9, 9),
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            1f);
        Utility.drawTextWithShadow(
            b,
            label,
            Game1.dialogueFont,
            new Vector2(bounds.Left + bounds.Right - dims.X, bounds.Top + bounds.Bottom - dims.Y) / 2f,
            Game1.textColor,
            1f,
            1f,
            -1,
            -1,
            0f);
    }

    private void OnToolbarIconsLoaded(object? sender, EventArgs e)
    {
        if (!Integrations.GMCM.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        Integrations.GMCM.Register(this.manifest, this.ResetConfig, this.SaveConfig);

        Integrations.GMCM.Api.AddComplexOption(
            this.manifest,
            I18n.Config_CustomizeToolbar_Name,
            this.DrawButton,
            I18n.Config_CustomizeToolbar_Tooltip,
            height: () => Game1.tileSize);
    }

    private void ResetConfig()
    {
        this.config.Scale = 2;
        foreach (var icon in this.config.Icons)
        {
            icon.Enabled = true;
        }

        this.config.Icons.Sort((i1, i2) => string.Compare(i1.Id, i2.Id, StringComparison.OrdinalIgnoreCase));
        this.toolbarIconsChanged.InvokeAll(this);
    }

    private void SaveConfig()
    {
        this.helper.WriteConfig(this.config);
        this.toolbarIconsChanged.InvokeAll(this);
    }
}