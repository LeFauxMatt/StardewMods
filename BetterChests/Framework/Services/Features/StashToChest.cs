namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>Stash items into placed chests and chests in the farmer's inventory.</summary>
internal sealed class StashToChest : BaseFeature<StashToChest>
{
    private readonly AssetHandler assetHandler;
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="StashToChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public StashToChest(
        AssetHandler assetHandler,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerHandler containerHandler,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModEvents modEvents,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(log, manifest, configManager)
    {
        this.assetHandler = assetHandler;
        this.containerFactory = containerFactory;
        this.containerHandler = containerHandler;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.StashToChest != RangeOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.Id,
            this.assetHandler.IconTexturePath,
            new Rectangle(16, 0, 16, 16),
            I18n.Button_StashToChest_Name());

        this.toolbarIconsIntegration.Api.IconPressed += this.OnIconPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
        this.toolbarIconsIntegration.Api.IconPressed -= this.OnIconPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
            || !this.containerFactory.TryGetOneFromMenu(out var container)
            || container.Options.StashToChest is RangeOption.Disabled or RangeOption.Default)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if ((Game1.activeClickableMenu as ItemGrabMenu)?.fillStacksButton?.containsPoint(x, y) != true)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.StashIntoContainer(container);
        Game1.playSound("Ship");
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.Config.Controls.StashItems.JustPressed())
        {
            return;
        }

        // Stash to All
        if (Context.IsPlayerFree)
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.StashItems);
            this.StashIntoAll();
            return;
        }

        if (!this.containerFactory.TryGetOneFromMenu(out var container)
            || container.Options.StashToChest is RangeOption.Disabled or RangeOption.Default)
        {
            return;
        }

        // Stash to Current
        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.StashItems);
        this.StashIntoContainer(container);
        Game1.playSound("Ship");
    }

    private void OnIconPressed(object? sender, IIconPressedEventArgs e)
    {
        if (e.Id == this.Id)
        {
            this.StashIntoAll();
        }
    }

    private void StashIntoAll()
    {
        if (!this.containerFactory.TryGetOne(Game1.player, out var containerFrom))
        {
            return;
        }

        var containerGroups =
            this
                .containerFactory.GetAll(Predicate)
                .GroupBy(container => container.Options.StashToChestPriority)
                .ToDictionary(group => group.Key, group => group.ToList());

        if (!containerGroups.Any())
        {
            Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
            return;
        }

        var topPriority = containerGroups.Keys.Max();
        var bottomPriority = containerGroups.Keys.Min();
        var stashedAny = false;

        for (var priority = topPriority; priority >= bottomPriority; --priority)
        {
            if (!containerGroups.TryGetValue(priority, out var containersTo))
            {
                continue;
            }

            var noneEligible = false;
            foreach (var containerTo in containersTo)
            {
                if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts))
                {
                    noneEligible = true;
                    break;
                }

                foreach (var (name, amount) in amounts)
                {
                    if (amount <= 0)
                    {
                        continue;
                    }

                    stashedAny = true;
                    this.Log.Trace(
                        "{0}: {{ Item: {1}, Quantity: {2}, From: {3}, To: {4} }}",
                        [this.Id, name, amount, containerFrom, containerTo]);
                }
            }

            if (noneEligible)
            {
                break;
            }
        }

        if (!stashedAny)
        {
            Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
            return;
        }

        Game1.playSound("Ship");
        return;

        bool Predicate(IStorageContainer container) =>
            container.Options.StashToChest is not (RangeOption.Disabled or RangeOption.Default)
            && container.Items.Count < container.Capacity
            && !this.Config.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name)
            && !(this.Config.StashToChestDisableLocations.Contains("UndergroundMine")
                && Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
            && container.Options.StashToChest.WithinRange(
                container.Options.StashToChestDistance,
                container.Location,
                container.TileLocation);
    }

    private void StashIntoContainer(IStorageContainer containerTo)
    {
        if (!this.containerFactory.TryGetOne(Game1.player, out var containerFrom))
        {
            return;
        }

        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts))
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