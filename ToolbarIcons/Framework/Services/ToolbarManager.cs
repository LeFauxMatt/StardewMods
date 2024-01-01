namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewValley.Menus;

// TODO: Center Toolbar Icons

/// <summary>Service for handling the toolbar icons on the screen.</summary>
internal sealed class ToolbarManager : BaseService
{
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly IModConfig modConfig;
    private readonly PerScreen<string> currentHoverText = new();
    private readonly EventsManager eventsManager;
    private readonly IModEvents modEvents;
    private readonly IGameContentHelper gameContentHelper;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<ComponentArea> lastArea = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent> lastButton = new();
    private readonly PerScreen<Toolbar> lastToolbar = new();
    private readonly AssetHandler assetHandler;
    private readonly ILog log;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ToolbarManager" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="eventsManager">Dependency used for custom events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public ToolbarManager(
        AssetHandler assetHandler,
        Dictionary<string, ClickableTextureComponent> components,
        EventsManager eventsManager,
        IGameContentHelper gameContentHelper,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        // Init
        this.assetHandler = assetHandler;
        this.components = components;
        this.eventsManager = eventsManager;
        this.gameContentHelper = gameContentHelper;
        this.inputHelper = inputHelper;
        this.log = log;
        this.modConfig = modConfig;
        this.modEvents = modEvents;
        this.reflectionHelper = reflectionHelper;

