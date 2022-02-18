namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IHudComponents" />
[FuryCoreService(true)]
internal class HudComponents : IHudComponents, IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<List<IClickableComponent>> _icons = new(() => new());
    private readonly PerScreen<List<CustomClickableComponent>> _shortcuts = new();
    private MethodInfo _overrideButtonReflected;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HudComponents" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public HudComponents(ConfigData config, IModHelper helper, IModServices services)
    {
        this._assetHandler = services.Lazy<AssetHandler>();
        this.Config = config;
        this.Helper = helper;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this.Helper.Events.Display.RenderingHud += this.OnRenderingHud;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

        services.Lazy<ICustomEvents>(
            customEvents =>
            {
                customEvents.HudComponentPressed += this.OnHudComponentPressed;
            });
    }

    /// <inheritdoc />
    public List<IClickableComponent> Icons
    {
        get => this._icons.Value;
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private ConfigData Config { get; }

    private IModHelper Helper { get; }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private IDictionary<string, SButton[]> Keybinds { get; } = new Dictionary<string, SButton[]>();

    private MethodInfo OverrideButtonReflected
    {
        get => this._overrideButtonReflected ??= Game1.input.GetType().GetMethod("OverrideButton");
    }

    private IEnumerable<CustomClickableComponent> Shortcuts
    {
        get => this._shortcuts.Value ??= (
                from icon in this.Assets.ToolbarData
                select new CustomClickableComponent(
                    new(
                        new(0, 0, 32, 32),
                        this.Helper.Content.Load<Texture2D>(icon.Value[1], ContentSource.GameContent),
                        new(16 * int.Parse(icon.Value[2]), 0, 16, 16),
                        2f)
                    {
                        hoverText = icon.Value[0],
                        name = icon.Key,
                    },
                    Enum.TryParse(icon.Value[3], out ComponentArea area) ? area : ComponentArea.Left))
            .ToList();
    }

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        if (!this.Config.ToolbarIcons)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var icon in this.Icons)
        {
            icon.TryHover(x, y);
            if (icon.Component.bounds.Contains(x, y))
            {
                this.HoverText = icon.HoverText;
            }
        }
    }

    private void OnHudComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (!this.Config.ToolbarIcons)
        {
            return;
        }

        if (this.Shortcuts.Contains(e.Component))
        {
            if (e.Component.Name.StartsWith("command:"))
            {
                var command = e.Component.Name[8..].Trim().Split(' ');
                this.Helper.ConsoleCommands.Trigger(command[0], command[1..]);
            }
            else if (e.Component.Name.StartsWith("keybind:"))
            {
                if (!this.Keybinds.TryGetValue(e.Component.Name, out var keybind))
                {
                    IList<SButton> buttons = new List<SButton>();
                    foreach (var key in e.Component.Name[8..].Trim().Split('+'))
                    {
                        if (Enum.TryParse(key, out SButton button))
                        {
                            buttons.Add(button);
                        }
                    }

                    keybind = buttons.ToArray();
                    this.Keybinds.Add(e.Component.Name, keybind);
                }

                foreach (var button in keybind)
                {
                    this.OverrideButton(button, true);
                }
            }

            e.SuppressInput();
        }
    }

    private void OnRenderedHud(object sender, RenderedHudEventArgs e)
    {
        if (!this.Config.ToolbarIcons)
        {
            return;
        }

        var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
        if (toolbar is null || !Game1.displayHUD || Game1.activeClickableMenu is not null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.HoverText, Game1.smallFont);
        }
    }

    private void OnRenderingHud(object sender, RenderingHudEventArgs e)
    {
        if (!this.Config.ToolbarIcons)
        {
            return;
        }

        var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
        if (toolbar is null || !Game1.displayHUD || Game1.activeClickableMenu is not null)
        {
            return;
        }

        var (_, playerGlobalY) = Game1.player.GetBoundingBox().Center;
        var (_, playerLocalY) = Game1.GlobalToLocal(globalPosition: new Vector2(0, playerGlobalY), viewport: Game1.viewport);
        var y = Game1.options.pinToolbarToggle || playerLocalY < Game1.viewport.Height / 2 + Game1.tileSize
            ? Game1.uiViewport.Height - Utility.makeSafeMarginY(8) - Game1.tileSize - IClickableMenu.borderWidth
            : Utility.makeSafeMarginY(8) + Game1.tileSize + IClickableMenu.borderWidth;
        if (this.Icons.Any(icon => icon.Y != y))
        {
            this.ReinitializeIcons(y);
        }

        foreach (var icon in this.Icons)
        {
            icon.Draw(e.SpriteBatch);
        }
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (this.Shortcuts.Any())
        {
            this.Icons.AddRange(this.Shortcuts);
            this.ReinitializeIcons();
        }
    }

    private void OverrideButton(SButton button, bool inputState)
    {
        this.OverrideButtonReflected.Invoke(Game1.input, new object[] { button, inputState });
    }

    private void ReinitializeIcons(int y = -1)
    {
        var icons = this.Icons.Where(icon => icon.Area is ComponentArea.Left).ToList();
        var (_, playerGlobalY) = Game1.player.GetBoundingBox().Center;
        var (_, playerLocalY) = Game1.GlobalToLocal(globalPosition: new Vector2(0, playerGlobalY), viewport: Game1.viewport);
        var alignBottom = Game1.options.pinToolbarToggle || playerLocalY < Game1.viewport.Height / 2 + Game1.tileSize;
        var x = (Game1.uiViewport.Width - Game1.tileSize * 12) / 2;
        if (y == -1)
        {
            y = alignBottom
                ? Game1.uiViewport.Height - Utility.makeSafeMarginY(8) - Game1.tileSize - IClickableMenu.borderWidth
                : Utility.makeSafeMarginY(8) + Game1.tileSize + IClickableMenu.borderWidth;
        }

        foreach (var icon in icons)
        {
            icon.X = x;
            icon.Y = y - (alignBottom ? icon.Component.bounds.Height : 0);
            x += icon.Component.bounds.Width + 4;
        }

        icons = this.Icons.Where(icon => icon.Area is ComponentArea.Right).ToList();
        x = (Game1.uiViewport.Width + Game1.tileSize * 12) / 2 - icons.Sum(icon => icon.Component.bounds.Width + 4) + 4;
        foreach (var icon in icons)
        {
            icon.X = x;
            icon.Y = y - (alignBottom ? icon.Component.bounds.Height : 0);
            x += icon.Component.bounds.Width + 4;
        }
    }
}