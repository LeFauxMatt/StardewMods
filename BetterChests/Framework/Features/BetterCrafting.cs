namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Enhances the <see cref="StardewValley.Menus.CraftingPage" />.
/// </summary>
internal sealed class BetterCrafting : Feature
{
    private const string Id = "furyx639.BetterChests/BetterCrafting";

    private static readonly MethodBase CraftingPageClickCraftingRecipe =
        AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe");

    private static readonly MethodBase CraftingPageConstructor = AccessTools.Constructor(
        typeof(CraftingPage),
        new[]
        {
            typeof(int),
            typeof(int),
            typeof(int),
            typeof(int),
            typeof(bool),
            typeof(bool),
            typeof(List<Chest>),
        });

    private static readonly MethodBase CraftingPageGetContainerContents =
        AccessTools.Method(typeof(CraftingPage), "getContainerContents");

    private static readonly MethodBase WorkbenchCheckForAction =
        AccessTools.Method(typeof(Workbench), nameof(Workbench.checkForAction));

#nullable disable
    private static BetterCrafting instance;
#nullable enable

    private readonly ModConfig config;
    private readonly PerScreen<Tuple<CraftingRecipe, int>?> craft = new();
    private readonly PerScreen<IList<StorageNode>> eligibleStorages = new(() => new List<StorageNode>());
    private readonly Harmony harmony;
    private readonly PerScreen<IReflectedField<Item?>?> heldItem = new();
    private readonly IModHelper helper;

    private readonly PerScreen<bool> inWorkbench = new();
    private readonly PerScreen<IList<StorageNode>> materialStorages = new(() => new List<StorageNode>());

    private EventHandler<CraftingStoragesLoadingEventArgs>? craftingStoragesLoading;

    private BetterCrafting(IModHelper helper, ModConfig config)
    {
        this.helper = helper;
        this.config = config;
        this.harmony = new(BetterCrafting.Id);
    }

    /// <summary>
    ///     Raised before storages are added to a Crafting Page.
    /// </summary>
    public static event EventHandler<CraftingStoragesLoadingEventArgs> CraftingStoragesLoading
    {
        add => BetterCrafting.instance.craftingStoragesLoading += value;
        remove => BetterCrafting.instance.craftingStoragesLoading -= value;
    }

    private static ModConfig Config => BetterCrafting.instance.config;

    private static Tuple<CraftingRecipe, int>? Craft
    {
        get => BetterCrafting.instance.craft.Value;
        set => BetterCrafting.instance.craft.Value = value;
    }

    private static IList<StorageNode> EligibleStorages => BetterCrafting.instance.eligibleStorages.Value;

    private static IReflectedField<Item?>? HeldItem
    {
        get => BetterCrafting.instance.heldItem.Value;
        set => BetterCrafting.instance.heldItem.Value = value;
    }

    private static bool InWorkbench
    {
        get => BetterCrafting.instance.inWorkbench.Value;
        set => BetterCrafting.instance.inWorkbench.Value = value;
    }

    private static IList<StorageNode> MaterialStorages => BetterCrafting.instance.materialStorages.Value;

    /// <summary>
    ///     Initializes <see cref="BetterCrafting" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterCrafting" /> class.</returns>
    public static BetterCrafting Init(IModHelper helper, ModConfig config)
    {
        return BetterCrafting.instance ??= new(helper, config);
    }

    /// <summary>
    ///     Opens the crafting menu.
    /// </summary>
    /// <returns>Returns true if crafting page could be displayed.</returns>
    public static bool ShowCraftingPage()
    {
        BetterCrafting.EligibleStorages.Clear();
        BetterCrafting.MaterialStorages.Clear();
        if (Integrations.BetterCrafting.IsLoaded)
        {
            Integrations.BetterCrafting.Api.OpenCraftingMenu(false, false, null, null, null, false);
            return true;
        }

        var width = 800 + (IClickableMenu.borderWidth * 2);
        var height = 600 + (IClickableMenu.borderWidth * 2);
        var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height).ToPoint();
        Game1.activeClickableMenu = new CraftingPage(x, y, width, height, false, true);
        return true;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading += BetterCrafting.OnCraftingStoragesLoading;
        this.helper.Events.GameLoop.UpdateTicked += BetterCrafting.OnUpdateTicked;
        this.helper.Events.GameLoop.UpdateTicking += BetterCrafting.OnUpdateTicking;
        this.helper.Events.Display.MenuChanged += BetterCrafting.OnMenuChanged;

