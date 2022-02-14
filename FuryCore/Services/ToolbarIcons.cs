namespace StardewMods.FuryCore.Services;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.MenuComponents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IToolbarIcons" />
[FuryCoreService(true)]
internal class ToolbarIcons : IToolbarIcons, IModService
{
    private readonly PerScreen<bool> _alignTop = new();
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<List<IMenuComponent>> _icons = new(() => new());
    private readonly PerScreen<bool> _init = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolbarIcons" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public ToolbarIcons(IModHelper helper)
    {
        this.Helper = helper;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this.Helper.Events.Display.RenderingHud += this.OnRenderingHud;
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

    private void OnCursorMoved(object sender, CursorMovedEventArgs e)
    {
        var (x, y) = Game1.getMousePosition(false);
        this.HoverText = string.Empty;
        foreach (var icon in this.Icons)
        {
            var bounds = icon.Component.bounds;
            if (x >= bounds.X && x <= bounds.X + bounds.Width / 2 && y >= bounds.Y && y <= bounds.Y + bounds.Height / 2)
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
            this.Init = true;
            this.AlignTop = alignTop;

            var icons = this.Icons.Where(icon => icon.Area is ComponentArea.Left).ToList();
            var x = (Game1.uiViewport.Width - Game1.tileSize * 12) / 2;
            var y = this.AlignTop
                ? Utility.makeSafeMarginY(8) + Game1.tileSize + IClickableMenu.borderWidth
                : Game1.uiViewport.Height - Utility.makeSafeMarginY(8) - Game1.tileSize - IClickableMenu.borderWidth;
            foreach (var icon in icons)
            {
                icon.X = x;
                icon.Y = y - (this.AlignTop ? 0 : icon.Component.bounds.Height) / 2;
                x += icon.Component.bounds.Width / 2 + 8;
            }

            icons = this.Icons.Where(icon => icon.Area is ComponentArea.Right).ToList();
            x = (Game1.uiViewport.Width + Game1.tileSize * 12) / 2 - icons.Sum(icon => icon.Component.bounds.Width / 2 + 8) + 8;
            foreach (var icon in icons)
            {
                icon.X = x;
                icon.Y = y - (this.AlignTop ? 0 : icon.Component.bounds.Height) / 2;
                x += icon.Component.bounds.Width / 2 + 8;
            }
        }

        foreach (var icon in this.Icons)
        {
            icon.Draw(e.SpriteBatch);
        }
    }
}