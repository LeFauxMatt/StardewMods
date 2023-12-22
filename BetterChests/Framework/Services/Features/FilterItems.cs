namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using HarmonyLib;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.Automate;
using StardewValley.Objects;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class FilterItems : BaseFeature
{
    private static readonly MethodBase ChestAddItem = AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem));

#nullable disable
    private static FilterItems instance;
#nullable enable
    private readonly AutomateIntegration automate;
    private readonly ContainerFactory containerFactory;

    private readonly Harmony harmony;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IReflectionHelper reflection;
    private readonly IModRegistry registry;

    private MethodBase? storeMethod;

    /// <summary>Initializes a new instance of the <see cref="FilterItems" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="registry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    /// <param name="automate">Dependency for integration with Automate.</param>
    /// <param name="containerFactory">Dependency for handling storages.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    public FilterItems(
        ILogging logging,
        ModConfig modConfig,
        Harmony harmony,
        IModRegistry registry,
        IReflectionHelper reflection,
        AutomateIntegration automate,
        ContainerFactory containerFactory,
        ItemGrabMenuManager itemGrabMenuManager)
        : base(logging, modConfig)
    {
        FilterItems.instance = this;
        this.harmony = harmony;
        this.registry = registry;
        this.reflection = reflection;
        this.automate = automate;
        this.containerFactory = containerFactory;
        this.itemGrabMenuManager = itemGrabMenuManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.FilterItems != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Patch(FilterItems.ChestAddItem, new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (this.automate.IsLoaded)
        {
            return;
        }

        this.storeMethod = this
            .registry.Get(this.automate.UniqueId)
            ?.GetType()
            .Assembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")
            ?.GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);

        if (this.storeMethod is not null)
        {
            this.harmony.Patch(this.storeMethod, new HarmonyMethod(typeof(FilterItems), nameof(FilterItems.Automate_Store_prefix)));
        }
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Unpatch(FilterItems.ChestAddItem, AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (this.storeMethod is not null)
        {
            this.harmony.Unpatch(this.storeMethod, AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Automate_Store_prefix)));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = FilterItems.instance.reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !FilterItems.instance.containerFactory.TryGetOne(___Chest, out var storage) || storage.MatchesFilter(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!FilterItems.instance.containerFactory.TryGetOne(__instance, out var storage) || storage.MatchesFilter(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.Context?.Options.FilterItems != FeatureOption.Enabled)
        {
            return;
        }

        this.itemGrabMenuManager.BottomMenu.AddHighlightMethod(e.Context.MatchesFilter);
    }
}
