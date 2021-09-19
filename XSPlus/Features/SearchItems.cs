namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Text.RegularExpressions;
    using Common.Extensions;
    using Common.Helpers;
    using CommonHarmony;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class SearchItems : BaseFeature
    {
        private const int SearchBarHeight = 24;
        private static readonly Type[] MenuWithInventoryDrawParams = { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) };
        private static readonly PerScreen<int> MenuPadding = new() { Value = -1 };
        private static readonly Rectangle FilledHeart = new(211, 428, 7, 6);
        private static readonly Rectangle EmptyHeart = new(218, 428, 7, 6);
        private static SearchItems Instance;
        private readonly IContentHelper _contentHelper;
        private readonly IInputHelper _inputHelper;
        private readonly Func<string> _getSearchTagSymbol;
        private readonly PerScreen<IClickableMenu> _menu = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<ClickableComponent> _searchArea = new() { Value = new ClickableComponent(Rectangle.Empty, string.Empty) };
        private readonly PerScreen<TextBox> _searchField = new();
        private readonly PerScreen<ClickableTextureComponent> _searchIcon = new();
        private readonly PerScreen<IList<ClickableTextureComponent>> _hearts = new() { Value = new List<ClickableTextureComponent>() };

        /// <summary>Initializes a new instance of the <see cref="SearchItems"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="getSearchTagSymbol">Get method for configured search tag symbol.</param>
        public SearchItems(IContentHelper contentHelper, IInputHelper inputHelper, Func<string> getSearchTagSymbol)
            : base("SearchItems")
        {
            SearchItems.Instance = this;
            this._contentHelper = contentHelper;
            this._inputHelper = inputHelper;
            this._getSearchTagSymbol = getSearchTagSymbol;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuConstructor += this.OnItemGrabMenuConstructor;
            CommonFeature.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
            CommonFeature.RenderedActiveMenu += this.OnRenderedActiveMenu;
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: new HarmonyMethod(typeof(SearchItems), nameof(SearchItems.ItemGrabMenu_draw_transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), SearchItems.MenuWithInventoryDrawParams),
                transpiler: new HarmonyMethod(typeof(SearchItems), nameof(SearchItems.MenuWithInventory_draw_transpiler)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuConstructor -= this.OnItemGrabMenuConstructor;
            CommonFeature.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
            CommonFeature.RenderedActiveMenu -= this.OnRenderedActiveMenu;
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                patch: AccessTools.Method(typeof(SearchItems), nameof(SearchItems.ItemGrabMenu_draw_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), SearchItems.MenuWithInventoryDrawParams),
                patch: AccessTools.Method(typeof(SearchItems), nameof(SearchItems.MenuWithInventory_draw_transpiler)));
        }

        /// <summary>Move/resize top dialogue box by search bar height.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                .Log("Moving backpack icon down by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                })
                .Repeat(3);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Sub),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Sub))
                .Log("Moving top dialogue box up by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Sub));
                });

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldc_I4_2),
                    new CodeInstruction(OpCodes.Mul),
                    new CodeInstruction(OpCodes.Add))
                .Log("Expanding top dialogue box height by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

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
            var patternPatches = new PatternPatches(instructions, Log.Monitor);

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64),
                    new CodeInstruction(OpCodes.Add))
                .Log("Moving bottom dialogue box down by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4, 192),
                    new CodeInstruction(OpCodes.Add))
                .Log("Shrinking bottom dialogue box height by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(SearchItems.GetMenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });

            foreach (CodeInstruction patternPatch in patternPatches)
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
            if (SearchItems.MenuPadding.Value != -1)
            {
                return SearchItems.MenuPadding.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !SearchItems.Instance.IsEnabledForItem(chest))
            {
                return SearchItems.MenuPadding.Value = 0; // Vanilla
            }

            return SearchItems.MenuPadding.Value = SearchItems.SearchBarHeight;
        }

        private void OnItemGrabMenuConstructor(object sender, CommonFeature.ItemGrabMenuConstructorEventArgs e)
        {
            if (!this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            e.ItemGrabMenu.yPositionOnScreen -= SearchItems.SearchBarHeight;
            e.ItemGrabMenu.height += SearchItems.SearchBarHeight;
            if (e.ItemGrabMenu.chestColorPicker != null)
            {
                e.ItemGrabMenu.chestColorPicker.yPositionOnScreen -= SearchItems.SearchBarHeight;
            }
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
            for (int i = 0; i < 10; i++)
            {
                this._hearts.Value.Add(new ClickableTextureComponent(
                    bounds: Rectangle.Empty,
                    texture: Game1.mouseCursors,
                    sourceRect: SearchItems.FilledHeart,
                    scale: 2.5f));
            }
        }

        private void OnItemGrabMenuChanged(object sender, CommonFeature.ItemGrabMenuChangedEventArgs e)
        {
            if (!e.Attached || !this.IsEnabledForItem(e.Chest))
            {
                CommonFeature.HighlightChestItems -= this.HighlightMethod;
                this._attached.Value = false;
                this._screenId.Value = -1;
                SearchItems.MenuPadding.Value = -1;
                return;
            }

            if (!this._attached.Value)
            {
                CommonFeature.HighlightChestItems += this.HighlightMethod;
                this._attached.Value = true;
                this._screenId.Value = Context.ScreenId;
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
                foreach (ClickableTextureComponent heart in this._hearts.Value)
                {
                    heart.bounds = new Rectangle(x, y, 16, 16);
                    y += 32;
                }
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
            }
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!this._attached.Value || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
            {
                return;
            }

            this._searchField.Value.Draw(e.SpriteBatch, false);
            this._searchIcon.Value.draw(e.SpriteBatch);

            // Get saved labels from search history
            if (!this._chest.Value.GetModDataList("Search", out var searchHistory))
            {
                return;
            }

            // Get saved labels from favorites
            if (!this._chest.Value.GetModDataList("Favorites", out var favorites))
            {
                favorites = new List<string>();
            }

            var labels = favorites.Union(searchHistory).Distinct().ToList();

            // Draw hearts/labels to the right of the chest menu along a vertical axis
            int x = itemGrabMenu.xPositionOnScreen - 480;
            int y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen;
            for (int i = 0; i < this._hearts.Value.Count; i++)
            {
                ClickableTextureComponent heart = this._hearts.Value[i];
                string label = labels.ElementAtOrDefault(i);
                if (label is null)
                {
                    return;
                }

                heart.sourceRect = favorites.Contains(label) ? SearchItems.FilledHeart : SearchItems.EmptyHeart;
                heart.draw(e.SpriteBatch);
                e.SpriteBatch.DrawString(Game1.smallFont, label, new Vector2(x + 32, y), Color.White);
                y += 32;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId)
            {
                return;
            }

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

            if (this._searchField.Value.Selected)
            {
                return true;
            }

            // Check if any labels to heart
            if (!this._chest.Value.GetModDataList("Search", out var searchHistory))
            {
                searchHistory = new List<string>();
            }

            if (!this._chest.Value.GetModDataList("Favorites", out var favorites))
            {
                favorites = new List<string>();
            }

            var labels = favorites.Union(searchHistory).Distinct().ToList();
            if (labels.Count == 0)
            {
                return false;
            }

            // Check if any heart was clicked on
            ClickableTextureComponent heart = this._hearts.Value.FirstOrDefault(heart => heart.containsPoint(x, y));
            if (heart is null)
            {
                return false;
            }

            // Check if clicked hearts corresponds to a label
            int index = this._hearts.Value.IndexOf(heart);
            if (index >= labels.Count)
            {
                return false;
            }

            // Toggle label on/off by adding to or removing from favorites
            string label = labels.ElementAt(index);
            if (favorites.Contains(label))
            {
                favorites.Remove(label);
            }
            else
            {
                favorites.Add(label);
            }

            this._chest.Value.SetModDataList("Favorites", favorites);
            return true;
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
            if (button == SButton.Enter)
            {
                if (!this._chest.Value.GetModDataList("Search", out var searchHistory))
                {
                    searchHistory = new List<string>();
                }

                string[] currentSearch = Regex.Split(this._searchField.Value.Text, @"\s+");
                searchHistory = searchHistory.Union(currentSearch).Reverse().Take(10).Reverse().ToList();
                this._chest.Value.SetModDataList("Search", searchHistory);
            }

            if (button != SButton.Escape)
            {
                return this._searchField.Value.Selected;
            }

            Game1.playSound("bigDeSelect");
            Game1.activeClickableMenu = null;
            return true;
        }

        private bool HighlightMethod(Item item)
        {
            return string.IsNullOrWhiteSpace(this._searchField.Value.Text) || item.SearchTags(Regex.Split(this._searchField.Value.Text, @"\s+"), this._getSearchTagSymbol());
        }
    }
}