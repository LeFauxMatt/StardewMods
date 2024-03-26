namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Allows a chest to be opened while in the farmer's inventory.</summary>
internal sealed class OpenHeldChest : BaseFeature<OpenHeldChest>
{
    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly ProxyChestFactory proxyChestFactory;

    /// <summary>Initializes a new instance of the <see cref="OpenHeldChest" /> class.</summary>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="proxyChestFactory">Dependency used for creating virtualized chests.</param>
    public OpenHeldChest(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        ProxyChestFactory proxyChestFactory)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.proxyChestFactory = proxyChestFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.OpenHeldChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            new HarmonyMethod(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            AccessTools.Method(typeof(OpenHeldChest), nameof(OpenHeldChest.Chest_addItem_prefix)));
    }

    // TODO: Recursive check

    /// <summary>Prevent adding chest into itself.</summary>
    [HarmonyPriority(Priority.High)]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (__instance != item)
        {
            return true;
        }

        __result = item;
        return false;
    }

    /// <summary>Open inventory for currently held chest.</summary>
    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsActionButton()
            || !this.containerFactory.TryGetOneFromPlayer(Game1.player, out var container)
            || container.Options.OpenHeldChest != FeatureOption.Enabled)
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        container.Mutex?.RequestLock(
            () =>
            {
                container.ShowMenu();
            });
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e) =>
        this.itemGrabMenuManager.Bottom.AddHighlightMethod(this.MatchesFilter);

    private bool MatchesFilter(Item item)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
            || !this.proxyChestFactory.TryGetProxy(item, out var chest))
        {
            return true;
        }

        // Prevent chest from being added into itself
        return itemGrabMenu.sourceItem != chest;
    }
}