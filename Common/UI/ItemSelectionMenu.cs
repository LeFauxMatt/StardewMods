namespace Common.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Helpers.ItemMatcher;
    using Common.Helpers.ItemRepository;
    using Common.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <summary>
    /// A menu for selecting items.
    /// </summary>
    internal class ItemSelectionMenu : ItemGrabMenu
    {
        private static readonly PerScreen<ItemSelectionMenu> Instance = new();
        private static IEnumerable<Item> AllItems;
        private readonly Action<string> _returnValue;
        private readonly ClickableComponent _searchArea;
        private readonly TextBox _searchField;
        private readonly ClickableTextureComponent _searchIcon;
        private readonly ItemMatcher _itemFilter;
        private readonly ItemMatcher _itemSelector;
        private readonly List<Item> _sortedItems = new();
        private readonly IList<Item> _items;
        private readonly IList<ClickableComponent> _tags;
        private readonly Range<int> _range;
        private readonly InventoryMenu _menu;
        private readonly int _columns;
        private IEnumerable<Item> _filteredItems;
        private int _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSelectionMenu"/> class.
        /// </summary>
        /// <param name="searchTagSymbol">Character that will be used to denote tags in search.</param>
        /// <param name="exitFunction">The method to run when exiting this menu.</param>
        /// <param name="initialValue">The initial search expression that will be used.</param>
        /// <param name="returnValue">An action that will accept the return value on exit.</param>
        public ItemSelectionMenu(string searchTagSymbol, onExit exitFunction, string initialValue, Action<string> returnValue)
            : base(
                inventory: new List<Item>(),
                reverseGrab: false,
                showReceivingMenu: true,
                highlightFunction: ItemSelectionMenu.HighlightMethod,
                behaviorOnItemSelectFunction: (item, who) => { },
                message: null,
                behaviorOnItemGrab: (item, who) => { },
                canBeExitedWithKey: false,
                source: ItemSelectionMenu.source_none)
        {
            ItemSelectionMenu.Instance.Value = this;
            ItemSelectionMenu.AllItems ??= new ItemRepository().GetAll().Select(i => i.CreateItem()).ToList();
            this._returnValue = returnValue;
            this.exitFunction = exitFunction;
            this.behaviorBeforeCleanup = this.BehaviorBeforeCleanup;
            Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            Events.Input.ButtonPressed += this.OnButtonPressed;
            this._items = this.ItemsToGrabMenu.actualInventory;
            this._tags = this.inventory.inventory;
            this._menu = this.ItemsToGrabMenu;
            this._columns = this._menu.capacity / this._menu.rows;
            this._offset = 0;
            this._range = new Range<int>(0, (ItemSelectionMenu.AllItems.Count().RoundUp(this._columns) / this._columns) - this._menu.rows);
            this._itemFilter = new ItemMatcher(searchTagSymbol);
            this._itemSelector = new ItemMatcher(searchTagSymbol);

            // Get saved labels from favorites
            this._itemSelector.SetSearch(initialValue);

            this.ReSyncInventory();

            this._searchField = new TextBox(Content.FromGame<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = this.ItemsToGrabMenu.xPositionOnScreen,
                Y = this.ItemsToGrabMenu.yPositionOnScreen - (14 * Game1.pixelZoom),
                Width = this.ItemsToGrabMenu.width,
                Selected = false,
            };

            this._searchIcon = new ClickableTextureComponent(Rectangle.Empty, Game1.mouseCursors, new Rectangle(80, 0, 13, 13), 2.5f)
            {
                bounds = new Rectangle(this.ItemsToGrabMenu.xPositionOnScreen + this.ItemsToGrabMenu.width - 38, this.ItemsToGrabMenu.yPositionOnScreen - (14 * Game1.pixelZoom) + 6, 32, 32),
            };

            this._searchArea = new ClickableComponent(new Rectangle(this._searchField.X, this._searchField.Y, this._searchField.Width, this._searchField.Height), string.Empty);
        }

        /// <summary>
        /// Gets the displayed items.
        /// </summary>
        private IEnumerable<Item> Items
        {
            get
            {
                // Filter for searched items
                this._filteredItems ??= ItemSelectionMenu.AllItems!.Where(item => this._itemFilter.Matches(item));

                // Bring selected items to top
                if (this._sortedItems.Count == 0)
                {
                    this._sortedItems.AddRange(this._filteredItems.OrderBy(item => this._itemSelector.Matches(item) ? 0 : 1));
                }

                // Skip scrolled items
                IEnumerable<Item> items = this._sortedItems.Skip(this.Offset * this._columns);

                return items;
            }
        }

        /// <summary>
        /// Gets or sets the number of rows the currently displayed items are offset by.
        /// </summary>
        private int Offset
        {
            get => this._range.Clamp(this._offset);
            set
            {
                value = this._range.Clamp(value);
                if (this._offset != value)
                {
                    this._offset = value;
                    this.ReSyncInventory();
                }
            }
        }

        /// <inheritdoc/>
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.okButton.containsPoint(x, y) && this.readyToClose())
            {
                this.exitThisMenu();
                if (Game1.currentLocation.currentEvent is {CurrentCommand: > 0})
                {
                    Game1.currentLocation.currentEvent.CurrentCommand++;
                }

                Game1.playSound("bigDeSelect");
            }

            var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (cc is not null)
            {
                var slotNumber = Convert.ToInt32(cc.name);
                var item = this.Items.ElementAtOrDefault(slotNumber);
                if (item is not null)
                {
                    var tag = item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("item_"));
                    if (tag is not null)
                    {
                        this._itemSelector.AddSearch($"#{tag}");
                        this.ReSyncInventory(false, true);
                    }
                }

                return;
            }

            cc = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (cc is not null)
            {
                this._itemSelector.RemoveSearch(cc.name);
                this.ReSyncInventory(false, true);
                return;
            }
        }

        /// <inheritdoc/>
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (cc is not null)
            {
                var slotNumber = Convert.ToInt32(cc.name);
                var item = this.Items.ElementAtOrDefault(slotNumber);
                if (item is not null)
                {
                    var tag = item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("item_"));
                    if (tag is not null)
                    {
                        this._itemSelector.AddSearch($"!#{tag}");
                        this.ReSyncInventory(false, true);
                    }
                }

                return;
            }
        }

        /// <inheritdoc/>
        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);
        }

        /// <inheritdoc/>
        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
        }

        /// <inheritdoc/>
        public override void receiveScrollWheelAction(int direction)
        {
            var point = Game1.getMousePosition(true);
            if (!this.ItemsToGrabMenu.isWithinBounds(point.X, point.Y))
            {
                return;
            }

            switch (direction)
            {
                case > 0:
                    this.Offset--;
                    return;
                case < 0:
                    this.Offset++;
                    return;
                default:
                    base.receiveScrollWheelAction(direction);
                    return;
            }
        }

        /// <inheritdoc/>
        public override void performHoverAction(int x, int y)
        {
            this.okButton.scale = this.okButton.containsPoint(x, y)
                ? Math.Min(1.1f, this.okButton.scale + 0.05f)
                : Math.Max(1f, this.okButton.scale - 0.05f);

            var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (cc is not null)
            {
                var slotNumber = Convert.ToInt32(cc.name);
                this.hoveredItem = this.Items.ElementAtOrDefault(slotNumber);
                this.hoverText = string.Empty;
                return;
            }

            cc = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (cc is not null)
            {
                this.hoveredItem = null;
                this.hoverText = cc?.name ?? string.Empty;
                return;
            }

            this.hoveredItem = null;
            this.hoverText = string.Empty;
        }

        /// <inheritdoc/>
        public override void update(GameTime time)
        {
            base.update(time);
        }

        /// <inheritdoc/>
        public override void draw(SpriteBatch b)
        {
            if (this.drawBG)
            {
                b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
            }

            Game1.drawDialogueBox(
                this.ItemsToGrabMenu.xPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearSideBorder,
                this.ItemsToGrabMenu.yPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearTopBorder - 24,
                this.ItemsToGrabMenu.width + (ItemSelectionMenu.borderWidth * 2) + (ItemSelectionMenu.spaceToClearSideBorder * 2),
                this.ItemsToGrabMenu.height + ItemSelectionMenu.spaceToClearTopBorder + (ItemSelectionMenu.borderWidth * 2) + 24,
                false,
                true);

            Game1.drawDialogueBox(
                this.inventory.xPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearSideBorder,
                this.inventory.yPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearTopBorder,
                this.inventory.width + (ItemSelectionMenu.borderWidth * 2) + (ItemSelectionMenu.spaceToClearSideBorder * 2),
                this.inventory.height + ItemSelectionMenu.spaceToClearTopBorder + (ItemSelectionMenu.borderWidth * 2),
                false,
                true);

            this.ItemsToGrabMenu.draw(b);

            for (var i = 0; i < this.ItemsToGrabMenu.capacity; i++)
            {
                var item = this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(i);
                if (item is not null)
                {
                    var highlight = this.ItemsToGrabMenu.highlightMethod(item);
                    var x = this.ItemsToGrabMenu.xPositionOnScreen + ((this.ItemsToGrabMenu.horizontalGap + Game1.tileSize) * (i % (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows)));
                    var y = this.yPositionOnScreen + ((this.ItemsToGrabMenu.verticalGap + Game1.tileSize + 4) * (i / (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows))) - 4;
                    item.drawInMenu(
                        b,
                        new Vector2(x, y),
                        this.ItemsToGrabMenu.inventory[i].scale,
                        highlight ? 1f : 0.25f,
                        0.865f,
                        StackDrawType.Hide,
                        Color.White,
                        highlight);
                }
            }

            this._searchField.Draw(b, false);
            this._searchIcon.draw(b);
            this.okButton.draw(b);

            foreach (var tag in this._tags)
            {
                var textPos = new Vector2(tag.bounds.X, tag.bounds.Y);
                if (this.hoverText == tag.name)
                {
                    b.DrawString(Game1.smallFont, tag.name, textPos + new Vector2(2f, 2f), Game1.textShadowColor);
                    b.DrawString(Game1.smallFont, tag.name, textPos + new Vector2(0f, 2f), Game1.textShadowColor);
                }

                b.DrawString(Game1.smallFont, tag.name, textPos, Game1.textColor);
            }

            if (this.hoveredItem != null)
            {
                ItemSelectionMenu.drawHoverText(
                    b,
                    $"#{string.Join("\n#", SearchPhrase.GetContextTags(this.hoveredItem).ToList())}",
                    Game1.smallFont,
                    0,
                    0,
                    -1,
                    this.hoveredItem.DisplayName);
            }

            Game1.mouseCursorTransparency = 1f;
            this.drawMouse(b);
        }

        private static bool HighlightMethod(Item item)
        {
            return ItemSelectionMenu.Instance.Value._itemSelector.Matches(item);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this._itemFilter.Search == this._searchField.Text)
            {
                return;
            }

            this._itemFilter.SetSearch(this._searchField.Text);
            this.ReSyncInventory(false, true);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            switch (e.Button)
            {
                case SButton.Escape when this.readyToClose():
                    this.exitThisMenu();
                    Input.Suppress(e.Button);
                    return;
                case SButton.Escape:
                    Input.Suppress(e.Button);
                    return;
                case SButton.Enter when !string.IsNullOrWhiteSpace(this._itemFilter.Search):
                    this._itemSelector.AddSearch(this._itemFilter.Search);
                    this._searchField.Text = string.Empty;
                    this.Offset = 0;
                    this.ReSyncInventory(true);
                    Input.Suppress(e.Button);
                    break;
                case SButton.MouseLeft or SButton.MouseRight:
                {
                    var point = Game1.getMousePosition(true);
                    this._searchField.Selected = this._searchArea.containsPoint(point.X, point.Y);
                    break;
                }
            }

            if (this._searchField.Selected)
            {
                if (e.Button is SButton.MouseRight)
                {
                    this._searchField.Text = string.Empty;
                    this.Offset = 0;
                }

                Input.Suppress(e.Button);
            }
        }

        private void BehaviorBeforeCleanup(IClickableMenu menu)
        {
            Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            Events.Input.ButtonPressed -= this.OnButtonPressed;
            this._returnValue(this._itemSelector.Search);
        }

        private void ReSyncInventory(bool clearFiltered = false, bool clearSorted = false)
        {
            this._items.Clear();
            if (clearFiltered)
            {
                this._filteredItems = null;
            }

            if (clearFiltered || clearSorted)
            {
                this._sortedItems.Clear();
            }

            this._range.Maximum = Math.Max(0, (this.Items.Count().RoundUp(this._columns) / this._columns) - this._menu.rows);
            for (var i = 0; i < this.ItemsToGrabMenu.capacity; i++)
            {
                var item = this.Items.ElementAtOrDefault(i);
                if (item is null)
                {
                    break;
                }

                this.ItemsToGrabMenu.actualInventory.Add(item);
            }

            this._tags.Clear();
            const float horizontalSpacing = 10; // 16
            const float verticalSpacing = 5;
            var areaBounds = new Rectangle(this.inventory.xPositionOnScreen, this.inventory.yPositionOnScreen, this.inventory.width, this.inventory.height);
            var textPos = new Vector2(areaBounds.X, areaBounds.Y);
            var textHeight = (int)Game1.smallFont.MeasureString("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789").Y;
            foreach (var searchValue in this._itemSelector.SearchValues)
            {
                var textWidth = (int)Game1.smallFont.MeasureString(searchValue).X;
                var nextX = (int)(textPos.X + textWidth + horizontalSpacing);
                if (!areaBounds.Contains(nextX, (int)textPos.Y))
                {
                    textPos.X = areaBounds.X;
                    textPos.Y += textHeight + verticalSpacing;
                }

                var tag = new ClickableComponent(new Rectangle((int)textPos.X, (int)textPos.Y, textWidth, textHeight), searchValue);
                this._tags.Add(tag);
                textPos.X += textWidth + horizontalSpacing;
            }
        }
    }
}