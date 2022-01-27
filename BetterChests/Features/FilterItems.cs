namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BetterChests.Enums;
using BetterChests.Interfaces;
using Common.Extensions;
using Common.Helpers;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class FilterItems : Feature
{
    private const string AutomateChestContainerType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";
    private const string AutomateModUniqueId = "Pathochild.Automate";

    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly Lazy<IMenuItems> _menuItems;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterItems"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public FilterItems(IConfigModel config, IModHelper helper, IServiceLocator services)
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

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
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
    private static bool Automate_Store_prefix(Chest ___Chest, object stack)
    {
        var item = FilterItems.Instance.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !FilterItems.Instance.ManagedChests.FindChest(___Chest, out var managedChest) || managedChest.ItemMatcherByType.Matches(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!FilterItems.Instance.ManagedChests.FindChest(__instance, out var managedChest) || managedChest.FilterItems == FeatureOption.Disabled || managedChest.ItemMatcherByType.Matches(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu?.IsPlayerChestMenu(out _) == true
            ? e.ItemGrabMenu
            : null;

        if (this.Menu is null || !this.ManagedChests.FindChest(e.Chest, out var managedChest) || managedChest.FilterItems == FeatureOption.Disabled)
        {
            return;
        }

        // Add highlighter to Menu Items
        this.MenuItems.AddHighlighter(managedChest.ItemMatcherByType);
    }
}