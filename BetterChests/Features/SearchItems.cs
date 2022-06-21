namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Common.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Helpers.PatternPatcher;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Adds a search bar to the top of the <see cref="ItemGrabMenu" />.
/// </summary>
internal class SearchItems : IFeature
{
    private const string Id = "BetterChests.SearchItems";
    private const int SearchBarHeight = 24;

    private readonly PerScreen<ItemMatcher?> _itemMatcher = new();
    private readonly PerScreen<ClickableComponent?> _searchArea = new();
    private readonly PerScreen<TextBox?> _searchField = new();
    private readonly PerScreen<ClickableTextureComponent?> _searchIcon = new();
    private readonly PerScreen<string> _searchText = new(() => string.Empty);

    private SearchItems(IModHelper helper)
    {
        this.Helper = helper;
        var drawMenuWithInventory = new[]
        {
            typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
        };

        HarmonyHelper.AddPatches(
            SearchItems.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                    typeof(SearchItems),
                    nameof(SearchItems.ItemGrabMenu_draw_transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), drawMenuWithInventory),
                    typeof(SearchItems),
                    nameof(SearchItems.MenuWithInventory_draw_transpiler),
                    PatchType.Transpiler),
            });
    }

    private static SearchItems? Instance { get; set; }

    private IModHelper Helper { get; }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value ??= new(false, Config.SearchTagSymbol.ToString());
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

    /// <summary>
    ///     Initializes <see cref="SearchItems" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="SearchItems" /> class.</returns>
    public static SearchItems Init(IModHelper helper)
    {
        return SearchItems.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(SearchItems.Id);
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(SearchItems.Id);
        this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static int GetPadding(MenuWithInventory menu)
    {
        return SearchItems.SearchBarHeight;
    }

    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(ItemGrabMenu)}.{nameof(ItemGrabMenu.draw)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Draw Backpack Patch
        // This adds SearchItems.GetMenuPadding() to the y-coordinate of the backpack sprite
        patcher.AddSeek(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))));
        patcher.AddPatch(
                   code =>
                   {
                       Log.Trace("Moving backpack icon down by search bar height.", true);
                       code.Add(new(OpCodes.Ldarg_0));
                       code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetPadding))));
                       code.Add(new(OpCodes.Add));
                   },
                   new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
               .Repeat(2);

        // ****************************************************************************************
        // Move Dialogue Patch
        // This subtracts SearchItems.GetMenuPadding() from the y-coordinate of the ItemsToGrabMenu
        // dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Moving top dialogue box up by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetPadding))));
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
        // This adds SearchItems.GetMenuPadding() to the height of the ItemsToGrabMenu dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Expanding top dialogue box by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetPadding))));
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
        Log.Trace($"Applying patches to {nameof(MenuWithInventory)}.{nameof(MenuWithInventory.draw)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Move Dialogue Patch
        // This adds SearchItems.GetMenuPadding() to the y-coordinate of the inventory dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Moving bottom dialogue box down by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetPadding))));
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
        // This adds SearchItems.GetMenuPadding() to the height of the inventory dialogue box
        patcher.AddPatch(
            code =>
            {
                Log.Trace("Shrinking bottom dialogue box height by search bar height.", true);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetPadding))));
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

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
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
            case SButton.Escape when itemGrabMenu.readyToClose():
                Game1.playSound("bigDeSelect");
                itemGrabMenu.exitThisMenu();
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

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu { ItemsToGrabMenu: { } itemsToGrabMenu } itemGrabMenu)
        {
            return;
        }

        this.ItemMatcher = new(false, Config.SearchTagSymbol.ToString());
        this.SearchField.X = itemsToGrabMenu.xPositionOnScreen;
        this.SearchField.Y = itemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom;
        this.SearchField.Selected = false;
        this.SearchArea.bounds = new(this.SearchField.X, this.SearchField.Y, this.SearchField.Width, this.SearchField.Height);
        this.SearchField.Width = itemsToGrabMenu.width;
        this.SearchIcon.bounds = new(itemsToGrabMenu.xPositionOnScreen + itemsToGrabMenu.width - 38, itemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom + 6, 32, 32);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        this.SearchField.Draw(e.SpriteBatch, false);
        this.SearchIcon.draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu)
        {
            return;
        }

        if (this.SearchText.Equals(this.SearchField.Text, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        this.SearchText = this.SearchField.Text;
        this.ItemMatcher.StringValue = this.SearchText;
    }
}