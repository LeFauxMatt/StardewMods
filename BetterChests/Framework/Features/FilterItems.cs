namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.Common.Enums;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class FilterItems : BaseFeature
{
    private static readonly MethodBase ChestAddItem = AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem));

#nullable disable
    private static FilterItems instance;
#nullable enable
    private readonly IModEvents events;

    private readonly Harmony harmony;
    private readonly IReflectionHelper reflection;
    private readonly IModRegistry registry;

    private MethodBase? storeMethod;

    /// <summary>Initializes a new instance of the <see cref="FilterItems" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="registry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    public FilterItems(
        IMonitor monitor,
        ModConfig config,
        IModEvents events,
        Harmony harmony,
        IModRegistry registry,
        IReflectionHelper reflection)
        : base(monitor, nameof(FilterItems), () => config.FilterItems is not FeatureOption.Disabled)
    {
        FilterItems.instance = this;
        this.events = events;
        this.harmony = harmony;
        this.registry = registry;
        this.reflection = reflection;
    }

    private static IReflectionHelper Reflection => FilterItems.instance.reflection;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Display.MenuChanged += FilterItems.OnMenuChanged;

        // Patches
        this.harmony.Patch(
            FilterItems.ChestAddItem,
            new(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (!Integrations.Automate.IsLoaded)
        {
            return;
        }

        this.storeMethod = this.registry.Get(Integrations.Automate.UniqueId)
            ?.GetType()
            .Assembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")
            ?.GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);

        if (this.storeMethod is not null)
        {
            this.harmony.Patch(this.storeMethod, new(typeof(FilterItems), nameof(FilterItems.Automate_Store_prefix)));
        }
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Display.MenuChanged -= FilterItems.OnMenuChanged;

        // Patches
        this.harmony.Unpatch(
            FilterItems.ChestAddItem,
            AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (this.storeMethod is not null)
        {
            this.harmony.Unpatch(
                this.storeMethod,
                AccessTools.Method(typeof(FilterItems), nameof(FilterItems.Automate_Store_prefix)));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = FilterItems.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !StorageHandler.TryGetOne(___Chest, out var storage) || storage.FilterMatches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!StorageHandler.TryGetOne(__instance, out var storage) || storage.FilterMatches(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu
            || BetterItemGrabMenu.Context is not
            {
                FilterItems: FeatureOption.Enabled,
            })
        {
            return;
        }

        BetterItemGrabMenu.Inventory?.AddHighlighter(BetterItemGrabMenu.Context.FilterMatcher);
    }
}