        // Patches
        this.harmony.Patch(
            BetterCrafting.CraftingPageConstructor,
            postfix: new(typeof(BetterCrafting), nameof(BetterCrafting.CraftingPage_constructor_postfix)));
        this.harmony.Patch(
            BetterCrafting.CraftingPageClickCraftingRecipe,
            new(typeof(BetterCrafting), nameof(BetterCrafting.CraftingPage_clickCraftingRecipe_prefix)));
        this.harmony.Patch(
            BetterCrafting.CraftingPageGetContainerContents,
            postfix: new(typeof(BetterCrafting), nameof(BetterCrafting.CraftingPage_getContainerContents_postfix)));
        this.harmony.Patch(
            BetterCrafting.WorkbenchCheckForAction,
            new(typeof(BetterCrafting), nameof(BetterCrafting.Workbench_checkForAction_prefix)));

        // Integrations
        if (!Integrations.BetterCrafting.IsLoaded)
        {
            return;
        }

        Integrations.BetterCrafting.Api.MenuPopulateContainers += BetterCrafting.OnMenuPopulateContainers;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading -= BetterCrafting.OnCraftingStoragesLoading;
        this.helper.Events.GameLoop.UpdateTicked -= BetterCrafting.OnUpdateTicked;
        this.helper.Events.GameLoop.UpdateTicking -= BetterCrafting.OnUpdateTicking;
        this.helper.Events.Display.MenuChanged -= BetterCrafting.OnMenuChanged;

        // Patches
        this.harmony.Unpatch(
            BetterCrafting.CraftingPageConstructor,
            AccessTools.Method(typeof(BetterCrafting), nameof(BetterCrafting.CraftingPage_constructor_postfix)));
        this.harmony.Unpatch(
            BetterCrafting.CraftingPageClickCraftingRecipe,
            AccessTools.Method(typeof(BetterCrafting), nameof(BetterCrafting.CraftingPage_clickCraftingRecipe_prefix)));
        this.harmony.Unpatch(
            BetterCrafting.CraftingPageGetContainerContents,
            AccessTools.Method(
                typeof(BetterCrafting),
                nameof(BetterCrafting.CraftingPage_getContainerContents_postfix)));
        this.harmony.Unpatch(
            BetterCrafting.WorkbenchCheckForAction,
            AccessTools.Method(typeof(BetterCrafting), nameof(BetterCrafting.Workbench_checkForAction_prefix)));

        // Integrations
        if (!Integrations.BetterCrafting.IsLoaded)
        {
            return;
        }

