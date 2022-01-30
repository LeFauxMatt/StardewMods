namespace StardewMods.FuryCore.Services;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Emit;
using Common.Extensions;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using Common.Models;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc cref="IMenuItems" />
[FuryCoreService(true)]
internal class MenuItems : IMenuItems, IModService
{
    private readonly PerScreen<Chest> _chest = new();
    private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();
    private readonly PerScreen<IDictionary<string, bool>> _itemFilterCache = new(() => new Dictionary<string, bool>());
    private readonly PerScreen<HashSet<ItemMatcher>> _itemFilters = new(() => new());
    private readonly PerScreen<IDictionary<string, bool>> _itemHighlightCache = new(() => new Dictionary<string, bool>());
    private readonly PerScreen<HashSet<ItemMatcher>> _itemHighlighters = new(() => new());
    private readonly PerScreen<IList<int>> _itemIndexes = new();
    private readonly PerScreen<IList<Item>> _itemsFiltered = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<int> _menuColumns = new();
    private readonly PerScreen<int> _offset = new(() => 0);
    private readonly PerScreen<Range<int>> _range = new(() => new());
    private readonly PerScreen<bool> _refreshInventory = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuItems" /> class.
    /// </summary>
    /// <param name="modEvents">Provides access to all SMAPI events.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuItems(IModEvents modEvents, IModServices services)
    {
        MenuItems.Instance = this;

        services.Lazy<HarmonyHelper>(
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

        services.Lazy<CustomEvents>(events => { events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged; });

        modEvents.World.ChestInventoryChanged += this.OnChestInventoryChanged;
        modEvents.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    public Chest Chest
    {
        get => this._chest.Value;
        private set => this._chest.Value = value;
    }

    /// <inheritdoc />
    public ItemGrabMenu Menu
    {
        get => this._menu.Value;
        private set => this._menu.Value = value;
    }

    /// <inheritdoc />
    public IList<Item> ActualInventory
    {
        get => this.Menu?.ItemsToGrabMenu.actualInventory;
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
                foreach (var (slot, index) in this.Menu.ItemsToGrabMenu.inventory.Select((slot, index) => (slot, index + (this.Offset * this.MenuColumns))))
                {
                    slot.name = (index < this.ItemIndexes.Count ? this.ItemIndexes[index] : int.MaxValue).ToString();
                }

                this.RefreshInventory = false;
            }

            return this.ItemsFiltered.Skip(this.Offset * this.MenuColumns);
        }
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

    private static MenuItems Instance { get; set; }

    private IList<int> ItemIndexes
    {
        get
        {
            return this._itemIndexes.Value ??= this.ItemsFiltered.Select(item => this.ActualInventory.IndexOf(item)).ToList();
        }
        set => this._itemIndexes.Value = value;
    }

    private IList<Item> ItemsFiltered
    {
        get
        {
            if (this._itemsFiltered.Value is null)
            {
                this._itemsFiltered.Value = this.ActualInventory.Where(this.FilterMethod).ToList();
                this.ItemIndexes = null;
                this.RefreshInventory = true;
            }

            return this._itemsFiltered.Value;
        }

        set => this._itemsFiltered.Value = value;
    }

    private bool RefreshInventory
    {
        get => this._refreshInventory.Value;
        set => this._refreshInventory.Value = value;
    }

    private IDictionary<string, bool> ItemFilterCache
    {
        get => this._itemFilterCache.Value;
    }

    private IDictionary<string, bool> ItemHighlightCache
    {
        get => this._itemHighlightCache.Value;
    }

    private HashSet<ItemMatcher> ItemFilters
    {
        get => this._itemFilters.Value;
    }

    private HashSet<ItemMatcher> ItemHighlighters
    {
        get => this._itemHighlighters.Value;
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

    /// <inheritdoc />
    public void AddFilter(ItemMatcher itemMatcher)
    {
        this.ItemFilters.Add(itemMatcher);
        itemMatcher.CollectionChanged += this.OnItemFilterChanged;
    }

    /// <inheritdoc />
    public void AddHighlighter(ItemMatcher itemMatcher)
    {
        this.ItemHighlighters.Add(itemMatcher);
        itemMatcher.CollectionChanged += this.OnItemHighlighterChanged;
    }

    /// <inheritdoc />
    public void ForceRefresh()
    {
        this.ItemFilterCache.Clear();
        this.ItemHighlightCache.Clear();
        this.ItemsFiltered = null;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(InventoryMenu)}.{nameof(InventoryMenu.draw)}");
        var patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

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

    private static IList<Item> DisplayedItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        if (MenuItems.Instance.Menu is null || !ReferenceEquals(inventoryMenu, MenuItems.Instance.Menu.ItemsToGrabMenu))
        {
            return actualInventory;
        }

        return MenuItems.Instance.ItemsDisplayed.Take(inventoryMenu.capacity).ToList();
    }

    [SortedEventPriority(EventPriority.High + 1000)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu?.IsPlayerChestMenu(out _) == true
            ? e.ItemGrabMenu
            : null;

        if (this.Menu is not null)
        {
            this.Chest = e.Chest;
            this.MenuColumns = this.Menu.GetColumnCount();

            if (this.Menu.inventory.highlightMethod.Target is not MenuItems)
            {
                this.OldHighlightMethod = this.Menu.inventory.highlightMethod;
                this.Menu.inventory.highlightMethod = this.HighlightMethod;
            }

            this.ForceRefresh();
        }

        foreach (var itemMatcher in this.ItemFilters)
        {
            itemMatcher.CollectionChanged -= this.OnItemFilterChanged;
        }

        this.ItemFilters.Clear();
        this.ItemHighlighters.Clear();
    }

    private void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
    {
        if (this.Menu is not null && ReferenceEquals(e.Chest, this.Chest))
        {
            this.ItemsFiltered = null;
        }
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer)
        {
            this.ItemsFiltered = null;
        }
    }

    private void OnItemFilterChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.ItemFilterCache.Clear();
        this.ItemsFiltered = null;
    }

    private void OnItemHighlighterChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.ItemHighlightCache.Clear();
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
        if (item is null || this.OldHighlightMethod?.Invoke(item) == false)
        {
            return false;
        }

        if (!this.ItemHighlightCache.TryGetValue(item.Name, out var highlighted))
        {
            highlighted = this.ItemHighlighters.All(itemMatcher => itemMatcher.Matches(item));
            this.ItemHighlightCache.Add(item.Name, highlighted);
        }

        return highlighted;
    }
}