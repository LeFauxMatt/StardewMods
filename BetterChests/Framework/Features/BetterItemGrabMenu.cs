namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.BetterChests.Framework.UI;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewValley.Menus;

/// <summary>
///     Enhances the <see cref="StardewValley.Menus.ItemGrabMenu" /> to support filters, sorting, and scrolling.
/// </summary>
internal sealed class BetterItemGrabMenu : Feature
{
    private const string Id = "furyx639.BetterChests/BetterItemGrabMenu";

    private static readonly MethodBase InventoryMenuConstructor = AccessTools.Constructor(
        typeof(InventoryMenu),
        new[]
        {
            typeof(int),
            typeof(int),
            typeof(bool),
            typeof(IList<Item>),
            typeof(InventoryMenu.highlightThisItem),
            typeof(int),
            typeof(int),
            typeof(int),
            typeof(int),
            typeof(bool),
        });

    private static readonly MethodBase InventoryMenuDraw = AccessTools.Method(
        typeof(InventoryMenu),
        nameof(InventoryMenu.draw),
        new[]
        {
            typeof(SpriteBatch),
            typeof(int),
            typeof(int),
            typeof(int),
        });

    private static readonly ConstructorInfo[] ItemGrabMenuConstructor =
    {
        AccessTools.Constructor(
            typeof(ItemGrabMenu),
            new[]
            {
                typeof(IList<Item>),
                typeof(bool),
                typeof(bool),
                typeof(InventoryMenu.highlightThisItem),
                typeof(ItemGrabMenu.behaviorOnItemSelect),
                typeof(string),
                typeof(ItemGrabMenu.behaviorOnItemSelect),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(bool),
                typeof(int),
                typeof(Item),
                typeof(int),
                typeof(object),
                typeof(ItemExitBehavior),
                typeof(bool),
            }),
        AccessTools.Constructor(
            typeof(ItemGrabMenu),
            new[]
            {
                typeof(IList<Item>),
                typeof(object),
            }),
    };

    private static readonly MethodBase ItemGrabMenuDraw = AccessTools.Method(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.draw),
        new[] { typeof(SpriteBatch) });

    private static readonly MethodBase ItemGrabMenuOrganizeItemsInList = AccessTools.Method(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.organizeItemsInList));

    private static readonly MethodBase MenuWithInventoryConstructor = AccessTools.Constructor(
        typeof(MenuWithInventory),
        new[]
        {
            typeof(InventoryMenu.highlightThisItem),
            typeof(bool),
            typeof(bool),
            typeof(int),
            typeof(int),
            typeof(int),
            typeof(ItemExitBehavior),
            typeof(bool),
        });

    private static readonly MethodBase MenuWithInventoryDraw = AccessTools.Method(
        typeof(MenuWithInventory),
        nameof(MenuWithInventory.draw),
        new[]
        {
            typeof(SpriteBatch),
            typeof(bool),
            typeof(bool),
            typeof(int),
            typeof(int),
            typeof(int),
        });

#nullable disable
    private static BetterItemGrabMenu instance;
