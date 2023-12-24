namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using HarmonyLib;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.Automate;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Objects;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class FilterItems : BaseFeature
{
#nullable disable
    private static FilterItems instance;
#nullable enable

    private readonly AutomateIntegration automateIntegration;
    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IModRegistry modRegistry;
    private readonly IReflectionHelper reflectionHelper;

    private MethodBase? storeMethod;

    /// <summary>Initializes a new instance of the <see cref="FilterItems" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="automateIntegration">Dependency for integration with Automate.</param>
    /// <param name="containerFactory">Dependency for handling storages.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public FilterItems(
        ILog log,
        ModConfig modConfig,
        AutomateIntegration automateIntegration,
        ContainerFactory containerFactory,
        Harmony harmony,
        ItemGrabMenuManager itemGrabMenuManager,
        IModRegistry modRegistry,
        IReflectionHelper reflectionHelper)
        : base(log, modConfig)
    {
        FilterItems.instance = this;
        this.automateIntegration = automateIntegration;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modRegistry = modRegistry;
        this.reflectionHelper = reflectionHelper;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.FilterItems != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (this.automateIntegration.IsLoaded)
        {
            return;
        }

        this.storeMethod = this
            .modRegistry.Get(this.automateIntegration.UniqueId)
            ?.GetType()
            .Assembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")
            ?.GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);

        if (this.storeMethod is not null)
        {
            this.harmony.Patch(
                this.storeMethod,
                new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Automate_Store_prefix)));
        }
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (this.storeMethod is not null)
        {
            this.harmony.Unpatch(
                this.storeMethod,
                AccessTools.DeclaredMethod(typeof(FilterItems), nameof(FilterItems.Automate_Store_prefix)));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = FilterItems.instance.reflectionHelper.GetProperty<Item>(stack, "Sample").GetValue();
        return !FilterItems.instance.containerFactory.TryGetOne(___Chest, out var container)
            || container.MatchesFilter(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!FilterItems.instance.containerFactory.TryGetOne(__instance, out var container)
            || container.MatchesFilter(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.FilterItems == Option.Enabled)
        {
            this.itemGrabMenuManager.Bottom.AddHighlightMethod(this.itemGrabMenuManager.Top.Container.MatchesFilter);
        }

        if (this.itemGrabMenuManager.Bottom.Container?.Options.FilterItems == Option.Enabled)
        {
            this.itemGrabMenuManager.Top.AddHighlightMethod(this.itemGrabMenuManager.Bottom.Container.MatchesFilter);
        }
    }
}