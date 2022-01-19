namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Common.Helpers;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Models;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class FilterItems : Feature
{
    private const string AutomateChestContainerType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";
    private const string AutomateModUniqueId = "Pathochild.Automate";

    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterItems"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public FilterItems(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        FilterItems.Instance = this;
        this._harmony = services.Lazy<HarmonyHelper>(FilterItems.AddPatches);
    }

    private static FilterItems Instance { get; set; }

    private HarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(nameof(FilterItems));
        this.FuryEvents.ItemsHighlighted += this.OnItemsHighlighted;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(nameof(FilterItems));
        this.FuryEvents.ItemsHighlighted -= this.OnItemsHighlighted;
    }

    private static void AddPatches(HarmonyHelper harmony)
    {
        harmony.AddPatch(
            nameof(FilterItems),
            AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
            typeof(FilterItems),
            nameof(FilterItems.Chest_addItem_prefix));

        if (!FilterItems.Instance.Helper.ModRegistry.IsLoaded(FilterItems.AutomateModUniqueId))
        {
            return;
        }

        var storeMethod = ReflectionHelper.GetAssemblyByName("Automate")?
            .GetType(FilterItems.AutomateChestContainerType)?
            .GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);
        if (storeMethod is not null)
        {
            harmony.AddPatch(
                nameof(FilterItems),
                storeMethod,
                typeof(FilterItems),
                nameof(FilterItems.Automate_Store_prefix));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Automate_Store_prefix(Chest ___Chest, object stack)
    {
        var item = FilterItems.Instance.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !FilterItems.Instance.ManagedChests.FindChest(___Chest, out var managedChest) || managedChest.Config.ItemMatcher.Matches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!FilterItems.Instance.ManagedChests.FindChest(__instance, out var managedChest) || managedChest.Config.ItemMatcher.Matches(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemsHighlighted(object sender, ItemsHighlightedEventArgs e)
    {
        if (this.ManagedChests.FindChest(e.Chest, out var managedChest))
        {
            e.AddHighlighter(managedChest.Config.ItemMatcher.Matches);
        }
    }
}