#nullable enable

    private readonly ModConfig config;
    private readonly PerScreen<StorageNode?> perScreenContext = new();
    private readonly PerScreen<ItemGrabMenu?> perScreenCurrentMenu = new();
    private readonly Harmony harmony;
    private readonly IModHelper helper;
    private readonly PerScreen<DisplayedItems?> perScreenInventory = new();
    private readonly PerScreen<DisplayedItems?> perScreenItemsToGrabMenu = new();
    private readonly PerScreen<Stack<IClickableMenu>> perScreenOverlaidMenus = new(() => new());
    private readonly PerScreen<bool> perScreenRefreshInventory = new();
    private readonly PerScreen<bool> perScreenRefreshItemsToGrabMenu = new();
    private readonly PerScreen<int> perScreenTopPadding = new();

    private EventHandler<ItemGrabMenu>? constructed;
    private EventHandler<ItemGrabMenu>? constructing;
    private EventHandler<SpriteBatch>? drawingMenu;

    private BetterItemGrabMenu(IModHelper helper, ModConfig config)
    {
        this.helper = helper;
        this.config = config;
        this.harmony = new(BetterItemGrabMenu.Id);
    }

    /// <summary>
    ///     Raised after <see cref="ItemGrabMenu" /> constructor.
    /// </summary>
    public static event EventHandler<ItemGrabMenu> Constructed
    {
        add => BetterItemGrabMenu.instance.constructed += value;
        remove => BetterItemGrabMenu.instance.constructed -= value;
    }

    /// <summary>
    ///     Raised before <see cref="ItemGrabMenu" /> constructor.
    /// </summary>
    public static event EventHandler<ItemGrabMenu> Constructing
    {
        add => BetterItemGrabMenu.instance.constructing += value;
        remove => BetterItemGrabMenu.instance.constructing -= value;
    }

    /// <summary>
    ///     Raised before <see cref="ItemGrabMenu" /> is drawn.
    /// </summary>
    public static event EventHandler<SpriteBatch> DrawingMenu
    {
        add => BetterItemGrabMenu.instance.drawingMenu += value;
        remove => BetterItemGrabMenu.instance.drawingMenu -= value;
    }

    /// <summary>
    ///     Gets the current <see cref="Storage" /> context.
    /// </summary>
    public static StorageNode? Context
    {
        get => BetterItemGrabMenu.instance.perScreenContext.Value;
        private set => BetterItemGrabMenu.instance.perScreenContext.Value = value;
    }

    /// <summary>
    ///     Gets the bottom inventory menu.
    /// </summary>
    public static DisplayedItems? Inventory
    {
        get => BetterItemGrabMenu.instance.perScreenInventory.Value;
        private set => BetterItemGrabMenu.instance.perScreenInventory.Value = value;
    }

    /// <summary>
    ///     Gets the top inventory menu.
    /// </summary>
    public static DisplayedItems? ItemsToGrabMenu
    {
        get => BetterItemGrabMenu.instance.perScreenItemsToGrabMenu.Value;
        private set => BetterItemGrabMenu.instance.perScreenItemsToGrabMenu.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to refresh inventory items on the next tick.
    /// </summary>
    public static bool RefreshInventory
    {
        get => BetterItemGrabMenu.instance.perScreenRefreshInventory.Value;
        set => BetterItemGrabMenu.instance.perScreenRefreshInventory.Value = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether to refresh chest items on the next tick.
    /// </summary>
    public static bool RefreshItemsToGrabMenu
    {
        get => BetterItemGrabMenu.instance.perScreenRefreshItemsToGrabMenu.Value;
        set => BetterItemGrabMenu.instance.perScreenRefreshItemsToGrabMenu.Value = value;
    }

    /// <summary>
    ///     Gets or sets the padding for the top of the ItemsToGrabMenu.
    /// </summary>
    public static int TopPadding
    {
        get => BetterItemGrabMenu.instance.perScreenTopPadding.Value;
        set => BetterItemGrabMenu.instance.perScreenTopPadding.Value = value;
    }

    private ItemGrabMenu? CurrentMenu
    {
        get => this.perScreenCurrentMenu.Value;
        set => this.perScreenCurrentMenu.Value = value;
    }

    private Stack<IClickableMenu> OverlaidMenus => this.perScreenOverlaidMenus.Value;

    /// <summary>
    ///     Adds an overlay to the current <see cref="StardewValley.Menus.ItemGrabMenu" />.
    /// </summary>
    /// <param name="menu">The <see cref="StardewValley.Menus.IClickableMenu" /> to add.</param>
    public static void AddOverlay(IClickableMenu menu)
    {
        BetterItemGrabMenu.instance.OverlaidMenus.Push(menu);
    }

    /// <summary>
    ///     Initializes <see cref="BetterItemGrabMenu" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterItemGrabMenu" /> class.</returns>
    public static BetterItemGrabMenu Init(IModHelper helper, ModConfig config)
    {
        return BetterItemGrabMenu.instance ??= new(helper, config);
    }

    /// <summary>
    ///     Invokes the BetterItemGrabMenu.DrawingMenu event.
    /// </summary>
    /// <param name="b">The sprite batch to draw to.</param>
    public static void InvokeDrawingMenu(SpriteBatch b)
    {
        BetterItemGrabMenu.instance.drawingMenu.InvokeAll(BetterItemGrabMenu.instance, b);
    }

    /// <summary>
    ///     Removes an overlay from the current <see cref="StardewValley.Menus.ItemGrabMenu" />.
    /// </summary>
    /// <returns>Returns the removed overlay.</returns>
    public static IClickableMenu RemoveOverlay()
    {
        return BetterItemGrabMenu.instance.OverlaidMenus.Pop();
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu_Low;
        this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        this.helper.Events.Player.InventoryChanged += BetterItemGrabMenu.OnInventoryChanged;
        this.helper.Events.World.ChestInventoryChanged += BetterItemGrabMenu.OnChestInventoryChanged;

        // Patches
        this.harmony.Patch(
            BetterItemGrabMenu.InventoryMenuDraw,
            transpiler: new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.InventoryMenu_draw_transpiler)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[0],
            postfix: new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_postfix)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[1],
            postfix: new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_postfix)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[0],
            new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_prefix)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[1],
            new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_prefix)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[0],
            transpiler: new(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_transpiler)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuDraw,
            new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_draw_prefix)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuDraw,
            transpiler: new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_draw_transpiler)));
        this.harmony.Patch(
            BetterItemGrabMenu.ItemGrabMenuOrganizeItemsInList,
            postfix: new(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.ItemGrabMenu_organizeItemsInList_postfix)));
        this.harmony.Patch(
            BetterItemGrabMenu.MenuWithInventoryConstructor,
            postfix: new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.MenuWithInventory_constructor_postfix)));
        this.harmony.Patch(
            BetterItemGrabMenu.MenuWithInventoryDraw,
            transpiler: new(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.MenuWithInventory_draw_transpiler)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu_Low;
        this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.helper.Events.Input.CursorMoved -= this.OnCursorMoved;
        this.helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
        this.helper.Events.Player.InventoryChanged -= BetterItemGrabMenu.OnInventoryChanged;
        this.helper.Events.World.ChestInventoryChanged -= BetterItemGrabMenu.OnChestInventoryChanged;

        // Patches
        this.harmony.Unpatch(
            BetterItemGrabMenu.InventoryMenuDraw,
            AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.InventoryMenu_draw_transpiler)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[0],
            AccessTools.Method(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_postfix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[1],
            AccessTools.Method(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_postfix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[0],
            AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_prefix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[1],
            AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_prefix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuConstructor[0],
            AccessTools.Method(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.ItemGrabMenu_constructor_transpiler)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuDraw,
            AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_draw_prefix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuDraw,
            AccessTools.Method(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.ItemGrabMenu_draw_transpiler)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.ItemGrabMenuOrganizeItemsInList,
            AccessTools.Method(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.ItemGrabMenu_organizeItemsInList_postfix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.MenuWithInventoryConstructor,
            AccessTools.Method(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.MenuWithInventory_constructor_postfix)));
        this.harmony.Unpatch(
            BetterItemGrabMenu.MenuWithInventoryDraw,
            AccessTools.Method(
                typeof(BetterItemGrabMenu),
                nameof(BetterItemGrabMenu.MenuWithInventory_draw_transpiler)));
    }

    private static IList<Item> ActualInventory(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        return ReferenceEquals(inventoryMenu, BetterItemGrabMenu.Inventory?.Menu)
            ? BetterItemGrabMenu.Inventory.Items
            : ReferenceEquals(inventoryMenu, BetterItemGrabMenu.ItemsToGrabMenu?.Menu)
                ? BetterItemGrabMenu.ItemsToGrabMenu.Items
                : actualInventory;
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static InventoryMenu GetItemsToGrabMenu(
        int xPosition,
        int yPosition,
        bool playerInventory,
        IList<Item> actualInventory,
        InventoryMenu.highlightThisItem highlightMethod,
        int capacity,
        int rows,
        int horizontalGap,
        int verticalGap,
        bool drawSlots,
        ItemGrabMenu menu)
    {
        if (BetterItemGrabMenu.Context is not
            {
                ResizeChestMenu: FeatureOption.Enabled, MenuCapacity: > 0, MenuRows: > 0,
            })
        {
            return new(
                xPosition,
                yPosition,
                playerInventory,
                actualInventory,
                highlightMethod,
                capacity,
                rows,
                horizontalGap,
                verticalGap,
                drawSlots);
        }

        return new(
            menu.xPositionOnScreen + (Game1.tileSize / 2),
            menu.yPositionOnScreen,
            playerInventory,
            actualInventory,
            highlightMethod,
            BetterItemGrabMenu.Context.MenuCapacity,
            BetterItemGrabMenu.Context.MenuRows,
            horizontalGap,
            verticalGap,
            drawSlots);
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))))
            {
                yield return instruction;
                yield return new(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(
                    typeof(BetterItemGrabMenu),
                    nameof(BetterItemGrabMenu.ActualInventory));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        if (BetterItemGrabMenu.Context is null)
        {
            BetterItemGrabMenu.Inventory = null;
            BetterItemGrabMenu.ItemsToGrabMenu = null;
            BetterItemGrabMenu.instance.constructed.InvokeAll(BetterItemGrabMenu.instance, __instance);
            return;
        }

        __instance.drawBG = false;
        __instance.yPositionOnScreen -= BetterItemGrabMenu.TopPadding;
        __instance.height += BetterItemGrabMenu.TopPadding;
        if (__instance.chestColorPicker is not null)
        {
            __instance.chestColorPicker.yPositionOnScreen -= BetterItemGrabMenu.TopPadding;
        }

        var inventory = new DisplayedItems(__instance.inventory, false);
        var itemsToGrabMenu = new DisplayedItems(__instance.ItemsToGrabMenu, true);

        if (BetterItemGrabMenu.instance.CurrentMenu is not null
            && ReferenceEquals(__instance.context, BetterItemGrabMenu.instance.CurrentMenu.context))
        {
            inventory.Offset = BetterItemGrabMenu.Inventory?.Offset ?? 0;
            itemsToGrabMenu.Offset = BetterItemGrabMenu.ItemsToGrabMenu?.Offset ?? 0;
        }

        BetterItemGrabMenu.instance.CurrentMenu = __instance;
        BetterItemGrabMenu.Inventory = inventory;
        BetterItemGrabMenu.ItemsToGrabMenu = itemsToGrabMenu;
        BetterItemGrabMenu.instance.constructed.InvokeAll(BetterItemGrabMenu.instance, __instance);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_constructor_prefix(ItemGrabMenu __instance, object? context)
    {
        if (context is null || !Storages.TryGetOne(context, out var storage))
        {
            BetterItemGrabMenu.Context = null;
            BetterItemGrabMenu.instance.constructing.InvokeAll(BetterItemGrabMenu.instance, __instance);
            return;
        }

        __instance.context = context;
        BetterItemGrabMenu.Context = storage;
        BetterItemGrabMenu.instance.constructing.InvokeAll(BetterItemGrabMenu.instance, __instance);
    }

    /// <summary>Replace assignments to ItemsToGrabMenu with method.</summary>
    [SuppressMessage(
        "ReSharper",
        "HeapView.BoxingAllocation",
        Justification = "Boxing allocation is required for Harmony.")]
    private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        CodeInstruction? newObj = null;

        foreach (var instruction in instructions)
        {
            if (newObj is not null)
            {
                if (instruction.StoresField(
                    AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))))
                {
                    yield return new(OpCodes.Ldarg_0);
                    yield return new(
                        CodeInstruction.Call(
                            typeof(BetterItemGrabMenu),
                            nameof(BetterItemGrabMenu.GetItemsToGrabMenu)));
                }
                else
                {
                    yield return newObj;
                }

                yield return instruction;
                newObj = null;
            }
            else if (instruction.Is(OpCodes.Newobj, BetterItemGrabMenu.InventoryMenuConstructor))
            {
                newObj = instruction;
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_draw_prefix(SpriteBatch b)
    {
        if (BetterItemGrabMenu.Context is null)
        {
            return;
        }

        b.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);
        BetterItemGrabMenu.InvokeDrawingMenu(b);
    }

    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var patchCount = -1;
        var addPadding = false;

        foreach (var instruction in instructions)
        {
            yield return instruction;

            switch (patchCount)
            {
                case -1 when instruction.LoadsField(
                    AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))):
                    patchCount = 3;
                    break;
                case > 0 when instruction.LoadsField(
                    AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))):
                    --patchCount;
                    yield return new(
                        OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.TopPadding)));
                    yield return new(OpCodes.Add);
                    break;
                default:
                    if (instruction.LoadsField(
                        AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))))
                    {
                        addPadding = true;
                    }
                    else if (addPadding)
                    {
                        addPadding = false;
                        yield return new(
                            OpCodes.Call,
                            AccessTools.PropertyGetter(
                                typeof(BetterItemGrabMenu),
                                nameof(BetterItemGrabMenu.TopPadding)));
                        yield return new(instruction.opcode);
                    }

                    break;
            }
        }
    }

    private static void ItemGrabMenu_organizeItemsInList_postfix(IList<Item> items)
    {
        if (BetterItemGrabMenu.instance.CurrentMenu is null)
        {
            return;
        }

        BetterItemGrabMenu.RefreshInventory |= ReferenceEquals(
            BetterItemGrabMenu.instance.CurrentMenu.inventory.actualInventory,
            items);
        BetterItemGrabMenu.RefreshItemsToGrabMenu |= ReferenceEquals(
            BetterItemGrabMenu.instance.CurrentMenu.ItemsToGrabMenu.actualInventory,
            items);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void MenuWithInventory_constructor_postfix(MenuWithInventory __instance)
    {
        if (__instance is not ItemGrabMenu || BetterItemGrabMenu.Context is null)
        {
            BetterItemGrabMenu.TopPadding = 0;
        }
    }

    private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsField(
                AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))))
            {
                yield return instruction;
                yield return new(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(BetterItemGrabMenu), nameof(BetterItemGrabMenu.TopPadding)));
                yield return new(OpCodes.Add);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        BetterItemGrabMenu.RefreshItemsToGrabMenu |= Game1.activeClickableMenu is ItemGrabMenu;
        BetterItemGrabMenu.RefreshInventory |= Game1.activeClickableMenu is ItemGrabMenu;
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        BetterItemGrabMenu.RefreshItemsToGrabMenu |= Game1.activeClickableMenu is ItemGrabMenu;
        BetterItemGrabMenu.RefreshInventory |= Game1.activeClickableMenu is ItemGrabMenu && e.IsLocalPlayer;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when this.OverlaidMenus.Any():
                this.OverlaidMenus.Last().receiveLeftClick(x, y);
                break;
            case SButton.MouseRight when this.OverlaidMenus.Any():
                this.OverlaidMenus.Last().receiveRightClick(x, y);
                break;
            case SButton.MouseLeft when BetterItemGrabMenu.Inventory?.LeftClick(x, y) == true:
                break;
            case SButton.MouseLeft when BetterItemGrabMenu.ItemsToGrabMenu?.LeftClick(x, y) == true:
                break;
            default:
                return;
        }

        this.helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.CurrentMenu is null || this.OverlaidMenus.Any())
        {
            return;
        }

        var displayedItems =
            BetterItemGrabMenu.Inventory is not null
            && this.CurrentMenu.currentlySnappedComponent is not null
            && BetterItemGrabMenu.Inventory.Menu.inventory.Contains(this.CurrentMenu.currentlySnappedComponent)
                ? BetterItemGrabMenu.Inventory
                : BetterItemGrabMenu.ItemsToGrabMenu;
        if (displayedItems is null)
        {
            return;
        }

        var offset = displayedItems.Offset;
        if (this.config.ControlScheme.ScrollUp.JustPressed()
            && (this.CurrentMenu.currentlySnappedComponent is null
                || displayedItems.Menu.inventory.Take(12).Contains(this.CurrentMenu.currentlySnappedComponent)))
        {
            displayedItems.Offset -= this.config.ControlScheme.ScrollPage.IsDown() ? displayedItems.Menu.rows : 1;
            if (offset != displayedItems.Offset)
            {
                this.helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.ScrollUp);
            }
        }

        if (this.config.ControlScheme.ScrollDown.JustPressed()
            && (this.CurrentMenu.currentlySnappedComponent is null
                || displayedItems.Menu.inventory.TakeLast(12).Contains(this.CurrentMenu.currentlySnappedComponent)))
        {
            displayedItems.Offset += this.config.ControlScheme.ScrollPage.IsDown() ? displayedItems.Menu.rows : 1;
            if (offset != displayedItems.Offset)
            {
                this.helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.ScrollDown);
            }
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.OverlaidMenus.Any())
        {
            this.OverlaidMenus.Last().performHoverAction(x, y);
            return;
        }

        BetterItemGrabMenu.Inventory?.Hover(x, y);
        BetterItemGrabMenu.ItemsToGrabMenu?.Hover(x, y);
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.OverlaidMenus.Any())
        {
            this.OverlaidMenus.Last().receiveScrollWheelAction(e.Delta);
            return;
        }

        if (BetterItemGrabMenu.Inventory?.Menu.isWithinBounds(x, y) == true)
        {
            var scroll = this.config.ControlScheme.ScrollPage.IsDown() ? BetterItemGrabMenu.Inventory.Menu.rows : 1;
            BetterItemGrabMenu.Inventory.Offset += e.Delta > 0 ? -scroll : scroll;
        }

        if (BetterItemGrabMenu.ItemsToGrabMenu?.Menu.isWithinBounds(x, y) == true)
        {
            var scroll = this.config.ControlScheme.ScrollPage.IsDown()
                ? BetterItemGrabMenu.ItemsToGrabMenu.Menu.rows
                : 1;
            BetterItemGrabMenu.ItemsToGrabMenu.Offset += e.Delta > 0 ? -scroll : scroll;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        BetterItemGrabMenu.ItemsToGrabMenu?.Draw(e.SpriteBatch);
        BetterItemGrabMenu.Inventory?.Draw(e.SpriteBatch);
    }

    [EventPriority(EventPriority.Low)]
    private void OnRenderedActiveMenu_Low(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        if (this.OverlaidMenus.Any())
        {
            foreach (var overlay in this.OverlaidMenus)
            {
                overlay.draw(e.SpriteBatch);
            }

            this.CurrentMenu.drawMouse(e.SpriteBatch);
            return;
        }

        if (this.CurrentMenu.hoveredItem is not null)
        {
            IClickableMenu.drawToolTip(
                e.SpriteBatch,
                this.CurrentMenu.hoveredItem.getDescription(),
                this.CurrentMenu.hoveredItem.DisplayName,
                this.CurrentMenu.hoveredItem,
                this.CurrentMenu.heldItem != null);
        }
        else if (!string.IsNullOrWhiteSpace(this.CurrentMenu.hoverText))
        {
            if (this.CurrentMenu.hoverAmount > 0)
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    this.CurrentMenu.hoverText,
                    string.Empty,
                    null,
                    true,
                    -1,
                    0,
                    null,
                    -1,
                    null,
                    this.CurrentMenu.hoverAmount);
            }
            else
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, this.CurrentMenu.hoverText, Game1.smallFont);
            }
        }

        this.CurrentMenu.drawMouse(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            { } clickableMenu when clickableMenu.GetChildMenu() is ItemGrabMenu itemGrabMenu => itemGrabMenu,
            ItemGrabMenu itemGrabMenu => itemGrabMenu,
            _ => null,
        };

        if (!ReferenceEquals(menu, this.CurrentMenu))
        {
            if (menu is null or { context: null })
            {
                this.CurrentMenu = null;
                this.OverlaidMenus.Clear();
            }
        }

        if (!BetterItemGrabMenu.RefreshInventory && !BetterItemGrabMenu.RefreshItemsToGrabMenu)
        {
            return;
        }

        var refreshInventory = BetterItemGrabMenu.RefreshInventory;
        var refreshItemsToGrabMenu = BetterItemGrabMenu.RefreshItemsToGrabMenu;
        BetterItemGrabMenu.RefreshInventory = false;
        BetterItemGrabMenu.RefreshItemsToGrabMenu = false;
        if (menu is null)
        {
            return;
        }

        if (refreshInventory)
        {
            BetterItemGrabMenu.Inventory?.RefreshItems();
        }

        if (refreshItemsToGrabMenu)
        {
            BetterItemGrabMenu.ItemsToGrabMenu?.RefreshItems();
        }
    }
}