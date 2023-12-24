namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Stash items into placed chests and chests in the farmer's inventory.</summary>
internal sealed class StashToChest : BaseFeature
{
    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="StashToChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public StashToChest(
        ILog log,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        IInputHelper inputHelper,
        IModEvents modEvents,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(log, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.StashToChest != RangeOption.Disabled;

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
            AssetHandler.IconTexturePath,
            new Rectangle(16, 0, 16, 16),
            I18n.Button_StashToChest_Name());

        this.toolbarIconsIntegration.Api.ToolbarIconPressed += this.OnToolbarIconPressed;
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
        this.toolbarIconsIntegration.Api.ToolbarIconPressed -= this.OnToolbarIconPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
            || Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            } itemGrabMenu
            || !this.containerFactory.TryGetOne(chest, out var container)
            || container.Options.StashToChest is RangeOption.Disabled or RangeOption.Default)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.fillStacksButton?.containsPoint(x, y) != true)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.StashIntoContainer(container);
        Game1.playSound("Ship");
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.ModConfig.Controls.StashItems.JustPressed())
        {
            return;
        }

        // Stash to All
        if (Context.IsPlayerFree)
        {
            this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.StashItems);
            this.StashIntoAll();
            return;
        }

        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            }
            || !this.containerFactory.TryGetOne(chest, out var container)
            || container.Options.StashToChest is RangeOption.Disabled or RangeOption.Default)
        {
            return;
        }

        // Stash to Current
        this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.StashItems);
        this.StashIntoContainer(container);
        Game1.playSound("Ship");
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == this.Id)
        {
            this.StashIntoAll();
        }
    }

    private void StashIntoAll()
    {
        if (!this.containerFactory.TryGetOne(Game1.player, out var farmerContainer))
        {
            return;
        }

        var containerGroups =
            this
                .containerFactory.GetAll(Predicate)
                .GroupBy(container => container.Options.StashToChestPriority)
                .ToDictionary(group => group.Key, group => group.ToList());

        var topPriority = containerGroups.Keys.Max();
        var bottomPriority = containerGroups.Keys.Min();

        var stashedAny = false;
        farmerContainer.ForEachItem(
            item =>
            {
                var stack = item.Stack;
                for (var priority = topPriority; priority >= bottomPriority; --priority)
                {
                    if (!containerGroups.TryGetValue(priority, out var storages))
                    {
                        continue;
                    }

                    foreach (var storage in storages)
                    {
                        if (!farmerContainer.Transfer(item, storage, out var remaining))
                        {
                            continue;
                        }

                        stashedAny = true;
                        var amount = stack - (remaining?.Stack ?? 0);
                        this.Log.Trace(
                            "StashToChest: {{ Item: {0}, Quantity: {1}, From: {2}, To: {3} }}",
                            item.Name,
                            amount.ToString(CultureInfo.InvariantCulture),
                            farmerContainer,
                            storage);

                        return true;
                    }
                }

                return true;
            });

        if (!stashedAny)
        {
            Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
            return;
        }

        Game1.playSound("Ship");
        return;

        bool Predicate(IContainer container) =>
            container.Options.StashToChest is RangeOption.Disabled or RangeOption.Default
            && !container.Options.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name)
            && !(container.Options.StashToChestDisableLocations.Contains("UndergroundMine")
                && Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
            && container.Options.StashToChest.WithinRangeOfPlayer(
                container.Options.StashToChestDistance,
                container.Location,
                container.TileLocation);
    }

    private void StashIntoContainer(IContainer container)
    {
        if (!this.containerFactory.TryGetOne(Game1.player, out var farmerContainer))
        {
            return;
        }

        farmerContainer.ForEachItem(
            item =>
            {
                var stack = item.Stack;
                if (!farmerContainer.Transfer(item, container, out var remaining))
                {
                    return true;
                }

                var amount = stack - (remaining?.Stack ?? 0);
                this.Log.Trace(
                    "StashToChest: {{ Item: {0}, Quantity: {1}, From: {2}, To: {3} }}",
                    item.Name,
                    amount.ToString(CultureInfo.InvariantCulture),
                    farmerContainer,
                    container);

                return true;
            });
    }
}