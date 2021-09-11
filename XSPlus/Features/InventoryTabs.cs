using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Common.Extensions;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class InventoryTabs : FeatureWithParam<HashSet<string>>
    {
        // ReSharper disable InconsistentNaming
        private static readonly Type[] ItemGrabMenu_constructor_params = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        // ReSharper restore InconsistentNaming
        private static InventoryTabs _feature;
        private readonly PerScreen<TabView> _tabView = new();
        private readonly PerScreen<IClickableMenu> _oldMenu = new();

        public InventoryTabs(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
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
                postfix: new HarmonyMethod(typeof(InventoryTabs), nameof(InventoryTabs.ItemGrabMenu_constructor_postfix))
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
                patch: AccessTools.Method(typeof(InventoryTabs), nameof(InventoryTabs.ItemGrabMenu_constructor_postfix))
            );
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _tabView.Value = new TabView();
        }
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, _oldMenu.Value))
                return;
            _oldMenu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } itemGrabMenu || !IsEnabled(chest))
            {
                CommonHelper.HighlightMethods_ItemsToGrabMenu -= HighlightMethod;
                Helper.Events.Display.RenderingActiveMenu -= OnRenderingActiveMenu;
                Helper.Events.Input.ButtonsChanged -= OnButtonsChanged;
                Helper.Events.Input.ButtonPressed -= OnButtonPressed;
                _tabView.Value.DetachMenu();
            }
            else if (!_tabView.Value.Attached)
            {
                CommonHelper.HighlightMethods_ItemsToGrabMenu += HighlightMethod;
                Helper.Events.Display.RenderingActiveMenu += OnRenderingActiveMenu;
                Helper.Events.Input.ButtonsChanged += OnButtonsChanged;
                Helper.Events.Input.ButtonPressed += OnButtonPressed;
                _tabView.Value.AttachMenu(itemGrabMenu);
            }
        }
        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            _tabView.Value.DrawComponents(e.SpriteBatch);
        }
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (XSPlus.Config.NextTab.JustPressed())
            {
                _tabView.Value.TabIndex++;
                _feature.Helper.Input.SuppressActiveKeybinds(XSPlus.Config.NextTab);
            }
            else if (XSPlus.Config.PreviousTab.JustPressed())
            {
                _tabView.Value.TabIndex--;
                _feature.Helper.Input.SuppressActiveKeybinds(XSPlus.Config.PreviousTab);
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            if (e.Button == SButton.MouseLeft && _tabView.Value.LeftClick(x, y))
            {
                Helper.Input.Suppress(e.Button);
            }
        }
        private bool HighlightMethod(Item item)
        {
            var currentTab = _tabView.Value.CurrentTab;
            return currentTab is null || currentTab.Tags.Any(item.MatchesTagExt);
        }
        /// <summary>Remove background to render chest tabs under menu</summary>
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !_feature.IsEnabled(chest))
                return;
            __instance.setBackgroundTransparency(false);
        }
        private class TabView
        {
            public bool Attached { get; private set; }
            public Tab CurrentTab { get; private set; }

            public int TabIndex
            {
                get => _tabs.IndexOf(CurrentTab);
                set
                {
                    if (value < 0)
                        value = value + _tabs.Count + 1;
                    CurrentTab = value < _tabs.Count ? _tabs[value] : null;
                }
            }
            private readonly IList<Tab> _tabs;
            private ItemGrabMenu _menu;
            private Chest _context;
            private int _screenId = -1;
            public TabView()
            {
                _tabs = _feature.Helper.Content.Load<List<Tab>>("assets/tabs.json", ContentSource.ModFolder);
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
                }
            }
            public void DrawComponents(SpriteBatch b)
            {
                if (_screenId != Context.ScreenId)
                    return;
                
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
                
                // Draw tabs between inventory menus along a horizontal axis
                var x = _menu.ItemsToGrabMenu.xPositionOnScreen;
                var y = _menu.ItemsToGrabMenu.yPositionOnScreen + _menu.ItemsToGrabMenu.height + 1 * Game1.pixelZoom;
                foreach (var tab in _tabs)
                {
                    Color color;
                    tab.bounds.X = x;
                    if (ReferenceEquals(CurrentTab, tab))
                    {
                        tab.bounds.Y = y + 1 * Game1.pixelZoom;
                        color = Color.White;
                    }
                    else
                    {
                        tab.bounds.Y = y;
                        color = Color.Gray;
                    }
                    tab.draw(b, color, 0.86f + tab.bounds.Y / 20000f);
                    x = tab.bounds.Right;
                }
            }
            public bool LeftClick(int x = -1, int y = -1)
            {
                if (_screenId != Context.ScreenId)
                    return false;
                // Check if any tab was clicked on
                var tab = _tabs.FirstOrDefault(heart => heart.containsPoint(x, y));
                if (tab is null)
                    return false;
                CurrentTab = ReferenceEquals(CurrentTab, tab) ? null : tab;
                return true;
            }
            public class Tab : ClickableTextureComponent
            {
                public string Name { get; set; }
                public string Image { get; set; }
                public HashSet<string> Tags { get; set; }
                [JsonConstructor]
                public Tab(string name, string image) : base(
                    new Rectangle(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    _feature.Helper.Content.Load<Texture2D>($"assets/{image}"),
                    Rectangle.Empty,
                    Game1.pixelZoom
                )
                {
                    hoverText = name;
                    Image = image;
                }
            }
        }
    }
}