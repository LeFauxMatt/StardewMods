namespace StardewMods.BetterChests.Framework.Services.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Unload a held chest's contents into another chest.</summary>
internal sealed class UnloadChest : BaseFeature<UnloadChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly ContainerOperations containerOperations;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="UnloadChest" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerOperations">Dependency used for handling operations between containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public UnloadChest(
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerOperations containerOperations,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModEvents modEvents)
        : base(log, manifest, configManager)
    {
        this.containerFactory = containerFactory;
        this.containerOperations = containerOperations;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.UnloadChest != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate() => this.modEvents.Input.ButtonPressed += this.OnButtonPressed;

    /// <inheritdoc />
    protected override void Deactivate() => this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;

    [EventPriority(EventPriority.Normal + 10)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button)
            || Game1.player.CurrentItem is null
            || !this.containerFactory.TryGetOne(Game1.player.CurrentItem, out var containerFrom)
            || containerFrom.Options.UnloadChest != Option.Enabled)
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
            || !this.containerFactory.TryGetOne(obj, out var containerTo)
            || containerTo.Options.UnloadChest != Option.Enabled)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        if (!this.containerOperations.Transfer(containerFrom, containerTo, out var amounts))
        {
            return;
        }

        foreach (var (name, amount) in amounts)
        {
            if (amount > 0)
            {
                this.Log.Trace(
                    "{0}: {{ Item: {1}, Quantity: {2}, From: {3}, To: {4} }}",
                    [this.Id, name, amount, containerFrom, containerTo]);
            }
        }
    }
}