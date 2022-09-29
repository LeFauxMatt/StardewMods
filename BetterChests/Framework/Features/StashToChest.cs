namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Handlers;
using StardewMods.Common.Enums;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>
///     Stash items into placed chests and chests in the farmer's inventory.
/// </summary>
internal sealed class StashToChest : IFeature
{
#nullable disable
    private static IFeature Instance;
#nullable enable

    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private bool _isActivated;

    private StashToChest(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    /// <summary>
    ///     Initializes <see cref="StashToChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="StashToChest" /> class.</returns>
    public static IFeature Init(IModHelper helper, ModConfig config)
    {
        return StashToChest.Instance ??= new StashToChest(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        if (!Integrations.ToolbarIcons.IsLoaded)
        {
            return;
        }

        Integrations.ToolbarIcons.API.AddToolbarIcon(
            "BetterChests.StashToChest",
            "furyx639.BetterChests/Icons",
            new(16, 0, 16, 16),
            I18n.Button_StashToChest_Name());
        Integrations.ToolbarIcons.API.ToolbarIconPressed += StashToChest.OnToolbarIconPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

        if (!Integrations.ToolbarIcons.IsLoaded)
        {
            return;
        }

        Integrations.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.StashToChest");
        Integrations.ToolbarIcons.API.ToolbarIconPressed -= StashToChest.OnToolbarIconPressed;
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
        var storages = Storages.All.ToArray();
        Array.Sort(storages);

        foreach (var storage in storages)
        {
            if (storage.StashToChest is not (FeatureOptionRange.Disabled or FeatureOptionRange.Default)
             && !storage.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name)
             && !(storage.StashToChestDisableLocations.Contains("UndergroundMine")
               && Game1.player.currentLocation is MineShaft mineShaft
               && mineShaft.Name.StartsWith("UndergroundMine"))
             && storage.StashToChest.WithinRangeOfPlayer(
                    storage.StashToChestDistance,
                    storage.Location,
                    storage.Position)
             && StashToChest.StashIntoStorage(storage))
            {
                stashedAny = true;
            }
        }

        if (stashedAny)
        {
            Game1.playSound("Ship");
            return;
        }

        Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
    }

    private static bool StashIntoStorage(BaseStorage storage)
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
            var tmp = storage.StashItem(Game1.player.Items[index], storage.StashToChestStacks is FeatureOption.Enabled);
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
         || BetterItemGrabMenu.Context?.StashToChest is null
                                                        or FeatureOptionRange.Disabled
                                                        or FeatureOptionRange.Default)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.fillStacksButton?.containsPoint(x, y) != true)
        {
            return;
        }

        this._helper.Input.Suppress(e.Button);
        StashToChest.StashIntoStorage(BetterItemGrabMenu.Context);
        Game1.playSound("Ship");
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this._config.ControlScheme.StashItems.JustPressed())
        {
            return;
        }

        // Stash to All
        if (Context.IsPlayerFree)
        {
            StashToChest.StashIntoAll();
            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.StashItems);
            return;
        }

        if (Game1.activeClickableMenu is not ItemGrabMenu
         || BetterItemGrabMenu.Context?.StashToChest is null
                                                        or FeatureOptionRange.Disabled
                                                        or FeatureOptionRange.Default)
        {
            return;
        }

        // Stash to Current
        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.StashItems);
        StashToChest.StashIntoStorage(BetterItemGrabMenu.Context);
        Game1.playSound("Ship");
    }
}