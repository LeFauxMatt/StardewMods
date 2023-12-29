namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Debris such as mined or farmed items can be collected into a Chest in the farmer's inventory.</summary>
internal sealed class CollectItems : BaseFeature<CollectItems>
{
#nullable disable
    private static CollectItems instance;
#nullable enable

    private readonly PerScreen<List<IContainer>> cachedContainers = new(() => []);
    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="CollectItems" /> class.</summary>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public CollectItems(
        ContainerFactory containerFactory,
        Harmony harmony,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents)
        : base(log, manifest, modConfig)
    {
        CollectItems.instance = this;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CollectItems != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.modEvents.Player.InventoryChanged += this.OnInventoryChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Debris), nameof(Debris.collect)),
            transpiler: new HarmonyMethod(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.modEvents.Player.InventoryChanged -= this.OnInventoryChanged;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(Debris), nameof(Debris.collect)),
            AccessTools.DeclaredMethod(typeof(CollectItems), nameof(CollectItems.Debris_collect_transpiler)));
    }

    private static bool AddItemToInventoryBool(Farmer farmer, Item? item, bool makeActiveObject)
    {
        if (item is null)
        {
            return true;
        }

        // Redirect to vanilla if currently disabled
        if (Game1.player.modData.ContainsKey(CollectItems.instance.Prefix + "Disable"))
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
        if (!CollectItems.instance.cachedContainers.Value.Any())
        {
            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        // Attempt to add item to storages
        foreach (var storage in CollectItems.instance.cachedContainers.Value)
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
        instructions.MethodReplacer(
            AccessTools.Method(typeof(Farmer), nameof(Farmer.addItemToInventoryBool)),
            AccessTools.Method(typeof(CollectItems), nameof(CollectItems.AddItemToInventoryBool)));

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        // Toggle Collect Items
        if (Context.IsPlayerFree && this.Config.Controls.ToggleCollectItems.JustPressed())
        {
            this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleCollectItems);
            var key = this.Prefix + "Disable";
            var disable = Game1.player.modData.ContainsKey(key);
            if (disable)
            {
                Game1.player.modData.Remove(key);
                this.Log.Trace("{0}: Set collect items on", [this.Id]);
                return;
            }

            Game1.player.modData[key] = "true";
            this.Log.Trace("{0}: Set collect items off", [this.Id]);
        }
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e) => this.resetCache.Value = true;

    private void RefreshEligible()
    {
        this.cachedContainers.Value.Clear();
        foreach (var storage in this.containerFactory.GetAllFromPlayer(
            Game1.player,
            container => container.Options.ChestFinder == Option.Enabled))
        {
            this.cachedContainers.Value.Add(storage);
        }
    }
}