using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
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

namespace XSPlus.Features
{
    internal class SearchItems : BaseFeature
    {
        // ReSharper disable InconsistentNaming
        private static readonly Type[] ItemGrabMenu_constructor_params = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        private static readonly Type[] MenuWithInventory_draw_params = { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) };
        // ReSharper restore InconsistentNaming
        private const int SearchBarHeight = 24;
        private static SearchItems _feature;
        private readonly PerScreen<SearchView> _searchView = new();
        private readonly PerScreen<IClickableMenu> _oldMenu = new();
        public SearchItems(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
            _feature = this;
        }
        protected override void EnableFeature()
        {
            // Events
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Display.MenuChanged += OnMenuChanged;
            
            // Patches
            Harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ItemGrabMenu_constructor_params),
                postfix: new HarmonyMethod(typeof(SearchItems), nameof(SearchItems.ItemGrabMenu_constructor_postfix))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] {typeof(SpriteBatch)}),
                transpiler: new HarmonyMethod(typeof(SearchItems), nameof(SearchItems.ItemGrabMenu_draw_transpiler))
            );
            Harmony.Patch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw),MenuWithInventory_draw_params),
                transpiler: new HarmonyMethod(typeof(SearchItems), nameof(SearchItems.MenuWithInventory_draw_transpiler))
            );
        }
        protected override void DisableFeature()
        {
            // Events
            Helper.Events.GameLoop.GameLaunched -= OnGameLaunched;
            Helper.Events.Display.MenuChanged -= OnMenuChanged;
            
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ItemGrabMenu_constructor_params),
                patch: AccessTools.Method(typeof(SearchItems), nameof(SearchItems.ItemGrabMenu_constructor_postfix))
            );
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] {typeof(SpriteBatch)}),
                patch: AccessTools.Method(typeof(SearchItems), nameof(SearchItems.ItemGrabMenu_draw_transpiler))
            );
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw),MenuWithInventory_draw_params),
                patch: AccessTools.Method(typeof(SearchItems), nameof(SearchItems.MenuWithInventory_draw_transpiler))
            );
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _searchView.Value = new SearchView();
        }
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, _oldMenu.Value))
                return;
            _oldMenu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu || !IsEnabled(chest))
            {
                CommonHelper.HighlightMethods_ItemsToGrabMenu -= HighlightMethod;
                Helper.Events.Display.RenderedActiveMenu -= OnRenderedActiveMenu;
                Helper.Events.Input.ButtonPressed -= OnButtonPressed;
                _searchView.Value.DetachMenu();
            }
            else if (!_searchView.Value.Attached)
            {
                CommonHelper.HighlightMethods_ItemsToGrabMenu += HighlightMethod;
                Helper.Events.Display.RenderedActiveMenu += OnRenderedActiveMenu;
                Helper.Events.Input.ButtonPressed += OnButtonPressed;
                _searchView.Value.AttachMenu(itemGrabMenu);
            }
        }
        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            _searchView.Value.DrawComponents(e.SpriteBatch);
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (e.Button)
            {
                case SButton.MouseLeft when _searchView.Value.LeftClick(x, y):
                case SButton.MouseRight when _searchView.Value.RightClick(x, y):
                    Helper.Input.Suppress(e.Button);
                    break;
                default:
                    if (_searchView.Value.ReceiveKeyPress(e.Button))
                        Helper.Input.Suppress(e.Button);
                    break;
            }
        }
        /// <summary>Expand menu height to accomodate the search bar</summary>
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !_feature.IsEnabled(chest))
                return;
            __instance.yPositionOnScreen -= SearchBarHeight;
            __instance.height += SearchBarHeight;
            if (__instance.chestColorPicker != null)
                __instance.chestColorPicker.yPositionOnScreen -= SearchBarHeight;
        }
        /// <summary>Move/resize top dialogue box by search bar height</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, _feature.Monitor);
            
            patternPatches
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                .Log("Moving backpack icon down by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(MenuPadding))));
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
                    new CodeInstruction(OpCodes.Sub)
                )
                .Log("Moving top dialogue box up by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(MenuPadding))));
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
                    new CodeInstruction(OpCodes.Add)
                )
                .Log("Expanding top dialogue box height by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _feature.Monitor.Log($"Failed to apply all patches in {typeof(ItemGrabMenu)}::{nameof(ItemGrabMenu.draw)}.", LogLevel.Warn);
        }
        /// <summary>Move/resize bottom dialogue box by search bar height</summary>
        private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var patternPatches = new PatternPatches(instructions, _feature.Monitor);
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64),
                    new CodeInstruction(OpCodes.Add)
                )
                .Log("Moving bottom dialogue box down by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                    new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                    new CodeInstruction(OpCodes.Add),
                    new CodeInstruction(OpCodes.Ldc_I4, 192),
                    new CodeInstruction(OpCodes.Add)
                )
                .Log("Shrinking bottom dialogue box height by search bar height.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SearchItems), nameof(MenuPadding))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                });
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                _feature.Monitor.Log($"Failed to apply all patches in {typeof(MenuWithInventory)}::{nameof(MenuWithInventory.draw)}.", LogLevel.Warn);
        }
        private static int MenuPadding(MenuWithInventory menu)
        {
            return menu is ItemGrabMenu { shippingBin: false, context: Chest chest } && _feature.IsEnabled(chest) ? SearchBarHeight : 0;
        }
        private bool HighlightMethod(Item item)
        {
            var currentSearch = _searchView.Value.CurrentSearch;
            return currentSearch.Any(search => item.SearchTag(search, XSPlus.Config.SearchTagSymbol));
        }
        private class SearchView
        {
            public bool Attached { get; private set; }
            public IEnumerable<string> CurrentSearch => Regex.Split(_searchField.Text, @"\s+");
            /// <summary>Corresponds to the bounds of the searchField.</summary>
            private readonly ClickableComponent _searchArea;
            /// <summary>Input to filter items by name or context tags.</summary>
            private readonly TextBox _searchField;
            /// <summary>Icon to display next to search box.</summary>
            private readonly ClickableTextureComponent _searchIcon;
            private readonly IList<ClickableTextureComponent> _hearts;
            private readonly Rectangle _emptyHeart;
            private readonly Rectangle _filledHeart;
            private ItemGrabMenu _menu;
            private Chest _context;
            private int _screenId = -1;
            public SearchView()
            {
                var texture = _feature.Helper.Content.Load<Texture2D>("LooseSprites\\Cursors", ContentSource.GameContent);
                _searchField = new TextBox(
                    textBoxTexture: _feature.Helper.Content.Load<Texture2D>("LooseSprites\\textBox", ContentSource.GameContent),
                    caretTexture: null,
                    font: _feature.Helper.Content.Load<SpriteFont>("Fonts\\SmallFont", ContentSource.GameContent),
                    textColor: Game1.textColor
                );
                _searchArea = new ClickableComponent(Rectangle.Empty, "");
                _searchIcon = new ClickableTextureComponent(
                    bounds: Rectangle.Empty,
                    texture: texture,
                    sourceRect: new Rectangle(80, 0, 13, 13),
                    scale: 2.5f
                );
                _hearts = new List<ClickableTextureComponent>();
                _emptyHeart = new Rectangle(218, 428, 7, 6);
                _filledHeart = new Rectangle(211, 428, 7, 6);
                for (var i = 0; i < 10; i++)
                {
                    var heart = new ClickableTextureComponent(
                        bounds: Rectangle.Empty,
                        texture: texture,
                        sourceRect: Rectangle.Empty,
                        scale: 2.5f
                    );
                    _hearts.Add(heart);
                }
            }
            public void DetachMenu()
            {
                _menu = null;
                Attached = false;
                _screenId = -1;
            }
            public void AttachMenu(ItemGrabMenu menu)
            {
                Attached = true;
                _menu = menu;
                _screenId = Context.ScreenId;
                
                if (!ReferenceEquals(_menu.context, _context))
                {
                    _context = (Chest)_menu.context;
                    _searchField.Text = "";                    
                }
                
                var upperBounds = new Rectangle(
                    _menu.ItemsToGrabMenu.xPositionOnScreen,
                    _menu.ItemsToGrabMenu.yPositionOnScreen,
                    _menu.ItemsToGrabMenu.width,
                    _menu.ItemsToGrabMenu.height
                );
                _searchField.X = upperBounds.X;
                _searchField.Y = upperBounds.Y - 14 * Game1.pixelZoom;
                _searchField.Width = upperBounds.Width;
                _searchField.Selected = false;
                _searchArea.bounds = new Rectangle(_searchField.X, _searchField.Y, _searchField.Width, _searchField.Height);
                _searchIcon.bounds = new Rectangle(upperBounds.Right - 38, upperBounds.Y - 14 * Game1.pixelZoom + 6, 32, 32);

                var x = _menu.xPositionOnScreen + _menu.width + 96; 
                var y = _menu.ItemsToGrabMenu.yPositionOnScreen + 10;
                foreach (var heart in _hearts)
                {
                    heart.bounds = new Rectangle(x, y, 16, 16);
                    y += 32;
                }
            }
            public void DrawComponents(SpriteBatch b)
            {
                if (_screenId != Context.ScreenId)
                    return;
                _searchField.Draw(b, false);
                _searchIcon.draw(b);
                
                // Get labels from favorites and search history
                if (!_context.GetModDataList("Search", out var searchHistory))
                    return;
                if (!_context.GetModDataList("Favorites", out var favorites))
                    favorites = new List<string>();
                var labels = favorites.Union(searchHistory).Distinct().ToList();
                
                // Draw hearts/labels to the right of the chest menu along a vertical axis
                var x = _menu.xPositionOnScreen + _menu.width + 96;
                var y = _menu.ItemsToGrabMenu.yPositionOnScreen;
                for (var i = 0; i < _hearts.Count; i++)
                {
                    var heart = _hearts[i];
                    var label = labels.ElementAtOrDefault(i);
                    if (label is null)
                        return;
                    heart.sourceRect = favorites.Contains(label) ? _filledHeart : _emptyHeart;
                    heart.draw(b);
                    b.DrawString(Game1.smallFont, label, new Vector2(x + 32, y), Color.White);
                    y += 32;
                }
                
                _menu.drawMouse(b);
            }
            public bool ReceiveKeyPress(SButton button)
            {
                if (_screenId != Context.ScreenId)
                    return false;
                if (button == SButton.Enter)
                {
                    if (!_context.GetModDataList("Search", out var searchHistory))
                        searchHistory = new List<string>();
                    var currentSearch = CurrentSearch;
                    searchHistory = searchHistory.Union(currentSearch).Reverse().Take(10).Reverse().ToList();
                    _context.SetModDataList("Search", searchHistory);
                }
                if (button != SButton.Escape)
                    return _searchField.Selected;
                Game1.playSound("bigDeSelect");
                Game1.activeClickableMenu = null;
                return true;
            }
            public bool LeftClick(int x = -1, int y = -1)
            {
                if (_screenId != Context.ScreenId)
                    return false;
                if (x != -1 && y != -1)
                    _searchField.Selected = _searchArea.containsPoint(x, y);
                if (_searchField.Selected)
                    return true;
                
                // Check if any labels to heart
                if (!_context.GetModDataList("Search", out var searchHistory))
                    searchHistory = new List<string>();
                if (!_context.GetModDataList("Favorites", out var favorites))
                    favorites = new List<string>();
                var labels = favorites.Union(searchHistory).Distinct().ToList();
                if (labels.Count == 0)
                    return false;
                
                // Check if any heart was clicked on
                var heart = _hearts.FirstOrDefault(heart => heart.containsPoint(x, y));
                if (heart is null)
                    return false;
                
                // Check if clicked hearts corresponds to a label
                var index = _hearts.IndexOf(heart);
                if (index >= labels.Count)
                    return false;
                
                // Toggle label on/off by adding to or removing from favorites
                var label = labels.ElementAt(index);
                if (favorites.Contains(label))
                    favorites.Remove(label);
                else
                    favorites.Add(label);
                _context.SetModDataList("Favorites", favorites);
                return true;
            }
            public bool RightClick(int x = -1, int y = -1)
            {
                if (_screenId != Context.ScreenId)
                    return false;
                if (x != -1 && y != -1)
                    _searchField.Selected = _searchArea.containsPoint(x, y);
                if (!_searchField.Selected)
                    return false;
                _searchField.Text = "";
                return true;
            }
        }
    }
}