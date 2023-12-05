namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class FilterItems : Feature
{
    private const string Id = "furyx639.BetterChests/FilterItems";

    private static readonly MethodBase ChestAddItem = AccessTools.Method(typeof(Chest), nameof(Chest.addItem));

#nullable disable
    private static FilterItems instance;
#nullable enable

    private readonly Harmony harmony;
    private readonly IModHelper helper;

    private MethodBase? storeMethod;

    private FilterItems(IModHelper helper)
    {
        this.helper = helper;
        this.harmony = new(FilterItems.Id);
    }

    private static IReflectionHelper Reflection => FilterItems.instance.helper.Reflection;

    /// <summary>Initializes <see cref="FilterItems" />.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="FilterItems" /> class.</returns>
    public static Feature Init(IModHelper helper) => FilterItems.instance ??= new(helper);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Display.MenuChanged += FilterItems.OnMenuChanged;

        // Patches
        this.harmony.Patch(
            FilterItems.ChestAddItem,
            new(typeof(FilterItems), nameof(FilterItems.Chest_addItem_prefix)));

        // Integrations
        if (!Integrations.Automate.IsLoaded)
        {
            return;
        }

        this.storeMethod = this.helper.ModRegistry.Get(Integrations.Automate.UniqueId)
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
        this.helper.Events.Display.MenuChanged -= FilterItems.OnMenuChanged;

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
        return !Storages.TryGetOne(___Chest, out var storage) || storage.FilterMatches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!Storages.TryGetOne(__instance, out var storage) || storage.FilterMatches(item))
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
