namespace StardewMods.ToolbarIcons;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.ToolbarIcons.Framework;
using StardewValley.Menus;

// TODO: Center Toolbar Icons

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private readonly PerScreen<Api?> _api = new();
    private readonly PerScreen<ComponentArea> _area = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent?> _button = new();
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<Toolbar?> _toolbar = new();

    private ModConfig? _config;

    private static bool ShowToolbar => Integrations.IsLoaded
                                    && Game1.displayHUD
                                    && Context.IsPlayerFree
                                    && Game1.activeClickableMenu is null
                                    && Game1.onScreenMenus.OfType<Toolbar>().Any();

    private Api Api => this._api.Value ??= new(this.Helper, this.ModConfig.Icons, this.Components);

    private ComponentArea Area
    {
        get => this._area.Value;
        set => this._area.Value = value;
    }

    private ClickableComponent? Button
    {
        get
        {
            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            if (this.Toolbar is not null && ReferenceEquals(toolbar, this.Toolbar))
            {
                return this._button.Value;
            }

            if (toolbar is null)
            {
                return null;
            }

            this.Toolbar = toolbar;
            var buttons = this.Helper.Reflection.GetField<List<ClickableComponent>>(toolbar, "buttons").GetValue();
            this._button.Value = buttons.First();
            return this._button.Value;
        }
    }

    private Dictionary<string, ClickableTextureComponent> Components { get; } = new();

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private ModConfig ModConfig => this._config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    private Toolbar? Toolbar
    {
        get => this._toolbar.Value;
        set => this._toolbar.Value = value;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        Integrations.Init(this.Helper, this.Api);
        ThemeHelper.Init(this.Helper, "furyx639.ToolbarIcons/Icons", "furyx639.ToolbarIcons/Arrows");
        Config.Init(this.Helper, this.ModManifest, this.ModConfig, this.Components);

        // Events
        this.Helper.Events.Content.AssetRequested += ModEntry.OnAssetRequested;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this.Helper.Events.Display.RenderingHud += this.OnRenderingHud;
        Integrations.ToolbarIconsLoaded += this.OnToolbarIconsLoaded;
        Config.ToolbarIconsChanged += this.OnToolbarIconsChanged;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return this.Api;
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.ToolbarIcons/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.ToolbarIcons/Arrows"))
        {
            e.LoadFromModFile<Texture2D>("assets/arrows.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.ToolbarIcons/Toolbar"))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!ModEntry.ShowToolbar || this.Helper.Input.IsSuppressed(e.Button))
        {
            return;
        }

        if (e.Button is not SButton.MouseLeft or SButton.MouseRight
         && !(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component =
            this.Components.Values.FirstOrDefault(component => component.visible && component.containsPoint(x, y));
        if (component is null)
        {
            return;
        }

        Game1.playSound("drumkit6");
        this.Api.Invoke(component.name);
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!ModEntry.ShowToolbar)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var component in this.Components.Values.Where(component => component.visible))
        {
            component.tryHover(x, y);
            if (component.bounds.Contains(x, y))
            {
                this.HoverText = component.hoverText;
            }
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!ModEntry.ShowToolbar)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.HoverText, Game1.smallFont);
        }
    }

    private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
    {
        if (!ModEntry.ShowToolbar)
        {
            return;
        }

        this.ReorientComponents();

        foreach (var component in this.Components.Values.Where(component => component.visible))
        {
            var icons = this.Helper.GameContent.Load<Texture2D>("furyx639.ToolbarIcons/Icons");
            e.SpriteBatch.Draw(
                icons,
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
        foreach (var icon in this.ModConfig.Icons)
        {
            if (this.Components.TryGetValue(icon.Id, out var component))
            {
                component.visible = icon.Enabled;
            }
        }

        this.ReorientComponents();
    }

    private void OnToolbarIconsLoaded(object? sender, EventArgs e)
    {
        this.ReorientComponents();
    }

    private void ReorientComponents()
    {
        if (this.Button is null || this.Toolbar is null || !this.Components.Values.Any(component => component.visible))
        {
            return;
        }

        var xAlign = this.Button.bounds.X < Game1.viewport.Width / 2;
        var yAlign = this.Button.bounds.Y < Game1.viewport.Height / 2;
        ComponentArea area;
        int x;
        int y;
        if (this.Toolbar.width > this.Toolbar.height)
        {
            x = this.Button.bounds.Left;
            if (yAlign)
            {
                area = ComponentArea.Top;
                y = this.Button.bounds.Bottom + 20;
            }
            else
            {
                area = ComponentArea.Bottom;
                y = this.Button.bounds.Top - 52;
            }
        }
        else
        {
            y = this.Button.bounds.Top;
            if (xAlign)
            {
                area = ComponentArea.Left;
                x = this.Button.bounds.Right + 20;
            }
            else
            {
                area = ComponentArea.Right;
                x = this.Button.bounds.Left - 52;
            }
        }

        var firstComponent = this.Components.Values.First(component => component.visible);
        if (area != this.Area || firstComponent.bounds.X != x || firstComponent.bounds.Y != y)
        {
            this.ReorientComponents(area, x, y);
        }
    }

    private void ReorientComponents(ComponentArea area, int x, int y)
    {
        this.Area = area;
        foreach (var icon in this.ModConfig.Icons)
        {
            if (this.Components.TryGetValue(icon.Id, out var component))
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