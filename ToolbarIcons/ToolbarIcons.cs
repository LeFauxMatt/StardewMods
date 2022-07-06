namespace StardewMods.ToolbarIcons;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewMods.ToolbarIcons.Helpers;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
public class ToolbarIcons : Mod
{
    private readonly PerScreen<Dictionary<string, string>> _actions = new(() => new());
    private readonly PerScreen<ToolbarIconsApi?> _api = new();
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<Dictionary<string, ClickableTextureComponent>> _icons = new(() => new());
    private ClickableTextureComponent? _icon;
    private MethodInfo? _overrideButtonReflected;

    private Dictionary<string, string> Actions
    {
        get => this._actions.Value;
    }

    private ToolbarIconsApi Api
    {
        get => this._api.Value ??= new(this.Helper.GameContent, this.Icons);
    }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private ClickableTextureComponent Icon
    {
        get => this._icon ??= new(
            new(0, 0, 32, 32),
            this.Helper.GameContent.Load<Texture2D>("furyx639.ToolbarIcons/Icons"),
            new(0, 0, 16, 16),
            2f);
    }

    private Dictionary<string, ClickableTextureComponent> Icons
    {
        get => this._icons.Value;
    }

    private IDictionary<string, SButton[]> Keybinds { get; } = new Dictionary<string, SButton[]>();

    private MethodInfo OverrideButtonReflected
    {
        get => this._overrideButtonReflected ??= Game1.input.GetType().GetMethod("OverrideButton")!;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        IntegrationHelper.Init(this.Helper, this.Api);

        if (this.Helper.ModRegistry.IsLoaded("furyx639.FuryCore"))
        {
            Log.Alert("Remove FuryCore, it is no longer needed by this mod!");
        }

        // Events
        this.Helper.Events.Content.AssetRequested += ToolbarIcons.OnAssetRequested;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this.Helper.Events.Display.RenderingHud += this.OnRenderingHud;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
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
        }
        else if (e.Name.IsEquivalentTo("furyx639.FuryCore/Toolbar"))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
        {
            return;
        }

        if (e.Button is not SButton.MouseLeft or SButton.MouseRight)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var icon = this.Icons.Values.FirstOrDefault(icon => icon.containsPoint(x, y));
        if (icon is not null)
        {
            Game1.playSound("drumkit6");
            if (this.Actions.TryGetValue(icon.name, out var action))
            {
                if (action.StartsWith("toggle:"))
                {
                    var command = action[7..].Trim();
                    foreach (var subIcon in this.Icons.Values.Where(subIcon => icon != subIcon && subIcon.name.StartsWith(command)))
                    {
                        subIcon.visible = !subIcon.visible;
                    }
                }
                else if (action.StartsWith("keybind:"))
                {
                    if (!this.Keybinds.TryGetValue(action, out var keybind))
                    {
                        var keys = action[8..].Trim().Split(' ');
                        IList<SButton> buttons = new List<SButton>();
                        foreach (var key in keys)
                        {
                            if (Enum.TryParse(key, out SButton button))
                            {
                                buttons.Add(button);
                            }
                        }

                        keybind = buttons.ToArray();
                        this.Keybinds.Add(action, keybind);
                    }

                    foreach (var button in keybind)
                    {
                        this.OverrideButton(button, true);
                    }
                }

                this.Helper.Input.Suppress(e.Button);
            }

            this.Api.Invoke(icon.name);
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var icon in this.Icons.Values.Where(icon => icon.visible))
        {
            icon.tryHover(x, y);
            if (icon.bounds.Contains(x, y))
            {
                this.HoverText = icon.hoverText;
            }
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
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
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
        {
            return;
        }

        var (_, playerGlobalY) = Game1.player.GetBoundingBox().Center;
        var (_, playerLocalY) = Game1.GlobalToLocal(globalPosition: new Vector2(0, playerGlobalY), viewport: Game1.viewport);
        var alignBottom = Game1.options.pinToolbarToggle || playerLocalY < Game1.viewport.Height / 2 + Game1.tileSize;
        var y = alignBottom
            ? Game1.uiViewport.Height - Utility.makeSafeMarginY(8) - Game1.tileSize - IClickableMenu.borderWidth
            : Utility.makeSafeMarginY(8) + Game1.tileSize + IClickableMenu.borderWidth;
        if (this.Icons.Values.Any(icon => icon.bounds.Y != y)
            || this.Icons.Values.Where(icon => icon.visible).Select(icon => icon.bounds.X).Distinct().Count() != this.Icons.Values.Count(icon => icon.visible))
        {
            this.ReorientComponents(y, alignBottom);
        }

        foreach (var icon in this.Icons.Values)
        {
            this.Icon.bounds.X = icon.bounds.X;
            this.Icon.bounds.Y = icon.bounds.Y;
            this.Icon.draw(e.SpriteBatch);
            icon.draw(e.SpriteBatch);
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        foreach (var (key, data) in this.Helper.GameContent.Load<IDictionary<string, string>>("furyx639.FuryCore/Toolbar"))
        {
            var info = data.Split('/');
            this.Api.AddToolbarIcon(key, info[1], new(16 * int.Parse(info[2]), 0, 16, 16), info[0]);
            this.Actions.Add(key, info[4]);
        }

        this.ReorientComponents();
    }

    private void OverrideButton(SButton button, bool inputState)
    {
        this.OverrideButtonReflected.Invoke(Game1.input, new object[] { button, inputState });
    }

    private void ReorientComponents(int y = -1, bool alignBottom = false)
    {
        var (_, playerGlobalY) = Game1.player.GetBoundingBox().Center;
        var (_, playerLocalY) = Game1.GlobalToLocal(globalPosition: new Vector2(0, playerGlobalY), viewport: Game1.viewport);
        var x = (Game1.uiViewport.Width - Game1.tileSize * 12) / 2;
        if (y == -1)
        {
            alignBottom = Game1.options.pinToolbarToggle || playerLocalY < Game1.viewport.Height / 2 + Game1.tileSize;
            y = alignBottom
                ? Game1.uiViewport.Height - Utility.makeSafeMarginY(8) - Game1.tileSize - IClickableMenu.borderWidth
                : Utility.makeSafeMarginY(8) + Game1.tileSize + IClickableMenu.borderWidth;
        }

        foreach (var icon in this.Icons.OrderBy(icon => icon.Key).Select(icon => icon.Value))
        {
            icon.bounds.X = x;
            icon.bounds.Y = y - (alignBottom ? icon.bounds.Height : 0);
            if (icon.visible)
            {
                x += icon.bounds.Width + 4;
            }
        }
    }
}