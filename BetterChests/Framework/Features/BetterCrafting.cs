namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Enhances the <see cref="StardewValley.Menus.CraftingPage" />.</summary>
internal sealed class BetterCrafting : BaseFeature
{
    private static readonly MethodBase CraftingPageClickCraftingRecipe =
        AccessTools.DeclaredMethod(typeof(CraftingPage), "clickCraftingRecipe");

    private static readonly ConstructorInfo CraftingPageConstructor =
        AccessTools.GetDeclaredConstructors(typeof(CraftingPage))[0];

    private static readonly MethodBase CraftingPageGetContainerContents =
        AccessTools.DeclaredMethod(typeof(CraftingPage), "getContainerContents");

    private static readonly MethodBase WorkbenchCheckForAction = AccessTools.DeclaredMethod(
        typeof(Workbench),
        nameof(Workbench.checkForAction));

#nullable disable
    private static BetterCrafting instance;
#nullable enable

    private readonly ModConfig config;
    private readonly PerScreen<Tuple<CraftingRecipe, int>?> craft = new();
    private readonly PerScreen<IList<StorageNode>> eligibleStorages = new(() => new List<StorageNode>());
    private readonly IModEvents events;
    private readonly Harmony harmony;
    private readonly PerScreen<IReflectedField<Item?>?> heldItem = new();
    private readonly IInputHelper input;
    private readonly PerScreen<bool> inWorkbench = new();
    private readonly PerScreen<IList<StorageNode>> materialStorages = new(() => new List<StorageNode>());
    private readonly IReflectionHelper reflection;

    private EventHandler<CraftingStoragesLoadingEventArgs>? craftingStoragesLoading;

    /// <summary>Initializes a new instance of the <see cref="BetterCrafting" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    public BetterCrafting(
        IMonitor monitor,
        ModConfig config,
        IModEvents events,
        Harmony harmony,
        IInputHelper input,
        IReflectionHelper reflection)
        : base(monitor, nameof(BetterCrafting))
    {
        BetterCrafting.instance = this;
        this.config = config;
        this.events = events;
        this.harmony = harmony;
        this.input = input;
        this.reflection = reflection;
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

    /// <summary>Raised before storages are added to a Crafting Page.</summary>
    public static event EventHandler<CraftingStoragesLoadingEventArgs> CraftingStoragesLoading
    {
        add => BetterCrafting.instance.craftingStoragesLoading += value;
        remove => BetterCrafting.instance.craftingStoragesLoading -= value;
    }

    /// <summary>Opens the crafting menu.</summary>
    /// <returns>Returns true if crafting page could be displayed.</returns>
    public static bool ShowCraftingPage()
    {
        BetterCrafting.EligibleStorages.Clear();
        BetterCrafting.MaterialStorages.Clear();
        if (IntegrationsManager.BetterCrafting.IsLoaded)
        {
            IntegrationsManager.BetterCrafting.Api.OpenCraftingMenu(false, false, null, null, null, false);
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
        this.events.GameLoop.UpdateTicked += BetterCrafting.OnUpdateTicked;
        this.events.GameLoop.UpdateTicking += BetterCrafting.OnUpdateTicking;
        this.events.Display.MenuChanged += BetterCrafting.OnMenuChanged;

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
        if (!IntegrationsManager.BetterCrafting.IsLoaded)
        {
            return;
        }

        IntegrationsManager.BetterCrafting.Api.MenuPopulateContainers += BetterCrafting.OnMenuPopulateContainers;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterCrafting.CraftingStoragesLoading -= BetterCrafting.OnCraftingStoragesLoading;
        this.events.GameLoop.UpdateTicked -= BetterCrafting.OnUpdateTicked;
        this.events.GameLoop.UpdateTicking -= BetterCrafting.OnUpdateTicking;
        this.events.Display.MenuChanged -= BetterCrafting.OnMenuChanged;

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
        if (!IntegrationsManager.BetterCrafting.IsLoaded)
        {
            return;
        }

        IntegrationsManager.BetterCrafting.Api.MenuPopulateContainers -= BetterCrafting.OnMenuPopulateContainers;
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
        BetterCrafting.HeldItem = BetterCrafting.instance.reflection.GetField<Item?>(__instance, "heldItem");
        BetterCrafting.instance.craftingStoragesLoading.InvokeAll(
            BetterCrafting.instance,
            new(BetterCrafting.EligibleStorages));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
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
            if (storage is not
                {
                    Data: Storage storageObject,
                })
            {
                continue;
            }

            foreach (var item in storageObject.Inventory)
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
        foreach (var storage in StorageHandler.All)
        {
            if (storage.CraftFromChest is FeatureOptionRange.Disabled or FeatureOptionRange.Default
                || storage.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
                || (storage.CraftFromChestDisableLocations.Contains("UndergroundMine")
                    && Game1.player.currentLocation is MineShaft mineShaft
                    && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
                || storage is not
                {
                    Data: Storage
                    {
                        Source: not null,
                    } storageObject,
                }
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
            && (!IntegrationsManager.BetterCrafting.IsLoaded
                || e.OldMenu?.GetType() != IntegrationsManager.BetterCrafting.Api.GetMenuType()))
        {
            return;
        }

        foreach (var storage in BetterCrafting.EligibleStorages)
        {
            if (storage is not
                {
                    Data: Storage storageObject,
                }
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
            if (storage is not
                {
                    Data: Storage storageObject,
                })
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
            if (storage is not
                {
                    Data: Storage storageObject,
                }
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

        if (heldItem is null)
        {
            BetterCrafting.HeldItem.SetValue(crafted);
        }
        else
        {
            if (!heldItem.Name.Equals(crafted.Name, StringComparison.OrdinalIgnoreCase)
                || !heldItem.getOne().canStackWith(crafted.getOne())
                || (heldItem.Stack + recipe.numberProducedPerCraft) - 1 >= heldItem.maximumStackSize())
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
        return;

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
                    if (storage is not
                        {
                            Data: Storage storageObject,
                        })
                    {
                        continue;
                    }

                    for (var i = storageObject.Inventory.Count - 1; i >= 0; --i)
                    {
                        var item = storageObject.Inventory[i];
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
                            storageObject.Inventory[i] = null;
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
                if (storage is not
                    {
                        Data: Storage storageObject,
                    }
                    || storageObject.Mutex is null)
                {
                    continue;
                }

                storageObject.Mutex.ReleaseLock();
            }
        }
    }

    private static void OnUpdateTicking(object? sender, UpdateTickingEventArgs e)
    {
        if (BetterCrafting.Craft is null || BetterCrafting.HeldItem is null || !BetterCrafting.MaterialStorages.Any())
        {
            return;
        }

        foreach (var storage in BetterCrafting.MaterialStorages)
        {
            if (storage is not
                {
                    Data: Storage storageObject,
                }
                || storageObject.Mutex is null)
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
        var amount = BetterCrafting.instance.input.IsDown(SButton.LeftShift)
            || BetterCrafting.instance.input.IsDown(SButton.RightShift)
                ? 5
                : 1;

        var crafted = recipe.createItem();
        if (heldItem is not null
            && (!heldItem.Name.Equals(crafted.Name, StringComparison.OrdinalIgnoreCase)
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
                if (storage is not
                    {
                        Data: Storage storageObject,
                    }
                    || storageObject.Mutex is null)
                {
                    continue;
                }

                foreach (var item in storageObject.Inventory.Where(
                    item => CraftingRecipe.ItemMatchesForCrafting(item, id)))
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
            if (storage is not
                {
                    Data: Storage storageObject,
                }
                || storageObject.Mutex is null)
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
