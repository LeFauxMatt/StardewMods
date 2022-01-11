namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using CommonHarmony;
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
    private const string AutomateAssemblyType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";
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

        if (FilterItems.Instance.Helper.ModRegistry.IsLoaded(FilterItems.AutomateModUniqueId))
        {
            harmony.AddPatch(
                nameof(FilterItems),
                new AssemblyPatch("Automate").Method(FilterItems.AutomateAssemblyType, "Store"),
                typeof(FilterItems),
                nameof(FilterItems.Automate_Store_prefix));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Automate_Store_prefix(Chest ___Chest, object stack)
    {
        return FilterItems.Instance.ChestAcceptsItem(___Chest, FilterItems.Instance.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue());
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (FilterItems.Instance.ChestAcceptsItem(__instance, item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemsHighlighted(object sender, ItemsHighlightedEventArgs e)
    {
        e.AddHighlighter(item => this.ChestAcceptsItem(e.Chest, item));
    }

    private bool ChestAcceptsItem(Chest chest, Item item)
    {
        return !this.ManagedChests.FindChest(chest, out var managedChest) || managedChest.Config.ItemMatcher.Matches(item);
    }
}