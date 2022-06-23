namespace StardewMods.Common.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.ItemRepository;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Menu for selecting <see cref="Item" /> based on their context tags.
/// </summary>
internal class ItemSelectionMenu : ItemGrabMenu
{
    private static HashSet<Item>? CachedItems;
    private static int? CachedLineHeight;
    private static List<ClickableComponent>? CachedTags;

    private const int HorizontalTagSpacing = 10;
    private const int VerticalTagSpacing = 5;
    private int _offset;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemSelectionMenu"/> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="context">The source object.</param>
    public ItemSelectionMenu(IModHelper helper, object? context)
        : base(
            new List<Item>(),
            false,
            true,
            null,
            (_, _) => { },
            null,
            (_, _) => { },
            canBeExitedWithKey: false,
            source: ItemSelectionMenu.source_none,
            context: context)
    {
        this.Helper = helper;
        this.ItemMatcher = new(true);
        this.ItemsToGrabMenu.highlightMethod = this.ItemMatcher.Matches;
        this.ItemsToGrabMenu.actualInventory = ItemSelectionMenu.Items.ToList();
    }

    private static IEnumerable<Item> Items
    {
        get => ItemSelectionMenu.CachedItems ??= new(new ItemRepository().GetAll().Select(item => item.Item));
    }

    private static IEnumerable<ClickableComponent> Tags
    {
        get => ItemSelectionMenu.CachedTags ??= (
                                                    from item in ItemSelectionMenu.Items
                                                    from tag in item.GetContextTags()
                                                    where !tag.StartsWith("id_") && !tag.StartsWith("item_") && !tag.StartsWith("preserve_")
                                                    orderby tag
                                                    select tag)
                                                .Distinct()
                                                .Select(tag =>
                                                {
                                                    var (width, height) = Game1.smallFont.MeasureString(tag).ToPoint();
                                                    return new ClickableComponent(new(0, 0, width, height), tag);
                                                })
                                                .ToList();
    }

    private static int LineHeight
    {
        get => ItemSelectionMenu.CachedLineHeight ??= ItemSelectionMenu.Tags.Max(tag => tag.bounds.Height) + ItemSelectionMenu.VerticalTagSpacing;
    }

    private IModHelper Helper { get; }

    private ItemMatcher ItemMatcher { get; }

    private int Offset
    {
        get => this._offset;
        set => this._offset = value;
    }

    /// <inheritdoc/>
    public override void draw(SpriteBatch b)
    {
        Game1.drawDialogueBox(
            this.ItemsToGrabMenu.xPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearSideBorder,
            this.ItemsToGrabMenu.yPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearTopBorder - 24,
            this.ItemsToGrabMenu.width + ItemSelectionMenu.borderWidth * 2 + ItemSelectionMenu.spaceToClearSideBorder * 2,
            this.ItemsToGrabMenu.height + ItemSelectionMenu.spaceToClearTopBorder + ItemSelectionMenu.borderWidth * 2 + 24,
            false,
            true);

        Game1.drawDialogueBox(
            this.inventory.xPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearSideBorder,
            this.inventory.yPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearTopBorder + 24,
            this.inventory.width + ItemSelectionMenu.borderWidth * 2 + ItemSelectionMenu.spaceToClearSideBorder * 2,
            this.inventory.height + ItemSelectionMenu.spaceToClearTopBorder + ItemSelectionMenu.borderWidth * 2 - 24,
            false,
            true);

        this.ItemsToGrabMenu.draw(b);
        this.okButton.draw(b);

        foreach (var tag in this.inventory.inventory.Where(cc => this.inventory.isWithinBounds(cc.bounds.X, cc.bounds.Bottom - this.Offset * ItemSelectionMenu.LineHeight)))
        {
            if (this.hoverText == tag.name)
            {
                Utility.drawTextWithShadow(
                    b,
                    tag.name,
                    Game1.smallFont,
                    new(tag.bounds.X, tag.bounds.Y - this.Offset * ItemSelectionMenu.LineHeight),
                    this.ItemMatcher.Contains(tag.name) ? Game1.textColor : Game1.unselectedOptionColor,
                    1f,
                    0.1f);
            }
            else
            {
                b.DrawString(
                    Game1.smallFont,
                    tag.name,
                    new(tag.bounds.X, tag.bounds.Y - this.Offset * ItemSelectionMenu.LineHeight),
                    this.ItemMatcher.Contains(tag.name) ? Game1.textColor : Game1.unselectedOptionColor);
            }
        }
    }

    /// <inheritdoc/>
    public override void performHoverAction(int x, int y)
    {
        this.okButton.scale = this.okButton.containsPoint(x, y)
            ? Math.Min(1.1f, this.okButton.scale + 0.05f)
            : Math.Max(1f, this.okButton.scale - 0.05f);

        var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (cc is not null && int.TryParse(cc.name, out var slotNumber))
        {
            this.hoveredItem = this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber);
            this.hoverText = string.Empty;
            return;
        }

        cc = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y + this.Offset * ItemSelectionMenu.LineHeight));
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
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.okButton.containsPoint(x, y) && this.readyToClose())
        {
            this.exitThisMenu();
            if (Game1.currentLocation.currentEvent is { CurrentCommand: > 0 })
            {
                Game1.currentLocation.currentEvent.CurrentCommand++;
            }

            Game1.playSound("bigDeselect");
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
            //this.AddTag(tag);
            return;
        }

        // Left click an existing tag to remove from filters
        itemSlot = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y + this.Offset * ItemSelectionMenu.LineHeight));
        if (itemSlot is not null && !string.IsNullOrWhiteSpace(itemSlot.name))
        {
            //this.AddTag(itemSlot.name);
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        // Right click an item slot to display dropdown with item's context tags
        if (this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y)) is { } itemSlot
            && int.TryParse(itemSlot.name, out var slotNumber)
            && this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber) is { } item)
        {
            var tags = new HashSet<string>(item.GetContextTags().Where(tag => !(tag.StartsWith("id_") || tag.StartsWith("preserve_"))));

            // Add extra quality levels
            if (tags.Contains("quality_none"))
            {
                tags.Add("quality_silver");
                tags.Add("quality_gold");
                tags.Add("quality_iridium");
            }

            //this.AddTagMenu(tags.ToList(), x, y);
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        var (x, y) = Game1.getMousePosition(true);
        if (!this.inventory.isWithinBounds(x, y))
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
}