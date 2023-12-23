namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

// TODO: Refactor UI/SearchBar to support SearchItems and ChestFinder

/// <summary>Adds a search bar to the top of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class SearchItems : BaseFeature
{
    private const int CountdownTimer = 20;
    private readonly PerScreen<string> cachedText = new(() => string.Empty);
    private readonly IInputHelper inputHelper;

    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<ItemMatcher> itemMatcher;

    private readonly IModEvents modEvents;
    private readonly PerScreen<ClickableComponent> searchArea;
    private readonly PerScreen<TextBox> searchField;
    private readonly PerScreen<ClickableTextureComponent> searchIcon;
    private readonly PerScreen<int> timeOut = new();

    /// <summary>Initializes a new instance of the <see cref="SearchItems" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public SearchItems(
        ILog log,
        ModConfig modConfig,
        IGameContentHelper gameContentHelper,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ItemMatcherFactory itemMatcherFactory,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modEvents = modEvents;

        this.itemMatcher = new PerScreen<ItemMatcher>(itemMatcherFactory.GetOneForSearch);
        this.searchArea =
            new PerScreen<ClickableComponent>(() => new ClickableComponent(Rectangle.Empty, string.Empty));

        this.searchField = new PerScreen<TextBox>(
            () => new TextBox(
                gameContentHelper.Load<Texture2D>("LooseSprites\\textBox"),
                null,
                Game1.smallFont,
                Game1.textColor));

        this.searchIcon = new PerScreen<ClickableTextureComponent>(
            () => new ClickableTextureComponent(
                Rectangle.Empty,
                Game1.mouseCursors,
                new Rectangle(80, 0, 13, 13),
                2.5f));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.SearchItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    private IEnumerable<Item> FilterBySearch(IEnumerable<Item> items)
    {
        if (this.ModConfig.DefaultOptions.HideUnselectedItems is FeatureOption.Enabled)
        {
            return items.Where(this.itemMatcher.Value.MatchesFilter);
        }

        return this.itemMatcher.Value.IsEmpty ? items : items.OrderByDescending(this.itemMatcher.Value.MatchesFilter);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || this.itemGrabMenuManager.CurrentMenu is null
            || e.Button is not (SButton.MouseLeft or SButton.MouseRight or SButton.ControllerA))
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when this.searchArea.Value.containsPoint(mouseX, mouseY):
                this.searchField.Value.Selected = true;
                break;
            case SButton.MouseRight when this.searchArea.Value.containsPoint(mouseX, mouseY):
                this.searchField.Value.Selected = true;
                this.searchField.Value.Text = string.Empty;
                break;
            case SButton.MouseLeft:
            case SButton.MouseRight:
                this.searchField.Value.Selected = false;
                break;
            case SButton.Escape when this.itemGrabMenuManager.CurrentMenu.readyToClose():
                Game1.playSound("bigDeSelect");
                this.itemGrabMenuManager.CurrentMenu.exitThisMenu();
                this.inputHelper.Suppress(e.Button);
                return;
            case SButton.Escape:
                return;
        }

        if (this.searchField.Value.Selected)
        {
            this.inputHelper.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.SearchItems != FeatureOption.Enabled
            || !this.ModConfig.Controls.ToggleSearch.JustPressed())
        {
            return;
        }

        this.isActive.Value = !this.isActive.Value;
        this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.ToggleSearch);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        this.searchField.Value.Draw(e.SpriteBatch, false);
        this.searchIcon.Value.draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!this.isActive.Value)
        {
            return;
        }

        if (this.timeOut.Value > 0 && --this.timeOut.Value == 0)
        {
            this.Log.Trace("SearchItems: {0}", this.cachedText.Value);
            this.itemMatcher.Value.SearchText = this.cachedText.Value;
        }

        if (this.searchField.Value.Text.Equals(this.cachedText.Value, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        this.timeOut.Value = SearchItems.CountdownTimer;
        this.cachedText.Value = this.searchField.Value.Text;
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.SearchItems != FeatureOption.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.itemGrabMenuManager.Top.AddHighlightMethod(this.itemMatcher.Value.MatchesFilter);
        this.itemGrabMenuManager.Top.AddOperation(this.FilterBySearch);
        this.isActive.Value = true;
    }
}