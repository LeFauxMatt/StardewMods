namespace StardewMods.BetterChests.Features;

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;

/// <summary>
///     Search for which chests have the item you're looking for.
/// </summary>
internal class ChestFinder : IFeature
{
    private readonly PerScreen<IStorageObject?> _fakeStorage = new();

    private ChestFinder(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static ChestFinder? Instance { get; set; }

    private ModConfig Config { get; }

    private IStorageObject FakeStorage
    {
        get => this._fakeStorage.Value ??= new ChestStorage(new(), Game1.getFarm(), this.Config.DefaultChest, Vector2.Zero);
    }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

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
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            if (IntegrationHelper.ToolbarIcons.IsLoaded)
            {
                IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
                    "BetterChests.FindChest",
                    "furyx639.BetterChests/Icons",
                    new(48, 0, 16, 16),
                    I18n.Button_FindChest_Name());
                IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
            }
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Display.RenderedHud -= this.OnRenderedHud;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

            if (IntegrationHelper.ToolbarIcons.IsLoaded)
            {
                IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.FindChest");
                IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
            }
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.ControlScheme.FindChest.JustPressed())
        {
            return;
        }

        this.OpenChestFinder();
        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.FindChest);
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!this.FakeStorage.FilterMatcher.Any())
        {
            return;
        }

        var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        var srcRect = new Rectangle(412, 495, 5, 4);
        foreach (var storage in StorageHelper.CurrentLocation)
        {
            if (!storage.Items.Any(this.FakeStorage.FilterMatches))
            {
                continue;
            }

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

            onScreenPos = Utility.ModifyCoordinatesForUIScale(onScreenPos);
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
        Game1.activeClickableMenu = new ItemSelectionMenu(this.FakeStorage, this.FakeStorage.FilterMatcher);
    }
}