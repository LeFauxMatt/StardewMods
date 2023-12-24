namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Unload a held chest's contents into another chest.</summary>
internal sealed class UnloadChest : BaseFeature
{
    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="UnloadChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public UnloadChest(
        ILog log,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        IInputHelper inputHelper,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.UnloadChest != Option.Disabled;

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
            || !this.containerFactory.TryGetOne(Game1.player.CurrentItem, out var fromStorage)
            || fromStorage.Options.UnloadChest != Option.Enabled)
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
            || !this.containerFactory.TryGetOne(obj, out var toStorage)
            || toStorage.Options.UnloadChest != Option.Enabled)
        {
            return;
        }

        // Stash items into target chest
        for (var index = fromStorage.Items.Count - 1; index >= 0; --index)
        {
            var item = fromStorage.Items[index];
            if (item is null)
            {
                continue;
            }

            var stack = item.Stack;
            if (!fromStorage.Transfer(item, toStorage, out var remaining))
            {
                continue;
            }

            var amount = stack - (remaining?.Stack ?? 0);
            this.Log.Trace(
                "UnloadChest: {{ Item: {0}, Quantity: {1}, From: {2}, To: {3} }}",
                item.Name,
                amount.ToString(CultureInfo.InvariantCulture),
                fromStorage,
                toStorage);
        }

        this.inputHelper.Suppress(e.Button);
    }
}