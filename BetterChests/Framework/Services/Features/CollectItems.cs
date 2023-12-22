namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;

/// <summary>Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.</summary>
internal sealed class CollectItems : BaseFeature
{
    private static readonly MethodBase DebrisCollect = AccessTools.Method(typeof(Debris), nameof(Debris.collect));

#nullable disable
    private static CollectItems instance;
#nullable enable
    private readonly PerScreen<List<IContainer>> cachedStorages = new(() => []);
    private readonly ContainerFactory containers;

    private readonly IModEvents events;
    private readonly Harmony harmony;
    private readonly IInputHelper input;
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="CollectItems" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="containers">Dependency used for accessing containers.</param>
    public CollectItems(ILogging logging, ModConfig modConfig, IModEvents events, IInputHelper input, Harmony harmony, ContainerFactory containers)
        : base(logging, modConfig)
    {
        CollectItems.instance = this;
        this.events = events;
        this.input = input;
        this.harmony = harmony;
        this.containers = containers;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.CollectItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.events.Player.InventoryChanged += this.OnInventoryChanged;

        // Patches
        this.harmony.Patch(CollectItems.DebrisCollect, transpiler: new HarmonyMethod(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.events.Player.InventoryChanged -= this.OnInventoryChanged;

        // Patches
        this.harmony.Unpatch(CollectItems.DebrisCollect, AccessTools.Method(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)));
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null)
        {
            return true;
        }

        // Redirect to vanilla if currently disabled
        if (Game1.player.modData.ContainsKey($"{CollectItems.instance.Prefix}/Disabled"))
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        // Check if cache needs to be refreshed
        if (CollectItems.instance.resetCache.Value)
        {
            CollectItems.instance.RefreshEligible();
            CollectItems.instance.resetCache.Value = false;
        }

        // Redirect to vanilla if no storages are available
        if (!CollectItems.instance.cachedStorages.Value.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        // Attempt to add item to storages
        foreach (var storage in CollectItems.instance.cachedStorages.Value)
        {
            if (storage.TryAdd(item, out var remaining) && remaining is null)
            {
                return true;
            }
        }

        // Revert to vanilla if item could not be added to any storages
        return farmer.addItemToInventoryBool(item, makeActiveObject);
    }

    private static IEnumerable<CodeInstruction> Debris_collect_transpiler(IEnumerable<CodeInstruction> instructions) =>
        instructions.MethodReplacer(AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)), AccessTools.Method(typeof(CollectItems), nameof(CollectItems.AddItemToInventoryBool)));

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        // Toggle Collect Items
        if (Context.IsPlayerFree && this.ModConfig.Controls.ToggleCollectItems.JustPressed())
        {
            var key = $"{this.Prefix}/Disabled";
            var disabled = Game1.player.modData.ContainsKey(key);
            if (disabled)
            {
                Game1.player.modData.Remove(key);
                return;
            }

            Game1.player.modData[key] = "true";
            this.input.SuppressActiveKeybinds(this.ModConfig.Controls.ToggleCollectItems);
        }
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e) => this.resetCache.Value = true;

    private void RefreshEligible()
    {
        this.cachedStorages.Value.Clear();
        foreach (var storage in this.containers.GetAllFromPlayer(Game1.player, storage => storage.Options.ChestFinder == FeatureOption.Enabled))
        {
            this.cachedStorages.Value.Add(storage);
        }
    }
}
