namespace StardewMods.FuryCore.UI;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Common.Helpers.ItemRepository;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     A menu for selecting items.
/// </summary>
public class ItemSelectionMenu : ItemGrabMenu
{
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
        ItemSelectionMenu.AllItems ??= new ItemRepository().GetAll().ToList();
        this.CustomEvents = services.FindService<ICustomEvents>();
        this.MenuItems = services.FindService<IMenuItems>();
        this.ItemsToGrabMenu.actualInventory = ItemSelectionMenu.AllItems.Select(item => item.Item).ToList();
        this.InputHelper = inputHelper;
        this.ItemMatcher = itemMatcher;
        this.ItemsToGrabMenu.highlightMethod = this.ItemMatcher.Matches;
        this.MenuItems.AddSortMethod(this.SortItems);
        this.RefreshTags();
    }

    private static IList<SearchableItem> AllItems { get; set; }

    private ICustomEvents CustomEvents { get; }

    private IInputHelper InputHelper { get; }

    private ItemMatcher ItemMatcher { get; }

    private IMenuItems MenuItems { get; }

    private DropDownMenu TagMenu { get; set; }

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
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        this.okButton.scale = this.okButton.containsPoint(x, y)
            ? Math.Min(1.1f, this.okButton.scale + 0.05f)
            : Math.Max(1f, this.okButton.scale - 0.05f);

        if (this.TagMenu is not null)
        {
            this.TagMenu?.TryHover(x, y);
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
            && this.MenuItems.ActualInventory.ElementAtOrDefault(slotNumber) is { } item)
        {
            var tags = new HashSet<string>(item.GetContextTags().Where(tag => !tag.StartsWith("id_")));

            // Add extra quality levels
            if (tags.Contains("quality_none"))
            {
                tags.Add("quality_silver");
                tags.Add("quality_gold");
                tags.Add("quality_iridium");
            }

            this.TagMenu = new(tags.ToList(), x, y, this.AddTag);
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
                this.MenuItems.Offset--;
                return;
            case < 0:
                this.MenuItems.Offset++;
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
        this.CustomEvents.RenderedItemGrabMenu += this.OnRenderedItemGrabMenu;
        this.ItemMatcher.CollectionChanged += this.OnCollectionChanged;
    }

    /// <summary>
    ///     Allows the <see cref="ItemSelectionMenu" /> to unregister SMAPI events from handling input.
    /// </summary>
    /// <param name="inputEvents">Events raised for player inputs.</param>
    public void UnregisterEvents(IInputEvents inputEvents)
    {
        inputEvents.ButtonPressed -= this.OnButtonPressed;
        this.CustomEvents.RenderedItemGrabMenu -= this.OnRenderedItemGrabMenu;
        this.ItemMatcher.CollectionChanged -= this.OnCollectionChanged;
    }

    private void AddTag(string tag)
    {
        if (this.InputHelper.IsDown(SButton.LeftShift) || this.InputHelper.IsDown(SButton.RightShift))
        {
            tag = $"!{tag}";
        }

        this.ItemMatcher.Add(tag);
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

    private void OnRenderedItemGrabMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        this.TagMenu?.Draw(e.SpriteBatch);
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

    private int SortItems(Item item)
    {
        return this.ItemMatcher.Matches(item) ? 0 : 1;
    }
}