namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Common.Extensions;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using Common.Models;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Models.GameObjects.Storages;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="IMenuItems" />
[FuryCoreService(true)]
internal class MenuItems : IMenuItems, IModService
{
    private readonly PerScreen<IStorageContainer> _context = new();
    private readonly PerScreen<IClickableComponent> _downArrow = new();
    private readonly Lazy<IGameObjects> _gameObjects;
    private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();
    private readonly PerScreen<IDictionary<string, bool>> _itemFilterCache = new(() => new Dictionary<string, bool>());
    private readonly PerScreen<HashSet<ItemMatcher>> _itemFilters = new(() => new());
    private readonly PerScreen<IDictionary<string, bool>> _itemHighlightCache = new(() => new Dictionary<string, bool>());
    private readonly PerScreen<HashSet<ItemMatcher>> _itemHighlighters = new(() => new());
    private readonly PerScreen<IList<int>> _itemIndexes = new();
    private readonly PerScreen<IList<Item>> _itemsFiltered = new();
    private readonly PerScreen<IEnumerable<Item>> _itemsSorted = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<int> _menuColumns = new();
    private readonly PerScreen<int> _offset = new(() => 0);
    private readonly PerScreen<Range<int>> _range = new(() => new());
    private readonly PerScreen<bool> _refreshInventory = new();
    private readonly PerScreen<Func<Item, int>> _sortMethod = new();
    private readonly PerScreen<IClickableComponent> _upArrow = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuItems" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuItems(ConfigData config, IModHelper helper, IModServices services)
    {
        MenuItems.Instance = this;
        this.Config = config;
        this._gameObjects = services.Lazy<IGameObjects>();
        helper.Events.World.ChestInventoryChanged += this.OnChestInventoryChanged;
        helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
        helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

        services.Lazy<ICustomEvents>(events =>
        {
            events.MenuItemsChanged += this.OnMenuItemsChanged;
            events.MenuComponentsLoading += this.OnMenuComponentsLoading;
            events.MenuComponentPressed += this.OnMenuComponentPressed;
        });

        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{FuryCore.ModUniqueId}.{nameof(MenuItems)}";
                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Method(
                        typeof(InventoryMenu),
                        nameof(InventoryMenu.draw),
                        new[]
                        {
                            typeof(SpriteBatch), typeof(int), typeof(int), typeof(int),
                        }),
                    typeof(MenuItems),
                    nameof(MenuItems.InventoryMenu_draw_transpiler),
                    PatchType.Transpiler);
                harmonyHelper.ApplyPatches(id);
            });
    }

    /// <inheritdoc />
    public IList<Item> ActualInventory
    {
        get => this.Menu?.ItemsToGrabMenu.actualInventory;
    }

    /// <inheritdoc />
    public IStorageContainer Context
    {
        get => this._context.Value;
        private set => this._context.Value = value;
    }

    /// <summary>
    ///     Gets any filters that will be applied to the items.
    /// </summary>
    public HashSet<ItemMatcher> ItemFilters
    {
        get => this._itemFilters.Value;
    }

    /// <summary>
    ///     Gets cached values for which items meet the highlight conditions.
    /// </summary>
    public IDictionary<string, bool> ItemHighlightCache
    {
        get => this._itemHighlightCache.Value;
    }

    /// <summary>
    ///     Gets any highlighters that will be applied to the items.
    /// </summary>
    public HashSet<ItemMatcher> ItemHighlighters
    {
        get => this._itemHighlighters.Value;
    }

    /// <inheritdoc />
    public IEnumerable<Item> ItemsDisplayed
    {
        get
        {
            if (this.Menu is null)
            {
                return null;
            }

            if (this.RefreshInventory)
            {
                foreach (var (slot, index) in this.Menu.ItemsToGrabMenu.inventory.Select((slot, index) => (slot, index + this.Offset * this.MenuColumns)))
                {
                    slot.name = (index < this.ItemIndexes.Count ? this.ItemIndexes[index] : int.MaxValue).ToString();
                }

                this.RefreshInventory = false;
            }

            return this.ItemsFiltered.Skip(this.Offset * this.MenuColumns);
        }
    }

    /// <inheritdoc />
    public ItemGrabMenu Menu
    {
        get => this._menu.Value;
        private set => this._menu.Value = value;
    }

    /// <inheritdoc />
    public int Offset
    {
        get
        {
            this.Range.Maximum = Math.Max(0, (this.ItemsFiltered.Count - this.Menu.ItemsToGrabMenu.capacity).RoundUp(this.MenuColumns) / this.MenuColumns);
            return this.Range.Clamp(this._offset.Value);
        }

        set
        {
            this.Range.Maximum = Math.Max(0, (this.ItemsFiltered.Count - this.Menu.ItemsToGrabMenu.capacity).RoundUp(this.MenuColumns) / this.MenuColumns);
            value = this.Range.Clamp(value);
            if (this._offset.Value != value)
            {
                this._offset.Value = value;
                this.RefreshInventory = true;
            }
        }
    }

    /// <inheritdoc />
    public int Rows
    {
        get => this.Range.Maximum;
    }

    /// <summary>
    ///     Gets or sets the method used to sort items.
    /// </summary>
    public Func<Item, int> SortMethod
    {
        get => this._sortMethod.Value;
        set => this._sortMethod.Value = value;
    }

    private static MenuItems Instance { get; set; }

    private ConfigData Config { get; }

    private IClickableComponent DownArrow
    {
        get => this._downArrow.Value ??= new CustomClickableComponent(new(new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new(421, 472, 11, 12), Game1.pixelZoom));
    }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IDictionary<string, bool> ItemFilterCache
    {
        get => this._itemFilterCache.Value;
    }

    private IList<Item> ItemsFiltered
    {
        get
        {
            if (this._itemsFiltered.Value is null)
            {
                this._itemsFiltered.Value = this.ItemsSorted.Where(this.FilterMethod).ToList();
                this.ItemIndexes = null;
                this.RefreshInventory = true;
            }

            return this._itemsFiltered.Value;
        }

        set => this._itemsFiltered.Value = value;
    }

    private IList<int> ItemIndexes
    {
        get
        {
            return this._itemIndexes.Value ??= this.ItemsFiltered.Select(item => this.ActualInventory.IndexOf(item)).ToList();
        }
        set => this._itemIndexes.Value = value;
    }

    private IEnumerable<Item> ItemsSorted
    {
        get
        {
            if (this._itemsSorted.Value is null)
            {
                this._itemsSorted.Value = this.SortMethod is not null
                    ? this.ActualInventory.OrderBy(this.SortMethod)
                    : this.ActualInventory.AsEnumerable();
                this.ItemsFiltered = null;
            }

            return this._itemsSorted.Value;
        }

        set => this._itemsSorted.Value = value;
    }

    private int MenuColumns
    {
        get => this._menuColumns.Value;
        set => this._menuColumns.Value = value;
    }

    private InventoryMenu.highlightThisItem OldHighlightMethod
    {
        get => this._highlightMethod.Value;
        set => this._highlightMethod.Value = value;
    }

    private Range<int> Range
    {
        get => this._range.Value;
    }

    private bool RefreshInventory
    {
        get => this._refreshInventory.Value;
        set => this._refreshInventory.Value = value;
    }

    private IClickableComponent UpArrow
    {
        get => this._upArrow.Value ??= new CustomClickableComponent(new(new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new(421, 459, 11, 12), Game1.pixelZoom));
    }

    /// <inheritdoc />
    public void AddSortMethod(Func<Item, int> sortMethod)
    {
        this.SortMethod = sortMethod;
        this.ItemsSorted = null;
    }

    /// <inheritdoc />
    public void ForceRefresh()
    {
        this.ItemFilterCache.Clear();
        this.ItemHighlightCache.Clear();
        this.ItemsFiltered = null;
        this.ItemsSorted = null;
    }

    private static IList<Item> DisplayedItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        if (MenuItems.Instance.Menu is null || !ReferenceEquals(inventoryMenu, MenuItems.Instance.Menu.ItemsToGrabMenu))
        {
            return actualInventory;
        }

        return MenuItems.Instance.ItemsDisplayed.Take(inventoryMenu.capacity).ToList();
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(InventoryMenu)}.{nameof(InventoryMenu.draw)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Actual Inventory Patch
        // Replaces all actualInventory with ItemsDisplayed.DisplayedItems(actualInventory)
        // which can filter/sort items separately from the actual inventory.
        patcher.AddPatchLoop(
            code =>
            {
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(MenuItems), nameof(MenuItems.DisplayedItems))));
            },
            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))));

        // Fill code buffer
        foreach (var inCode in instructions)
        {
            // Return patched code segments
            foreach (var outCode in patcher.From(inCode))
            {
                yield return outCode;
            }
        }

        // Return remaining code
        foreach (var outCode in patcher.FlushBuffer())
        {
            yield return outCode;
        }

        Log.Trace($"{patcher.AppliedPatches.ToString()} / {patcher.TotalPatches.ToString()} patches applied.");
        if (patcher.AppliedPatches < patcher.TotalPatches)
        {
            Log.Warn("Failed to applied all patches!");
        }
    }

    private bool FilterMethod(Item item)
    {
        if (item is null)
        {
            return false;
        }

        if (!this.ItemFilterCache.TryGetValue(item.Name, out var filtered))
        {
            filtered = this.ItemFilters.All(itemMatcher => itemMatcher.Matches(item));
            this.ItemFilterCache.Add(item.Name, filtered);
        }

        return filtered;
    }

    private bool HighlightMethod(Item item)
    {
        if (item is null)
        {
            return false;
        }

        if (!this.ItemHighlightCache.TryGetValue(item.Name, out var highlighted))
        {
            highlighted = this.OldHighlightMethod?.Invoke(item) == true
                          && this.ItemHighlighters.All(itemMatcher => itemMatcher.Matches(item));
            this.ItemHighlightCache.Add(item.Name, highlighted);
        }

        return highlighted;
    }

    private void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
    {
        if (this.Menu is not null && this.Context is StorageChest storageChest && ReferenceEquals(e.Chest, storageChest.Chest))
        {
            this.ItemsFiltered = null;
            this.ItemsSorted = null;
        }
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer)
        {
            this.ItemsFiltered = null;
            this.ItemsSorted = null;
        }
    }

    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (!this.Config.ScrollMenuOverflow)
        {
            return;
        }

        if (e.Component == this.UpArrow)
        {
            this.Offset--;
        }
        else if (e.Component == this.DownArrow)
        {
            this.Offset++;
        }
        else
        {
            return;
        }

        this.UpArrow.Component.visible = this.Offset > 0;
        this.DownArrow.Component.visible = this.Offset < this.Rows;
    }

    private void OnMenuComponentsLoading(object sender, MenuComponentsLoadingEventArgs e)
    {
        if (!this.Config.ScrollMenuOverflow || e.Menu is not ItemGrabMenu { context: { } context, ItemsToGrabMenu: { } itemsToGrabMenu } itemGrabMenu || this.GameObjects.TryGetGameObject(context, out var gameObject) || gameObject is not IStorageContainer)
        {
            return;
        }

        // Add Up/Down Arrows
        e.AddComponent(this.UpArrow);
        e.AddComponent(this.DownArrow);

        // Initialize Arrow visibility
        this.UpArrow.Component.visible = this.Offset > 0;
        this.DownArrow.Component.visible = this.Offset < this.Rows;

        // Align to ItemsToGrabMenu top/bottom inventory slots
        var topSlot = itemGrabMenu.GetColumnCount() - 1;
        var bottomSlot = itemsToGrabMenu.capacity - 1;
        this.UpArrow.Component.bounds.X = itemsToGrabMenu.xPositionOnScreen + itemsToGrabMenu.width + 8;
        this.UpArrow.Component.bounds.Y = itemsToGrabMenu.inventory[topSlot].bounds.Center.Y - 6 * Game1.pixelZoom;
        this.DownArrow.Component.bounds.X = itemsToGrabMenu.xPositionOnScreen + itemsToGrabMenu.width + 8;
        this.DownArrow.Component.bounds.Y = itemsToGrabMenu.inventory[bottomSlot].bounds.Center.Y - 6 * Game1.pixelZoom;

        // Assign Neighbor IDs
        this.UpArrow.Component.leftNeighborID = itemsToGrabMenu.inventory[topSlot].myID;
        itemsToGrabMenu.inventory[topSlot].rightNeighborID = this.UpArrow.Id;
        this.DownArrow.Component.leftNeighborID = itemsToGrabMenu.inventory[bottomSlot].myID;
        itemsToGrabMenu.inventory[bottomSlot].rightNeighborID = this.DownArrow.Id;
        this.UpArrow.Component.downNeighborID = this.DownArrow.Id;
        this.DownArrow.Component.upNeighborID = this.UpArrow.Id;
    }

    private void OnMenuItemsChanged(object sender, MenuItemsChangedEventArgs e)
    {
        this.Menu = e.Menu;
        this.Context = e.Context;
        this.MenuColumns = this.Menu.GetColumnCount();

        if (this.Menu.inventory.highlightMethod.Target is not MenuItems)
        {
            this.OldHighlightMethod = this.Menu.inventory.highlightMethod;
            this.Menu.inventory.highlightMethod = this.HighlightMethod;
        }
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        if (this.Menu is null || !this.Config.ScrollMenuOverflow)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.Menu.ItemsToGrabMenu.isWithinBounds(x, y))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.Offset--;
                break;
            case < 0:
                this.Offset++;
                break;
            default:
                return;
        }

        this.UpArrow.Component.visible = this.Offset > 0;
        this.DownArrow.Component.visible = this.Offset < this.Rows;
    }
}