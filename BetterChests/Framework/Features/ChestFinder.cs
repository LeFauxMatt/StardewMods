namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Handlers;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Helpers;
using StardewValley.Menus;

/// <summary>
///     Search for which chests have the item you're looking for.
/// </summary>
internal sealed class ChestFinder : IFeature
{
    private const int MaxTimeOut = 20;

#nullable disable
    private static IFeature Instance;
#nullable enable

    private readonly ModConfig _config;
    private readonly PerScreen<int> _currentIndex = new();
    private readonly PerScreen<IList<BaseStorage>> _foundStorages = new(() => new List<BaseStorage>());
    private readonly IModHelper _helper;
    private readonly PerScreen<ItemMatcher?> _itemMatcher = new();
    private readonly PerScreen<SearchBar> _searchBar = new(() => new());
    private readonly PerScreen<string> _searchText = new(() => string.Empty);
    private readonly PerScreen<bool> _showSearch = new();
    private readonly PerScreen<IList<object>> _storageContexts = new(() => new List<object>());
    private readonly PerScreen<int> _timeOut = new();

    private bool _isActivated;

    private ChestFinder(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    private int CurrentIndex
    {
        get => this._currentIndex.Value;
        set => this._currentIndex.Value = value;
    }

    private IList<BaseStorage> FoundStorages => this._foundStorages.Value;

    private ItemMatcher ItemMatcher =>
        this._itemMatcher.Value ??= new(false, this._config.SearchTagSymbol.ToString());

    private SearchBar SearchBar => this._searchBar.Value;

    private string SearchText
    {
        get => this._searchText.Value;
        set => this._searchText.Value = value;
    }

    private bool ShowSearch
    {
        get => this._showSearch.Value && Game1.displayHUD && Context.IsPlayerFree && Game1.activeClickableMenu is null;
        set => this._showSearch.Value = value;
    }

    private IList<object> StorageContexts => this._storageContexts.Value;

    private int TimeOut
    {
        get => this._timeOut.Value;
        set => this._timeOut.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="ChestFinder" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestFinder" /> class.</returns>
    public static IFeature Init(IModHelper helper, ModConfig config)
    {
        return ChestFinder.Instance ??= new ChestFinder(helper, config);
    }

    /// <inheritdoc />
    public void SetActivated(bool value)
    {
        if (this._isActivated == value)
        {
            return;
        }

        this._isActivated = value;
        if (this._isActivated)
        {
            this._helper.Events.Display.RenderedHud += this.OnRenderedHud;
            this._helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            this._helper.Events.World.ChestInventoryChanged += this.OnChestInventoryChanged;
            this._helper.Events.Player.Warped += this.OnWarped;

            if (!Integrations.ToolbarIcons.IsLoaded)
            {
                return;
            }

            Integrations.ToolbarIcons.API.AddToolbarIcon(
                "BetterChests.FindChest",
                "furyx639.BetterChests/Icons",
                new(48, 0, 16, 16),
                I18n.Button_FindChest_Name());
            Integrations.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
            return;
        }

        this._helper.Events.Display.RenderedHud -= this.OnRenderedHud;
        this._helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this._helper.Events.World.ChestInventoryChanged -= this.OnChestInventoryChanged;
        this._helper.Events.Player.Warped -= this.OnWarped;

        if (!Integrations.ToolbarIcons.IsLoaded)
        {
            return;
        }

        Integrations.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.FindChest");
        Integrations.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.ShowSearch)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft:
                this.SearchBar.receiveLeftClick(x, y);
                break;
            case SButton.MouseRight:
                this.SearchBar.receiveRightClick(x, y);
                break;
            default:
                return;
        }

        if (Game1.activeClickableMenu is SearchBar)
        {
            this._helper.Input.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Context.IsPlayerFree && this._config.ControlScheme.FindChest.JustPressed())
        {
            this.ShowSearch = true;
            if (this.ShowSearch)
            {
                this.SearchBar.SetFocus();
            }

            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.FindChest);
            return;
        }

