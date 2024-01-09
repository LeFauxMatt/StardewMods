namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewMods.ToolbarIcons.Framework.UI;
using StardewValley.Menus;

/// <summary>Handles generic mod config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        Dictionary<string, ClickableTextureComponent> components,
        IEventSubscriber eventSubscriber,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IManifest manifest,
        IModHelper modHelper)
        : base(modHelper)
    {
        this.manifest = manifest;
        this.components = components;
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;

        eventSubscriber.Subscribe<ToolbarIconsLoadedEventArgs>(this.OnToolbarIconsLoaded);
    }

    /// <inheritdoc />
    public List<ToolbarIcon> Icons => this.Config.Icons;

    /// <inheritdoc />
    public float Scale => this.Config.Scale;

    /// <inheritdoc />
    public override DefaultConfig GetDefault()
    {
        var defaultConfig = base.GetDefault();

        // Add icons to config with default sorting
        defaultConfig.Icons.Sort((i1, i2) => string.Compare(i1.Id, i2.Id, StringComparison.OrdinalIgnoreCase));

        return defaultConfig;
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
                Game1.activeClickableMenu.SetChildMenu(new ToolbarIconsMenu(this.Config.Icons, this.components));
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

    private void OnToolbarIconsLoaded(ToolbarIconsLoadedEventArgs e)
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();

        // Register mod configuration
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        gmcm.AddComplexOption(
            this.manifest,
            I18n.Config_CustomizeToolbar_Name,
            this.DrawButton,
            I18n.Config_CustomizeToolbar_Tooltip,
            height: () => 64);
    }
}