namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BetterChests.Extensions;
using BetterChests.Models;
using Common.Helpers;
using CommonHarmony;
using FuryCore.Attributes;
using FuryCore.Enums;
using FuryCore.Helpers;
using FuryCore.Models;
using FuryCore.Services;
using FuryCore.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class SearchItems : Feature
{
    private const int SearchBarHeight = 24;

    private readonly PerScreen<ManagedChest> _managedChest = new();
    private readonly PerScreen<ItemsDisplayedEventArgs> _displayedItems = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly PerScreen<ItemMatcher> _itemMatcher = new();
    private readonly PerScreen<TextBox> _searchField = new();
    private readonly PerScreen<ClickableTextureComponent> _searchIcon = new();
    private readonly PerScreen<ClickableComponent> _searchArea = new();
    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchItems"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public SearchItems(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        SearchItems.Instance = this;
        this._harmony = services.Lazy<HarmonyHelper>(SearchItems.AddPatches);
    }

    private static SearchItems Instance { get; set; }

    private HarmonyHelper HarmonyHelper
    {
        get => this._harmony.Value;
    }

    private ItemsDisplayedEventArgs DisplayedItems
    {
        get => this._displayedItems.Value;
        set => this._displayedItems.Value = value;
    }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value ??= new(false, this.Config.SearchTagSymbol.ToString());
    }

    private ManagedChest ManagedChest
    {
        get => this._managedChest.Value;
        set => this._managedChest.Value = value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private ClickableComponent SearchArea
    {
        get => this._searchArea.Value ??= new(new(this.SearchField.X, this.SearchField.Y, this.SearchField.Width, this.SearchField.Height), string.Empty);
    }

    private TextBox SearchField
    {
        get => this._searchField.Value ??= this.GetSearchField();
    }

    private ClickableTextureComponent SearchIcon
    {
        get => this._searchIcon.Value ??= this.GetSearchIcon();
    }

    private string SearchText { get; set; }

    /// <inheritdoc/>
    public override void Activate()
    {
        this.HarmonyHelper.ApplyPatches(nameof(SearchItems));
        this.FuryEvents.ItemsDisplayed += this.OnItemsDisplayed;
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.RenderedItemGrabMenu += this.OnRenderedItemGrabMenu;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc/>
    public override void Deactivate()
    {
        this.HarmonyHelper.UnapplyPatches(nameof(SearchItems));
        this.FuryEvents.ItemsDisplayed -= this.OnItemsDisplayed;
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.RenderedItemGrabMenu -= this.OnRenderedItemGrabMenu;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static void AddPatches(HarmonyHelper harmony)
    {
        var drawMenuWithInventory = new[]
        {
            typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
        };

        harmony.AddPatches(
            nameof(SearchItems),
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

    private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace("Moving backpack icon down by search bar height.");
        var moveBackpackPatch = new PatternPatch();
        moveBackpackPatch.Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                         .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                         .Patch(
                             delegate(LinkedList<CodeInstruction> list)
                             {
                                 list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                                 list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                                 list.AddLast(new CodeInstruction(OpCodes.Add));
                             })
                         .Repeat(3);

        Log.Trace("Moving top dialogue box up by search bar height.");
        var moveDialogueBoxPatch = new PatternPatch();
        moveDialogueBoxPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Sub),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Sub),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Sub));
                });

        Log.Trace("Expanding top dialogue box by search bar height.");
        var resizeDialogueBoxPatch = new PatternPatch();
        resizeDialogueBoxPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldc_I4_2), new CodeInstruction(OpCodes.Mul), new CodeInstruction(OpCodes.Add),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

        var patternPatches = new PatternPatches(instructions);
        patternPatches.AddPatch(moveBackpackPatch);
        patternPatches.AddPatch(moveDialogueBoxPatch);
        patternPatches.AddPatch(resizeDialogueBoxPatch);

        foreach (var patternPatch in patternPatches)
        {
            yield return patternPatch;
        }

        if (!patternPatches.Done)
        {
            Log.Warn($"Failed to apply all patches in {typeof(ItemGrabMenu)}::{nameof(ItemGrabMenu.draw)}.");
        }
    }

    private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace("Moving bottom dialogue box down by search bar height.");
        var moveDialogueBoxPatch = new PatternPatch();
        moveDialogueBoxPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64),
                    new CodeInstruction(OpCodes.Add),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

        Log.Trace("Shrinking bottom dialogue box height by search bar height.");
        var resizeDialogueBoxPatch = new PatternPatch();
        resizeDialogueBoxPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4, 192),
                    new CodeInstruction(OpCodes.Add),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

        var patternPatches = new PatternPatches(instructions);
        patternPatches.AddPatch(moveDialogueBoxPatch);
        patternPatches.AddPatch(resizeDialogueBoxPatch);

        foreach (var patternPatch in patternPatches)
        {
            yield return patternPatch;
        }

        if (!patternPatches.Done)
        {
            Log.Warn($"Failed to apply all patches in {typeof(MenuWithInventory)}::{nameof(MenuWithInventory.draw)}.");
        }
    }

    private static int GetMenuPadding(MenuWithInventory menu)
    {
        return SearchItems.Instance.MenuPadding(menu);
    }

    private int MenuPadding(MenuWithInventory menu)
    {
        return ReferenceEquals(menu, this.Menu) && this.ManagedChest is not null
            ? SearchItems.SearchBarHeight
            : 0;
    }

    private void OnItemsDisplayed(object sender, ItemsDisplayedEventArgs e)
    {
        this.DisplayedItems = e;
        e.AddFilter(this.ItemMatcher);
    }

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (ReferenceEquals(this.Menu, e.ItemGrabMenu))
        {
            return;
        }

        this.Menu = e.ItemGrabMenu;
        if (this.Menu?.IsPlayerChestMenu(out _) != true || !this.ManagedChests.FindChest(e.Chest, out var managedChest))
        {
            this.Menu = null;
            return;
        }

        if (!ReferenceEquals(this.ManagedChest, managedChest))
        {
            this.ManagedChest = managedChest;
            this.SearchText = string.Empty;
        }

        if (e.IsNew)
        {
            var padding = this.MenuPadding(this.Menu);
            this.Menu.yPositionOnScreen -= padding;
            this.Menu.height += padding;
            if (this.Menu.chestColorPicker is not null and not HslColorPicker)
            {
                this.Menu.chestColorPicker.yPositionOnScreen -= padding;
            }
        }

        this.SearchField.X = this.Menu.ItemsToGrabMenu.xPositionOnScreen;
        this.SearchField.Y = this.Menu.ItemsToGrabMenu.yPositionOnScreen - (14 * Game1.pixelZoom);
        this.SearchField.Selected = false;
        this.SearchField.Width = this.Menu.ItemsToGrabMenu.width;
        this.SearchIcon.bounds = new(this.Menu.ItemsToGrabMenu.xPositionOnScreen + this.Menu.ItemsToGrabMenu.width - 38, this.Menu.ItemsToGrabMenu.yPositionOnScreen - (14 * Game1.pixelZoom) + 6, 32, 32);
    }

    private void OnRenderedItemGrabMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this.Menu is not null)
        {
            this.SearchField.Draw(e.SpriteBatch, false);
            this.SearchIcon.draw(e.SpriteBatch);
        }
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (this.Menu is not null && this.SearchField.Text != this.SearchText)
        {
            this.SearchText = this.SearchField.Text;
            this.ItemMatcher.StringValue = this.SearchText;
            this.DisplayedItems?.ForceRefresh();
        }
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (this.Menu is null)
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
            case SButton.Escape when this.Menu.readyToClose():
                Game1.playSound("bigDeSelect");
                this.Menu.exitThisMenu();
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

    private TextBox GetSearchField()
    {
        return new(Game1.content.Load<Texture2D>(@"LooseSprites\textBox"), null, Game1.smallFont, Game1.textColor);
    }

    private ClickableTextureComponent GetSearchIcon()
    {
        return new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f);
    }
}