namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.UI;
using StardewValley.Menus;

/// <summary>Handles generic mod config menu.</summary>
internal sealed class ConfigMenuManager
{
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly ModConfig modConfig;
    private readonly EventsManager eventsManager;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IModHelper modHelper;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigMenuManager" /> class.</summary>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="eventsManager">Dependency used for custom events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ConfigMenuManager(
        IModHelper modHelper,
        ModConfig modConfig,
        Dictionary<string, ClickableTextureComponent> components,
        EventsManager eventsManager,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IManifest manifest)
    {
        this.modHelper = modHelper;
        this.manifest = manifest;
        this.modConfig = modConfig;
        this.components = components;
        this.eventsManager = eventsManager;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;

        eventsManager.ToolbarIconsLoaded += this.OnToolbarIconsLoaded;
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
                Game1.activeClickableMenu.SetChildMenu(new ToolbarIconsMenu(this.modConfig.Icons, this.components));
                return;
            }
        }

        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(432, 439, 9, 9),
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
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        this.genericModConfigMenuIntegration.Api.Register(this.manifest, this.ResetConfig, this.SaveConfig);

        this.genericModConfigMenuIntegration.Api.AddComplexOption(
            this.manifest,
            I18n.Config_CustomizeToolbar_Name,
            this.DrawButton,
            I18n.Config_CustomizeToolbar_Tooltip,
            height: () => 64);
    }

    private void ResetConfig()
    {
        this.modConfig.Scale = 2;
        foreach (var icon in this.modConfig.Icons)
        {
            icon.Enabled = true;
        }

        this.modConfig.Icons.Sort((i1, i2) => string.Compare(i1.Id, i2.Id, StringComparison.OrdinalIgnoreCase));
        this.eventsManager.InvokeToolbarIconsChanged();
    }

    private void SaveConfig()
    {
        this.modHelper.WriteConfig(this.modConfig);
        this.eventsManager.InvokeToolbarIconsChanged();
    }
}