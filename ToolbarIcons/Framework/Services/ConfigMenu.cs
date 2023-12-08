namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.UI;
using StardewValley.Menus;

/// <summary>Handles generic mod config menu.</summary>
internal sealed class ConfigMenu
{
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly ModConfig config;
    private readonly EventsManager customEvents;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IModHelper helper;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigMenu" /> class.</summary>
    /// <param name="helper">Dependency for events, input, and content.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="customEvents">Dependency used for custom events.</param>
    /// <param name="gmcm">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ConfigMenu(
        IModHelper helper,
        ModConfig config,
        Dictionary<string, ClickableTextureComponent> components,
        EventsManager customEvents,
        GenericModConfigMenuIntegration gmcm,
        IManifest manifest)
    {
        this.helper = helper;
        this.manifest = manifest;
        this.config = config;
        this.components = components;
        this.customEvents = customEvents;
        this.gmcm = gmcm;

        customEvents.ToolbarIconsLoaded += this.OnToolbarIconsLoaded;
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
            new Vector2((bounds.Left + bounds.Right) - dims.X, (bounds.Top + bounds.Bottom) - dims.Y) / 2f,
            Game1.textColor,
            1f,
            1f,
            -1,
            -1,
            0f);
    }

    private void OnToolbarIconsLoaded(object? sender, EventArgs e)
    {
        if (!this.gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        this.gmcm.Register(this.manifest, this.ResetConfig, this.SaveConfig);

        this.gmcm.Api.AddComplexOption(
            this.manifest,
            I18n.Config_CustomizeToolbar_Name,
            this.DrawButton,
            I18n.Config_CustomizeToolbar_Tooltip,
            height: () => 64);
    }

    private void ResetConfig()
    {
        this.config.Scale = 2;
        foreach (var icon in this.config.Icons)
        {
            icon.Enabled = true;
        }

        this.config.Icons.Sort((i1, i2) => string.Compare(i1.Id, i2.Id, StringComparison.OrdinalIgnoreCase));
        this.customEvents.InvokeToolbarIconsChanged();
    }

    private void SaveConfig()
    {
        this.helper.WriteConfig(this.config);
        this.customEvents.InvokeToolbarIconsChanged();
    }
}