        // Events
        eventsManager.ToolbarIconsLoaded += this.OnToolbarIconsLoaded;
        eventsManager.ToolbarIconsChanged += this.OnToolbarIconsChanged;
    }

    private static bool ShowToolbar =>
        Game1.displayHUD
        && Context.IsPlayerFree
        && Game1.activeClickableMenu is null
        && Game1.onScreenMenus.OfType<Toolbar>().Any();

    /// <summary>Adds an icon next to the <see cref="Toolbar" />.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    /// <param name="texturePath">The path to the texture icon.</param>
    /// <param name="sourceRect">The source rectangle of the icon.</param>
    /// <param name="hoverText">Text to appear when hovering over the icon.</param>
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText)
    {
        var icon = this.modConfig.Icons.FirstOrDefault(icon => icon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (icon is null)
        {
            icon = new ToolbarIcon(id);
            this.modConfig.Icons.Add(icon);
        }

        if (this.components.ContainsKey(id))
        {
            return;
        }

        this.log.Trace("Adding icon: {0}", [id]);
        this.components.Add(
            id,
            new ClickableTextureComponent(
                new Rectangle(0, 0, 32, 32),
                this.gameContentHelper.Load<Texture2D>(texturePath),
                sourceRect ?? new Rectangle(0, 0, 16, 16),
                2f)
            {
                hoverText = hoverText,
                name = id,
                visible = icon.Enabled,
            });
    }

    /// <summary>Removes an icon.</summary>
    /// <param name="id">A unique identifier for the icon.</param>
    public void RemoveToolbarIcon(string id)
    {
        var toolbarIcon = this.modConfig.Icons.FirstOrDefault(
            toolbarIcon => toolbarIcon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (toolbarIcon is null)
        {
            return;
        }

        this.log.Trace("Removing icon: {0}", [id]);
        this.modConfig.Icons.Remove(toolbarIcon);
        this.components.Remove(id);
    }

    private bool TryGetButton([NotNullWhen(true)] out ClickableComponent? button)
    {
        var activeToolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
        if (this.lastToolbar.IsActiveForScreen() && activeToolbar == this.lastToolbar.Value)
        {
            button = this.lastButton.Value;
            return true;
        }

        if (activeToolbar is null)
        {
            button = null;
            return false;
        }

        this.lastToolbar.Value = activeToolbar;
        var buttons = this.reflectionHelper.GetField<List<ClickableComponent>>(activeToolbar, "buttons").GetValue();
        button = this.lastButton.Value = buttons.First();
        return true;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar || this.inputHelper.IsSuppressed(e.Button))
        {
            return;
        }

        if (e.Button is not (SButton.MouseLeft or SButton.MouseRight)
            && !(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component =
            this.components.Values.FirstOrDefault(component => component.visible && component.containsPoint(x, y));

        if (component is null)
        {
            return;
        }

        Game1.playSound("drumkit6");
        this.eventsManager.InvokeToolbarIconPressed(component.name, e.Button);
        this.inputHelper.Suppress(e.Button);
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.currentHoverText.Value = string.Empty;
        foreach (var component in this.components.Values.Where(component => component.visible))
        {
            component.tryHover(x, y);
            if (component.bounds.Contains(x, y))
            {
                this.currentHoverText.Value = component.hoverText;
            }
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(this.currentHoverText.Value))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.currentHoverText.Value, Game1.smallFont);
        }
    }

    private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
    {
        if (!ToolbarManager.ShowToolbar)
        {
            return;
        }

        this.ReorientComponents();

        foreach (var component in this.components.Values.Where(component => component.visible))
        {
            e.SpriteBatch.Draw(
                this.gameContentHelper.Load<Texture2D>(this.assetHandler.IconPath),
                new Vector2(component.bounds.X, component.bounds.Y),
                new Rectangle(0, 0, 16, 16),
                Color.White,
                0f,
                Vector2.Zero,
                2f,
                SpriteEffects.None,
                1f);

            component.draw(e.SpriteBatch);
        }
    }

    private void OnToolbarIconsChanged(object? sender, EventArgs e)
    {
        foreach (var icon in this.modConfig.Icons)
        {
            if (this.components.TryGetValue(icon.Id, out var component))
            {
                component.visible = icon.Enabled;
            }
        }

        this.ReorientComponents();
    }

    private void OnToolbarIconsLoaded(object? sender, EventArgs e)
    {
        // Init
        this.ReorientComponents();

        // Events
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.CursorMoved += this.OnCursorMoved;
        this.modEvents.Display.RenderedHud += this.OnRenderedHud;
        this.modEvents.Display.RenderingHud += this.OnRenderingHud;
    }

    private void ReorientComponents()
    {
        if (!this.TryGetButton(out var button) || this.components.Values.All(component => !component.visible))
        {
            return;
        }

        var xAlign = button.bounds.X < Game1.viewport.Width / 2;
        var yAlign = button.bounds.Y < Game1.viewport.Height / 2;
        ComponentArea area;
        int x;
        int y;
        if (this.lastToolbar.Value.width > this.lastToolbar.Value.height)
        {
            x = button.bounds.Left;
            if (yAlign)
            {
                area = ComponentArea.Top;
                y = button.bounds.Bottom + 20;
            }
            else
            {
                area = ComponentArea.Bottom;
                y = button.bounds.Top - 52;
            }
        }
        else
        {
            y = button.bounds.Top;
            if (xAlign)
            {
                area = ComponentArea.Left;
                x = button.bounds.Right + 20;
            }
            else
            {
                area = ComponentArea.Right;
                x = button.bounds.Left - 52;
            }
        }

        var firstComponent = this.components.Values.First(component => component.visible);
        if (!this.lastArea.IsActiveForScreen()
            || area != this.lastArea.Value
            || firstComponent.bounds.X != x
            || firstComponent.bounds.Y != y)
        {
            this.ReorientComponents(area, x, y);
        }
    }

    private void ReorientComponents(ComponentArea area, int x, int y)
    {
        this.lastArea.Value = area;
        foreach (var icon in this.modConfig.Icons)
        {
            if (this.components.TryGetValue(icon.Id, out var component))
            {
                if (!icon.Enabled)
                {
                    component.visible = false;
                    continue;
                }

                component.visible = true;
                component.bounds.X = x;
                component.bounds.Y = y;
                switch (area)
                {
                    case ComponentArea.Top:
                    case ComponentArea.Bottom:
                        x += component.bounds.Width + 4;
                        break;
                    case ComponentArea.Right:
                    case ComponentArea.Left:
                        y += component.bounds.Height + 4;
                        break;
                    case ComponentArea.Custom:
                    default:
                        break;
                }
            }
        }
    }
}