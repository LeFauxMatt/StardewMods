#nullable disable

namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Integrations.ToolbarIcons;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Locations;

/// <inheritdoc />
internal class StashToChest : Feature
{
    private readonly PerScreen<IManagedStorage> _currentStorage = new();
    private readonly Lazy<IMenuComponents> _menuComponents;

    /// <summary>
    ///     Initializes a new instance of the <see cref="StashToChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    /// <param name="toolbarIcons">Integration with Toolbar Icons.</param>
    public StashToChest(IConfigModel config, IModHelper helper, IModServices services, ToolbarIconsIntegration toolbarIcons)
        : base(config, helper, services)
    {
        this.ToolbarIcons = toolbarIcons;
        this._menuComponents = services.Lazy<IMenuComponents>();
    }

    /// <summary>
    ///     Gets a value indicating which storages are eligible for stashing into.
    /// </summary>
    public List<KeyValuePair<IGameObjectType, IManagedStorage>> EligibleStorages
    {
        get
        {
            var storages = new List<KeyValuePair<IGameObjectType, IManagedStorage>>();
            storages.AddRange(
                from inventoryStorage in this.ManagedObjects.InventoryStorages
                where inventoryStorage.Value.StashToChest >= FeatureOptionRange.Inventory
                      && inventoryStorage.Value.OpenHeldChest == FeatureOption.Enabled
                select new KeyValuePair<IGameObjectType, IManagedStorage>(inventoryStorage.Key, inventoryStorage.Value));

            foreach (var (locationObject, locationStorage) in this.ManagedObjects.LocationStorages)
            {
                // Disabled in config or by location name
                if (locationStorage.StashToChest == FeatureOptionRange.Disabled || locationStorage.StashToChestDisableLocations.Contains(Game1.player.currentLocation.Name))
                {
                    continue;
                }

                // Disabled in mines
                if (locationStorage.StashToChestDisableLocations.Contains("UndergroundMine") && Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine"))
                {
                    continue;
                }

                switch (locationStorage.StashToChest)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !locationObject.Location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when locationStorage.StashToChestDistance == -1:
                    case FeatureOptionRange.Location when Utility.withinRadiusOfPlayer((int)locationObject.Position.X * 64, (int)locationObject.Position.Y * 64, locationStorage.StashToChestDistance, Game1.player):
                        storages.Add(new(locationObject, locationStorage));
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    case FeatureOptionRange.Inventory:
                    default:
                        continue;
                }
            }

            return storages.OrderByDescending(storage => storage.Value.StashToChestPriority).ToList();
        }
    }

    private IManagedStorage CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
    }

    private IMenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    private ToolbarIconsIntegration ToolbarIcons { get; }

    /// <inheritdoc />
    protected override void Activate()
    {
        if (this.ToolbarIcons.IsLoaded)
        {
            this.ToolbarIcons.API!.AddToolbarIcon(
                "BetterChests.StashToChest",
                $"{BetterChests.ModUniqueId}/Icons",
                new(16, 0, 16, 16),
                I18n.Button_StashToChest_Name());
            this.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
        }

        this.CustomEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
        this.MenuComponents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        if (this.ToolbarIcons.IsLoaded)
        {
            this.ToolbarIcons.API!.RemoveToolbarIcon("BetterChests.StashToChest");
            this.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
        }

        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
        this.MenuComponents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private static bool StashItems(IManagedStorage managedStorage, IGameObjectType gameObjectType = null)
    {
        if (managedStorage is null)
        {
            return false;
        }

        var stashedAny = false;
        for (var index = 0; index < Game1.player.MaxItems; index++)
        {
            var item = Game1.player.Items[index];
            if (item?.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot") != false)
            {
                continue;
            }

            var stack = item.Stack;
            var tmp = managedStorage.StashItem(item);
            if (tmp is not null && stack == item.Stack)
            {
                continue;
            }

            stashedAny = true;
            switch (gameObjectType)
            {
                case InventoryItem(var farmer, var i):
                    Log.Info($"Item {item.Name} stashed in  {managedStorage.QualifiedItemId} with farmer {farmer.Name} at slot {i.ToString()}.\n");
                    break;
                case LocationObject(var gameLocation, var (x, y)):
                    Log.Info($"Item {item.Name} stashed in  \"{managedStorage.QualifiedItemId}\" at location {gameLocation.NameOrUniqueName} at coordinates ({((int)x).ToString()},{((int)y).ToString()}).");
                    break;
                default:
                    Log.Info($"Item {item.Name} stashed in  \"{managedStorage.QualifiedItemId}\" of currently accessed storage.");
                    break;
            }

            if (tmp is null)
            {
                Game1.player.Items[index] = null;
            }
        }

        return stashedAny;
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!this.Config.ControlScheme.StashItems.JustPressed())
        {
            return;
        }

        // Stash to Current
        if (this.CurrentStorage is not null && StashToChest.StashItems(this.CurrentStorage))
        {
            Game1.playSound("Ship");
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.StashItems);
            return;
        }

        // Stash to all
        if (Context.IsPlayerFree && this.StashItems())
        {
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.StashItems);
        }
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        this.CurrentStorage = e.Context is not null && this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedStorage) && managedStorage.StashToChest != FeatureOptionRange.Disabled
            ? managedStorage
            : null;
    }

    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (this.CurrentStorage is null || e.Component.ComponentType is not ComponentType.FillStacksButton || (e.Button is not SButton.MouseLeft && !e.Button.IsActionButton()))
        {
            return;
        }

        if (StashToChest.StashItems(this.CurrentStorage))
        {
            Game1.playSound("Ship");
            e.SuppressInput();
        }
    }

    private void OnToolbarIconPressed(object sender, string id)
    {
        if (id == "BetterChests.StashToChest")
        {
            this.StashItems();
        }
    }

    private bool StashItems()
    {
        var stashedAny = false;
        foreach (var (gameObjectType, eligibleStorage) in this.EligibleStorages)
        {
            if (StashToChest.StashItems(eligibleStorage, gameObjectType))
            {
                stashedAny = true;
            }
        }

        if (stashedAny)
        {
            Game1.playSound("Ship");
            return true;
        }

        Game1.showRedMessage(I18n.Alert_StashToChest_NoEligible());
        return false;
    }
}