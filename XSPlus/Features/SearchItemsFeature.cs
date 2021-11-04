namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection.Emit;
    using Common.Helpers;
    using Common.Helpers.ItemMatcher;
    using Common.Services;
    using CommonHarmony;
    using CommonHarmony.Enums;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;
    using static HarmonyLib.AccessTools;

    /// <inheritdoc cref="BaseFeature" />
    internal class SearchItemsFeature : BaseFeature
    {
        private const int SearchBarHeight = 24;
        private static SearchItemsFeature Instance;
        private readonly PerScreen<ItemMatcher> _itemMatcher = new();
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private readonly PerScreen<int> _menuPadding = new(() => -1);
        private readonly PerScreen<ClickableComponent> _searchArea = new(() => new(Rectangle.Empty, string.Empty));
        private readonly PerScreen<TextBox> _searchField = new();
        private readonly PerScreen<ClickableTextureComponent> _searchIcon = new();
        private DisplayedInventoryService _displayedInventory;
        private HarmonyService _harmony;
        private ItemGrabMenuChangedService _itemGrabMenuChanged;
        private ModConfigService _modConfig;
        private RenderedActiveMenuService _renderedActiveMenu;

        private SearchItemsFeature(ServiceManager serviceManager)
            : base("SearchItems", serviceManager)
        {
            SearchItemsFeature.Instance ??= this;

            // Dependencies
            this.AddDependency<ModConfigService>(service => this._modConfig = service as ModConfigService);
            this.AddDependency<ItemGrabMenuChangedService>(service => this._itemGrabMenuChanged = service as ItemGrabMenuChangedService);
            this.AddDependency<RenderedActiveMenuService>(service => this._renderedActiveMenu = service as RenderedActiveMenuService);
            this.AddDependency<DisplayedInventoryService>(service => this._displayedInventory = service as DisplayedInventoryService);
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;
                    var drawMenuWithInventory = new[]
                    {
                        typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
                    };

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(
                            typeof(ItemGrabMenu),
                            nameof(ItemGrabMenu.draw),
                            new[]
                            {
                                typeof(SpriteBatch),
                            }),
                        typeof(SearchItemsFeature),
                        nameof(SearchItemsFeature.ItemGrabMenu_draw_transpiler),
                        PatchType.Transpiler);

                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), drawMenuWithInventory),
                        typeof(SearchItemsFeature),
                        nameof(SearchItemsFeature.MenuWithInventory_draw_transpiler),
                        PatchType.Transpiler);
                });
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this._itemGrabMenuChanged.AddHandler(this.OnItemGrabMenuChanged);
            this._renderedActiveMenu.AddHandler(this.OnRenderedActiveMenu);
            this._displayedInventory.AddHandler(this.FilterMethod);
            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChanged.RemoveHandler(this.OnItemGrabMenuChanged);
            this._renderedActiveMenu.RemoveHandler(this.OnRenderedActiveMenu);
            this._displayedInventory.RemoveHandler(this.FilterMethod);
            this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        /// <summary>Move/resize top dialogue box by search bar height.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Moving backpack icon down by search bar height.");
            var moveBackpackPatch = new PatternPatch();
            moveBackpackPatch.Find(new CodeInstruction(OpCodes.Ldfld, Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                             .Find(new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                             .Patch(
                                 delegate(LinkedList<CodeInstruction> list)
                                 {
                                     list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                                     list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                                     list.AddLast(new CodeInstruction(OpCodes.Add));
                                 })
                             .Repeat(3);

            Log.Trace("Moving top dialogue box up by search bar height.");
            var moveDialogueBoxPatch = new PatternPatch();
            moveDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))), new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))), new CodeInstruction(OpCodes.Sub), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))), new CodeInstruction(OpCodes.Sub),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                        list.AddLast(new CodeInstruction(OpCodes.Sub));
                    });

            Log.Trace("Expanding top dialogue box by search bar height.");
            var resizeDialogueBoxPatch = new PatternPatch();
            resizeDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))), new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.height))), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))), new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))), new CodeInstruction(OpCodes.Ldc_I4_2), new CodeInstruction(OpCodes.Mul), new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
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

        /// <summary>Move/resize bottom dialogue box by search bar height.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Moving bottom dialogue box down by search bar height.");
            var moveDialogueBoxPatch = new PatternPatch();
            moveDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))), new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))), new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64), new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                        list.AddLast(new CodeInstruction(OpCodes.Add));
                    });

            Log.Trace("Shrinking bottom dialogue box height by search bar height.");
            var resizeDialogueBoxPatch = new PatternPatch();
            resizeDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.height))), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))), new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))), new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldc_I4, 192), new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
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

        private static int MenuPadding(MenuWithInventory menu)
        {
            if (SearchItemsFeature.Instance._menuPadding.Value != -1)
            {
                return SearchItemsFeature.Instance._menuPadding.Value;
            }

            if (menu is not ItemGrabMenu {context: Chest chest} || !SearchItemsFeature.Instance.IsEnabledForItem(chest))
            {
                return SearchItemsFeature.Instance._menuPadding.Value = 0; // Vanilla
            }

            return SearchItemsFeature.Instance._menuPadding.Value = SearchItemsFeature.SearchBarHeight;
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._menu.Value = null;
                this._menuPadding.Value = 0;
                return;
            }

            var upperBounds = new Rectangle(
                e.ItemGrabMenu.ItemsToGrabMenu.xPositionOnScreen,
                e.ItemGrabMenu.ItemsToGrabMenu.yPositionOnScreen,
                e.ItemGrabMenu.ItemsToGrabMenu.width,
                e.ItemGrabMenu.ItemsToGrabMenu.height);

            this._menuPadding.Value = SearchItemsFeature.SearchBarHeight;
            this._itemMatcher.Value = new(this._modConfig.ModConfig.SearchTagSymbol);

            this._searchField.Value ??= new(
                this.Helper.Content.Load<Texture2D>("LooseSprites\\textBox", ContentSource.GameContent),
                null,
                Game1.smallFont,
                Game1.textColor);

            this._searchField.Value.X = upperBounds.X;
            this._searchField.Value.Y = upperBounds.Y - 14 * Game1.pixelZoom;
            this._searchField.Value.Width = upperBounds.Width;
            this._searchField.Value.Selected = false;

            this._searchIcon.Value ??= new(
                Rectangle.Empty,
                Game1.mouseCursors,
                new(80, 0, 13, 13),
                2.5f);

            this._searchIcon.Value.bounds = new(upperBounds.Right - 38, upperBounds.Y - 14 * Game1.pixelZoom + 6, 32, 32);

            this._searchArea.Value.bounds = new(this._searchField.Value.X, this._searchField.Value.Y, this._searchField.Value.Width, this._searchField.Value.Height);

            if (this._menu.Value is null || !ReferenceEquals(e.Chest, this._menu.Value.Chest))
            {
                this._searchField.Value.Text = string.Empty;
            }

            if (e.IsNew)
            {
                e.ItemGrabMenu.yPositionOnScreen -= SearchItemsFeature.SearchBarHeight;
                e.ItemGrabMenu.height += SearchItemsFeature.SearchBarHeight;
                if (e.ItemGrabMenu.chestColorPicker is not null)
                {
                    e.ItemGrabMenu.chestColorPicker.yPositionOnScreen -= SearchItemsFeature.SearchBarHeight;
                }
            }

            this._menu.Value = e;
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            this._searchField.Value.Draw(e.SpriteBatch, false);
            this._searchIcon.Value.draw(e.SpriteBatch);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || this._itemMatcher.Value.Search == this._searchField.Value.Text)
            {
                return;
            }

            this._itemMatcher.Value.SetSearch(this._searchField.Value.Text);
            this._displayedInventory.ReSyncInventory(this._menu.Value.ItemGrabMenu.ItemsToGrabMenu, true);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Check if search bar was clicked on
            var point = Game1.getMousePosition(true);
            switch (e.Button)
            {
                case SButton.MouseLeft when this.LeftClick(point.X, point.Y):
                case SButton.MouseRight when this.RightClick(point.X, point.Y):
                    this.Helper.Input.Suppress(e.Button);
                    break;
                default:
                    if (this.KeyPress(e.Button))
                    {
                        this.Helper.Input.Suppress(e.Button);
                    }

                    break;
            }
        }

        private bool LeftClick(int x = -1, int y = -1)
        {
            if (x != -1 && y != -1)
            {
                this._searchField.Value.Selected = this._searchArea.Value.containsPoint(x, y);
            }

            return this._searchField.Value.Selected;
        }

        private bool RightClick(int x = -1, int y = -1)
        {
            if (x != -1 && y != -1)
            {
                this._searchField.Value.Selected = this._searchArea.Value.containsPoint(x, y);
            }

            if (!this._searchField.Value.Selected)
            {
                return false;
            }

            this._searchField.Value.Text = string.Empty;
            return true;
        }

        private bool KeyPress(SButton button)
        {
            if (button != SButton.Escape)
            {
                return this._searchField.Value.Selected;
            }

            if (this._menu.Value is not null && !this._menu.Value.ItemGrabMenu.readyToClose())
            {
                return false;
            }

            Game1.playSound("bigDeSelect");
            Game1.activeClickableMenu = null;
            return true;
        }

        private bool FilterMethod(Item item)
        {
            return this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || this._itemMatcher.Value.Matches(item);
        }
    }
}