namespace StardewMods.BetterChests.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Helpers.ItemRepository;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;

/// <summary>
///     Menu for selecting <see cref="Item" /> based on their context tags.
/// </summary>
internal class ItemSelectionMenu : ItemGrabMenu
{
    private const int HorizontalTagSpacing = 10;
    private const int VerticalTagSpacing = 5;

    private static readonly Lazy<List<Item>> ItemsLazy = new(
        () => new(from item in new ItemRepository().GetAll() select item.Item));

    private static readonly Lazy<List<ClickableComponent>> TagsLazy = new(
        () => (
                from item in ItemSelectionMenu.Items
                from tag in item.GetContextTagsExt()
                where !tag.StartsWith("id_") && !tag.StartsWith("item_") && !tag.StartsWith("preserve_")
                orderby tag
                select tag).Distinct()
                           .Select(
                               tag =>
                               {
                                   var localTag = ItemSelectionMenu.Translation!.Get($"tag.{tag}").Default(tag);
                                   var (tagWidth, tagHeight) = Game1.smallFont.MeasureString(localTag).ToPoint();
                                   return new ClickableComponent(new(0, 0, tagWidth, tagHeight), tag);
                               })
                           .ToList());

    private static readonly Lazy<int> LineHeightLazy = new(
        () => ItemSelectionMenu.AllTags.Max(tag => tag.bounds.Height) + ItemSelectionMenu.VerticalTagSpacing);


    private static ITranslationHelper? Translation;

    private readonly DisplayedItems _displayedItems;
    private readonly List<ClickableComponent> _displayedTags = new();
    private readonly IInputHelper _input;
    private readonly HashSet<string> _selected;
    private readonly IItemMatcher _selection;

