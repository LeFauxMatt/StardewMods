namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Search for which chests have the item you're looking for.</summary>
internal sealed class ChestFinder : BaseFeature
{
    private const int CountdownTimer = 20;
    private readonly PerScreen<bool> activeSearch = new();
    private readonly PerScreen<List<ChestContainer>> cachedContainers = new(() => []);
    private readonly PerScreen<string> cachedText = new(() => string.Empty);
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<int> currentIndex = new();
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<ItemMatcher> itemMatcher;
    private readonly IModEvents modEvents;
    private readonly PerScreen<bool> resetCache = new(() => true);
    private readonly PerScreen<SearchBar> searchBar = new(() => new SearchBar());
    private readonly PerScreen<int> timeOut = new();
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="ChestFinder" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public ChestFinder(
        ILog log,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        IInputHelper inputHelper,
        ItemMatcherFactory itemMatcherFactory,
        IModEvents modEvents,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(log, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
        this.itemMatcher = new PerScreen<ItemMatcher>(itemMatcherFactory.GetOneForSearch);
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.ChestFinder != FeatureOption.Disabled;

    private bool IsSearchActive =>
        Context.IsPlayerFree
        && Game1.displayHUD
        && Game1.activeClickableMenu is null or SearchBar
        && this.activeSearch.Value;

    private bool IsFoundChestOpen =>
        Game1.activeClickableMenu is ItemGrabMenu
        {
            context: Chest chest,
        }
        && chest == this.cachedContainers.Value.ElementAtOrDefault(this.currentIndex.Value)?.Chest;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedHud += this.OnRenderedHud;
        this.modEvents.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.modEvents.World.ChestInventoryChanged += this.OnChestInventoryChanged;
        this.modEvents.Player.Warped += this.OnWarped;

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.Id,
            AssetHandler.IconTexturePath,
            new Rectangle(48, 0, 16, 16),
            I18n.Button_FindChest_Name());

        this.toolbarIconsIntegration.Api.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedHud -= this.OnRenderedHud;
        this.modEvents.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.modEvents.World.ChestInventoryChanged -= this.OnChestInventoryChanged;
        this.modEvents.Player.Warped -= this.OnWarped;

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
        this.toolbarIconsIntegration.Api.ToolbarIconPressed -= this.OnToolbarIconPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.IsSearchActive)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft:
                this.searchBar.Value.receiveLeftClick(mouseX, mouseY);
                break;
            case SButton.MouseRight:
                this.searchBar.Value.receiveRightClick(mouseX, mouseY);
                break;
            default:
                return;
        }

        if (Game1.activeClickableMenu is SearchBar)
        {
            this.inputHelper.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        // Activate Search Bar
        if (Context.IsPlayerFree
            && Game1.displayHUD
            && Game1.activeClickableMenu is null
            && this.ModConfig.Controls.FindChest.JustPressed())
        {
            this.OpenSearchBar();
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.FindChest);
            return;
        }

        // Close Search Bar
        if (this.IsSearchActive && this.ModConfig.Controls.CloseChestFinder.JustPressed())
        {
            this.CloseSearchBar();
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.CloseChestFinder);
            return;
        }

        if (!this.activeSearch.Value || !this.cachedContainers.Value.Any())
        {
            return;
        }

        // Open Found Chest
        if (this.IsSearchActive && this.ModConfig.Controls.OpenFoundChest.JustPressed())
        {
            this.OpenFoundChest();
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.CloseChestFinder);
            return;
        }

        if (!this.IsFoundChestOpen)
        {
            return;
        }

        // Open Next Chest
        if (this.ModConfig.Controls.OpenNextChest.JustPressed())
        {
            this.currentIndex.Value++;
            this.OpenFoundChest();
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.CloseChestFinder);
        }

        // Open Previous Chest
        if (this.ModConfig.Controls.OpenPreviousChest.JustPressed())
        {
            this.currentIndex.Value--;
            this.OpenFoundChest();
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.OpenPreviousChest);
        }
    }

    private void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        if (e.Location.Equals(Game1.currentLocation))
        {
            this.resetCache.Value = true;
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        // Check if active
        if (!this.IsSearchActive)
        {
            return;
        }

        // Check if storages needs to be reset
        if (this.resetCache.Value)
        {
            this.SearchForStorages();
            this.resetCache.Value = false;
        }

        // Check if there are any storages found
        if (!this.cachedContainers.Value.Any())
        {
            this.searchBar.Value.draw(e.SpriteBatch);
            return;
        }

        var bounds = Game1.graphics.GraphicsDevice.Viewport.Bounds;
        var srcRect = new Rectangle(412, 495, 5, 4);
        foreach (var container in this.cachedContainers.Value)
        {
            var pos = (container.TileLocation + new Vector2(0.5f, -0.75f)) * Game1.tileSize;
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
                    new Vector2(2f, 2f),
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
                new Vector2((float)srcRect.Width * Game1.pixelZoom, (float)srcRect.Height * Game1.pixelZoom));

            e.SpriteBatch.Draw(
                Game1.mouseCursors,
                onScreenPos,
                srcRect,
                Color.White,
                rotation,
                new Vector2(2f, 2f),
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);
        }

        this.searchBar.Value.draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.IsSearchActive)
        {
            return;
        }

        if (this.timeOut.Value > 0 && --this.timeOut.Value == 0)
        {
            this.Log.Trace("{0}: Searching for \"{0}\"", this.Id, this.cachedText.Value);
            this.itemMatcher.Value.SearchText = this.cachedText.Value;
            this.resetCache.Value = true;
        }

        if (this.searchBar.Value.SearchText.Equals(this.cachedText.Value, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        this.timeOut.Value = ChestFinder.CountdownTimer;
        this.cachedText.Value = this.searchBar.Value.SearchText;
    }

    private void OnWarped(object? sender, WarpedEventArgs e) => this.resetCache.Value = true;

    private void OpenSearchBar()
    {
        this.activeSearch.Value = true;
        this.searchBar.Value.SetFocus();
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == this.Id)
        {
            this.OpenSearchBar();
        }
    }

    private void CloseSearchBar()
    {
        this.searchBar.Value.exitThisMenuNoSound();
        this.activeSearch.Value = false;
        this.cachedContainers.Value.Clear();
        this.resetCache.Value = true;
    }

    private void OpenFoundChest()
    {
        if (this.currentIndex.Value < 0)
        {
            this.currentIndex.Value = this.cachedContainers.Value.Count - 1;
        }
        else if (this.currentIndex.Value >= this.cachedContainers.Value.Count)
        {
            this.currentIndex.Value = 0;
        }

        this.cachedContainers.Value[this.currentIndex.Value].Chest.ShowMenu();
    }

    private void SearchForStorages()
    {
        this.cachedContainers.Value.Clear();
        if (this.itemMatcher.Value.IsEmpty)
        {
            return;
        }

        foreach (var container in this
            .containerFactory.GetAllFromLocation(
                Game1.player.currentLocation,
                container => container.Options.ChestFinder == FeatureOption.Enabled)
            .OfType<ChestContainer>())
        {
            if (container.Items.Any(this.itemMatcher.Value.MatchesFilter))
            {
                this.cachedContainers.Value.Add(container);
            }
        }

        this.Log.Trace("{0}: Found {1} chests", this.Id, this.cachedContainers.Value.Count);
        this.currentIndex.Value = 0;
    }
}