namespace StardewMods.Common.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc cref="IEventPublisher" />
internal sealed class EventManager : BaseEventManager
{
    private readonly IModEvents? modEvents;

    /// <summary>Initializes a new instance of the <see cref="EventManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to SMAPI events.</param>
    public EventManager(ILog log, IManifest manifest, IModEvents modEvents)
        : base(log, manifest) =>
        this.modEvents = modEvents;

    /// <inheritdoc />
    public override void Subscribe<TEventArgs>(Action<TEventArgs> handler)
    {
        base.Subscribe(handler);
        var eventType = typeof(TEventArgs);
        if (this.Subscribers[eventType].Count == 1)
        {
            this.AddSmapiEvent(eventType.Name);
        }
    }

    /// <inheritdoc />
    public override void Unsubscribe<TEventArgs>(Action<TEventArgs> handler)
    {
        base.Unsubscribe(handler);
        var eventType = typeof(TEventArgs);
        if (!this.Subscribers.ContainsKey(eventType))
        {
            this.RemoveSmapiEvent(eventType.Name);
        }
    }

    private void AddSmapiEvent(string eventName)
    {
        if (this.modEvents is null)
        {
            return;
        }

        switch (eventName)
        {
            // Content
            case nameof(AssetRequestedEventArgs):
                this.modEvents.Content.AssetRequested += this.PublishEvent;
                return;
            case nameof(AssetsInvalidatedEventArgs):
                this.modEvents.Content.AssetsInvalidated += this.PublishEvent;
                return;
            case nameof(AssetReadyEventArgs):
                this.modEvents.Content.AssetReady += this.PublishEvent;
                return;
            case nameof(LocaleChangedEventArgs):
                this.modEvents.Content.LocaleChanged += this.PublishEvent;
                return;

            // Display
            case nameof(MenuChangedEventArgs):
                this.modEvents.Display.MenuChanged += this.PublishEvent;
                return;
            case nameof(RenderingEventArgs):
                this.modEvents.Display.Rendering += this.PublishEvent;
                return;
            case nameof(RenderedEventArgs):
                this.modEvents.Display.Rendered += this.PublishEvent;
                return;
            case nameof(RenderingWorldEventArgs):
                this.modEvents.Display.RenderingWorld += this.PublishEvent;
                return;
            case nameof(RenderedWorldEventArgs):
                this.modEvents.Display.RenderedWorld += this.PublishEvent;
                return;
            case nameof(RenderingActiveMenuEventArgs):
                this.modEvents.Display.RenderingActiveMenu += this.PublishEvent;
                return;
            case nameof(RenderedActiveMenuEventArgs):
                this.modEvents.Display.RenderedActiveMenu += this.PublishEvent;
                return;
            case nameof(RenderingHudEventArgs):
                this.modEvents.Display.RenderingHud += this.PublishEvent;
                return;
            case nameof(RenderedHudEventArgs):
                this.modEvents.Display.RenderedHud += this.PublishEvent;
                return;
            case nameof(WindowResizedEventArgs):
                this.modEvents.Display.WindowResized += this.PublishEvent;
                return;

            // Game loop
            case nameof(GameLaunchedEventArgs):
                this.modEvents.GameLoop.GameLaunched += this.PublishEvent;
                return;
            case nameof(UpdateTickedEventArgs):
                this.modEvents.GameLoop.UpdateTicked += this.PublishEvent;
                return;
            case nameof(UpdateTickingEventArgs):
                this.modEvents.GameLoop.UpdateTicking += this.PublishEvent;
                return;
            case nameof(OneSecondUpdateTickedEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicked += this.PublishEvent;
                return;
            case nameof(OneSecondUpdateTickingEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicking += this.PublishEvent;
                return;
            case nameof(SaveCreatedEventArgs):
                this.modEvents.GameLoop.SaveCreated += this.PublishEvent;
                return;
            case nameof(SaveCreatingEventArgs):
                this.modEvents.GameLoop.SaveCreating += this.PublishEvent;
                return;
            case nameof(SavedEventArgs):
                this.modEvents.GameLoop.Saved += this.PublishEvent;
                return;
            case nameof(SavingEventArgs):
                this.modEvents.GameLoop.Saving += this.PublishEvent;
                return;
            case nameof(SaveLoadedEventArgs):
                this.modEvents.GameLoop.SaveLoaded += this.PublishEvent;
                return;
            case nameof(DayStartedEventArgs):
                this.modEvents.GameLoop.DayStarted += this.PublishEvent;
                return;
            case nameof(DayEndingEventArgs):
                this.modEvents.GameLoop.DayEnding += this.PublishEvent;
                return;
            case nameof(TimeChangedEventArgs):
                this.modEvents.GameLoop.TimeChanged += this.PublishEvent;
                return;
            case nameof(ReturnedToTitleEventArgs):
                this.modEvents.GameLoop.ReturnedToTitle += this.PublishEvent;
                return;

            // Input
            case nameof(ButtonsChangedEventArgs):
                this.modEvents.Input.ButtonsChanged += this.PublishEvent;
                return;
            case nameof(ButtonPressedEventArgs):
                this.modEvents.Input.ButtonPressed += this.PublishEvent;
                return;
            case nameof(ButtonReleasedEventArgs):
                this.modEvents.Input.ButtonReleased += this.PublishEvent;
                return;
            case nameof(CursorMovedEventArgs):
                this.modEvents.Input.CursorMoved += this.PublishEvent;
                return;
            case nameof(MouseWheelScrolledEventArgs):
                this.modEvents.Input.MouseWheelScrolled += this.PublishEvent;
                return;

            // Multiplayer
            case nameof(PeerContextReceivedEventArgs):
                this.modEvents.Multiplayer.PeerContextReceived += this.PublishEvent;
                return;
            case nameof(PeerConnectedEventArgs):
                this.modEvents.Multiplayer.PeerConnected += this.PublishEvent;
                return;
            case nameof(ModMessageReceivedEventArgs):
                this.modEvents.Multiplayer.ModMessageReceived += this.PublishEvent;
                return;
            case nameof(PeerDisconnectedEventArgs):
                this.modEvents.Multiplayer.PeerDisconnected += this.PublishEvent;
                return;

            // Player
            case nameof(InventoryChangedEventArgs):
                this.modEvents.Player.InventoryChanged += this.PublishEvent;
                return;
            case nameof(LevelChangedEventArgs):
                this.modEvents.Player.LevelChanged += this.PublishEvent;
                return;
            case nameof(WarpedEventArgs):
                this.modEvents.Player.Warped += this.PublishEvent;
                return;

            // World
            case nameof(LocationListChangedEventArgs):
                this.modEvents.World.LocationListChanged += this.PublishEvent;
                return;
            case nameof(BuildingListChangedEventArgs):
                this.modEvents.World.BuildingListChanged += this.PublishEvent;
                return;
            case nameof(ChestInventoryChangedEventArgs):
                this.modEvents.World.ChestInventoryChanged += this.PublishEvent;
                return;
            case nameof(DebrisListChangedEventArgs):
                this.modEvents.World.DebrisListChanged += this.PublishEvent;
                return;
            case nameof(FurnitureListChangedEventArgs):
                this.modEvents.World.FurnitureListChanged += this.PublishEvent;
                return;
            case nameof(LargeTerrainFeatureListChangedEventArgs):
                this.modEvents.World.LargeTerrainFeatureListChanged += this.PublishEvent;
                return;
            case nameof(NpcListChangedEventArgs):
                this.modEvents.World.NpcListChanged += this.PublishEvent;
                return;
            case nameof(ObjectListChangedEventArgs):
                this.modEvents.World.ObjectListChanged += this.PublishEvent;
                return;
            case nameof(TerrainFeatureListChangedEventArgs):
                this.modEvents.World.TerrainFeatureListChanged += this.PublishEvent;
                return;
        }
    }

    private void RemoveSmapiEvent(string eventName)
    {
        if (this.modEvents is null)
        {
            return;
        }

        switch (eventName)
        {
            // Content
            case nameof(AssetRequestedEventArgs):
                this.modEvents.Content.AssetRequested -= this.PublishEvent;
                return;
            case nameof(AssetsInvalidatedEventArgs):
                this.modEvents.Content.AssetsInvalidated -= this.PublishEvent;
                return;
            case nameof(AssetReadyEventArgs):
                this.modEvents.Content.AssetReady -= this.PublishEvent;
                return;
            case nameof(LocaleChangedEventArgs):
                this.modEvents.Content.LocaleChanged -= this.PublishEvent;
                return;

            // Display
            case nameof(MenuChangedEventArgs):
                this.modEvents.Display.MenuChanged -= this.PublishEvent;
                return;
            case nameof(RenderingEventArgs):
                this.modEvents.Display.Rendering -= this.PublishEvent;
                return;
            case nameof(RenderedEventArgs):
                this.modEvents.Display.Rendered -= this.PublishEvent;
                return;
            case nameof(RenderingWorldEventArgs):
                this.modEvents.Display.RenderingWorld -= this.PublishEvent;
                return;
            case nameof(RenderedWorldEventArgs):
                this.modEvents.Display.RenderedWorld -= this.PublishEvent;
                return;
            case nameof(RenderingActiveMenuEventArgs):
                this.modEvents.Display.RenderingActiveMenu -= this.PublishEvent;
                return;
            case nameof(RenderedActiveMenuEventArgs):
                this.modEvents.Display.RenderedActiveMenu -= this.PublishEvent;
                return;
            case nameof(RenderingHudEventArgs):
                this.modEvents.Display.RenderingHud -= this.PublishEvent;
                return;
            case nameof(RenderedHudEventArgs):
                this.modEvents.Display.RenderedHud -= this.PublishEvent;
                return;
            case nameof(WindowResizedEventArgs):
                this.modEvents.Display.WindowResized -= this.PublishEvent;
                return;

            // Game loop
            case nameof(GameLaunchedEventArgs):
                this.modEvents.GameLoop.GameLaunched -= this.PublishEvent;
                return;
            case nameof(UpdateTickedEventArgs):
                this.modEvents.GameLoop.UpdateTicked -= this.PublishEvent;
                return;
            case nameof(UpdateTickingEventArgs):
                this.modEvents.GameLoop.UpdateTicking -= this.PublishEvent;
                return;
            case nameof(OneSecondUpdateTickedEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicked -= this.PublishEvent;
                return;
            case nameof(OneSecondUpdateTickingEventArgs):
                this.modEvents.GameLoop.OneSecondUpdateTicking -= this.PublishEvent;
                return;
            case nameof(SaveCreatedEventArgs):
                this.modEvents.GameLoop.SaveCreated -= this.PublishEvent;
                return;
            case nameof(SaveCreatingEventArgs):
                this.modEvents.GameLoop.SaveCreating -= this.PublishEvent;
                return;
            case nameof(SavedEventArgs):
                this.modEvents.GameLoop.Saved -= this.PublishEvent;
                return;
            case nameof(SavingEventArgs):
                this.modEvents.GameLoop.Saving -= this.PublishEvent;
                return;
            case nameof(SaveLoadedEventArgs):
                this.modEvents.GameLoop.SaveLoaded -= this.PublishEvent;
                return;
            case nameof(DayStartedEventArgs):
                this.modEvents.GameLoop.DayStarted -= this.PublishEvent;
                return;
            case nameof(DayEndingEventArgs):
                this.modEvents.GameLoop.DayEnding -= this.PublishEvent;
                return;
            case nameof(TimeChangedEventArgs):
                this.modEvents.GameLoop.TimeChanged -= this.PublishEvent;
                return;
            case nameof(ReturnedToTitleEventArgs):
                this.modEvents.GameLoop.ReturnedToTitle -= this.PublishEvent;
                return;

            // Input
            case nameof(ButtonsChangedEventArgs):
                this.modEvents.Input.ButtonsChanged -= this.PublishEvent;
                return;
            case nameof(ButtonPressedEventArgs):
                this.modEvents.Input.ButtonPressed -= this.PublishEvent;
                return;
            case nameof(ButtonReleasedEventArgs):
                this.modEvents.Input.ButtonReleased -= this.PublishEvent;
                return;
            case nameof(CursorMovedEventArgs):
                this.modEvents.Input.CursorMoved -= this.PublishEvent;
                return;
            case nameof(MouseWheelScrolledEventArgs):
                this.modEvents.Input.MouseWheelScrolled -= this.PublishEvent;
                return;

            // Multiplayer
            case nameof(PeerContextReceivedEventArgs):
                this.modEvents.Multiplayer.PeerContextReceived -= this.PublishEvent;
                return;
            case nameof(PeerConnectedEventArgs):
                this.modEvents.Multiplayer.PeerConnected -= this.PublishEvent;
                return;
            case nameof(ModMessageReceivedEventArgs):
                this.modEvents.Multiplayer.ModMessageReceived -= this.PublishEvent;
                return;
            case nameof(PeerDisconnectedEventArgs):
                this.modEvents.Multiplayer.PeerDisconnected -= this.PublishEvent;
                return;

            // Player
            case nameof(InventoryChangedEventArgs):
                this.modEvents.Player.InventoryChanged -= this.PublishEvent;
                return;
            case nameof(LevelChangedEventArgs):
                this.modEvents.Player.LevelChanged -= this.PublishEvent;
                return;
            case nameof(WarpedEventArgs):
                this.modEvents.Player.Warped -= this.PublishEvent;
                return;

            // World
            case nameof(LocationListChangedEventArgs):
                this.modEvents.World.LocationListChanged -= this.PublishEvent;
                return;
            case nameof(BuildingListChangedEventArgs):
                this.modEvents.World.BuildingListChanged -= this.PublishEvent;
                return;
            case nameof(ChestInventoryChangedEventArgs):
                this.modEvents.World.ChestInventoryChanged -= this.PublishEvent;
                return;
            case nameof(DebrisListChangedEventArgs):
                this.modEvents.World.DebrisListChanged -= this.PublishEvent;
                return;
            case nameof(FurnitureListChangedEventArgs):
                this.modEvents.World.FurnitureListChanged -= this.PublishEvent;
                return;
            case nameof(LargeTerrainFeatureListChangedEventArgs):
                this.modEvents.World.LargeTerrainFeatureListChanged -= this.PublishEvent;
                return;
            case nameof(NpcListChangedEventArgs):
                this.modEvents.World.NpcListChanged -= this.PublishEvent;
                return;
            case nameof(ObjectListChangedEventArgs):
                this.modEvents.World.ObjectListChanged -= this.PublishEvent;
                return;
            case nameof(TerrainFeatureListChangedEventArgs):
                this.modEvents.World.TerrainFeatureListChanged -= this.PublishEvent;
                return;

            // Specialized
            case nameof(LoadStageChangedEventArgs):
                this.modEvents.Specialized.LoadStageChanged -= this.PublishEvent;
                return;
            case nameof(UnvalidatedUpdateTickedEventArgs):
                this.modEvents.Specialized.UnvalidatedUpdateTicked -= this.PublishEvent;
                return;
            case nameof(UnvalidatedUpdateTickingEventArgs):
                this.modEvents.Specialized.UnvalidatedUpdateTicking -= this.PublishEvent;
                return;
        }
    }

    private void PublishEvent<TEventArgs>(object? sender, TEventArgs eventArgs)
        where TEventArgs : EventArgs =>
        this.Publish(eventArgs);
}