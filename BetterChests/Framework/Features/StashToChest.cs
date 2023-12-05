namespace StardewMods.BetterChests.Framework.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>Stash items into placed chests and chests in the farmer's inventory.</summary>
internal sealed class StashToChest : BaseFeature
{
#nullable disable
    private static StashToChest instance;
#nullable enable

    private readonly ModConfig config;
    private readonly IModEvents events;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="StashToChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public StashToChest(IMonitor monitor, ModConfig config, IModEvents events, IInputHelper input)
        : base(monitor, nameof(StashToChest), () => config.StashToChest is not FeatureOptionRange.Disabled)
    {
        StashToChest.instance = this;
        this.config = config;
        this.events = events;
        this.input = input;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.events.Input.ButtonPressed += this.OnButtonPressed;

        // Integrations
        if (!IntegrationService.ToolbarIcons.IsLoaded)
        {
            return;
        }

        IntegrationService.ToolbarIcons.Api.AddToolbarIcon(
            "BetterChests.StashToChest",
            "furyx639.BetterChests/Icons",
            new(16, 0, 16, 16),
            I18n.Button_StashToChest_Name());

        IntegrationService.ToolbarIcons.Api.ToolbarIconPressed += StashToChest.OnToolbarIconPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.events.Input.ButtonPressed -= this.OnButtonPressed;

        // Integrations
        if (!IntegrationService.ToolbarIcons.IsLoaded)
        {
            return;
        }

        IntegrationService.ToolbarIcons.Api.RemoveToolbarIcon("BetterChests.StashToChest");
        IntegrationService.ToolbarIcons.Api.ToolbarIconPressed -= StashToChest.OnToolbarIconPressed;
    }

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == "BetterChests.StashToChest")
        {
            StashToChest.StashIntoAll();
        }
    }

    private static void StashIntoAll()
    {
        var stashedAny = false;
        var storages = StorageService.All.ToArray();
        Array.Sort(storages);

        foreach (var storage in storages)
        {
            if (storage.StashToChest is FeatureOptionRange.Disabled or FeatureOptionRange.Default
                || storage.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name)
                || (storage.StashToChestDisableLocations.Contains("UndergroundMine")
                    && Game1.player.currentLocation is MineShaft mineShaft
                    && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
                || storage is not
                {
                    Data: Storage storageObject,
                }
                || !storage.StashToChest.WithinRangeOfPlayer(
                    storage.StashToChestDistance,
                    storageObject.Location,
                    storageObject.Position)
                || !StashToChest.StashIntoStorage(storage))
            {
                continue;
            }

            stashedAny = true;
        }

        if (stashedAny)
        {
            Game1.playSound("Ship");
            return;
        }

        Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
    }

    private static bool StashIntoStorage(StorageNode storage)
    {
        var stashedAny = false;

        for (var index = 0; index < Game1.player.MaxItems; ++index)
        {
            if (Game1.player.Items[index] is null
                || Game1.player.Items[index].modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
            {
                continue;
            }

            var stack = Game1.player.Items[index].Stack;
            var tmp = storage.StashItem(
                StashToChest.instance.Monitor,
                Game1.player.Items[index],
                storage.StashToChestStacks is FeatureOption.Enabled);

            if (tmp is null)
            {
                Game1.player.Items[index] = null;
            }

            stashedAny = stashedAny || Game1.player.Items[index] is null || stack != Game1.player.Items[index].Stack;
        }

        return stashedAny;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
            || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
            || BetterItemGrabMenu.Context is not
            {
                StashToChest: FeatureOptionRange.Inventory or FeatureOptionRange.Location or FeatureOptionRange.World,
            })
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.fillStacksButton?.containsPoint(x, y) != true)
        {
            return;
        }

        this.input.Suppress(e.Button);
        StashToChest.StashIntoStorage(BetterItemGrabMenu.Context);
        Game1.playSound("Ship");
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.config.ControlScheme.StashItems.JustPressed())
        {
            return;
        }

        // Stash to All
        if (Context.IsPlayerFree)
        {
            StashToChest.StashIntoAll();
            this.input.SuppressActiveKeybinds(this.config.ControlScheme.StashItems);
            return;
        }

        if (Game1.activeClickableMenu is not ItemGrabMenu
            || BetterItemGrabMenu.Context is not
            {
                StashToChest: FeatureOptionRange.Inventory or FeatureOptionRange.Location or FeatureOptionRange.World,
            })
        {
            return;
        }

        // Stash to Current
        this.input.SuppressActiveKeybinds(this.config.ControlScheme.StashItems);
        StashToChest.StashIntoStorage(BetterItemGrabMenu.Context);
        Game1.playSound("Ship");
    }
}
