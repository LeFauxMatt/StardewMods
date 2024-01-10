namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Objects;

/// <summary>Unload a held chest's contents into another chest.</summary>
internal sealed class UnloadChest : BaseFeature<UnloadChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IInputHelper inputHelper;
    private readonly ProxyChestFactory proxyChestFactory;

    /// <summary>Initializes a new instance of the <see cref="UnloadChest" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public UnloadChest(
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        ProxyChestFactory proxyChestFactory)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.inputHelper = inputHelper;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.UnloadChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);

    /// <inheritdoc />
    protected override void Deactivate() => this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button)
            || !this.containerFactory.TryGetOneFromPlayer(Game1.player, out var containerFrom)
            || containerFrom.Options.UnloadChest != FeatureOption.Enabled)
        {
            return;
        }

        if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, e.Cursor.GrabTile, out var containerTo)
            || containerTo.Options.UnloadChest != FeatureOption.Enabled)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);

        if (this.Config.UnloadChestSwap)
        {
            if (!Game1.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out var obj)
                || obj is not Chest chest
                || !this.proxyChestFactory.TryCreateRequest(chest, out var request))
            {
                return;
            }

            Game1.currentLocation.Objects.Remove(e.Cursor.GrabTile);
            if (!Utility.tryToPlaceItem(
                Game1.currentLocation,
                Game1.player.ActiveObject,
                (int)e.Cursor.GrabTile.X * Game1.tileSize,
                (int)e.Cursor.GrabTile.Y * Game1.tileSize))
            {
                Game1.currentLocation.Objects.Add(e.Cursor.GrabTile, obj);
                request.Cancel();
                return;
            }

            Game1.player.ActiveObject = request.Item;
            request.Confirm();

            // Swap container from and to
            if (!this.containerFactory.TryGetOneFromLocation(Game1.currentLocation, e.Cursor.GrabTile, out containerTo)
                || !this.containerFactory.TryGetOneFromPlayer(Game1.player, out containerFrom))
            {
                return;
            }
        }

        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts, true))
        {
            return;
        }

        foreach (var (name, amount) in amounts)
        {
            if (amount > 0)
            {
                this.Log.Trace(
                    "{0}: {{ Item: {1}, Quantity: {2}, From: {3}, To: {4} }}",
                    this.Id,
                    name,
                    amount,
                    containerFrom,
                    containerTo);
            }
        }
    }
}