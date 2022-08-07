namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;

/// <summary>
///     Search for which chests have the item you're looking for.
/// </summary>
internal class ChestFinder : IFeature
{
    private static ChestFinder? Instance;

    private readonly ModConfig _config;
    private readonly IModHelper _helper;
    private readonly PerScreen<IItemMatcher?> _itemMatcher = new();
    private readonly PerScreen<HashSet<IStorageObject>> _storages = new(() => new());

    private bool _isActivated;

    private ChestFinder(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    private IItemMatcher ItemMatcher =>
        this._itemMatcher.Value ??= new ItemMatcher(false, this._config.SearchTagSymbol.ToString());

    private HashSet<IStorageObject> Storages => this._storages.Value;

    /// <summary>
    ///     Initializes <see cref="ChestFinder" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestFinder" /> class.</returns>
    public static ChestFinder Init(IModHelper helper, ModConfig config)
    {
        return ChestFinder.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this._helper.Events.World.ChestInventoryChanged += this.OnChestInventoryChanged;

        if (!IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            return;
        }

        IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
            "BetterChests.FindChest",
            "furyx639.BetterChests/Icons",
            new(48, 0, 16, 16),
            I18n.Button_FindChest_Name());
        IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        this._helper.Events.Display.RenderedHud -= this.OnRenderedHud;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this._helper.Events.World.ChestInventoryChanged -= this.OnChestInventoryChanged;

        if (!IntegrationHelper.ToolbarIcons.IsLoaded)
        {
            return;
        }

        IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.FindChest");
        IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this._config.ControlScheme.FindChest.JustPressed())
        {
            return;
        }

        this.OpenChestFinder();
        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.FindChest);
    }

    private void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        if (!e.Location.Equals(Game1.currentLocation) || !this.ItemMatcher.Any())
        {
            return;
        }

        var storage =
            StorageHelper.CurrentLocation.FirstOrDefault(storage => ReferenceEquals(storage.Context, e.Chest));
        if (storage is null)
        {
            return;
        }

        if (storage.Items.Any(this.ItemMatcher.Matches))
        {
            if (!this.Storages.Contains(storage))
            {
                this.Storages.Add(storage);
            }

            return;
        }

        this.Storages.RemoveWhere(cachedStorage => ReferenceEquals(cachedStorage.Context, e.Chest));
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not SearchBar || e.NewMenu is not null)
        {
            return;
        }

        this.Storages.Clear();
        if (this.ItemMatcher.Any())
        {
            this.Storages.UnionWith(
                StorageHelper.CurrentLocation.Where(storage => storage.Items.Any(this.ItemMatcher.Matches)));
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Storages.Any())
        {
            return;
        }

        var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        var srcRect = new Rectangle(412, 495, 5, 4);
        foreach (var storage in this.Storages)
        {
            var pos = storage.Position * 64f + new Vector2(32, -48);
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
            this.OpenChestFinder();
        }
    }

    private void OpenChestFinder()
    {
        Game1.activeClickableMenu = new SearchBar(this.ItemMatcher);
    }
}