#nullable disable

namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using CommonHarmony.Enums;
using CommonHarmony.Models;
using CommonHarmony.Services;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.UI;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class SearchItems : Feature
{
    private const int SearchBarHeight = 24;

    private readonly PerScreen<IGameObject> _context = new();
    private readonly PerScreen<int> _currentPadding = new();
    private readonly PerScreen<ItemMatcher> _itemMatcher = new();
    private readonly PerScreen<MenuWithInventory> _menu = new();
    private readonly Lazy<IMenuItems> _menuItems;
    private readonly PerScreen<int?> _menuPadding = new();
    private readonly PerScreen<ClickableComponent> _searchArea = new();
    private readonly PerScreen<TextBox> _searchField = new();
    private readonly PerScreen<ClickableTextureComponent> _searchIcon = new();
    private readonly PerScreen<string> _searchText = new(() => string.Empty);
    private readonly PerScreen<IStorageData> _storageData = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="SearchItems" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    /// <param name="harmony">Helper to apply/reverse harmony patches.</param>
    public SearchItems(IConfigModel config, IModHelper helper, IModServices services, HarmonyHelper harmony)
        : base(config, helper, services)
    {
        SearchItems.Instance = this;
        this.Harmony = harmony;
        var drawMenuWithInventory = new[]
        {
            typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
        };

        this.Harmony.AddPatches(
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
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static SearchItems Instance { get; set; }

    private IGameObject Context
    {
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
            if (this.Menu is not ItemGrabMenu itemGrabMenu || relativePadding == 0)
            {
                return;
            }

            this.Menu.yPositionOnScreen -= relativePadding;
            this.Menu.height += relativePadding;

            if (itemGrabMenu.chestColorPicker is not null and not HslColorPicker)
            {
                itemGrabMenu.chestColorPicker.yPositionOnScreen -= relativePadding;
            }
        }
    }

    private HarmonyHelper Harmony { get; }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value ??= new(false, this.Config.SearchTagSymbol.ToString());
    }

    private MenuWithInventory Menu
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
            return this._menuPadding.Value ??= this.StorageData is not null
                ? SearchItems.SearchBarHeight
                : 0;
        }
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
        set
        {
            if (this._searchText.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            this.ItemMatcher.StringValue = value;
            this._searchText.Value = value;
        }
    }

    private IStorageData StorageData
    {
        get => this._storageData.Value;
        set => this._storageData.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.CustomEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
        this.CustomEvents.RenderedClickableMenu += this.OnRenderedClickableMenu;
        this.MenuItems.MenuItemsChanged += this.OnMenuItemsChanged;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
        this.CustomEvents.RenderedClickableMenu -= this.OnRenderedClickableMenu;
        this.MenuItems.MenuItemsChanged -= this.OnMenuItemsChanged;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static int GetMenuPadding(MenuWithInventory menu)
    {
        if (!ReferenceEquals(SearchItems.Instance.Menu, menu))
        {
            SearchItems.Instance.Menu = menu;
            SearchItems.Instance.StorageData = menu switch
            {
                ItemSelectionMenu when SearchItems.Instance.Config.DefaultChest.SearchItems == FeatureOption.Enabled => SearchItems.Instance.Config.DefaultChest,
                ItemGrabMenu { context: not null } itemGrabMenu when SearchItems.Instance.ManagedObjects.TryGetManagedStorage(itemGrabMenu.context, out var managedStorage) && managedStorage.SearchItems == FeatureOption.Enabled => managedStorage,
                _ => null,
            };
        }

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
        if (this.StorageData is null || !ReferenceEquals(this.Menu, Game1.activeClickableMenu))
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
    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        this.Menu = e.Menu as MenuWithInventory;
        this.Context = e.Context;
        this.StorageData = e.Menu switch
        {
            ItemSelectionMenu when this.Config.DefaultChest.SearchItems == FeatureOption.Enabled => this.Config.DefaultChest,
            ItemGrabMenu when e.Context is not null && this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedStorage) && managedStorage.SearchItems == FeatureOption.Enabled => managedStorage,
            _ => null,
        };

        if (e.IsNew || this.Menu is null)
        {
            this._currentPadding.Value = 0;
            this.CurrentPadding = this.MenuPadding;
        }

        if (this.StorageData is null || this.Menu is not ItemGrabMenu { ItemsToGrabMenu: { } itemsToGrabMenu })
        {
            return;
        }

        this.SearchField.X = itemsToGrabMenu.xPositionOnScreen;
        this.SearchField.Y = itemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom;
        this.SearchField.Selected = false;
        this.SearchArea.bounds = new(this.SearchField.X, this.SearchField.Y, this.SearchField.Width, this.SearchField.Height);
        this.SearchField.Width = itemsToGrabMenu.width;
        this.SearchIcon.bounds = new(itemsToGrabMenu.xPositionOnScreen + itemsToGrabMenu.width - 38, itemsToGrabMenu.yPositionOnScreen - 14 * Game1.pixelZoom + 6, 32, 32);
    }

    private void OnMenuItemsChanged(object sender, IMenuItemsChangedEventArgs e)
    {
        switch (e.Menu)
        {
            case ItemSelectionMenu when this.Config.DefaultChest.SearchItems == FeatureOption.Enabled:
            case not null when e.Context is not null && this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedStorage) && managedStorage.SearchItems == FeatureOption.Enabled:
                e.AddFilter(this.ItemMatcher);
                this.SearchText = this.SearchField.Text;
                break;
        }
    }

    private void OnRenderedClickableMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this.StorageData is not null)
        {
            this.SearchField.Draw(e.SpriteBatch, false);
            this.SearchIcon.draw(e.SpriteBatch);
        }
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (this.StorageData is not null)
        {
            this.SearchText = this.SearchField.Text;
        }
    }
}