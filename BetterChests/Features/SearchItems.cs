namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.PatternPatcher;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Adds a search bar to the top of the <see cref="ItemGrabMenu" />.
/// </summary>
internal class SearchItems : IFeature
{
    private const int ExtraSpace = 24;
    private const string Id = "furyx639.BetterChests/SearchItems";

    private readonly PerScreen<object?> _context = new();
    private readonly PerScreen<ItemGrabMenu?> _currentMenu = new();
    private readonly PerScreen<IItemMatcher?> _itemMatcher = new();
    private readonly PerScreen<ClickableComponent?> _searchArea = new();
    private readonly PerScreen<TextBox?> _searchField = new();
    private readonly PerScreen<ClickableTextureComponent?> _searchIcon = new();
    private readonly PerScreen<string> _searchText = new(() => string.Empty);
    private readonly PerScreen<IStorageObject?> _storage = new();

    private SearchItems(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
        HarmonyHelper.AddPatches(
            SearchItems.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                    typeof(SearchItems),
                    nameof(SearchItems.ItemGrabMenu_constructor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                    typeof(SearchItems),
                    nameof(SearchItems.ItemGrabMenu_draw_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), new[] { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) }),
                    typeof(SearchItems),
                    nameof(SearchItems.MenuWithInventory_draw_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static SearchItems? Instance { get; set; }

    private ModConfig Config { get; }

    private object? Context
    {
        get => this._context.Value;
        set => this._context.Value = value;
    }

    private ItemGrabMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private IItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value ??= new ItemMatcher(false, this.Config.SearchTagSymbol.ToString(), this.Helper.Translation);
        set => this._itemMatcher.Value = value;
    }

    private ClickableComponent SearchArea
    {
        get => this._searchArea.Value ??= new(Rectangle.Empty, string.Empty);
    }

    private TextBox SearchField
    {
        get => this._searchField.Value ??= new(this.Helper.GameContent.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor);
    }

    private ClickableTextureComponent SearchIcon
    {
        get => this._searchIcon.Value ??= new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f);
    }

    private string SearchText
    {
        get => this._searchText.Value;
        set => this._searchText.Value = value;
    }

    private IStorageObject? Storage
    {
        get => this._storage.Value;
        set => this._storage.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="SearchItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="SearchItems" /> class.</returns>
    public static SearchItems Init(IModHelper helper, ModConfig config)
    {
        return SearchItems.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            HarmonyHelper.ApplyPatches(SearchItems.Id);
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            HarmonyHelper.UnapplyPatches(SearchItems.Id);
            this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
            this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        }
    }