        Integrations.BetterCrafting.Api.MenuPopulateContainers -= BetterCrafting.OnMenuPopulateContainers;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool CraftingPage_clickCraftingRecipe_prefix(
        CraftingPage __instance,
        ref bool ___cooking,
        ref int ___currentCraftingPage,
        ref Item? ___heldItem,
        ClickableTextureComponent c,
        bool playSound)
    {
        if (___cooking
            || !BetterCrafting.TryCrafting(__instance.pagesOfCraftingRecipes[___currentCraftingPage][c], ___heldItem))
        {
            return true;
        }

        if (playSound)
        {
            Game1.playSound("coin");
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CraftingPage_constructor_postfix(CraftingPage __instance)
    {
        BetterCrafting.HeldItem = BetterCrafting.instance.helper.Reflection.GetField<Item?>(__instance, "heldItem");
        BetterCrafting.instance.craftingStoragesLoading.InvokeAll(
            BetterCrafting.instance,
            new(BetterCrafting.EligibleStorages));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CraftingPage_getContainerContents_postfix(ref IList<Item> __result)
    {
        if (!BetterCrafting.EligibleStorages.Any())
        {
            return;
        }

        __result = new List<Item>();
        foreach (var storage in BetterCrafting.EligibleStorages)
        {
            if (storage is not { Data: Storage storageObject })
            {
                continue;
            }

            foreach (var item in storageObject.Items)
            {
                if (item is not null)
                {
                    __result.Add(item);
                }
            }
        }
    }

    private static void OnCraftingStoragesLoading(object? sender, CraftingStoragesLoadingEventArgs e)
    {
        if (!BetterCrafting.InWorkbench
            || BetterCrafting.Config.CraftFromWorkbench is FeatureOptionRange.Default or FeatureOptionRange.Disabled)
        {
            return;
        }

        BetterCrafting.InWorkbench = false;
        IList<StorageNode> storages = new List<StorageNode>();
        foreach (var storage in Storages.All)
        {
            if (storage.CraftFromChest is FeatureOptionRange.Disabled or FeatureOptionRange.Default
                || storage.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
                || (storage.CraftFromChestDisableLocations.Contains("UndergroundMine")
                    && Game1.player.currentLocation is MineShaft mineShaft
                    && mineShaft.Name.StartsWith("UndergroundMine"))
                || storage is not { Data: Storage { Source: { } } storageObject }
                || !BetterCrafting.Config.CraftFromWorkbench.WithinRangeOfPlayer(
                    BetterCrafting.Config.CraftFromWorkbenchDistance,
                    storageObject.Location,
                    storageObject.Position))
            {
                continue;
            }

            storages.Add(storage);
        }

        e.AddStorages(storages);
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not (CraftingPage or GameMenu)
            && (!Integrations.BetterCrafting.IsLoaded
                || e.OldMenu?.GetType() != Integrations.BetterCrafting.Api.GetMenuType()))
        {
            return;
        }

        foreach (var storage in BetterCrafting.EligibleStorages)
        {
            if (storage is not { Data: Storage storageObject }
                || storageObject.Mutex is null
                || !storageObject.Mutex.IsLockHeld())
            {
                continue;
            }

            storageObject.Mutex.ReleaseLock();
        }

        BetterCrafting.EligibleStorages.Clear();
        BetterCrafting.MaterialStorages.Clear();
        BetterCrafting.Craft = null;
        BetterCrafting.HeldItem = null;
    }

    private static void OnMenuPopulateContainers(IPopulateContainersEvent e)
    {
        BetterCrafting.instance.craftingStoragesLoading.InvokeAll(
            BetterCrafting.instance,
            new(BetterCrafting.EligibleStorages));
        if (!BetterCrafting.EligibleStorages.Any())
        {
            return;
        }

        foreach (var storage in BetterCrafting.EligibleStorages)
        {
            if (storage is not { Data: Storage storageObject })
            {
                continue;
            }

            e.Containers.Add(new(storage, storageObject.Location));
        }
    }

    private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (BetterCrafting.Craft is null || BetterCrafting.HeldItem is null)
        {
            return;
        }

        foreach (var storage in BetterCrafting.MaterialStorages)
        {
            if (storage is not { Data: Storage storageObject }
                || storageObject.Mutex is null
                || !storageObject.Mutex.IsLockHeld())
            {
                return;
            }
        }

        var (recipe, amount) = BetterCrafting.Craft;
        var crafted = recipe.createItem();
        var heldItem = BetterCrafting.HeldItem.GetValue();
        BetterCrafting.Craft = null;

        void ConsumeIngredients()
        {
            foreach (var (id, quantity) in recipe.recipeList)
            {
                var required = quantity * amount;
                for (var i = Game1.player.Items.Count - 1; i >= 0; --i)
                {
                    var item = Game1.player.Items[i];
                    if (!CraftingRecipe.ItemMatchesForCrafting(item, id))
                    {
                        continue;
                    }

                    if (item.Stack > required)
                    {
                        item.Stack -= required;
                        required = 0;
                    }
                    else
                    {
                        required -= item.Stack;
                        Game1.player.Items[i] = null;
                    }

                    if (required <= 0)
                    {
                        break;
                    }
                }

                if (required <= 0)
                {
                    continue;
                }

                foreach (var storage in BetterCrafting.MaterialStorages)
                {
                    if (storage is not { Data: Storage storageObject })
                    {
                        continue;
                    }

                    for (var i = storageObject.Items.Count - 1; i >= 0; --i)
                    {
                        var item = storageObject.Items[i];
                        if (item is null || !CraftingRecipe.ItemMatchesForCrafting(item, id))
                        {
                            continue;
                        }

                        if (item.Stack >= required)
                        {
                            item.Stack -= required;
                            required = 0;
                        }
                        else
                        {
                            required -= item.Stack;
                            storageObject.Items[i] = null;
                        }

                        if (required <= 0)
                        {
                            break;
                        }
                    }

                    storageObject.ClearNulls();
                    if (required <= 0)
                    {
                        break;
                    }
                }
            }

            foreach (var storage in BetterCrafting.MaterialStorages)
            {
                if (storage is not { Data: Storage storageObject } || storageObject.Mutex is null)
                {
                    continue;
                }

                storageObject.Mutex.ReleaseLock();
            }
        }

        if (heldItem is null)
        {
            BetterCrafting.HeldItem.SetValue(crafted);
        }
        else
        {
            if (!heldItem.Name.Equals(crafted.Name)
                || !heldItem.getOne().canStackWith(crafted.getOne())
                || heldItem.Stack + recipe.numberProducedPerCraft - 1 >= heldItem.maximumStackSize())
            {
                return;
            }

            heldItem.Stack += recipe.numberProducedPerCraft;
        }

        ConsumeIngredients();

        Game1.player.checkForQuestComplete(null, -1, -1, crafted, null, 2);
        if (Game1.player.craftingRecipes.ContainsKey(recipe.name))
        {
            Game1.player.craftingRecipes[recipe.name] += recipe.numberProducedPerCraft;
        }

        Game1.stats.checkForCraftingAchievements();
        if (!Game1.options.gamepadControls || heldItem is null || !Game1.player.couldInventoryAcceptThisItem(heldItem))
        {
            return;
        }

        Game1.player.addItemToInventoryBool(heldItem);
        BetterCrafting.HeldItem.SetValue(null);
    }

    private static void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (BetterCrafting.Craft is null || BetterCrafting.HeldItem is null || !BetterCrafting.MaterialStorages.Any())
        {
            return;
        }

        foreach (var storage in BetterCrafting.MaterialStorages)
        {
            if (storage is not { Data: Storage storageObject } || storageObject.Mutex is null)
            {
                continue;
            }

            storageObject.Mutex.Update(storageObject.Location);
        }
    }

