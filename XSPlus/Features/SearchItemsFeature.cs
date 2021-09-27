namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection.Emit;
    using Common.Helpers.ItemMatcher;
    using Common.Services;
    using CommonHarmony;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
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
        private static readonly Type[] MenuWithInventoryDrawParams = { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) };
        private static SearchItemsFeature Instance = null!;
        private readonly IContentHelper _contentHelper;
        private readonly IInputHelper _inputHelper;
        private readonly IGameLoopEvents _gameLoopEvents;
        private readonly IInputEvents _inputEvents;
        private readonly ModConfigService _modConfigService;
        private readonly ItemGrabMenuConstructedService _itemGrabMenuConstructedService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly DisplayedInventoryService _displayedChestInventoryService;
        private readonly PerScreen<ItemGrabMenu> _menu = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<ClickableComponent> _searchArea = new() { Value = new ClickableComponent(Rectangle.Empty, string.Empty) };
        private readonly PerScreen<TextBox> _searchField = new();
        private readonly PerScreen<ClickableTextureComponent> _searchIcon = new();
        private readonly PerScreen<int> _menuPadding = new() { Value = -1 };
        private readonly PerScreen<ItemMatcher> _itemMatcher = new();

        /// <summary>Initializes a new instance of the <see cref="SearchItemsFeature"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="gameLoopEvents">Events linked to the game's update loop.</param>
        /// <param name="inputEvents">Events raised when player provides input.</param>
        /// <param name="modConfigService">Service to handle read/write to ModConfig.</param>
        /// <param name="itemGrabMenuConstructedService">Service to handle creation/invocation of ItemGrabMenuConstructed event.</param>
        /// <param name="itemGrabMenuChangedService">Service to handle creation/invocation of ItemGrabMenuChanged event.</param>
        /// <param name="displayedChestInventoryService">Service for manipulating the displayed items in an inventory menu.</param>
        /// <param name="renderedActiveMenuService">Service to handle creation/invocation of RenderedActiveMenu event.</param>
        public SearchItemsFeature(
            IContentHelper contentHelper,
            IInputHelper inputHelper,
            IGameLoopEvents gameLoopEvents,
            IInputEvents inputEvents,
            ModConfigService modConfigService,
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            DisplayedInventoryService displayedChestInventoryService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("SearchItems")
        {
            SearchItemsFeature.Instance = this;
            this._contentHelper = contentHelper;
            this._inputHelper = inputHelper;
            this._gameLoopEvents = gameLoopEvents;
            this._inputEvents = inputEvents;
            this._modConfigService = modConfigService;
            this._itemGrabMenuConstructedService = itemGrabMenuConstructedService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._displayedChestInventoryService = displayedChestInventoryService;
            this._renderedActiveMenuService = renderedActiveMenuService;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedEvent);
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;

            // Patches
            harmony.Patch(
                original: Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: new HarmonyMethod(typeof(SearchItemsFeature), nameof(SearchItemsFeature.ItemGrabMenu_draw_transpiler)));
            harmony.Patch(
                original: Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), SearchItemsFeature.MenuWithInventoryDrawParams),
                transpiler: new HarmonyMethod(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuWithInventory_draw_transpiler)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuConstructedService.RemoveHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChangedEvent);
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;

            // Patches
            harmony.Unpatch(
                original: Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                patch: Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.ItemGrabMenu_draw_transpiler)));
            harmony.Unpatch(
                original: Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), SearchItemsFeature.MenuWithInventoryDrawParams),
                patch: Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuWithInventory_draw_transpiler)));
        }

        /// <summary>Move/resize top dialogue box by search bar height.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Moving backpack icon down by search bar height.");
            var moveBackpackPatch = new PatternPatch(PatchType.Replace);
            moveBackpackPatch.Find(new CodeInstruction(OpCodes.Ldfld, Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                             .Find(new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                             .Patch(delegate(LinkedList<CodeInstruction> list)
                             {
                                 list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                                 list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                                 list.AddLast(new CodeInstruction(OpCodes.Add));
                             })
                             .Repeat(3);

            Log.Trace("Moving top dialogue box up by search bar height.");
            var moveDialogueBoxPatch = new PatternPatch(PatchType.Replace);
            moveDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Sub),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Sub),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Sub));
                });

            Log.Trace("Expanding top dialogue box by search bar height.");
            var resizeDialogueBoxPatch = new PatternPatch(PatchType.Replace);
            resizeDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Ldc_I4_2),
                        new CodeInstruction(OpCodes.Mul),
                        new CodeInstruction(OpCodes.Add),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            var patternPatches = new PatternPatches(instructions);
            patternPatches.AddPatch(moveBackpackPatch);
            patternPatches.AddPatch(moveDialogueBoxPatch);
            patternPatches.AddPatch(resizeDialogueBoxPatch);

            foreach (CodeInstruction patternPatch in patternPatches)
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
            var moveDialogueBoxPatch = new PatternPatch(PatchType.Replace);
            moveDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64),
                        new CodeInstruction(OpCodes.Add),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            Log.Trace("Shrinking bottom dialogue box height by search bar height.");
            var resizeDialogueBoxPatch = new PatternPatch(PatchType.Replace);
            resizeDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Ldsfld, Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldc_I4, 192),
                        new CodeInstruction(OpCodes.Add),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, Method(typeof(SearchItemsFeature), nameof(SearchItemsFeature.MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            var patternPatches = new PatternPatches(instructions);
            patternPatches.AddPatch(moveDialogueBoxPatch);
            patternPatches.AddPatch(resizeDialogueBoxPatch);

            foreach (CodeInstruction patternPatch in patternPatches)
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

            if (menu is not ItemGrabMenu { context: Chest chest } || !SearchItemsFeature.Instance.IsEnabledForItem(chest))
            {
                return SearchItemsFeature.Instance._menuPadding.Value = 0; // Vanilla
            }

            return SearchItemsFeature.Instance._menuPadding.Value = SearchItemsFeature.SearchBarHeight;
        }

        private void OnItemGrabMenuConstructedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            e.ItemGrabMenu.yPositionOnScreen -= SearchItemsFeature.SearchBarHeight;
            e.ItemGrabMenu.height += SearchItemsFeature.SearchBarHeight;
            if (e.ItemGrabMenu.chestColorPicker is not null)
            {
                e.ItemGrabMenu.chestColorPicker.yPositionOnScreen -= SearchItemsFeature.SearchBarHeight;
            }
        }

        private void OnItemGrabMenuChangedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._renderedActiveMenuService.RemoveHandler(this.OnRenderedActiveMenu);
                this._displayedChestInventoryService.RemoveHandler(this.FilterMethod);
                this._gameLoopEvents.UpdateTicked -= this.OnUpdateTicked;
                this._inputEvents.ButtonPressed -= this.OnButtonPressed;
                this._menuPadding.Value = 0;
                return;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this._searchField.Value.Text = string.Empty;
            }

            this._renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            this._displayedChestInventoryService.AddHandler(this.FilterMethod);
            this._gameLoopEvents.UpdateTicked += this.OnUpdateTicked;
            this._inputEvents.ButtonPressed += this.OnButtonPressed;
            this._menu.Value = e.ItemGrabMenu;
            this._menuPadding.Value = SearchItemsFeature.SearchBarHeight;
            this._itemMatcher.Value = new ItemMatcher(this._modConfigService.ModConfig.SearchTagSymbol);
            var upperBounds = new Rectangle(
                e.ItemGrabMenu.ItemsToGrabMenu.xPositionOnScreen,
                e.ItemGrabMenu.ItemsToGrabMenu.yPositionOnScreen,
                e.ItemGrabMenu.ItemsToGrabMenu.width,
                e.ItemGrabMenu.ItemsToGrabMenu.height);
            this._searchField.Value.X = upperBounds.X;
            this._searchField.Value.Y = upperBounds.Y - (14 * Game1.pixelZoom);
            this._searchField.Value.Width = upperBounds.Width;
            this._searchField.Value.Selected = false;
            this._searchArea.Value.bounds = new Rectangle(this._searchField.Value.X, this._searchField.Value.Y, this._searchField.Value.Width, this._searchField.Value.Height);
            this._searchIcon.Value.bounds = new Rectangle(upperBounds.Right - 38, upperBounds.Y - (14 * Game1.pixelZoom) + 6, 32, 32);
            int x = e.ItemGrabMenu.xPositionOnScreen - 480 - 8;
            int y = e.ItemGrabMenu.ItemsToGrabMenu.yPositionOnScreen + 10;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._searchField.Value = new TextBox(
                textBoxTexture: this._contentHelper.Load<Texture2D>("LooseSprites\\textBox", ContentSource.GameContent),
                caretTexture: null,
                font: Game1.smallFont,
                textColor: Game1.textColor);
            this._searchIcon.Value = new ClickableTextureComponent(
                bounds: Rectangle.Empty,
                texture: Game1.mouseCursors,
                sourceRect: new Rectangle(80, 0, 13, 13),
                scale: 2.5f);
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            this._searchField.Value.Draw(e.SpriteBatch, false);
            this._searchIcon.Value.draw(e.SpriteBatch);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this._itemMatcher.Value.Search == this._searchField.Value.Text)
            {
                return;
            }

            this._itemMatcher.Value.SetSearch(this._searchField.Value.Text);
            this._displayedChestInventoryService.ReSyncInventory();
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Check if search bar was clicked on
            Point point = Game1.getMousePosition(true);
            switch (e.Button)
            {
                case SButton.MouseLeft when this.LeftClick(point.X, point.Y):
                case SButton.MouseRight when this.RightClick(point.X, point.Y):
                    this._inputHelper.Suppress(e.Button);
                    break;
                default:
                    if (this.KeyPress(e.Button))
                    {
                        this._inputHelper.Suppress(e.Button);
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
            this._itemMatcher.Value.SetSearch(string.Empty);
            return true;
        }

        private bool KeyPress(SButton button)
        {
            if (button != SButton.Escape)
            {
                return this._searchField.Value.Selected;
            }

            Game1.playSound("bigDeSelect");
            Game1.activeClickableMenu = null;
            return true;
        }

        private bool FilterMethod(Item item)
        {
            return this._itemMatcher.Value.Matches(item);
        }
    }
}