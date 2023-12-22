namespace StardewMods.BetterChests.Framework.Services;

using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Extensions;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Manages the item grab menu in the game.</summary>
internal sealed class ItemGrabMenuManager : BaseService
{
#nullable disable
    private static ItemGrabMenuManager instance;
#nullable enable
    private readonly PerScreen<InventoryMenuManager> bottomMenu;

    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<IContainer?> currentContainer = new();
    private readonly PerScreen<ItemGrabMenu?> currentMenu = new();
    private readonly PerScreen<InventoryMenuManager> topMenu;

    private EventHandler<ItemGrabMenuChangedEventArgs>? itemGrabMenuChanged;

    /// <summary>Initializes a new instance of the <see cref="ItemGrabMenuManager" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    public ItemGrabMenuManager(ILogging logging, IModEvents modEvents, Harmony harmony, ContainerFactory containerFactory)
        : base(logging)
    {
        // Init
        ItemGrabMenuManager.instance = this;
        this.containerFactory = containerFactory;
        this.topMenu = new PerScreen<InventoryMenuManager>(() => new InventoryMenuManager(logging));
        this.bottomMenu = new PerScreen<InventoryMenuManager>(() => new InventoryMenuManager(logging));

        // Events
        modEvents.Display.MenuChanged += this.OnMenuChanged;

        // Patches
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) }),
            new HarmonyMethod(typeof(ItemGrabMenuManager), nameof(ItemGrabMenuManager.InventoryMenu_draw_prefix)),
            new HarmonyMethod(typeof(ItemGrabMenuManager), nameof(ItemGrabMenuManager.InventoryMenu_draw_postfix)));
    }

    /// <summary>Gets the current container for the item grab menu.</summary>
    public IContainer? CurrentContainer => this.currentContainer.Value;

    /// <summary>Gets the current item grab menu.</summary>
    public ItemGrabMenu? CurrentMenu => this.currentMenu.Value;

    /// <summary>Gets the inventory menu manager for the top inventory menu.</summary>
    public IInventoryMenuManager TopMenu => this.topMenu.Value;

    /// <summary>Gets the inventory menu manager for the bottom inventory menu.</summary>
    public IInventoryMenuManager BottomMenu => this.bottomMenu.Value;

    /// <summary>Event raised when the item grab menu has changed.</summary>
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged
    {
        add => this.itemGrabMenuChanged += value;
        remove => this.itemGrabMenuChanged -= value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_prefix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(ItemGrabMenuManager.instance.topMenu.Value.Source)
            ? ItemGrabMenuManager.instance.topMenu.Value
            : __instance.Equals(ItemGrabMenuManager.instance.bottomMenu.Value.Source)
                ? ItemGrabMenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Context is null)
        {
            return;
        }

        // Save original inventory
        __state.Inventory = __instance.inventory;

        // Apply operations
        __instance.actualInventory = __state.ApplyOperation(__instance.actualInventory).ToList();
        __instance.inventory = [];
        foreach (var item in __instance.actualInventory)
        {
            var index = __state.Context.Items.IndexOf(item);
            __instance.inventory.Add(__state.Inventory[index]);
        }

        if (__instance.inventory.Count < __state.Capacity)
        {
            __instance.inventory.AddRange(__state.Inventory.TakeLast(__state.Capacity - __instance.inventory.Count));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_postfix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        if (__state?.Context is null)
        {
            return;
        }

        // Restore original inventory
        __instance.actualInventory = __state.Context.Items;
        __instance.inventory = __state.Inventory;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu
            {
                context: Chest chest,
            } itemGrabMenu
            || !this.containerFactory.TryGetOne(chest, out var container))
        {
            this.currentContainer.Value = null;
            this.currentMenu.Value = null;
            this.itemGrabMenuChanged.InvokeAll(this, new ItemGrabMenuChangedEventArgs());
            return;
        }

        this.currentContainer.Value = container;
        this.currentMenu.Value = itemGrabMenu;
        var eventArgs = new ItemGrabMenuChangedEventArgs(container, itemGrabMenu);

        // Update top menu
        this.topMenu.Value.Reset();
        this.topMenu.Value.Source = itemGrabMenu.ItemsToGrabMenu;
        this.topMenu.Value.Context = container;
        if (itemGrabMenu.ItemsToGrabMenu.highlightMethod != this.topMenu.Value.HighlightMethod)
        {
            this.topMenu.Value.OriginalHighlightMethod = itemGrabMenu.ItemsToGrabMenu.highlightMethod;
            itemGrabMenu.ItemsToGrabMenu.highlightMethod = this.topMenu.Value.OriginalHighlightMethod;
        }

        // Update bottom menu
        this.bottomMenu.Value.Reset();
        this.bottomMenu.Value.Source = itemGrabMenu.inventory;

        if (itemGrabMenu.inventory.actualInventory.Equals(Game1.player.Items) && this.containerFactory.TryGetOne(Game1.player, out container))
        {
            this.bottomMenu.Value.Context = container;
        }

        if (itemGrabMenu.inventory.highlightMethod != this.bottomMenu.Value.HighlightMethod)
        {
            this.bottomMenu.Value.OriginalHighlightMethod = itemGrabMenu.inventory.highlightMethod;
            itemGrabMenu.inventory.highlightMethod = this.bottomMenu.Value.OriginalHighlightMethod;
        }

        // Reset filters
        this.itemGrabMenuChanged.InvokeAll(this, eventArgs);
    }
}
