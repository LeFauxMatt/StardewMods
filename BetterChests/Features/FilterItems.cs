namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Common.Enums;
using Common.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Objects;

// TODO: Add highlighter

/// <summary>
///     Restricts what items can be added into a chest.
/// </summary>
internal class FilterItems : IFeature
{
    private const string Id = "BetterChests.FilterItems";

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

        if (Integrations.Automate.IsLoaded)
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
        HarmonyHelper.ApplyPatches(FilterItems.Id);
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(FilterItems.Id);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = FilterItems.Instance!.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        if (!StorageHelper.TryGetOne(___Chest, out var storage)
            || storage.FilterItems == FeatureOption.Disabled
            || storage.FilterItemsList?.Any() != true)
        {
            return true;
        }

        var itemMatcher = new ItemMatcher(true);
        foreach (var filter in storage.FilterItemsList)
        {
            itemMatcher.Add(filter);
        }

        return itemMatcher.Matches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!StorageHelper.TryGetOne(__instance, out var storage)
            || storage.FilterItems == FeatureOption.Disabled
            || storage.FilterItemsList?.Any() != true)
        {
            return true;
        }

        var itemMatcher = new ItemMatcher(true);
        foreach (var filter in storage.FilterItemsList)
        {
            itemMatcher.Add(filter);
        }

        if (itemMatcher.Matches(item))
        {
            __result = item;
            return false;
        }

        return true;
    }
}