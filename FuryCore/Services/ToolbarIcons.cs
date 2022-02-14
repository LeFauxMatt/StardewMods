namespace StardewMods.FuryCore.Services;

using System.Collections.Generic;
using System.Linq;
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
    /// <param name="services">Provides access to internal and external services.</param>
    public ToolbarIcons(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
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
        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var icon in this.Icons)
        {
            icon.TryHover(x, y, 0.25f);

            if (icon.Component?.containsPoint(x, y) == true)
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

        var alignTop = toolbar.yPositionOnScreen < Game1.viewport.Height / 2;
        if (this.AlignTop != alignTop || !this.Init)
        {
            this.Init = true;
            this.AlignTop = alignTop;

            var icons = this.Icons.Where(icon => icon.Area is ComponentArea.Left).ToList();
            var x = Game1.uiViewport.Width / 2 - 384 - 16 - (Game1.tileSize + 8) * icons.Count;
            foreach (var icon in icons)
            {
                icon.X = x;
                icon.Y = toolbar.yPositionOnScreen - 96 + 8;
                x += icon.Component.bounds.Width + 8;
            }

            icons = this.Icons.Where(icon => icon.Area is ComponentArea.Right).ToList();
            x = Game1.uiViewport.Width / 2 + 384 + 16 + 8;
            foreach (var icon in icons)
            {
                icon.X = x;
                icon.Y = toolbar.yPositionOnScreen - 96 + 8;
                x += icon.Component.bounds.Width + 8;
            }
        }

        foreach (var icon in this.Icons)
        {
            icon.Draw(e.SpriteBatch);
        }

        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.HoverText, Game1.smallFont);
        }
    }
}