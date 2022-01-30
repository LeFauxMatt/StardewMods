namespace StardewMods.FuryCore.UI;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Common.Extensions;
using Common.Helpers.ItemRepository;
using Common.Models;
using StardewMods.FuryCore.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     A menu for selecting items.
/// </summary>
public class ItemSelectionMenu : ItemGrabMenu
{
    private int _offset;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemSelectionMenu" /> class.
    /// </summary>
    /// <param name="inputHelper">API for changing input states.</param>
    /// <param name="itemMatcher">Matches items against name and context tags.</param>
    public ItemSelectionMenu(IInputHelper inputHelper, ItemMatcher itemMatcher)
        : base(
            new List<Item>(),
            false,
            true,
            null,
            (_, _) => { },
            null,
            (_, _) => { },
            canBeExitedWithKey: false,
            source: ItemSelectionMenu.source_none)
    {
        ItemSelectionMenu.AllItems ??= new ItemRepository().GetAll().ToList();
        this.InputHelper = inputHelper;
        this.ItemMatcher = itemMatcher;
        this.ItemsToGrabMenu.highlightMethod = this.ItemMatcher.Matches;
        this.Columns = this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
        this.RefreshTags();

        this.SearchField = new(Game1.content.Load<Texture2D>(@"LooseSprites\textBox"), null, Game1.smallFont, Game1.textColor)
        {
            X = this.ItemsToGrabMenu.xPositionOnScreen,
            Y = this.ItemsToGrabMenu.yPositionOnScreen - (14 * Game1.pixelZoom),
            Width = this.ItemsToGrabMenu.width,
            Selected = false,
        };

        this.SearchIcon = new(Rectangle.Empty, Game1.mouseCursors, new(80, 0, 13, 13), 2.5f)
        {
            bounds = new(this.ItemsToGrabMenu.xPositionOnScreen + this.ItemsToGrabMenu.width - 38, this.ItemsToGrabMenu.yPositionOnScreen - (14 * Game1.pixelZoom) + 6, 32, 32),
        };

        this.SearchArea = new(new(this.SearchField.X, this.SearchField.Y, this.SearchField.Width, this.SearchField.Height), string.Empty);
    }

    private static IList<SearchableItem> AllItems { get; set; }

    private IInputHelper InputHelper { get; }

    private int Columns { get; }

    private ClickableComponent SearchArea { get; }

    private TextBox SearchField { get; }

    private ClickableTextureComponent SearchIcon { get; }

    private DropDownMenu TagMenu { get; set; }

    private ItemMatcher ItemMatcher { get; }

    private IList<SearchableItem> DisplayedItems { get; set; }

    private string SearchText { get; set; }

    private IEnumerable<SearchableItem> SortedItems { get; set; }

    private IEnumerable<SearchableItem> Items
    {
        get
        {
            if (this.SortedItems is null)
            {
                this.SortedItems = this.ItemMatcher.Any()
                    ? ItemSelectionMenu.AllItems.OrderBy(item => this.ItemMatcher.Matches(item.Item) ? 0 : 1)
                    : ItemSelectionMenu.AllItems.AsEnumerable();

                this.DisplayedItems = null;
            }

            this.DisplayedItems ??= string.IsNullOrWhiteSpace(this.SearchField.Text)
                ? this.SortedItems.ToList()
                : this.SortedItems.Where(item => item.NameContains(this.SearchField.Text)).ToList();

            return this.DisplayedItems.Skip(this.Offset * this.Columns);
        }
    }

    private int Offset
    {
        get
        {
            this.Range.Maximum = Math.Max(0, (this.DisplayedItems.Count - this.ItemsToGrabMenu.capacity).RoundUp(this.Columns) / this.Columns);
            return this.Range.Clamp(this._offset);
        }

        set
        {
            this.Range.Maximum = Math.Max(0, (this.DisplayedItems.Count - this.ItemsToGrabMenu.capacity).RoundUp(this.Columns) / this.Columns);
            this._offset = this.Range.Clamp(value);
        }
    }

    private Range<int> Range { get; } = new();