    private DropDownList? _dropDown;
    private int _offset;
    private bool _refreshItems;
    private bool _suppressInput = true;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemSelectionMenu" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="matcher">ItemMatcher for holding the selected item tags.</param>
    /// <param name="input">SMAPI helper for input.</param>
    /// <param name="translation">Translations from the i18n folder.</param>
    public ItemSelectionMenu(object? context, IItemMatcher matcher, IInputHelper input, ITranslationHelper translation)
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
        ItemSelectionMenu.Translation ??= translation;
        this._input = input;
        this._selected = new(matcher);
        this._selection = matcher;
        this.ItemsToGrabMenu.actualInventory = ItemSelectionMenu.Items;
        this._displayedItems = BetterItemGrabMenu.ItemsToGrabMenu!;
        this._displayedItems.AddHighlighter(this._selection);
        this._displayedItems.AddTransformer(this.SortBySelection);
        this._displayedItems.ItemsRefreshed += this.OnItemsRefreshed;
        this._displayedItems.RefreshItems();
    }

    private static List<ClickableComponent> AllTags => ItemSelectionMenu.TagsLazy.Value;

    private static List<Item> Items => ItemSelectionMenu.ItemsLazy.Value;

    private static int LineHeight => ItemSelectionMenu.LineHeightLazy.Value;

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        b.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

        BetterItemGrabMenu.InvokeDrawingMenu(b);

        Game1.drawDialogueBox(
            this.ItemsToGrabMenu.xPositionOnScreen
          - ItemSelectionMenu.borderWidth
          - ItemSelectionMenu.spaceToClearSideBorder,
            this.ItemsToGrabMenu.yPositionOnScreen
          - ItemSelectionMenu.borderWidth
          - ItemSelectionMenu.spaceToClearTopBorder
          - 24,
            this.ItemsToGrabMenu.width
          + ItemSelectionMenu.borderWidth * 2
          + ItemSelectionMenu.spaceToClearSideBorder * 2,
            this.ItemsToGrabMenu.height
          + ItemSelectionMenu.spaceToClearTopBorder
          + ItemSelectionMenu.borderWidth * 2
          + 24,
            false,
            true);

        Game1.drawDialogueBox(
            this.inventory.xPositionOnScreen - ItemSelectionMenu.borderWidth - ItemSelectionMenu.spaceToClearSideBorder,
            this.inventory.yPositionOnScreen
          - ItemSelectionMenu.borderWidth
          - ItemSelectionMenu.spaceToClearTopBorder
          + 24,
            this.inventory.width + ItemSelectionMenu.borderWidth * 2 + ItemSelectionMenu.spaceToClearSideBorder * 2,
            this.inventory.height + ItemSelectionMenu.spaceToClearTopBorder + ItemSelectionMenu.borderWidth * 2 - 24,
            false,
            true);

        this.ItemsToGrabMenu.draw(b);
        this.okButton.draw(b);

        foreach (var tag in this._displayedTags.Where(
                     cc => this.inventory.isWithinBounds(
                         cc.bounds.X,
                         cc.bounds.Bottom - this._offset * ItemSelectionMenu.LineHeight)))
        {
            var localTag = ItemSelectionMenu.Translation!.Get($"tag.{tag.name}").Default(tag.name);
            if (this.hoverText == tag.name)
            {
                Utility.drawTextWithShadow(
                    b,
                    localTag,
                    Game1.smallFont,
                    new(tag.bounds.X, tag.bounds.Y - this._offset * ItemSelectionMenu.LineHeight),
                    this._selected.Contains(tag.name) ? Game1.textColor : Game1.unselectedOptionColor,
                    1f,
                    0.1f);
            }
            else
            {
                b.DrawString(
                    Game1.smallFont,
                    localTag,
                    new(tag.bounds.X, tag.bounds.Y - this._offset * ItemSelectionMenu.LineHeight),
                    this._selected.Contains(tag.name) ? Game1.textColor : Game1.unselectedOptionColor);
            }
        }

        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        if (this._suppressInput)
        {
            return;
        }

        this.okButton.scale = this.okButton.containsPoint(x, y)
            ? Math.Min(1.1f, this.okButton.scale + 0.05f)
            : Math.Max(1f, this.okButton.scale - 0.05f);

        if (this.ItemsToGrabMenu.isWithinBounds(x, y))
        {
            var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
            if (cc is not null && int.TryParse(cc.name, out var slotNumber))
            {
                this.hoveredItem = this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber);
                this.hoverText = string.Empty;
                return;
            }
        }

        if (this.inventory.isWithinBounds(x, y))
        {
            var cc = this._displayedTags.FirstOrDefault(
                slot => slot.containsPoint(x, y + this._offset * ItemSelectionMenu.LineHeight));
            if (cc is not null)
            {
                this.hoveredItem = null;
                this.hoverText = cc.name ?? string.Empty;
                return;
            }
        }

        this.hoveredItem = null;
        this.hoverText = string.Empty;
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this._suppressInput)
        {
            return;
        }

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
         && item.GetContextTagsExt().FirstOrDefault(contextTag => contextTag.StartsWith("item_")) is { } tag
         && !string.IsNullOrWhiteSpace(tag))
        {
            this.AddTag(tag);
            return;
        }

        // Left click a tag on bottom menu
        itemSlot = this._displayedTags.FirstOrDefault(
            slot => slot.containsPoint(x, y + this._offset * ItemSelectionMenu.LineHeight));
        if (itemSlot is not null && !string.IsNullOrWhiteSpace(itemSlot.name))
        {
            this.AddOrRemoveTag(itemSlot.name);
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        if (this._suppressInput)
        {
            return;
        }

        // Right click an item slot to display dropdown with item's context tags
        if (this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y)) is not { } itemSlot
         || !int.TryParse(itemSlot.name, out var slotNumber)
         || this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber) is not { } item)
        {
            return;
        }

        var tags = new HashSet<string>(
            item.GetContextTagsExt().Where(tag => !(tag.StartsWith("id_") || tag.StartsWith("preserve_"))));

        // Add extra quality levels
        if (tags.Contains("quality_none"))
        {
            tags.Add("quality_silver");
            tags.Add("quality_gold");
            tags.Add("quality_iridium");
        }

        if (this._dropDown is not null)
        {
            BetterItemGrabMenu.RemoveOverlay();
        }

        this._dropDown = new(tags.ToList(), x, y, this.Callback, ItemSelectionMenu.Translation!);
        BetterItemGrabMenu.AddOverlay(this._dropDown);
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        if (this._suppressInput)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.inventory.isWithinBounds(x, y))
        {
            return;
        }

        switch (direction)
        {
            case > 0 when this._offset >= 1:
                this._offset--;
                return;
            case < 0 when this._displayedTags.Last().bounds.Bottom
                        - this._offset * ItemSelectionMenu.LineHeight
                        - this.inventory.yPositionOnScreen
                       >= this.inventory.height:
                this._offset++;
                return;
            default:
                base.receiveScrollWheelAction(direction);
                return;
        }
    }

    /// <inheritdoc />
    public override void update(GameTime time)
    {
        if (this._suppressInput
         && (this._parentMenu is null
          || (Game1.oldMouseState.LeftButton is ButtonState.Pressed
           && Mouse.GetState().LeftButton is ButtonState.Released)))
        {
            this._suppressInput = false;
        }

        if (this._refreshItems)
        {
            this._refreshItems = false;
            foreach (var tag in this._selected.Where(tag => !ItemSelectionMenu.AllTags.Any(cc => cc.name.Equals(tag))))
            {
                var localTag = ItemSelectionMenu.Translation!.Get($"tag.{tag}").Default(tag);
                var (textWidth, textHeight) = Game1.smallFont.MeasureString(localTag).ToPoint();
                ItemSelectionMenu.AllTags.Add(new(new(0, 0, textWidth, textHeight), tag));
            }

            this._displayedTags.Clear();
            this._displayedTags.AddRange(
                this._selected.Any()
                    ?
                    from tag in ItemSelectionMenu.AllTags
                    where this._selected.Contains(tag.name)
                       || (tag.name[..1] != "!" && this._displayedItems.Items.Any(item => item.HasContextTag(tag.name)))
                    orderby this._selected.Contains(tag.name) ? 0 : 1, tag.name[..1] == "!" ? tag.name[1..] : tag.name
                    select tag
                    :
                    from tag in ItemSelectionMenu.AllTags
                    where tag.name[..1] != "!" && this._displayedItems.Items.Any(item => item.HasContextTag(tag.name))
                    orderby tag.name
                    select tag);
            var x = this.inventory.xPositionOnScreen;
            var y = this.inventory.yPositionOnScreen;
            var matched = this._selection.Any();

            foreach (var tag in this._displayedTags)
            {
                if (matched && !this._selected.Contains(tag.name))
                {
                    matched = false;
                    x = this.inventory.xPositionOnScreen;
                    y += ItemSelectionMenu.LineHeight;
                }
                else if (x + tag.bounds.Width + ItemSelectionMenu.HorizontalTagSpacing
                      >= this.inventory.xPositionOnScreen + this.inventory.width)
                {
                    x = this.inventory.xPositionOnScreen;
                    y += ItemSelectionMenu.LineHeight;
                }

                tag.bounds.X = x;
                tag.bounds.Y = y;
                x += tag.bounds.Width + ItemSelectionMenu.HorizontalTagSpacing;
            }
        }

        if (this._selected.SetEquals(this._selection))
        {
            return;
        }

        var added = this._selected.Except(this._selection).ToList();
        var removed = this._selection.Except(this._selected).ToList();
        foreach (var tag in added)
        {
            this._selection.Add(tag);
        }

        foreach (var tag in removed)
        {
            this._selection.Remove(tag);
        }

        this._displayedItems.RefreshItems();
    }

    private void AddOrRemoveTag(string tag)
    {
        var oppositeTag = tag[..1] == "!" ? tag[1..] : $"!{tag}";
        if (this._input.IsDown(SButton.LeftShift) || this._input.IsDown(SButton.RightShift))
        {
            (tag, oppositeTag) = (oppositeTag, tag);
        }

        if (this._selected.Contains(oppositeTag))
        {
            this._selected.Remove(oppositeTag);
        }

        if (this._selected.Contains(tag))
        {
            this._selected.Remove(tag);
        }
        else
        {
            this._selected.Add(tag);
        }
    }

    private void AddTag(string tag)
    {
        var oppositeTag = tag[..1] == "!" ? tag[1..] : $"!{tag}";
        if (this._input.IsDown(SButton.LeftShift) || this._input.IsDown(SButton.RightShift))
        {
            (tag, oppositeTag) = (oppositeTag, tag);
        }

        if (this._selected.Contains(oppositeTag))
        {
            this._selected.Remove(oppositeTag);
        }

        if (!this._selected.Contains(tag))
        {
            this._selected.Add(tag);
        }
    }

    private void Callback(string? tag)
    {
        if (tag is not null)
        {
            this.AddTag(tag);
        }

        BetterItemGrabMenu.RemoveOverlay();
        this._dropDown = null;
    }

    private void OnItemsRefreshed(object? sender, List<Item> items)
    {
        this._refreshItems = true;
    }

    private IEnumerable<Item> SortBySelection(IEnumerable<Item> items)
    {
        return this._selection.Any() ? items.OrderBy(item => this._selection.Matches(item) ? 0 : 1) : items;
    }
}