namespace StardewMods.BetterChests.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Restricts what items can be added into a chest.
/// </summary>
internal sealed class FilterItems : IFeature
{
    private const string Id = "furyx639.BetterChests/FilterItems";

#nullable disable
    private static FilterItems Instance;
#nullable enable

    private readonly IModHelper _helper;

    private bool _isActivated;

    private FilterItems(IModHelper helper)
    {
        this._helper = helper;
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

        if (!Integrations.Automate.IsLoaded)
        {
            return;
        }

        var storeMethod = ReflectionHelper.GetAssemblyByName("Automate")
                                          ?.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")
                                          ?.GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);
        if (storeMethod is not null)
        {
            HarmonyHelper.AddPatch(
                FilterItems.Id,
                storeMethod,
                typeof(FilterItems),
                nameof(FilterItems.Automate_Store_prefix));
        }
    }

    private static IReflectionHelper Reflection => FilterItems.Instance._helper.Reflection;

    /// <summary>
    ///     Initializes <see cref="FilterItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="FilterItems" /> class.</returns>
    public static IFeature Init(IModHelper helper)
    {
        return FilterItems.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(FilterItems.Id);
        this._helper.Events.Display.MenuChanged += FilterItems.OnMenuChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(FilterItems.Id);
        this._helper.Events.Display.MenuChanged -= FilterItems.OnMenuChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = FilterItems.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !Storages.TryGetOne(___Chest, out var storage) || storage.FilterMatches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
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
        if (e.NewMenu is not ItemGrabMenu || BetterItemGrabMenu.Context?.FilterItems is not FeatureOption.Enabled)
        {
            return;
        }

        BetterItemGrabMenu.Inventory?.AddHighlighter(BetterItemGrabMenu.Context.FilterMatcher);
    }
}