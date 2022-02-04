namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Common.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class FilterItems : Feature
{
    private const string AutomateChestContainerType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";
    private const string AutomateModUniqueId = "Pathochild.Automate";

    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly Lazy<IMenuItems> _menuItems;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilterItems" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public FilterItems(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        FilterItems.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
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
                        this.Id,
                        storeMethod,
                        typeof(FilterItems),
                        nameof(FilterItems.Automate_Store_prefix));
                }
            });
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static FilterItems Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Automate_Store_prefix(Chest ___Chest, object stack)
    {
        var item = FilterItems.Instance.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !FilterItems.Instance.ManagedChests.FindChest(___Chest, out var managedChest) || managedChest.ItemMatcher.Matches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!FilterItems.Instance.ManagedChests.FindChest(__instance, out var managedChest)
            || managedChest.FilterItems == FeatureOption.Disabled
            || managedChest.ItemMatcher.Matches(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.MenuItems.Menu is null
            || !this.ManagedChests.FindChest(e.Chest, out var managedChest)
            || managedChest.FilterItems == FeatureOption.Disabled)
        {
            return;
        }

        // Add highlighter to Menu Items
        this.MenuItems.AddHighlighter(managedChest.ItemMatcher);
    }
}