namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.StorageHandlers;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Enhances the <see cref="StardewValley.Menus.CraftingPage" />.
/// </summary>
internal class BetterCrafting : IFeature
{
    private const string Id = "furyx639.BetterChests/BetterCrafting";

    private static BetterCrafting? Instance;

    private readonly PerScreen<Tuple<CraftingRecipe, int>?> _craft = new();
    private readonly PerScreen<IReflectedField<Item?>?> _heldItem = new();
    private readonly IModHelper _helper;
    private readonly PerScreen<List<IStorageObject>> _materialContainers = new(() => new());
    private readonly PerScreen<List<IStorageObject>> _storages = new(() => new());

    private bool _isActivated;

    private BetterCrafting(IModHelper helper)
    {
        this._helper = helper;
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
            });
    }

    private static Tuple<CraftingRecipe, int>? Craft
    {
        get => BetterCrafting.Instance!._craft.Value;
        set => BetterCrafting.Instance!._craft.Value = value;
    }

    private static IReflectedField<Item?>? HeldItem
    {
        get => BetterCrafting.Instance!._heldItem.Value;
        set => BetterCrafting.Instance!._heldItem.Value = value;
    }

    private static List<IStorageObject> MaterialContainers => BetterCrafting.Instance!._materialContainers.Value;

    private static List<IStorageObject> Storages => BetterCrafting.Instance!._storages.Value;

    /// <summary>
    ///     Adds additional materials for use in the current <see cref="CraftingPage" />.
    /// </summary>
    /// <param name="storages">The storages to add.</param>
    public static void AddMaterials(IEnumerable<IStorageObject> storages)
    {
        foreach (var storage in storages)
        {
            if (!BetterCrafting.Storages.Contains(storage))
            {
                BetterCrafting.Storages.Add(storage);
            }
        }
    }

    /// <summary>
    ///     Initializes <see cref="BetterCrafting" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="BetterCrafting" /> class.</returns>
    public static BetterCrafting Init(IModHelper helper)
    {
        return BetterCrafting.Instance ??= new(helper);
    }

    /// <summary>
    ///     Opens the crafting menu.
    /// </summary>
    /// <param name="storages">The storages to craft from.</param>
    public static void ShowCraftingPage(IEnumerable<IStorageObject> storages)
    {
        BetterCrafting.Storages.Clear();
        BetterCrafting.Storages.AddRange(storages.OfType<ChestStorage>());

        if (Integrations.BetterCrafting.IsLoaded)
        {
            Tuple<object, GameLocation> StorageForBetterCrafting(IStorageObject storage)
            {
                return new(new StorageWrapper(storage), storage.Location);
            }

            Integrations.BetterCrafting.API.OpenCraftingMenu(
                false,
                false,
                null,
                null,
                null,
                false,
                BetterCrafting.Storages.Select(StorageForBetterCrafting).ToList());
            return;
        }

        var width = 800 + IClickableMenu.borderWidth * 2;
        var height = 600 + IClickableMenu.borderWidth * 2;
        var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height).ToPoint();
        Game1.activeClickableMenu = new CraftingPage(x, y, width, height, false, true);
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
        this._helper.Events.GameLoop.UpdateTicked += BetterCrafting.OnUpdateTicked;
        this._helper.Events.GameLoop.UpdateTicking += BetterCrafting.OnUpdateTicking;
        this._helper.Events.Display.MenuChanged += BetterCrafting.OnMenuChanged;
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
        this._helper.Events.GameLoop.UpdateTicked -= BetterCrafting.OnUpdateTicked;
        this._helper.Events.GameLoop.UpdateTicking -= BetterCrafting.OnUpdateTicking;
        this._helper.Events.Display.MenuChanged -= BetterCrafting.OnMenuChanged;
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
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CraftingPage_getContainerContents_postfix(ref IList<Item> __result)
    {
        if (!BetterCrafting.Storages.Any())
        {
            return;
        }

        __result = BetterCrafting.Storages.SelectMany(storage => storage.Items.OfType<Item>()).ToList();
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not CraftingPage
         && (!Integrations.BetterCrafting.IsLoaded
          || e.OldMenu?.GetType() != Integrations.BetterCrafting.API.GetMenuType()))
        {
            return;
        }

        foreach (var storage in BetterCrafting.Storages.Where(storage => storage.Mutex?.IsLockHeld() is true))
        {
            storage.Mutex!.ReleaseLock();
        }

        BetterCrafting.Storages.Clear();
        BetterCrafting.MaterialContainers.Clear();
        BetterCrafting.Craft = null;
        BetterCrafting.HeldItem = null;
    }

    private static void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (BetterCrafting.Craft is null
         || BetterCrafting.HeldItem is null
         || !BetterCrafting.MaterialContainers.All(storage => storage.Mutex?.IsLockHeld() is true))
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

                foreach (var storage in BetterCrafting.MaterialContainers)
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

            foreach (var storage in BetterCrafting.MaterialContainers)
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
        foreach (var storage in BetterCrafting.Storages)
        {
            storage.Mutex?.Update(storage.Location);
        }
    }

    private static bool TryCrafting(CraftingRecipe recipe, Item? heldItem)
    {
        if (!BetterCrafting.Storages.Any() || BetterCrafting.Craft is not null)
        {
            return false;
        }

        BetterCrafting.MaterialContainers.Clear();
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

            foreach (var storage in BetterCrafting.Storages.Where(storage => storage.Mutex is not null))
            {
                foreach (var item in storage.Items.Where(IsValid))
                {
                    BetterCrafting.MaterialContainers.Add(storage);
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

            BetterCrafting.MaterialContainers.Clear();
            return false;
        }

        foreach (var storage in BetterCrafting.MaterialContainers)
        {
            storage.Mutex!.RequestLock();
        }

        BetterCrafting.Craft = new(recipe, amount);
        return true;
    }
}