namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.UI;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class SearchItems : Feature
{
    private const int SearchBarHeight = 24;

    private readonly PerScreen<object> _context = new();
    private readonly PerScreen<int> _currentPadding = new();
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly PerScreen<ItemMatcher> _itemMatcher = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<IMenuItems> _menuItems;
    private readonly PerScreen<int?> _menuPadding = new();
    private readonly PerScreen<ClickableComponent> _searchArea = new();
    private readonly PerScreen<TextBox> _searchField = new();
    private readonly PerScreen<ClickableTextureComponent> _searchIcon = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchItems" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public SearchItems(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        SearchItems.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                var drawMenuWithInventory = new[]
                {
                    typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
                };

                harmony.AddPatches(
                    this.Id,
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
            });
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static SearchItems Instance { get; set; }

    private object Context
    {
        get => this._context.Value;
        set
        {
            if (!ReferenceEquals(this._context.Value, value))
            {
                this.SearchField.Text = string.Empty;
                this._context.Value = value;
            }
        }
    }

    private int CurrentPadding
    {
        set
        {
            if (this._currentPadding.Value == value)
            {
                return;
            }

            var relativePadding = value - this._currentPadding.Value;
            this._currentPadding.Value = value;
            if (this.Menu is null)
            {
                return;
            }

            this.Menu.yPositionOnScreen -= relativePadding;
            this.Menu.height += relativePadding;

            if (this.Menu.chestColorPicker is not null and not HslColorPicker)
            {
                this.Menu.chestColorPicker.yPositionOnScreen -= relativePadding;
            }
        }
    }

    private IHarmonyHelper HarmonyHelper
    {
        get => this._harmony.Value;
    }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value ??= new(false, this.Config.SearchTagSymbol.ToString());
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set
        {
            this._menu.Value = value;
            this._menuPadding.Value = null;
        }
    }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    private int MenuPadding
    {
        get
        {
            return this._menuPadding.Value ??= this.Menu switch
            {
                ItemSelectionMenu when this.Config.DefaultChest.SearchItems == FeatureOption.Enabled => SearchItems.SearchBarHeight,
                { context: not null } when this.ManagedObjects.FindManagedStorage(this.Menu.context, out var managedStorage) && managedStorage.SearchItems == FeatureOption.Enabled => SearchItems.SearchBarHeight,
                _ => 0,
            };
        }
    }

    private ClickableComponent SearchArea
    {
        get => this._searchArea.Value ??= new(Rectangle.Empty, string.Empty);
    }

    private TextBox SearchField
    {
        get => this._searchField.Value ??= new(this.Helper.Content.Load<Texture2D>("LooseSprites\\textBox", ContentSource.GameContent), null, Game1.smallFont, Game1.textColor);
    }

    private ClickableTextureComponent SearchIcon
    {
        get => this._searchIcon.Value ??= new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f);
    }

    private string SearchText { get; set; }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.HarmonyHelper.ApplyPatches(this.Id);
        this.CustomEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.CustomEvents.RenderedItemGrabMenu += this.OnRenderedItemGrabMenu;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.HarmonyHelper.UnapplyPatches(this.Id);
        this.CustomEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.CustomEvents.RenderedItemGrabMenu -= this.OnRenderedItemGrabMenu;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static int GetMenuPadding(MenuWithInventory menu)
    {
        SearchItems.Instance.Menu = menu as ItemGrabMenu;
        return SearchItems.Instance.MenuPadding;
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
                       code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
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
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
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
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
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
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
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
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
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

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (this.Menu is null || !ReferenceEquals(this.Menu, Game1.activeClickableMenu))
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

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu switch
        {
            ItemSelectionMenu when this.Config.DefaultChest.SearchItems == FeatureOption.Enabled => e.ItemGrabMenu,
            { context: not null } when this.ManagedObjects.FindManagedStorage(e.Context, out var managedStorage) && managedStorage.SearchItems == FeatureOption.Enabled => e.ItemGrabMenu,
            _ => null,
        };

        this.Context = e.Context ?? this.Context;
        if (e.IsNew || this.Menu is null)
        {
            this._currentPadding.Value = 0;
            this.CurrentPadding = this.MenuPadding;
        }

        if (this.Menu is null)
        {
            return;
        }

        this.MenuItems.AddFilter(this.ItemMatcher);
        this.ItemMatcher.StringValue = this.SearchText = this.SearchField.Text;
        this.SearchField.X = this.Menu.ItemsToGrabMenu.xPositionOnScreen;
        this.SearchField.Y = this.Menu.ItemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom;
        this.SearchField.Selected = false;
        this.SearchArea.bounds = new(this.SearchField.X, this.SearchField.Y, this.SearchField.Width, this.SearchField.Height);
        this.SearchField.Width = this.Menu.ItemsToGrabMenu.width;
        this.SearchIcon.bounds = new(this.Menu.ItemsToGrabMenu.xPositionOnScreen + this.Menu.ItemsToGrabMenu.width - 38, this.Menu.ItemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom + 6, 32, 32);
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
        }
    }
}