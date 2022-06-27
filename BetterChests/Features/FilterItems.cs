namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Restricts what items can be added into a chest.
/// </summary>
internal class FilterItems : IFeature
{
    private const string Id = "furyx639.BetterChests/FilterItems";

    private FilterItems(IModHelper helper)
    {
        this.Helper = helper;
        HarmonyHelper.AddPatches(
            FilterItems.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                    typeof(FilterItems),
                    nameof(FilterItems.Chest_addItem_prefix),
                    PatchType.Prefix),
            });

        if (IntegrationHelper.Automate.IsLoaded)
        {
            var storeMethod = ReflectionHelper.GetAssemblyByName("Automate")?
                .GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")?
                .GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);
            if (storeMethod is not null)
            {
                HarmonyHelper.AddPatch(
                    FilterItems.Id,
                    storeMethod,
                    typeof(FilterItems),
                    nameof(FilterItems.Automate_Store_prefix));
            }
        }
    }

    private static FilterItems? Instance { get; set; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    /// <summary>
    ///     Initializes <see cref="FilterItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="FilterItems" /> class.</returns>
    public static FilterItems Init(IModHelper helper)
    {
        return FilterItems.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            HarmonyHelper.ApplyPatches(FilterItems.Id);
            this.Helper.Events.Display.MenuChanged += FilterItems.OnMenuChanged;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            HarmonyHelper.UnapplyPatches(FilterItems.Id);
            this.Helper.Events.Display.MenuChanged -= FilterItems.OnMenuChanged;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = FilterItems.Instance!.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !StorageHelper.TryGetOne(___Chest, out var storage) || storage.FilterMatches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!StorageHelper.TryGetOne(__instance, out var storage) || storage.FilterMatches(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu { context: { } context }
            || !StorageHelper.TryGetOne(context, out var storage)
            || storage.FilterItems == FeatureOption.Disabled)
        {
            return;
        }

        if (BetterItemGrabMenu.Inventory is not null)
        {
            BetterItemGrabMenu.Inventory.AddHighlighter(storage.FilterMatcher);
        }
    }
}