    private static bool TryCrafting(CraftingRecipe recipe, Item? heldItem)
    {
        if (!BetterCrafting.EligibleStorages.Any() || BetterCrafting.Craft is not null)
        {
            return false;
        }

        BetterCrafting.MaterialStorages.Clear();
        var amount = BetterCrafting.instance.helper.Input.IsDown(SButton.LeftShift)
            || BetterCrafting.instance.helper.Input.IsDown(SButton.RightShift)
                ? 5
                : 1;
        var crafted = recipe.createItem();
        if (heldItem is not null
            && (!heldItem.Name.Equals(crafted.Name)
                || !heldItem.getOne().canStackWith(crafted.getOne())
                || heldItem.Stack + (recipe.numberProducedPerCraft * amount) > heldItem.maximumStackSize()))
        {
            return false;
        }

        foreach (var (id, quantity) in recipe.recipeList)
        {
            var required = quantity * amount;
            foreach (var item in Game1.player.Items.Where(item => CraftingRecipe.ItemMatchesForCrafting(item, id)))
            {
                required -= item.Stack;
                if (required <= 0)
                {
                    break;
                }
            }

            if (required <= 0)
            {
                continue;
            }

            foreach (var storage in BetterCrafting.EligibleStorages)
            {
                if (storage is not { Data: Storage storageObject } || storageObject.Mutex is null)
                {
                    continue;
                }

                foreach (var item in storageObject.Items.Where(item => CraftingRecipe.ItemMatchesForCrafting(item, id)))
                {
                    BetterCrafting.MaterialStorages.Add(storage);
                    required -= item!.Stack;
                    if (required <= 0)
                    {
                        break;
                    }
                }

                if (required <= 0)
                {
                    break;
                }
            }

            if (required <= 0)
            {
                continue;
            }

            BetterCrafting.MaterialStorages.Clear();
            return false;
        }

        foreach (var storage in BetterCrafting.MaterialStorages)
        {
            if (storage is not { Data: Storage storageObject } || storageObject.Mutex is null)
            {
                continue;
            }

            storageObject.Mutex.RequestLock();
        }

        BetterCrafting.Craft = new(recipe, amount);
        return true;
    }

    [HarmonyPriority(Priority.High)]
    private static bool Workbench_checkForAction_prefix(bool justCheckingForActivity)
    {
        if (justCheckingForActivity
            || BetterCrafting.instance.config.CraftFromWorkbench is (FeatureOptionRange.Disabled
                or FeatureOptionRange.Default))
        {
            return true;
        }

        BetterCrafting.InWorkbench = true;
        return !BetterCrafting.ShowCraftingPage();
    }
}