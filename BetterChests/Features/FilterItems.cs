namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Common.Extensions;
using Common.Helpers;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Models;
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
    private readonly Lazy<HarmonyHelper> _harmony;
    private readonly Lazy<IMenuItems> _menuItems;

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
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static FilterItems Instance { get; set; }

    private HarmonyHelper Harmony
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
    public override void Activate()
    {
        this.Harmony.ApplyPatches(nameof(FilterItems));
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(nameof(FilterItems));
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
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

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
        if (!this.Menu?.IsPlayerChestMenu(out _) != true || this.ManagedChests.FindChest(e.Chest, out var managedChest))
        {
            return;
        }

        // Add highlighter to Menu Items
        this.MenuItems.AddHighlighter(managedChest.Config.ItemMatcher);
    }
}