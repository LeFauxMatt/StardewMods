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
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.MenuComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Models.MenuComponents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IToolbarIcons" />
[FuryCoreService(true)]
internal class ToolbarIcons : IToolbarIcons, IModService
{
    private readonly PerScreen<bool> _alignTop = new();
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<List<IMenuComponent>> _icons = new(() => new());
    private readonly PerScreen<bool> _init = new();
    private readonly PerScreen<List<CustomMenuComponent>> _shortcuts = new();
    private MethodInfo _overrideButtonReflected;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolbarIcons" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ToolbarIcons(IModHelper helper, IModServices services)
    {
        this._assetHandler = services.Lazy<AssetHandler>();
        this.Helper = helper;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this.Helper.Events.Display.RenderingHud += this.OnRenderingHud;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        services.Lazy<ICustomEvents>(
            customEvents => { customEvents.ToolbarIconPressed += this.OnToolbarIconPressed; });
    }

    /// <inheritdoc />
    public List<IMenuComponent> Icons
    {
        get => this._icons.Value;
    }

    private bool AlignTop
    {
        get => this._alignTop.Value;
        set => this._alignTop.Value = value;
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IModHelper Helper { get; }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private bool Init
    {
        get => this._init.Value;
        set => this._init.Value = value;
    }

    private IDictionary<string, SButton[]> Keybinds { get; } = new Dictionary<string, SButton[]>();

    private MethodInfo OverrideButtonReflected
    {
        get => this._overrideButtonReflected ??= Game1.input.GetType().GetMethod("OverrideButton");
    }

    private IEnumerable<CustomMenuComponent> Shortcuts
    {
        get => this._shortcuts.Value ??= (
                from icon in this.Assets.ToolbarData
                select new CustomMenuComponent(
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
        var (x, y) = Game1.getMousePosition(false);
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

    private void OnRenderedHud(object sender, RenderedHudEventArgs e)
    {
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
        var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
        if (toolbar is null || !Game1.displayHUD || Game1.activeClickableMenu is not null)
        {
            return;
        }

        var (_, playerGlobalY) = Game1.player.GetBoundingBox().Center;
        var (_, playerLocalY) = Game1.GlobalToLocal(globalPosition: new Vector2(0, playerGlobalY), viewport: Game1.viewport);
        var alignTop = Game1.options.pinToolbarToggle || playerLocalY > Game1.viewport.Height / 2 + Game1.tileSize;
        if (this.AlignTop != alignTop || !this.Init)
        {
            this.ReinitializeIcons(alignTop);
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

    private void OnToolbarIconPressed(object sender, ToolbarIconPressedEventArgs e)
    {
        if (this.Shortcuts.Contains(e.Component))
        {
            if (!this.Keybinds.TryGetValue(e.Component.Name, out var keybind))
            {
                IList<SButton> buttons = new List<SButton>();
                foreach (var key in e.Component.Name.Split(' '))
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

            e.SuppressInput();
        }
    }

    private void OverrideButton(SButton button, bool inputState)
    {
        this.OverrideButtonReflected.Invoke(Game1.input, new object[] { button, inputState });
    }

    private void ReinitializeIcons(bool alignTop = false)
    {
        this.AlignTop = alignTop;
        this.Init = true;

        var icons = this.Icons.Where(icon => icon.Area is ComponentArea.Left).ToList();
        var x = (Game1.uiViewport.Width - Game1.tileSize * 12) / 2;
        var y = this.AlignTop
            ? Utility.makeSafeMarginY(8) + Game1.tileSize + IClickableMenu.borderWidth
            : Game1.uiViewport.Height - Utility.makeSafeMarginY(8) - Game1.tileSize - IClickableMenu.borderWidth;
        foreach (var icon in icons)
        {
            icon.X = x;
            icon.Y = y - (this.AlignTop ? 0 : icon.Component.bounds.Height);
            x += icon.Component.bounds.Width + 4;
        }

        icons = this.Icons.Where(icon => icon.Area is ComponentArea.Right).ToList();
        x = (Game1.uiViewport.Width + Game1.tileSize * 12) / 2 - icons.Sum(icon => icon.Component.bounds.Width + 4) + 4;
        foreach (var icon in icons)
        {
            icon.X = x;
            icon.Y = y - (this.AlignTop ? 0 : icon.Component.bounds.Height);
            x += icon.Component.bounds.Width + 4;
        }
    }
}