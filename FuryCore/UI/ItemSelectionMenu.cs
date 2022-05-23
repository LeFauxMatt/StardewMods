#nullable disable

namespace StardewMods.FuryCore.UI;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Common.Helpers.ItemRepository;
using Common.Models;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

// TODO: Add clear all button

/// <summary>
///     A menu for selecting items.
/// </summary>
public class ItemSelectionMenu : ItemGrabMenu
{
    private const int HorizontalTagSpacing = 10;
    private const int VerticalTagSpacing = 5;
    private int _offset;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemSelectionMenu" /> class.
    /// </summary>
    /// <param name="inputHelper">API for changing input states.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    /// <param name="itemMatcher">Matches items against name and context tags.</param>
    public ItemSelectionMenu(IInputHelper inputHelper, IModServices services, ItemMatcher itemMatcher)
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
            context: new Chest(true))
    {
        if (ItemSelectionMenu.AllItems is null)
        {
            ItemSelectionMenu.AllItems = new(new ItemRepository().GetAll().Select(item => item.Item));
            ItemSelectionMenu.AllTags = ItemSelectionMenu.AllItems
                                                         .SelectMany(item => item.GetContextTags())
                                                         .Where(tag => !(tag.StartsWith("id_") || tag.StartsWith("item_") || tag.StartsWith("preserve_")))
                                                         .Distinct()
                                                         .OrderBy(tag => tag)
                                                         .Select(
                                                             tag =>
                                                             {
                                                                 var (textWidth, textHeight) = Game1.smallFont.MeasureString(tag).ToPoint();
                                                                 return new ClickableComponent(new(0, 0, textWidth, textHeight), tag);
                                                             })
                                                         .ToList();
            ItemSelectionMenu.LineHeight = ItemSelectionMenu.AllTags.Max(tag => tag.bounds.Height) + ItemSelectionMenu.VerticalTagSpacing;
        }

        this.CustomEvents = services.FindService<ICustomEvents>();
        this.MenuItems = services.FindService<IMenuItems>();
        this.InputHelper = inputHelper;
        this.ItemMatcher = itemMatcher;

        this.ItemsToGrabMenu.actualInventory = ItemSelectionMenu.AllItems.ToList();
        this.ItemsToGrabMenu.highlightMethod = this.ItemMatcher.Matches;
        this.RefreshTags();
    }

    private static HashSet<Item> AllItems { get; set; }

    private static IList<ClickableComponent> AllTags { get; set; }

    private static int LineHeight { get; set; }

    private ICustomEvents CustomEvents { get; }

    private IInputHelper InputHelper { get; }

    private ItemMatcher ItemMatcher { get; }

    private IMenuItems MenuItems { get; }

    private int Offset
    {
        get => this._offset;
        set => this._offset = this.Range.Clamp(value);
    }

    private Range<int> Range { get; } = new();

    private DropDownMenu TagMenu { get; set; }

    /// <summary>
    ///     Adds a dropdown menu of context tags.
    /// </summary>
    /// <param name="tags">The context tags to show.</param>
    /// <param name="x">The x-coordinate of the dropdown menu.</param>
    /// <param name="y">The y-coordinate of the dropdown menu.</param>
    /// <returns>True if the new tag menu was added.</returns>
    public bool AddTagMenu(IList<string> tags, int x, int y)
    {
        if (this.TagMenu is not null)
        {
            return false;
        }

        this.TagMenu = new(tags, x, y, this.AddTag);
        return true;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        this.okButton.scale = this.okButton.containsPoint(x, y)
            ? Math.Min(1.1f, this.okButton.scale + 0.05f)
            : Math.Max(1f, this.okButton.scale - 0.05f);

        if (this.TagMenu is not null)
        {
            this.TagMenu.TryHover(x, y);
            this.hoveredItem = null;
            this.hoverText = string.Empty;
            return;
        }

        var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (cc is not null && int.TryParse(cc.name, out var slotNumber))
        {
            this.hoveredItem = this.MenuItems.ActualInventory.ElementAtOrDefault(slotNumber);
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
            this.AddTag(tag);
            return;
        }

        // Left click an existing tag to remove from filters
        itemSlot = this.inventory.inventory.FirstOrDefault(slot => slot.containsPoint(x, y + this.Offset * ItemSelectionMenu.LineHeight));
        if (itemSlot is not null && !string.IsNullOrWhiteSpace(itemSlot.name))
        {
            this.AddTag(itemSlot.name);
        }
    }

    /// <inheritdoc />
    public override void receiveRightClick(int x, int y, bool playSound = true)
    {
        // Right click an item slot to display dropdown with item's context tags
        if (this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y)) is { } itemSlot
            && int.TryParse(itemSlot.name, out var slotNumber)
            && this.MenuItems.ActualInventory.ElementAtOrDefault(slotNumber) is { } item)
        {
            var tags = new HashSet<string>(item.GetContextTags().Where(tag => !(tag.StartsWith("id_") || tag.StartsWith("preserve_"))));

            // Add extra quality levels
            if (tags.Contains("quality_none"))
            {
                tags.Add("quality_silver");
                tags.Add("quality_gold");
                tags.Add("quality_iridium");
            }

            this.AddTagMenu(tags.ToList(), x, y);
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

    /// <summary>
    ///     Allows the <see cref="ItemSelectionMenu" /> to register SMAPI events for handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void RegisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed += this.OnButtonPressed;
        this.MenuItems.MenuItemsChanged += this.OnMenuItemsChanged;
        this.CustomEvents.RenderedClickableMenu += this.OnRenderedClickableMenu;
        this.ItemMatcher.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    ///     Allows the <see cref="ItemSelectionMenu" /> to unregister SMAPI events from handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void UnregisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed -= this.OnButtonPressed;
        this.MenuItems.MenuItemsChanged -= this.OnMenuItemsChanged;
        this.CustomEvents.RenderedClickableMenu -= this.OnRenderedClickableMenu;
        this.ItemMatcher.CollectionChanged -= this.OnCollectionChanged;
    }

    private void AddTag(string tag)
    {
        if (this.InputHelper.IsDown(SButton.LeftShift) || this.InputHelper.IsDown(SButton.RightShift))
        {
            // Inverse Tag
            if (this.ItemMatcher.Contains(tag))
            {
                this.ItemMatcher.Remove(tag);
                this.ItemMatcher.Add(tag.StartsWith("!") ? tag[1..] : $"!{tag}");
                return;
            }

            // Remove
            tag = tag.StartsWith("!") ? tag[1..] : $"!{tag}";
            if (this.ItemMatcher.Contains(tag))
            {
                this.ItemMatcher.Remove(tag);
                return;
            }

            // Add
            this.ItemMatcher.Add(tag);
            return;
        }

        // Remove
        if (this.ItemMatcher.Contains(tag))
        {
            this.ItemMatcher.Remove(tag);
            return;
        }

        // Add
        this.ItemMatcher.Add(tag);
    }

    [EventPriority(EventPriority.High + 1)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (e.IsSuppressed())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);

        switch (e.Button)
        {
            case SButton.Escape when this.readyToClose():
                this.InputHelper.Suppress(e.Button);
                this.exitThisMenu();
                return;

            case SButton.Escape:
                break;

            case SButton.MouseLeft when this.TagMenu is not null:
                this.TagMenu.LeftClick(x, y);
                this.TagMenu = null;
                break;

            case SButton.MouseRight when this.TagMenu is not null:
                this.TagMenu = null;
                break;

            default:
                return;
        }

        this.InputHelper.Suppress(e.Button);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        this.RefreshTags();
        this.MenuItems.ForceRefresh();
    }

    private void OnMenuItemsChanged(object sender, IMenuItemsChangedEventArgs e)
    {
        e.SetSortMethod(this.SortItems);
    }

    private void OnRenderedClickableMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        this.TagMenu?.Draw(e.SpriteBatch);
    }

    private void RefreshTags()
    {
        var tags = ItemSelectionMenu.AllTags.AsEnumerable();
        if (this.ItemMatcher.Any())
        {
            tags = tags.Concat(this.ItemMatcher
                                   .Except(ItemSelectionMenu.AllTags.Select(cc => cc.name))
                                   .Select(
                                       tag =>
                                       {
                                           var (textWidth, textHeight) = Game1.smallFont.MeasureString(tag).ToPoint();
                                           return new ClickableComponent(new(0, 0, textWidth, textHeight), tag);
                                       }))
                       .OrderBy(cc => this.ItemMatcher.Contains(cc.name) ? 0 : 1)
                       .ThenBy(cc => cc.name);
        }

        this.inventory.inventory = tags.ToList();

        var x = this.inventory.xPositionOnScreen;
        var y = this.inventory.yPositionOnScreen;
        var matched = this.ItemMatcher.Any();

        foreach (var tag in this.inventory.inventory)
        {
            if (matched && !this.ItemMatcher.Contains(tag.name))
            {
                matched = false;
                x = this.inventory.xPositionOnScreen;
                y += ItemSelectionMenu.LineHeight;
            }
            else if (x + tag.bounds.Width + ItemSelectionMenu.HorizontalTagSpacing >= this.inventory.xPositionOnScreen + this.inventory.width)
            {
                x = this.inventory.xPositionOnScreen;
                y += ItemSelectionMenu.LineHeight;
            }

            tag.bounds.X = x;
            tag.bounds.Y = y;
            x += tag.bounds.Width + ItemSelectionMenu.HorizontalTagSpacing;
        }

        this.Range.Maximum = (this.inventory.inventory.Max(cc => cc.bounds.Top) - this.inventory.yPositionOnScreen) / ItemSelectionMenu.LineHeight;
        this.Offset = 0;
    }

    private int SortItems(Item item)
    {
        return this.ItemMatcher.Matches(item) ? 0 : 1;
    }
}