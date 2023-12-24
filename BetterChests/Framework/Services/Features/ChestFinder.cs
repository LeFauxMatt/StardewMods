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

/// <summary>Search for which chests have the item you're looking for.</summary>
internal sealed class ChestFinder : BaseFeature
{
    private readonly PerScreen<List<Pointer>> pointers = new(() => []);
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<int> currentIndex = new();
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly PerScreen<ItemMatcher> itemMatcher;
    private readonly IModEvents modEvents;
    private readonly PerScreen<bool> resetCache = new(() => true);
    private readonly PerScreen<SearchBar> searchBar;
    private readonly PerScreen<SearchOverlay> searchOverlay;
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
        this.searchBar = new PerScreen<SearchBar>(
            () => new SearchBar(
                () => this.itemMatcher.Value.SearchText,
                value =>
                {
                    this.Log.Trace("{0}: Searching for {1}", this.Id, value);
                    this.itemMatcher.Value.SearchText = value;
                    this.resetCache.Value = true;
                }));

        this.searchOverlay = new PerScreen<SearchOverlay>(() => new SearchOverlay(this.searchBar.Value));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.ChestFinder != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedHud += this.OnRenderedHud;
        this.modEvents.Display.RenderingHud += this.OnRenderingHud;
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
        this.modEvents.Display.RenderingHud -= this.OnRenderingHud;
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
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.MouseRight)
            || !Context.IsPlayerFree
            || !Game1.displayHUD)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft:
                this.searchOverlay.Value.receiveLeftClick(mouseX, mouseY);
                break;
            case SButton.MouseRight:
                this.searchOverlay.Value.receiveRightClick(mouseX, mouseY);
                break;
            default: return;
        }

        if (Game1.activeClickableMenu is SearchOverlay)
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
        if (this.isActive.Value && this.ModConfig.Controls.CloseChestFinder.JustPressed())
        {
            this.CloseSearchBar();
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.CloseChestFinder);
            return;
        }

        if (!this.isActive.Value
            || !this.pointers.Value.Any()
            || !this.ModConfig.Controls.OpenFoundChest.JustPressed())
        {
            return;
        }

        // Open Found Chest
        if (Game1.activeClickableMenu is ItemGrabMenu)
        {
            this.currentIndex.Value++;
        }
        else
        {
            this.currentIndex.Value = 0;
        }

        this.OpenFoundChest();
        this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.OpenFoundChest);
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
        if (!this.isActive.Value || !Game1.displayHUD || Game1.activeClickableMenu is SearchOverlay)
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
        foreach (var pointer in this.pointers.Value)
        {
            pointer.Draw(e.SpriteBatch);
        }

        this.searchOverlay.Value.draw(e.SpriteBatch);
    }

    private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
    {
        if (!this.isActive.Value || !Game1.displayHUD)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.searchBar.Value.Update(mouseX, mouseY);
    }

    private void OnWarped(object? sender, WarpedEventArgs e) => this.resetCache.Value = true;

    private void OpenSearchBar()
    {
        this.isActive.Value = true;
        this.searchOverlay.Value.Show();
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
        this.isActive.Value = false;
        this.pointers.Value.Clear();
        this.resetCache.Value = true;
    }

    private void OpenFoundChest()
    {
        if (this.currentIndex.Value < 0)
        {
            this.currentIndex.Value = this.pointers.Value.Count - 1;
        }
        else if (this.currentIndex.Value >= this.pointers.Value.Count)
        {
            this.currentIndex.Value = 0;
        }

        this.pointers.Value[this.currentIndex.Value].Container.ShowMenu();
    }

    private void SearchForStorages()
    {
        this.pointers.Value.Clear();
        if (this.itemMatcher.Value.IsEmpty)
        {
            return;
        }

        foreach (var container in this
            .containerFactory.GetAllFromLocation(
                Game1.player.currentLocation,
                container => container.Options.ChestFinder == Option.Enabled)
            .Where(container => container is ChestContainer or ObjectContainer))
        {
            if (container.Items.Any(this.itemMatcher.Value.MatchesFilter))
            {
                this.pointers.Value.Add(new Pointer(container));
            }
        }

        this.Log.Trace("{0}: Found {1} chests", this.Id, this.pointers.Value.Count);
        this.currentIndex.Value = 0;
    }
}