    /// <summary>
    ///     Allows the <see cref="ItemSelectionMenu" /> to register SMAPI events for handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void RegisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed += this.OnButtonPressed;
        this.ItemMatcher.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    ///     Allows the <see cref="ItemSelectionMenu" /> to unregister SMAPI events from handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void UnregisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed -= this.OnButtonPressed;
        this.ItemMatcher.CollectionChanged -= this.OnCollectionChanged;
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.okButton.containsPoint(x, y) && this.readyToClose())
        {
            this.exitThisMenu();
            if (Game1.currentLocation.currentEvent is { CurrentCommand: > 0 })
            {
                Game1.currentLocation.currentEvent.CurrentCommand++;
            }

            Game1.playSound("bigDeSelect");
            return;
        }

        // Left click an item slot to add individual item tag to filters
        var itemSlot = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (itemSlot is not null
            && int.TryParse(itemSlot.name, out var slotNumber)
            && this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber) is { } item
            && item.GetContextTags().FirstOrDefault(contextTag => contextTag.StartsWith("item_")) is { } tag
            && !string.IsNullOrWhiteSpace(tag))
        {
            this.ItemMatcher.Add(tag);
            return;
        }

        // Left click an existing tag to remove from filters
        itemSlot = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (itemSlot is not null && !string.IsNullOrWhiteSpace(itemSlot.name))
        {
            this.ItemMatcher.Remove(itemSlot.name);
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        // Right click an item slot to display dropdown with item's context tags
        if (this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y)) is { } itemSlot
            && int.TryParse(itemSlot.name, out var slotNumber)
            && this.Items.ElementAtOrDefault(slotNumber) is { Item: { } item })
        {
            var tags = ItemMatcher.GetContextTags(item).ToList();
            if (tags.Contains("quality_none"))
            {
                tags.Add("quality_silver");
                tags.Add("quality_gold");
                tags.Add("quality_iridium");
            }

            this.TagMenu = new(tags, x, y, this.AddTag);
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        var (x, y) = Game1.getMousePosition(true);
        if (!this.ItemsToGrabMenu.isWithinBounds(x, y))
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

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        this.okButton.scale = this.okButton.containsPoint(x, y)
            ? Math.Min(1.1f, this.okButton.scale + 0.05f)
            : Math.Max(1f, this.okButton.scale - 0.05f);

        this.TagMenu?.TryHover(x, y);

        var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (cc is not null)
        {
            var slotNumber = Convert.ToInt32(cc.name);
            this.hoveredItem = this.Items.ElementAtOrDefault(slotNumber)?.Item;
            this.hoverText = string.Empty;
            return;
        }

        cc = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (cc is not null)
        {
            this.hoveredItem = null;
            this.hoverText = cc.name ?? string.Empty;
            return;
        }

        this.hoveredItem = null;
        this.hoverText = string.Empty;
    }

    /// <inheritdoc />
    public override void update(GameTime time)
    {
        if (this.SearchText != this.SearchField.Text)
        {
            this.SearchText = this.SearchField.Text;
            this.DisplayedItems = null;
        }
    }

    /// <inheritdoc />
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

        foreach (var (item, index) in this.Items.Take(this.ItemsToGrabMenu.capacity).Select((item, index) => (item.Item, index)))
        {
            var highlight = this.ItemMatcher.Matches(item);
            var x = this.ItemsToGrabMenu.xPositionOnScreen + ((this.ItemsToGrabMenu.horizontalGap + Game1.tileSize) * (index % (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows)));
            var y = this.yPositionOnScreen + ((this.ItemsToGrabMenu.verticalGap + Game1.tileSize + 4) * (index / (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows))) - 4;
            item.drawInMenu(
                b,
                new(x, y),
                this.ItemsToGrabMenu.inventory[index].scale,
                highlight ? 1f : 0.25f,
                0.865f,
                StackDrawType.Hide,
                Color.White,
                highlight);
        }

        this.SearchField.Draw(b, false);
        this.SearchIcon.draw(b);
        this.okButton.draw(b);

        foreach (var tag in this.inventory.inventory)
        {
            if (this.hoverText == tag.name)
            {
                Utility.drawTextWithShadow(b, tag.name, Game1.smallFont, new(tag.bounds.X, tag.bounds.Y), Game1.textColor, 1f, 0.1f);
            }
            else
            {
                b.DrawString(Game1.smallFont, tag.name, new(tag.bounds.X, tag.bounds.Y), Game1.textColor);
            }
        }

        if (this.TagMenu is not null)
        {
            this.TagMenu.Draw(b);
        }
        else if (this.hoveredItem != null)
        {
            ItemSelectionMenu.drawToolTip(b, this.hoveredItem.getDescription(), this.hoveredItem.DisplayName, this.hoveredItem);
        }

        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        var (x, y) = Game1.getMousePosition(true);

        switch (e.Button)
        {
            case SButton.Escape when this.readyToClose():
                this.InputHelper.Suppress(e.Button);
                this.exitThisMenu();
                return;

            case SButton.Escape:
                this.InputHelper.Suppress(e.Button);
                return;

            case SButton.MouseLeft:
                this.SearchField.Selected = this.SearchArea.containsPoint(x, y);
                this.TagMenu?.LeftClick(x, y);
                this.TagMenu = null;
                break;

            case SButton.MouseRight:
                this.SearchField.Selected = this.SearchArea.containsPoint(x, y);
                if (this.SearchField.Selected)
                {
                    this.SearchField.Text = string.Empty;
                    this.Offset = 0;
                }

                this.TagMenu = null;

                break;
        }

        if (this.SearchField.Selected)
        {
            this.InputHelper.Suppress(e.Button);
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.SortedItems = null;
        this.RefreshTags();
    }

    private void AddTag(string tag)
    {
        if (this.InputHelper.IsDown(SButton.LeftShift) || this.InputHelper.IsDown(SButton.RightShift))
        {
            tag = $"!{tag}";
        }

        this.ItemMatcher.Add(tag);
    }

    private void RefreshTags()
    {
        this.inventory.inventory = this.ItemMatcher.Select(
            value =>
            {
                var (textWidth, textHeight) = Game1.smallFont.MeasureString(value).ToPoint();
                return new ClickableComponent(new(0, 0, textWidth, textHeight), value);
            }).ToList();

        if (!this.inventory.inventory.Any())
        {
            return;
        }

        const int horizontalSpacing = 10; // 16
        const int verticalSpacing = 5;
        var areaBounds = new Rectangle(this.inventory.xPositionOnScreen, this.inventory.yPositionOnScreen, this.inventory.width, this.inventory.height);
        var maxHeight = this.inventory.inventory.Max(tag => tag.bounds.Height);
        var (x, y) = areaBounds.Location;

        foreach (var tag in this.inventory.inventory)
        {
            if (!areaBounds.Contains(x + tag.bounds.Width + horizontalSpacing, y))
            {
                x = areaBounds.X;
                y += maxHeight + verticalSpacing;
            }

            tag.bounds.X = x;
            tag.bounds.Y = y;
            x += tag.bounds.Width + horizontalSpacing;
        }
    }
}