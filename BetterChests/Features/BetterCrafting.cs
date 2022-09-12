namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Enhances the <see cref="StardewValley.Menus.CraftingPage" />.
/// </summary>
internal sealed class BetterCrafting : IFeature
{
    private const string Id = "furyx639.BetterChests/BetterCrafting";

    private static BetterCrafting? Instance;

    private readonly ModConfig _config;
    private readonly PerScreen<Tuple<CraftingRecipe, int>?> _craft = new();
    private readonly PerScreen<IList<IStorageObject>> _eligibleStorages = new(() => new List<IStorageObject>());
    private readonly PerScreen<IReflectedField<Item?>?> _heldItem = new();
    private readonly IModHelper _helper;

    private readonly PerScreen<bool> _inWorkbench = new();
    private readonly PerScreen<IList<IStorageObject>> _materialStorages = new(() => new List<IStorageObject>());

    private EventHandler<ICraftingStoragesLoadingEventArgs>? _craftingStoragesLoading;
    private bool _isActivated;

    private BetterCrafting(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        HarmonyHelper.AddPatches(
            BetterCrafting.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Constructor(
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
                        }),
                    typeof(BetterCrafting),
                    nameof(BetterCrafting.CraftingPage_constructor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe"),
                    typeof(BetterCrafting),
                    nameof(BetterCrafting.CraftingPage_clickCraftingRecipe_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                    typeof(BetterCrafting),
                    nameof(BetterCrafting.CraftingPage_getContainerContents_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(Workbench), nameof(Workbench.checkForAction)),
                    typeof(BetterCrafting),
                    nameof(BetterCrafting.Workbench_checkForAction_prefix),
                    PatchType.Prefix),
            });
    }

    /// <summary>
    ///     Raised before storages are added to a Crafting Page.
    /// </summary>
    public static event EventHandler<ICraftingStoragesLoadingEventArgs> CraftingStoragesLoading
    {
        add => BetterCrafting.Instance!._craftingStoragesLoading += value;
        remove => BetterCrafting.Instance!._craftingStoragesLoading -= value;
    }

    private static ModConfig Config => BetterCrafting.Instance!._config;

    private static Tuple<CraftingRecipe, int>? Craft
    {
        get => BetterCrafting.Instance!._craft.Value;
        set => BetterCrafting.Instance!._craft.Value = value;
    }

    private static IList<IStorageObject> EligibleStorages => BetterCrafting.Instance!._eligibleStorages.Value;

    private static IReflectedField<Item?>? HeldItem
    {
        get => BetterCrafting.Instance!._heldItem.Value;
        set => BetterCrafting.Instance!._heldItem.Value = value;
    }

    private static bool InWorkbench
    {
        get => BetterCrafting.Instance!._inWorkbench.Value;
        set => BetterCrafting.Instance!._inWorkbench.Value = value;
    }

    private static IList<IStorageObject> MaterialStorages => BetterCrafting.Instance!._materialStorages.Value;

    /// <summary>
    ///     Initializes <see cref="BetterCrafting" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterCrafting" /> class.</returns>
    public static BetterCrafting Init(IModHelper helper, ModConfig config)
    {
        return BetterCrafting.Instance ??= new(helper, config);
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
            Integrations.BetterCrafting.API.OpenCraftingMenu(false, false, null, null, null, false);
            return true;
        }

        var width = 800 + IClickableMenu.borderWidth * 2;
        var height = 600 + IClickableMenu.borderWidth * 2;
        var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height).ToPoint();
        Game1.activeClickableMenu = new CraftingPage(x, y, width, height, false, true);
        return true;
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(BetterCrafting.Id);
        BetterCrafting.CraftingStoragesLoading += BetterCrafting.OnCraftingStoragesLoading;
        this._helper.Events.GameLoop.UpdateTicked += BetterCrafting.OnUpdateTicked;
        this._helper.Events.GameLoop.UpdateTicking += BetterCrafting.OnUpdateTicking;
        this._helper.Events.Display.MenuChanged += BetterCrafting.OnMenuChanged;

        if (!Integrations.BetterCrafting.IsLoaded)
        {
            return;
        }

        Integrations.BetterCrafting.API.MenuPopulateContainers += BetterCrafting.OnMenuPopulateContainers;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(BetterCrafting.Id);
        BetterCrafting.CraftingStoragesLoading -= BetterCrafting.OnCraftingStoragesLoading;
        this._helper.Events.GameLoop.UpdateTicked -= BetterCrafting.OnUpdateTicked;
        this._helper.Events.GameLoop.UpdateTicking -= BetterCrafting.OnUpdateTicking;
        this._helper.Events.Display.MenuChanged -= BetterCrafting.OnMenuChanged;

        if (!Integrations.BetterCrafting.IsLoaded)
        {
            return;
        }

        Integrations.BetterCrafting.API.MenuPopulateContainers -= BetterCrafting.OnMenuPopulateContainers;
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
        BetterCrafting.HeldItem = BetterCrafting.Instance!._helper.Reflection.GetField<Item?>(__instance, "heldItem");
        BetterCrafting.Instance._craftingStoragesLoading.InvokeAll(
            BetterCrafting.Instance,
            new CraftingStoragesLoadingEventArgs(BetterCrafting.EligibleStorages));
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

        __result = BetterCrafting.EligibleStorages.SelectMany(storage => storage.Items.OfType<Item>()).ToList();
    }

    private static void OnCraftingStoragesLoading(object? sender, ICraftingStoragesLoadingEventArgs e)
    {
        if (!BetterCrafting.InWorkbench
         || BetterCrafting.Config.CraftFromWorkbench is FeatureOptionRange.Default or FeatureOptionRange.Disabled)
        {
            return;
        }

        BetterCrafting.InWorkbench = false;
        e.AddStorages(
            from storage in Storages.All
            where storage.CraftFromChest is not (FeatureOptionRange.Disabled or FeatureOptionRange.Default)
               && storage.CraftFromChestDisableLocations?.Contains(Game1.player.currentLocation.Name) != true
               && !(storage.CraftFromChestDisableLocations?.Contains("UndergroundMine") == true
                 && Game1.player.currentLocation is MineShaft mineShaft
                 && mineShaft.Name.StartsWith("UndergroundMine"))
               && storage.Source is not null
               && BetterCrafting.Config.CraftFromWorkbench.WithinRangeOfPlayer(
                      BetterCrafting.Config.CraftFromWorkbenchDistance,
                      storage.Location,
                      storage.Position)
            select storage);
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not (CraftingPage or GameMenu)
         && (!Integrations.BetterCrafting.IsLoaded
          || e.OldMenu?.GetType() != Integrations.BetterCrafting.API.GetMenuType()))
        {
            return;
        }

        foreach (var storage in BetterCrafting.EligibleStorages.Where(storage => storage.Mutex?.IsLockHeld() is true))
        {
            storage.Mutex!.ReleaseLock();
        }

        BetterCrafting.EligibleStorages.Clear();
        BetterCrafting.MaterialStorages.Clear();
        BetterCrafting.Craft = null;
        BetterCrafting.HeldItem = null;
    }

    private static void OnMenuPopulateContainers(IPopulateContainersEvent e)
    {
        BetterCrafting.Instance!._craftingStoragesLoading.InvokeAll(
            BetterCrafting.Instance,
            new CraftingStoragesLoadingEventArgs(BetterCrafting.EligibleStorages));
        if (!BetterCrafting.EligibleStorages.Any())
        {
            return;
        }

        foreach (var (storage, location) in BetterCrafting.EligibleStorages.Select(
                     BetterCrafting.StorageForBetterCrafting))
        {
            e.Containers.Add(new(storage, location));
        }
    }

    private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (BetterCrafting.Craft is null
         || BetterCrafting.HeldItem is null
         || !BetterCrafting.MaterialStorages.All(storage => storage.Mutex?.IsLockHeld() is true))
        {
            return;
        }

        var (recipe, amount) = BetterCrafting.Craft;
        var crafted = recipe.createItem();
        var heldItem = BetterCrafting.HeldItem.GetValue();
        BetterCrafting.Craft = null;

        void ConsumeIngredients()
        {
            foreach (var (id, quantity) in recipe.recipeList)
            {
                bool IsValid(Item? item)
                {
                    return item is SObject { bigCraftable.Value: false } obj
                        && (item.ParentSheetIndex == id
                         || item.Category == id
                         || CraftingRecipe.isThereSpecialIngredientRule(obj, id));
                }

                var required = quantity * amount;
                for (var i = Game1.player.Items.Count - 1; i >= 0; i--)
                {
                    var item = Game1.player.Items[i];
                    if (!IsValid(item))
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
                    for (var i = storage.Items.Count - 1; i >= 0; i--)
                    {
                        var item = storage.Items[i];
                        if (item is null || !IsValid(item))
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
                            storage.Items[i] = null;
                        }

                        if (required <= 0)
                        {
                            break;
                        }
                    }

                    storage.ClearNulls();
                    if (required <= 0)
                    {
                        break;
                    }
                }
            }

            foreach (var storage in BetterCrafting.MaterialStorages)
            {
                storage.Mutex!.ReleaseLock();
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
        foreach (var storage in BetterCrafting.EligibleStorages)
        {
            storage.Mutex?.Update(storage.Location);
        }
    }

    private static Tuple<object, GameLocation> StorageForBetterCrafting(IStorageObject storage)
    {
        return new(new StorageWrapper(storage), storage.Location);
    }

    private static bool TryCrafting(CraftingRecipe recipe, Item? heldItem)
    {
        if (!BetterCrafting.EligibleStorages.Any() || BetterCrafting.Craft is not null)
        {
            return false;
        }

        BetterCrafting.MaterialStorages.Clear();
        var amount = BetterCrafting.Instance!._helper.Input.IsDown(SButton.LeftShift)
                  || BetterCrafting.Instance._helper.Input.IsDown(SButton.RightShift)
            ? 5
            : 1;
        var crafted = recipe.createItem();
        if (heldItem is not null
         && (!heldItem.Name.Equals(crafted.Name)
          || !heldItem.getOne().canStackWith(crafted.getOne())
          || heldItem.Stack + recipe.numberProducedPerCraft * amount > heldItem.maximumStackSize()))
        {
            return false;
        }

        foreach (var (id, quantity) in recipe.recipeList)
        {
            bool IsValid(Item? item)
            {
                return item is SObject { bigCraftable.Value: false } obj
                    && (item.ParentSheetIndex == id
                     || item.Category == id
                     || CraftingRecipe.isThereSpecialIngredientRule(obj, id));
            }

            var required = quantity * amount;
            foreach (var item in Game1.player.Items.Where(IsValid))
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

            foreach (var storage in BetterCrafting.EligibleStorages.Where(storage => storage.Mutex is not null))
            {
                foreach (var item in storage.Items.Where(IsValid))
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
            storage.Mutex!.RequestLock();
        }

        BetterCrafting.Craft = new(recipe, amount);
        return true;
    }

    [HarmonyPriority(Priority.High)]
    private static bool Workbench_checkForAction_prefix(bool justCheckingForActivity)
    {
        if (justCheckingForActivity
         || BetterCrafting.Instance!._config.CraftFromWorkbench is (FeatureOptionRange.Disabled
                                                                    or FeatureOptionRange.Default))
        {
            return true;
        }

        BetterCrafting.InWorkbench = true;
        return !BetterCrafting.ShowCraftingPage();
    }

    private class CraftingStoragesLoadingEventArgs : ICraftingStoragesLoadingEventArgs
    {
        private readonly IList<IStorageObject> _storages;

        public CraftingStoragesLoadingEventArgs(IList<IStorageObject> storages)
        {
            this._storages = storages;
        }

        public void AddStorage(IStorageObject storage)
        {
            if (!this._storages.Contains(storage))
            {
                this._storages.Add(storage);
            }
        }

        public void AddStorages(IEnumerable<IStorageObject> storages)
        {
            foreach (var storage in storages)
            {
                this.AddStorage(storage);
            }
        }
    }
}