    private static int GetExtraSpace(MenuWithInventory menu)
    {
        switch (menu)
        {
            case ItemGrabMenu { context: null } or not ItemGrabMenu:
                SearchItems.Instance!.Context = null;
                SearchItems.Instance.Storage = null;
                return 0;
            case ItemGrabMenu { context: { } context } when !ReferenceEquals(SearchItems.Instance!.Context, context):
                SearchItems.Instance.Context = context;
                SearchItems.Instance.Storage = StorageHelper.TryGetOne(context, out var storage) ? storage : null;
                break;
        }

        return SearchItems.Instance.Storage?.SearchItems switch
        {
            null or FeatureOption.Disabled => 0,
            _ => SearchItems.ExtraSpace,
        };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        __instance.yPositionOnScreen -= SearchItems.ExtraSpace;
        __instance.height += SearchItems.ExtraSpace;
    }

    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.{nameof(ItemGrabMenu.draw)} from {nameof(SearchItems)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Draw Backpack Patch
        // This adds SearchItems.GetExtraSpace() to the y-coordinate of the backpack sprite
        patcher.AddSeek(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))));
        patcher.AddPatch(
                   code =>
                   {
                       Log.Trace("Moving backpack icon down by search bar height.", true);
                       code.Add(new(OpCodes.Ldarg_0));
                       code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetExtraSpace))));
                       code.Add(new(OpCodes.Add));
                   },
                   new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
               .Repeat(2);

        // ****************************************************************************************
        // Move Dialogue Patch
        // This subtracts SearchItems.GetExtraSpace() from the y-coordinate of the ItemsToGrabMenu
        // dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Moving top dialogue box up by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetExtraSpace))));
                code.Add(new(OpCodes.Sub));
            },
            new(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
            new(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
            new(OpCodes.Sub),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
            new(OpCodes.Sub));

        // ****************************************************************************************
        // Expand Dialogue Patch
        // This adds SearchItems.GetExtraSpace() to the height of the ItemsToGrabMenu dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Expanding top dialogue box by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetExtraSpace))));
                code.Add(new(OpCodes.Add));
            },
            new(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
            new(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
            new(OpCodes.Add),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
            new(OpCodes.Ldc_I4_2),
            new(OpCodes.Mul),
            new(OpCodes.Add));

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

    private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(MenuWithInventory)}.{nameof(MenuWithInventory.draw)} from {nameof(SearchItems)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Move Dialogue Patch
        // This adds SearchItems.GetExtraSpace() to the y-coordinate of the inventory dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Moving bottom dialogue box down by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetExtraSpace))));
                code.Add(new(OpCodes.Add));
            },
            new(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
            new(OpCodes.Add),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
            new(OpCodes.Add),
            new(OpCodes.Ldc_I4_S, (sbyte)64),
            new(OpCodes.Add));

        // ****************************************************************************************
        // Shrink Dialogue Patch
        // This adds SearchItems.GetExtraSpace() to the height of the inventory dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Shrinking bottom dialogue box height by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetExtraSpace))));
                code.Add(new(OpCodes.Add));
            },
            new(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
            new(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
            new(OpCodes.Add),
            new(OpCodes.Ldc_I4, 192),
            new(OpCodes.Add));

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

    private IEnumerable<Item> FilterBySearch(IEnumerable<Item> items)
    {
        return this.ItemMatcher.Any() ? items.Where(this.ItemMatcher.Matches) : items;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null || !this.SearchArea.visible)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when this.SearchArea.containsPoint(x, y):
                this.SearchField.Selected = true;
                break;
            case SButton.MouseRight when this.SearchArea.containsPoint(x, y):
                this.SearchField.Selected = true;
                this.SearchField.Text = string.Empty;
                break;
            case SButton.MouseLeft:
            case SButton.MouseRight:
                this.SearchField.Selected = false;
                break;
            case SButton.Escape when this.CurrentMenu.readyToClose():
                Game1.playSound("bigDeSelect");
                this.CurrentMenu.exitThisMenu();
                this.Helper.Input.Suppress(e.Button);
                return;
            case SButton.Escape:
                return;
        }

        if (this.SearchField.Selected)
        {
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null || !this.SearchArea.visible)
        {
            return;
        }

        this.SearchField.Draw(e.SpriteBatch, false);
        this.SearchIcon.draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu itemGrabMenu => itemGrabMenu,
            { } clickableMenu when clickableMenu.GetChildMenu() is ItemGrabMenu itemGrabMenu => itemGrabMenu,
            _ => null,
        };

        if (!ReferenceEquals(menu, this.CurrentMenu))
        {
            this.CurrentMenu = menu;
            if (this.CurrentMenu is not { context: { } context, ItemsToGrabMenu: { } itemsToGrabMenu }
                || !StorageHelper.TryGetOne(context, out var storage)
                || storage.SearchItems == FeatureOption.Disabled)
            {
                this.SearchArea.visible = false;
                return;
            }

            this.ItemMatcher = new ItemMatcher(false, this.Config.SearchTagSymbol.ToString(), this.Helper.Translation);
            this.SearchField.X = itemsToGrabMenu.xPositionOnScreen;
            this.SearchField.Y = itemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom;
            this.SearchField.Width = itemsToGrabMenu.width;
            this.SearchField.Selected = false;
            this.SearchArea.visible = true;
            this.SearchArea.bounds = new(this.SearchField.X, this.SearchField.Y, this.SearchField.Width, this.SearchField.Height);
            this.SearchIcon.bounds = new(this.SearchField.X + this.SearchField.Width - 38, this.SearchField.Y + 6, 32, 32);

            BetterItemGrabMenu.ItemsToGrabMenu?.AddTransformer(this.FilterBySearch);
        }

        if (this.CurrentMenu is null || !this.SearchArea.visible)
        {
            return;
        }

        if (this.SearchText.Equals(this.SearchField.Text, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        this.SearchText = this.SearchField.Text;
        this.ItemMatcher.StringValue = this.SearchText;
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
    }
}