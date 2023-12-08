namespace StardewMods.BetterChests.Framework.Features;

using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Locations;

/// <summary>Craft using items from placed chests and chests in the farmer's inventory.</summary>
internal sealed class CraftFromChest : BaseFeature
{
    private readonly ModConfig config;
    private readonly IModEvents events;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="CraftFromChest" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public CraftFromChest(IMonitor monitor, ModConfig config, IModEvents events, IInputHelper input)
        : base(monitor, nameof(CraftFromChest), () => config.CraftFromChest is not FeatureOptionRange.Disabled)
    {
        this.config = config;
        this.events = events;
        this.input = input;
    }

    private static IEnumerable<StorageNode> Eligible
    {
        get
        {
            foreach (var storage in StorageHandler.All)
            {
                if (storage.CraftFromChest is FeatureOptionRange.Disabled or FeatureOptionRange.Default
                    || storage.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
                    || (storage.CraftFromChestDisableLocations.Contains("UndergroundMine")
                        && Game1.player.currentLocation is MineShaft mineShaft
                        && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
                    || storage is not
                    {
                        Data: Storage storageObject,
                    }
                    || !storage.CraftFromChest.WithinRangeOfPlayer(
                        storage.CraftFromChestDistance,
                        storageObject.Location,
                        storageObject.Position))
                {
                    continue;
                }

                yield return storage;
            }
        }
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading += CraftFromChest.OnCraftingStoragesLoading;
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;

        // Integrations
        if (IntegrationsManager.ToolbarIcons.IsLoaded)
        {
            IntegrationsManager.ToolbarIcons.Api.AddToolbarIcon(
                "BetterChests.CraftFromChest",
                "furyx639.BetterChests/Icons",
                new(32, 0, 16, 16),
                I18n.Button_CraftFromChest_Name());

            IntegrationsManager.ToolbarIcons.Api.ToolbarIconPressed += CraftFromChest.OnToolbarIconPressed;
        }

        if (!IntegrationsManager.BetterCrafting.IsLoaded)
        {
            return;
        }

        IntegrationsManager.BetterCrafting.Api.RegisterInventoryProvider(typeof(StorageNode), new StorageProvider());
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading -= CraftFromChest.OnCraftingStoragesLoading;
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Integrations
        if (IntegrationsManager.ToolbarIcons.IsLoaded)
        {
            IntegrationsManager.ToolbarIcons.Api.RemoveToolbarIcon("BetterChests.CraftFromChest");
            IntegrationsManager.ToolbarIcons.Api.ToolbarIconPressed -= CraftFromChest.OnToolbarIconPressed;
        }

        if (!IntegrationsManager.BetterCrafting.IsLoaded)
        {
            return;
        }

        IntegrationsManager.BetterCrafting.Api.UnregisterInventoryProvider(typeof(ChestStorage));
        IntegrationsManager.BetterCrafting.Api.UnregisterInventoryProvider(typeof(FridgeStorage));
        IntegrationsManager.BetterCrafting.Api.UnregisterInventoryProvider(typeof(JunimoHutStorage));
        IntegrationsManager.BetterCrafting.Api.UnregisterInventoryProvider(typeof(ObjectStorage));
        IntegrationsManager.BetterCrafting.Api.UnregisterInventoryProvider(typeof(ShippingBinStorage));
    }

    private static void OnCraftingStoragesLoading(object? sender, CraftingStoragesLoadingEventArgs e) =>
        e.AddStorages(CraftFromChest.Eligible);

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        if (id != "BetterChests.CraftFromChest")
        {
            return;
        }

        if (!CraftFromChest.Eligible.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        BetterCrafting.ShowCraftingPage();
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.config.ControlScheme.OpenCrafting.JustPressed())
        {
            return;
        }

        this.input.SuppressActiveKeybinds(this.config.ControlScheme.OpenCrafting);
        if (!CraftFromChest.Eligible.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        BetterCrafting.ShowCraftingPage();
    }
}
