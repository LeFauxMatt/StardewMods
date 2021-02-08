using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace ExpandedStorage.Framework.UI
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    internal class MenuViewModel : IDisposable
    {
        private static readonly PerScreen<MenuViewModel> Instance = new();

        private static IModEvents _events;
        private static IInputHelper _inputHelper;
        private static IReflectionHelper _reflection;
        private static ModConfig _config;

        /// <summary>The screen ID for which the overlay was created, to support split-screen mode.</summary>
        private readonly int _screenId;
        
        private readonly MenuModel _model;
        private readonly MenuView _view;
        internal static void Init(IModEvents events, IInputHelper inputHelper, IReflectionHelper reflection, ModConfig config)
        {
            _events = events;
            _inputHelper = inputHelper;
            _reflection = reflection;
            _config = config;

            // Events
            _events.GameLoop.UpdateTicked += OnUpdateTicked;
            _events.Display.MenuChanged += OnMenuChanged;
        }

        private MenuViewModel(ItemGrabMenu menu)
        {
            _screenId = Context.ScreenId;
            _model = MenuModel.Get(menu);

            if (_model.StorageConfig == null)
                return;
            
            _view = new MenuView(menu.ItemsToGrabMenu,
                new MenuViewOptions
                {
                    ShowArrows = _config.ShowOverlayArrows && _model.StorageConfig != null,
                    ShowSearch = _config.ShowSearchBar && _model.StorageConfig != null && _model.StorageConfig.ShowSearchBar
                },
                Scroll,
                SetTab,
                SetSearch);

            if (_view.SearchField != null)
            {
                _view.SearchField.Text = _model.SearchText;
            }
            
            // Events
            _model.ItemChanged += OnItemChanged;
            _events.Input.ButtonsChanged += OnButtonsChanged;
            _events.Input.ButtonPressed += OnButtonPressed;
            _events.Input.CursorMoved += OnCursorMoved;
            _events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
            
            var reflectedBehaviorFunction =
                _reflection.GetField<ItemGrabMenu.behaviorOnItemSelect>(menu, "behaviorFunction");
            var behaviorOnItemSelect = reflectedBehaviorFunction.GetValue();
            reflectedBehaviorFunction.SetValue(delegate(Item item, Farmer who)
            {
                behaviorOnItemSelect?.Invoke(item, who);
                _model.RefreshItems();
            });
            
            var behaviorOnItemGrab = menu.behaviorOnItemGrab;
            menu.behaviorOnItemGrab = delegate(Item item, Farmer who)
            {
                behaviorOnItemGrab?.Invoke(item, who);
                _model.RefreshItems();
            };

            if (_model.StorageConfig == null)
                return;
            
            foreach (var tab in _model.StorageTabs)
            {
                _view.AddTab(tab.Texture, tab.TabName);
            }

            _view.CurrentTab = _model.CurrentTab;
            _model.RefreshItems();
        }
        
        public void Dispose()
        {
            Instance.Value = null;
            _events.Input.ButtonsChanged -= OnButtonsChanged;
            _events.Input.ButtonPressed -= OnButtonPressed;
            _events.Input.CursorMoved -= OnCursorMoved;
            _events.Input.MouseWheelScrolled -= OnMouseWheelScrolled;
            _model.ItemChanged -= OnItemChanged;
            _model?.Dispose();
            _view?.Dispose();
        }
        
        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Instance.Value != null && (!Context.HasScreenId(Instance.Value._screenId) || Game1.activeClickableMenu is not ItemGrabMenu))
                Instance.Value.Dispose();
            if (Instance.Value?._view?.SearchField != null)
                Instance.Value._model.SearchText = Instance.Value._view.SearchField.Text;
        }
        
        /// <summary>
        /// Resets scrolling/overlay when chest menu exits or context changes.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            Instance.Value?.Dispose();
            if (e.NewMenu is ItemGrabMenu {shippingBin: false} menu)
                Instance.Value = new MenuViewModel(menu);
        }

        /// <summary>Attempts to scroll offset by one row of slots relative to the inventory menu.</summary>
        /// <param name="direction">The direction which to scroll to.</param>
        private void Scroll(int direction)
        {
            switch (direction)
            {
                case > 0 when _model.SkippedRows > 0:
                    _model.SkippedRows--;
                    break;
                case < 0 when _model.SkippedRows < _model.MaxRows:
                    _model.SkippedRows++;
                    break;
            }
        }
        
        /// <summary>Sets the current tab by index.</summary>
        private void SetTab(int index) => _model.CurrentTab = index;
        
        /// <summary>Switch to previous tab.</summary>
        private void PreviousTab() => _model.CurrentTab--;
        
        /// <summary>Switch to next tab.</summary>
        private void NextTab() => _model.CurrentTab++;
        
        /// <summary>Filter items by search text.</summary>
        private void SetSearch(string searchText) => _model.SearchText = searchText;
        
        /// <summary>Track if configured control buttons are pressed or pass input to overlay.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (_view == null || Context.ScreenId != _screenId)
                return;

            if (_config.AllowModdedCapacity)
            {
                if (_config.Controls.ScrollDown.JustPressed())
                {
                    Scroll(-1);
                    _inputHelper.SuppressActiveKeybinds(_config.Controls.ScrollDown);
                }
                else if (_config.Controls.ScrollUp.JustPressed())
                {
                    Scroll(1);
                    _inputHelper.SuppressActiveKeybinds(_config.Controls.ScrollUp);
                }
            }

            if (!_config.ShowTabs)
                return;
            
            if (_config.Controls.PreviousTab.JustPressed())
            {
                PreviousTab();
                _view.CurrentTab = _model.CurrentTab;
                _inputHelper.SuppressActiveKeybinds(_config.Controls.PreviousTab);
            }
            else if (_config.Controls.NextTab.JustPressed())
            {
                NextTab();
                _view.CurrentTab = _model.CurrentTab;
                _inputHelper.SuppressActiveKeybinds(_config.Controls.NextTab);
            }
        }

        /// <summary>Track if configured control buttons are pressed or pass input to overlay.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (_view == null || Context.ScreenId != _screenId)
                return;
            
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            
            if ((e.Button == SButton.MouseLeft || e.Button.IsUseToolButton()) && _view.LeftClick(x, y))
                _inputHelper.Suppress(e.Button);
            else if ((e.Button == SButton.MouseRight || e.Button.IsActionButton()) && _view.RightClick(x, y))
                _inputHelper.Suppress(e.Button);
            else if (_view.ReceiveKeyPress(e.Button))
                _inputHelper.Suppress(e.Button);
        }
        
        /// <summary>Raised after the player moves the in-game cursor.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (_view == null || Context.ScreenId != _screenId)
                return;
            
            var x = Game1.getMouseX(true);
            var y = Game1.getMouseY(true);
            _view.Hover(x, y);
        }
        
        /// <summary>Raised after the player scrolls the mouse wheel.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (_view == null || Context.ScreenId != _screenId)
                return;
            
            Scroll(e.Delta);
            
            var cur = Game1.oldMouseState;
            Game1.oldMouseState = new MouseState(
                cur.X,
                cur.Y,
                e.NewValue,
                cur.LeftButton,
                cur.MiddleButton,
                cur.RightButton,
                cur.XButton1,
                cur.XButton2
            );
        }

        /// <summary>Sync UI state to currently filtered view.</summary>
        /// <param name="sender">The even sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnItemChanged(object sender, EventArgs e)
        {
            // Update Inventory Menu to correct item slot
            for (var i = 0; i < _model.Menu.ItemsToGrabMenu.inventory.Count; i++)
            {
                var item = _model.FilteredItems.ElementAtOrDefault(i);
                _model.Menu.ItemsToGrabMenu.inventory[i].name = item != null
                    ? _model.Items.IndexOf(item).ToString()
                    : _model.Items.Count.ToString();
            }
            
            
            // Show/hide arrows
            if (_view.UpArrow != null)
            {
                _view.UpArrow.visible = _model.SkippedRows > 0;
            }

            if (_view.DownArrow != null)
            {
                _view.DownArrow.visible = _model.SkippedRows < _model.MaxRows;
            }
        }
    }
}