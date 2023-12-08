namespace StardewMods.ToolbarIcons.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewValley.Menus;

// TODO: Center Toolbar Icons

/// <summary>Service for handling the toolbar icons on the screen.</summary>
internal sealed class ToolbarHandler
{
    private readonly Dictionary<string, ClickableTextureComponent> components;
    private readonly ModConfig config;
    private readonly PerScreen<string> currentHoverText = new();
    private readonly EventsManager customEvents;
    private readonly IModEvents events;
    private readonly IGameContentHelper gameContent;
    private readonly IInputHelper input;
    private readonly PerScreen<ComponentArea> lastArea = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent> lastButton = new();
    private readonly PerScreen<Toolbar> lastToolbar = new();
    private readonly IMonitor monitor;
    private readonly IReflectionHelper reflection;

    /// <summary>Initializes a new instance of the <see cref="ToolbarHandler" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="components">Dependency used for the toolbar icon components.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="customEvents">Dependency used for custom events.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    public ToolbarHandler(
        IMonitor monitor,
        Dictionary<string, ClickableTextureComponent> components,
        ModConfig config,
        EventsManager customEvents,
        IModEvents events,
        IGameContentHelper gameContent,
        IInputHelper input,
        IReflectionHelper reflection)
    {
        // Init
        this.monitor = monitor;
        this.components = components;
        this.config = config;
        this.customEvents = customEvents;
        this.events = events;
        this.gameContent = gameContent;
        this.input = input;
        this.reflection = reflection;

        // Events
        customEvents.ToolbarIconsLoaded += this.OnToolbarIconsLoaded;
        customEvents.ToolbarIconsChanged += this.OnToolbarIconsChanged;
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
        var icon = this.config.Icons.FirstOrDefault(icon => icon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (icon is null)
        {
            icon = new(id);
            this.config.Icons.Add(icon);
        }

        if (this.components.ContainsKey(id))
        {
            return;
        }

        this.monitor.Log($"Adding icon: {id}");
        this.components.Add(
            id,
            new(new(0, 0, 32, 32), this.gameContent.Load<Texture2D>(texturePath), sourceRect ?? new(0, 0, 16, 16), 2f)
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
        var toolbarIcon = this.config.Icons.FirstOrDefault(
            toolbarIcon => toolbarIcon.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (toolbarIcon is null)
        {
            return;
        }

        this.monitor.Log($"Removing icon: {id}");
        this.config.Icons.Remove(toolbarIcon);
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
        var buttons = this.reflection.GetField<List<ClickableComponent>>(activeToolbar, "buttons").GetValue();
        button = this.lastButton.Value = buttons.First();
        return true;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!ToolbarHandler.ShowToolbar || this.input.IsSuppressed(e.Button))
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
        this.customEvents.InvokeToolbarIconPressed(component.name);
        this.input.Suppress(e.Button);
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!ToolbarHandler.ShowToolbar)
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
        if (!ToolbarHandler.ShowToolbar)
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
        if (!ToolbarHandler.ShowToolbar)
        {
            return;
        }

        this.ReorientComponents();

        foreach (var component in this.components.Values.Where(component => component.visible))
        {
            e.SpriteBatch.Draw(
                this.gameContent.Load<Texture2D>(AssetHandler.IconPath),
                new(component.bounds.X, component.bounds.Y),
                new(0, 0, 16, 16),
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
        foreach (var icon in this.config.Icons)
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
        this.events.Input.ButtonPressed += this.OnButtonPressed;
        this.events.Input.CursorMoved += this.OnCursorMoved;
        this.events.Display.RenderedHud += this.OnRenderedHud;
        this.events.Display.RenderingHud += this.OnRenderingHud;
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
        foreach (var icon in this.config.Icons)
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