        if (Game1.activeClickableMenu is ItemGrabMenu && this._config.ControlScheme.OpenNextChest.JustPressed())
        {
            ++this.CurrentIndex;
            if (this.CurrentIndex < 0 || this.CurrentIndex >= this.FoundStorages.Count)
            {
                this.CurrentIndex = 0;
            }

            if (this.CurrentIndex < this.FoundStorages.Count)
            {
                this.FoundStorages[this.CurrentIndex].ShowMenu();
            }

            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.CloseChestFinder);
        }

        if (!this.ShowSearch && Game1.activeClickableMenu != this.SearchBar)
        {
            return;
        }

        if (this._config.ControlScheme.CloseChestFinder.JustPressed())
        {
            this.SearchBar.exitThisMenuNoSound();
            this.ShowSearch = false;
            this.SearchText = string.Empty;
            this.ItemMatcher.Clear();
            this.FoundStorages.Clear();
            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.CloseChestFinder);
        }

        if (this._config.ControlScheme.OpenFoundChest.JustPressed())
        {
            if (this.CurrentIndex < 0 || this.CurrentIndex >= this.FoundStorages.Count)
            {
                this.CurrentIndex = 0;
            }

            if (this.CurrentIndex < this.FoundStorages.Count)
            {
                this.FoundStorages[this.CurrentIndex].ShowMenu();
            }

            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.CloseChestFinder);
        }
    }

    private void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        if (!e.Location.Equals(Game1.currentLocation) || string.IsNullOrWhiteSpace(this.SearchText))
        {
            return;
        }

        this.RefreshStorages();
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree && Game1.activeClickableMenu != this.SearchBar)
        {
            return;
        }

        if (this.ShowSearch)
        {
            this.SearchBar.draw(e.SpriteBatch);
        }

        if (!this.FoundStorages.Any())
        {
            return;
        }

        var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        var srcRect = new Rectangle(412, 495, 5, 4);
        foreach (var storage in this.FoundStorages)
        {
            var pos = (storage.Position + new Vector2(0.5f, -0.75f)) * Game1.tileSize;
            var onScreenPos = default(Vector2);
            if (Utility.isOnScreen(pos, 64))
            {
                onScreenPos = Game1.GlobalToLocal(Game1.viewport, pos + new Vector2(0, 0));
                onScreenPos = Utility.ModifyCoordinatesForUIScale(onScreenPos);
                e.SpriteBatch.Draw(
                    Game1.mouseCursors,
                    onScreenPos,
                    srcRect,
                    Color.White,
                    (float)Math.PI,
                    new(2f, 2f),
                    Game1.pixelZoom,
                    SpriteEffects.None,
                    1f);
                continue;
            }

            var rotation = 0f;
            if (pos.X > Game1.viewport.MaxCorner.X - 64)
            {
                onScreenPos.X = bounds.Right - 8f;
                rotation = (float)Math.PI / 2f;
            }
            else if (pos.X < Game1.viewport.X)
            {
                onScreenPos.X = 8f;
                rotation = -(float)Math.PI / 2f;
            }
            else
            {
                onScreenPos.X = pos.X - Game1.viewport.X;
            }

            if (pos.Y > Game1.viewport.MaxCorner.Y - 64)
            {
                onScreenPos.Y = bounds.Bottom - 8f;
                rotation = (float)Math.PI;
            }
            else if (pos.Y < Game1.viewport.Y)
            {
                onScreenPos.Y = 8f;
            }
            else
            {
                onScreenPos.Y = pos.Y - Game1.viewport.Y;
            }

            if ((int)onScreenPos.X == 8 && (int)onScreenPos.Y == 8)
            {
                rotation += (float)Math.PI / 4f;
            }
            else if ((int)onScreenPos.X == 8 && (int)onScreenPos.Y == bounds.Bottom - 8)
            {
                rotation += (float)Math.PI / 4f;
            }
            else if ((int)onScreenPos.X == bounds.Right - 8 && (int)onScreenPos.Y == 8)
            {
                rotation -= (float)Math.PI / 4f;
            }
            else if ((int)onScreenPos.X == bounds.Right - 8 && (int)onScreenPos.Y == bounds.Bottom - 8)
            {
                rotation -= (float)Math.PI / 4f;
            }

            onScreenPos = Utility.makeSafe(
                onScreenPos,
                new((float)srcRect.Width * Game1.pixelZoom, (float)srcRect.Height * Game1.pixelZoom));
            e.SpriteBatch.Draw(
                Game1.mouseCursors,
                onScreenPos,
                srcRect,
                Color.White,
                rotation,
                new(2f, 2f),
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);
        }
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == "BetterChests.FindChest")
        {
            this.ShowSearch = true;
        }

        if (this.ShowSearch)
        {
            this.SearchBar.SetFocus();
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.ShowSearch && Game1.activeClickableMenu != this.SearchBar)
        {
            return;
        }

        if (this.TimeOut > 0 && --this.TimeOut == 0)
        {
            Log.Trace($"ChestFinder: {this.SearchText}");
            this.ItemMatcher.StringValue = this.SearchText;
            this.RefreshStorages();
        }

        if (this.SearchBar.SearchText.Equals(this.SearchText, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        this.TimeOut = ChestFinder.MaxTimeOut;
        this.SearchText = this.SearchBar.SearchText;
    }

    private void OnWarped(object? sender, WarpedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(this.SearchText))
        {
            return;
        }

        this.RefreshStorages();
    }

    private void RefreshStorages()
    {
        this.FoundStorages.Clear();
        this.StorageContexts.Clear();
        if (!this.ItemMatcher.Any())
        {
            return;
        }

        var storages = new List<BaseStorage>();
        foreach (var storage in Storages.CurrentLocation)
        {
            if (this.StorageContexts.Contains(storage.Context) || !storage.Items.Any(this.ItemMatcher.Matches))
            {
                continue;
            }

            this.StorageContexts.Add(storage.Context);
            storages.Add(storage);
        }

        storages.Sort(
            (s1, s2) =>
            {
                var d1 = Math.Abs(s1.Position.X - Game1.player.getTileX())
                       + Math.Abs(s1.Position.Y - Game1.player.getTileY());
                var d2 = Math.Abs(s2.Position.X - Game1.player.getTileX())
                       + Math.Abs(s2.Position.Y - Game1.player.getTileY());
                return d1.CompareTo(d2);
            });

        foreach (var storage in storages)
        {
            this.FoundStorages.Add(storage);
        }

        this.CurrentIndex = 0;
    }
}