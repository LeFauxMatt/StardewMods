namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Adds a search bar to the top of the <see cref="ItemGrabMenu" />.</summary>
internal sealed class SearchItems : BaseFeature<SearchItems>
{
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<ItemMatcher> itemMatcher;
    private readonly IModEvents modEvents;
    private readonly PerScreen<SearchBar> searchBar;

    /// <summary>Initializes a new instance of the <see cref="SearchItems" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public SearchItems(
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ItemMatcherFactory itemMatcherFactory,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents)
        : base(log, manifest, modConfig)
    {
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modEvents = modEvents;

        this.itemMatcher = new PerScreen<ItemMatcher>(itemMatcherFactory.GetOneForSearch);
        this.searchBar = new PerScreen<SearchBar>(
            () => new SearchBar(
                () => this.itemMatcher.Value.SearchText,
                value =>
                {
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        this.Log.Trace("{0}: Searching for {1}", [this.Id, value]);
                    }

                    this.itemMatcher.Value.SearchText = value;
                }));
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.SearchItems != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Display.RenderingActiveMenu -= this.OnRenderingActiveMenu;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    private IEnumerable<Item> FilterBySearch(IEnumerable<Item> items) =>
        this.itemMatcher.Value.IsEmpty
            ? items
            : this.Config.SearchItemsMethod switch
            {
                Method.Sorted or Method.GrayedOut => items.OrderByDescending(this.itemMatcher.Value.MatchesFilter),
                Method.Hidden => items.Where(this.itemMatcher.Value.MatchesFilter),
                _ => items,
            };

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft:
                this.searchBar.Value.LeftClick(mouseX, mouseY);
                break;
            case SButton.MouseRight:
                this.searchBar.Value.RightClick(mouseX, mouseY);
                break;
            case SButton.Escape when this.itemGrabMenuManager.CurrentMenu.readyToClose():
                Game1.playSound("bigDeSelect");
                this.itemGrabMenuManager.CurrentMenu.exitThisMenu();
                this.inputHelper.Suppress(e.Button);
                return;
            case SButton.Escape: return;
        }

        if (this.searchBar.Value.Selected)
        {
            this.inputHelper.Suppress(e.Button);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.SearchItems != Option.Enabled
            || !this.Config.Controls.ToggleSearch.JustPressed())
        {
            return;
        }

        this.isActive.Value = !this.isActive.Value;
        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleSearch);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        this.searchBar.Value.Draw(e.SpriteBatch);
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (!this.isActive.Value || this.itemGrabMenuManager.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.searchBar.Value.Update(mouseX, mouseY);
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Menu is null
            || this.itemGrabMenuManager.Top.Container?.Options.SearchItems != Option.Enabled)
        {
            this.isActive.Value = false;
            this.searchBar.Value.Clear();
            return;
        }

        var top = this.itemGrabMenuManager.Top;
        this.isActive.Value = true;
        this.searchBar.Value.MoveTo(
            top.Menu.xPositionOnScreen + 512,
            top.Menu.yPositionOnScreen - (IClickableMenu.borderWidth / 2) - Game1.tileSize - (top.Rows == 3 ? 20 : 4));

        this.searchBar.Value.SetWidth(top.Columns == 12 ? 284 : 384);
        this.itemGrabMenuManager.Top.AddHighlightMethod(this.itemMatcher.Value.MatchesFilter);
        this.itemGrabMenuManager.Top.AddOperation(this.FilterBySearch);
    }
}