namespace StardewMods.FindAnything.Framework.Services;

using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.FindAnything;
using StardewMods.FindAnything.Framework.Interfaces;
using StardewMods.FindAnything.Framework.Models.Events;
using StardewMods.FindAnything.Framework.UI;

/// <summary>Responsible for handling searches.</summary>
internal sealed class SearchHandler : BaseService<SearchHandler>
{
    private readonly EventManager eventManager;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly IModConfig modConfig;
    private readonly PerScreen<List<Pointer>> pointers = new(() => []);
    private readonly PerScreen<bool> resetCache = new(() => true);
    private readonly PerScreen<SearchBar> searchBar;
    private readonly PerScreen<SearchOverlay> searchOverlay;
    private readonly PerScreen<string> searchTerm = new();

    /// <summary>Initializes a new instance of the <see cref="SearchHandler" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public SearchHandler(
        EventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(log, manifest)
    {
        // Init
        this.eventManager = eventManager;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;

        this.searchBar = new PerScreen<SearchBar>(
            () => new SearchBar(() => this.SearchTerm, value => this.SearchTerm = value));

        this.searchOverlay = new PerScreen<SearchOverlay>(() => new SearchOverlay(this.searchBar.Value));

        // Events
        this.eventManager.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
        this.eventManager.Subscribe<RenderingHudEventArgs>(this.OnRenderingHud);
        this.eventManager.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.eventManager.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.eventManager.Subscribe<WarpedEventArgs>(this.OnWarped);
    }

    private string SearchTerm
    {
        get => this.searchTerm.Value;
        set
        {
            this.Log.Trace("{0}: Searching for {1}", this.Id, value);
            this.searchTerm.Value = value;
            this.resetCache.Value = true;
        }
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
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

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!this.modConfig.Controls.ToggleSearch.JustPressed())
        {
            return;
        }

        switch (this.isActive.Value)
        {
            // Activate Search Bar
            case false when Context.IsPlayerFree && Game1.displayHUD && Game1.activeClickableMenu is null:
                this.OpenSearch();
                this.inputHelper.SuppressActiveKeybinds(this.modConfig.Controls.ToggleSearch);
                return;

            // Close Search Bar
            case true:
                this.CloseSearch();
                this.inputHelper.SuppressActiveKeybinds(this.modConfig.Controls.ToggleSearch);
                break;
        }
    }

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (!this.isActive.Value || !Game1.displayHUD || Game1.activeClickableMenu is SearchOverlay)
        {
            return;
        }

        // Check if search needs to be reset
        if (this.resetCache.Value)
        {
            // Do search
            this.resetCache.Value = false;
            this.Search();
        }

        // Check if there are any found objects
        foreach (var pointer in this.pointers.Value)
        {
            pointer.Draw(e.SpriteBatch);
        }

        this.searchOverlay.Value.draw(e.SpriteBatch);
    }

    private void OnRenderingHud(RenderingHudEventArgs e)
    {
        if (!this.isActive.Value || !Game1.displayHUD)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.searchBar.Value.Update(mouseX, mouseY);
    }

    private void OnWarped(WarpedEventArgs e) => this.resetCache.Value = true;

    private void OpenSearch()
    {
        this.isActive.Value = true;
        this.searchOverlay.Value.Show();
    }

    private void CloseSearch()
    {
        this.isActive.Value = false;
        this.pointers.Value.Clear();
    }

    private void Search()
    {
        this.pointers.Value.Clear();
        if (string.IsNullOrWhiteSpace(this.SearchTerm))
        {
            return;
        }

        var searchSubmittedEventArgs = new SearchSubmittedEventArgs(this.SearchTerm, Game1.currentLocation);
        this.eventManager.Publish<ISearchSubmitted, SearchSubmittedEventArgs>(searchSubmittedEventArgs);

        foreach (var searchResult in searchSubmittedEventArgs.SearchResults)
        {
            this.pointers.Value.Add(new Pointer(searchResult));
        }

        this.Log.Trace("{0}: Found {1} results for {2}", this.Id, this.pointers.Value.Count, this.SearchTerm);
    }
}