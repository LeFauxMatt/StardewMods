// namespace StardewMods.BetterChests.Framework.UI;
//
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework.Input;
// using StardewMods.BetterChests.Framework.Services;
// using StardewMods.BetterChests.Framework.Services.Features;
// using StardewMods.BetterChests.Framework.Services.Scoped;
// using StardewValley.Menus;
//
// /// <summary>Menu for selecting <see cref="Item" /> based on their context tags.</summary>
// internal sealed class ItemSelectionMenu : ItemGrabMenu
// {
//     private const int HorizontalTagSpacing = 10;
//     private const int VerticalTagSpacing = 5;
//
//     private static readonly Lazy<List<Item>> ItemsLazy = new(
//         () => new(new ItemRepository().GetAll().Select(item => item.Item)));
//
//     private static readonly IDictionary<string, string> LocalTags = new Dictionary<string, string>();
//
//     private static readonly Lazy<List<ClickableComponent>> TagsLazy = new(
//         () =>
//         {
//             var components = new List<ClickableComponent>();
//             foreach (var tag in ItemSelectionMenu.Items.SelectMany(item => item.GetContextTags()))
//             {
//                 if (ItemSelectionMenu.LocalTags.ContainsKey(tag)
//                     || tag.StartsWith("id_", StringComparison.OrdinalIgnoreCase)
//                     || tag.StartsWith("item_", StringComparison.OrdinalIgnoreCase)
//                     || tag.StartsWith("preserve_", StringComparison.OrdinalIgnoreCase))
//                 {
//                     continue;
//                 }
//
//                 ItemSelectionMenu.LocalTags[tag] = ItemSelectionMenu.translation.Get($"tag.{tag}").Default(tag);
//                 var (tagWidth, tagHeight) = Game1.smallFont.MeasureString(ItemSelectionMenu.LocalTags[tag]).ToPoint();
//                 components.Add(new(new(0, 0, tagWidth, tagHeight), tag));
//             }
//
//             components.Sort(
//                 (t1, t2) => string.Compare(
//                     ItemSelectionMenu.LocalTags[t1.name],
//                     ItemSelectionMenu.LocalTags[t2.name],
//                     StringComparison.OrdinalIgnoreCase));
//
//             return components;
//         });
//
//     private static readonly Lazy<int> LineHeightLazy = new(
//         () => ItemSelectionMenu.AllTags.Max(tag => tag.bounds.Height) + ItemSelectionMenu.VerticalTagSpacing);
//
// #nullable disable
//     private static ITranslationHelper translation;
// #nullable enable
//
//     private readonly DisplayedItems displayedItems;
//     private readonly List<ClickableComponent> displayedTags = new();
//     private readonly IInputHelper input;
//     private readonly HashSet<string> selected;
//     private readonly ItemMatcher selection;
//
//     private DropDownList? dropDown;
//     private int offset;
//     private bool refreshItems;
//     private bool suppressInput = true;
//
//     /// <summary>Initializes a new instance of the <see cref="ItemSelectionMenu" /> class.</summary>
//     /// <param name="context">The source object.</param>
//     /// <param name="matcher">ItemMatcher for holding the selected item tags.</param>
//     /// <param name="input">SMAPI helper for input.</param>
//     /// <param name="translation">Translations from the i18n folder.</param>
//     public ItemSelectionMenu(object? context, ItemMatcher matcher, IInputHelper input, ITranslationHelper translation)
//         : base(
//             new List<Item>(),
//             false,
//             true,
//             null,
//             (_, _) => { },
//             null,
//             (_, _) => { },
//             canBeExitedWithKey: false,
//             source: ItemGrabMenu.source_none,
//             context: context)
//     {
//         ItemSelectionMenu.translation ??= translation;
//         this.input = input;
//         this.selected = new(matcher);
//         this.selection = matcher;
//         this.ItemsToGrabMenu.actualInventory = ItemSelectionMenu.Items;
//         this.displayedItems = BetterItemGrabMenu.ItemsToGrabMenu!;
//         this.displayedItems.AddHighlighter(this.selection);
//         this.displayedItems.AddTransformer(this.SortBySelection);
//         this.displayedItems.ItemsRefreshed += this.OnItemsRefreshed;
//         this.displayedItems.RefreshItems();
//     }
//
//     private static List<ClickableComponent> AllTags => ItemSelectionMenu.TagsLazy.Value;
//
//     private static List<Item> Items => ItemSelectionMenu.ItemsLazy.Value;
//
//     private static int LineHeight => ItemSelectionMenu.LineHeightLazy.Value;
//
//     /// <inheritdoc />
//     public override void draw(SpriteBatch b)
//     {
//         b.Draw(
//             Game1.fadeToBlackRect,
//             new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
//             Color.Black * 0.5f);
//
//         BetterItemGrabMenu.InvokeDrawingMenu(b);
//
//         var x = this.ItemsToGrabMenu.xPositionOnScreen
//             - IClickableMenu.borderWidth
//             - IClickableMenu.spaceToClearSideBorder;
//
//         var y = this.ItemsToGrabMenu.yPositionOnScreen
//             - IClickableMenu.borderWidth
//             - IClickableMenu.spaceToClearTopBorder
//             - 24;
//
//         var boxWidth = this.ItemsToGrabMenu.width
//             + (IClickableMenu.borderWidth * 2)
//             + (IClickableMenu.spaceToClearSideBorder * 2);
//
//         var boxHeight = this.ItemsToGrabMenu.height
//             + IClickableMenu.spaceToClearTopBorder
//             + (IClickableMenu.borderWidth * 2)
//             + 24;
//
//         Game1.drawDialogueBox(x, y, boxWidth, boxHeight, false, true);
//
//         x = this.inventory.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder;
//         y = (this.inventory.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder) + 24;
//         boxWidth = this.inventory.width
//             + (IClickableMenu.borderWidth * 2)
//             + (IClickableMenu.spaceToClearSideBorder * 2);
//
//         boxHeight = (this.inventory.height + IClickableMenu.spaceToClearTopBorder + (IClickableMenu.borderWidth * 2))
//             - 24;
//
//         Game1.drawDialogueBox(x, y, boxWidth, boxHeight, false, true);
//
//         this.ItemsToGrabMenu.draw(b);
//         this.okButton.draw(b);
//
//         foreach (var tag in this.displayedTags.Where(
//             cc => this.inventory.isWithinBounds(
//                 cc.bounds.X,
//                 cc.bounds.Bottom - (this.offset * ItemSelectionMenu.LineHeight))))
//         {
//             var localTag = ItemSelectionMenu.translation!.Get($"tag.{tag.name}").Default(tag.name);
//             var color = !this.selected.Contains(tag.name)
//                 ? Game1.unselectedOptionColor
//                 : tag.name[..1] == "!"
//                     ? Color.DarkRed
//                     : Game1.textColor;
//
//             if (this.hoverText == tag.name)
//             {
//                 Utility.drawTextWithShadow(
//                     b,
//                     localTag,
//                     Game1.smallFont,
//                     new(tag.bounds.X, tag.bounds.Y - (this.offset * ItemSelectionMenu.LineHeight)),
//                     color,
//                     1f,
//                     0.1f);
//             }
//             else
//             {
//                 b.DrawString(
//                     Game1.smallFont,
//                     localTag,
//                     new(tag.bounds.X, tag.bounds.Y - (this.offset * ItemSelectionMenu.LineHeight)),
//                     color);
//             }
//         }
//
//         this.drawMouse(b);
//     }
//
//     /// <inheritdoc />
//     public override void performHoverAction(int x, int y)
//     {
//         if (this.suppressInput)
//         {
//             return;
//         }
//
//         this.okButton.scale = this.okButton.containsPoint(x, y)
//             ? Math.Min(1.1f, this.okButton.scale + 0.05f)
//             : Math.Max(1f, this.okButton.scale - 0.05f);
//
//         if (this.ItemsToGrabMenu.isWithinBounds(x, y))
//         {
//             var cc = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
//             if (cc is not null && int.TryParse(cc.name, out var slotNumber))
//             {
//                 this.hoveredItem = this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber);
//                 this.hoverText = string.Empty;
//                 return;
//             }
//         }
//
//         if (this.inventory.isWithinBounds(x, y))
//         {
//             var cc = this.displayedTags.FirstOrDefault(
//                 slot => slot.containsPoint(x, y + (this.offset * ItemSelectionMenu.LineHeight)));
//
//             if (cc is not null)
//             {
//                 this.hoveredItem = null;
//                 this.hoverText = cc.name ?? string.Empty;
//                 return;
//             }
//         }
//
//         this.hoveredItem = null;
//         this.hoverText = string.Empty;
//     }
//
//     /// <inheritdoc />
//     public override void receiveLeftClick(int x, int y, bool playSound = true)
//     {
//         if (this.suppressInput)
//         {
//             return;
//         }
//
//         if (this.okButton.containsPoint(x, y) && this.readyToClose())
//         {
//             this.exitThisMenu();
//             if (Game1.currentLocation.currentEvent is
//                 {
//                     CurrentCommand: > 0,
//                 })
//             {
//                 ++Game1.currentLocation.currentEvent.CurrentCommand;
//             }
//
//             Game1.playSound("bigDeSelect");
//             return;
//         }
//
//         // Left click an item slot to add individual item tag to filters
//         var itemSlot = this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
//         if (itemSlot is not null
//             && int.TryParse(itemSlot.name, out var slotNumber)
//             && this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber) is
//                 { } item
//             && item.GetContextTagsExt()
//                     .FirstOrDefault(contextTag => contextTag.StartsWith("item_", StringComparison.OrdinalIgnoreCase)) is
//                 { } tag
//             && !string.IsNullOrWhiteSpace(tag))
//         {
//             this.AddTag(tag);
//             return;
//         }
//
//         // Left click a tag on bottom menu
//         itemSlot = this.displayedTags.FirstOrDefault(
//             slot => slot.containsPoint(x, y + (this.offset * ItemSelectionMenu.LineHeight)));
//
//         if (itemSlot is not null && !string.IsNullOrWhiteSpace(itemSlot.name))
//         {
//             this.AddOrRemoveTag(itemSlot.name);
//         }
//     }
//
//     /// <inheritdoc />
//     public override void receiveRightClick(int x, int y, bool playSound = true)
//     {
//         if (this.suppressInput)
//         {
//             return;
//         }
//
//         // Right click an item slot to display dropdown with item's context tags
//         if (this.ItemsToGrabMenu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y)) is not
//                 { } itemSlot
//             || !int.TryParse(itemSlot.name, out var slotNumber)
//             || this.ItemsToGrabMenu.actualInventory.ElementAtOrDefault(slotNumber) is not
//                 { } item)
//         {
//             return;
//         }
//
//         var tags = new HashSet<string>(
//             item.GetContextTagsExt()
//                 .Where(
//                     tag => !(tag.StartsWith("id_", StringComparison.OrdinalIgnoreCase)
//                         || tag.StartsWith("preserve_", StringComparison.OrdinalIgnoreCase))));
//
//         // Add extra quality levels
//         if (tags.Contains("quality_none"))
//         {
//             tags.Add("quality_silver");
//             tags.Add("quality_gold");
//             tags.Add("quality_iridium");
//         }
//
//         if (this.dropDown is not null)
//         {
//             BetterItemGrabMenu.RemoveOverlay();
//         }
//
//         this.dropDown = new(tags.ToList(), x, y, this.Callback, ItemSelectionMenu.translation!);
//         BetterItemGrabMenu.AddOverlay(this.dropDown);
//     }
//
//     /// <inheritdoc />
//     public override void receiveScrollWheelAction(int direction)
//     {
//         if (this.suppressInput)
//         {
//             return;
//         }
//
//         var (x, y) = Game1.getMousePosition(true);
//         if (!this.inventory.isWithinBounds(x, y))
//         {
//             return;
//         }
//
//         switch (direction)
//         {
//             case > 0 when this.offset >= 1:
//                 --this.offset;
//                 return;
//             case < 0 when this.displayedTags.Last().bounds.Bottom
//                 - (this.offset * ItemSelectionMenu.LineHeight)
//                 - this.inventory.yPositionOnScreen
//                 >= this.inventory.height:
//                 ++this.offset;
//                 return;
//             default:
//                 base.receiveScrollWheelAction(direction);
//                 return;
//         }
//     }
//
//     /// <inheritdoc />
//     public override void update(GameTime time)
//     {
//         if (this.suppressInput
//             && (this._parentMenu is null
//                 || (Game1.oldMouseState.LeftButton is ButtonState.Pressed
//                     && Mouse.GetState().LeftButton is ButtonState.Released)))
//         {
//             this.suppressInput = false;
//         }
//
//         if (this.refreshItems)
//         {
//             this.refreshItems = false;
//             foreach (var tag in this.selected.Where(tag => !ItemSelectionMenu.LocalTags.ContainsKey(tag)))
//             {
//                 if (tag[..1] == "!")
//                 {
//                     ItemSelectionMenu.LocalTags[tag] =
//                         "!" + ItemSelectionMenu.translation.Get($"tag.{tag[1..]}").Default(tag);
//                 }
//                 else
//                 {
//                     ItemSelectionMenu.LocalTags[tag] = ItemSelectionMenu.translation.Get($"tag.{tag}").Default(tag);
//                 }
//
//                 var (tagWidth, tagHeight) = Game1.smallFont.MeasureString(ItemSelectionMenu.LocalTags[tag]).ToPoint();
//                 ItemSelectionMenu.AllTags.Add(new(new(0, 0, tagWidth, tagHeight), tag));
//             }
//
//             this.displayedTags.Clear();
//             this.displayedTags.AddRange(
//                 ItemSelectionMenu.AllTags.Where(
//                     tag => (this.selected.Any() && this.selected.Contains(tag.name))
//                         || (tag.name[..1] != "!"
//                             && !this.selected.Contains($"!{tag.name}")
//                             && this.displayedItems.Items.Any(item => item.HasContextTag(tag.name)))));
//
//             this.displayedTags.Sort(
//                 (t1, t2) =>
//                 {
//                     var s1 = this.selected.Contains(t1.name);
//                     var s2 = this.selected.Contains(t2.name);
//                     var strA = ItemSelectionMenu.LocalTags[t1.name][..1] == "!"
//                         ? ItemSelectionMenu.LocalTags[t1.name][1..]
//                         : ItemSelectionMenu.LocalTags[t1.name];
//
//                     var strB = ItemSelectionMenu.LocalTags[t2.name][..1] == "!"
//                         ? ItemSelectionMenu.LocalTags[t2.name][1..]
//                         : ItemSelectionMenu.LocalTags[t2.name];
//
//                     return s1 switch
//                     {
//                         true when !s2 => -1,
//                         false when s2 => 1,
//                         _ => string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase),
//                     };
//                 });
//
//             var x = this.inventory.xPositionOnScreen;
//             var y = this.inventory.yPositionOnScreen;
//             var matched = this.selection.Any();
//
//             foreach (var tag in this.displayedTags)
//             {
//                 if (matched && !this.selected.Contains(tag.name))
//                 {
//                     matched = false;
//                     x = this.inventory.xPositionOnScreen;
//                     y += ItemSelectionMenu.LineHeight;
//                 }
//                 else if (x + tag.bounds.Width + ItemSelectionMenu.HorizontalTagSpacing
//                     >= this.inventory.xPositionOnScreen + this.inventory.width)
//                 {
//                     x = this.inventory.xPositionOnScreen;
//                     y += ItemSelectionMenu.LineHeight;
//                 }
//
//                 tag.bounds.X = x;
//                 tag.bounds.Y = y;
//                 x += tag.bounds.Width + ItemSelectionMenu.HorizontalTagSpacing;
//             }
//         }
//
//         if (this.selected.SetEquals(this.selection))
//         {
//             return;
//         }
//
//         var added = this.selected.Except(this.selection).ToList();
//         var removed = this.selection.Except(this.selected).ToList();
//         foreach (var tag in added)
//         {
//             this.selection.Add(tag);
//         }
//
//         foreach (var tag in removed)
//         {
//             this.selection.Remove(tag);
//         }
//
//         this.displayedItems.RefreshItems();
//     }
//
//     private void AddOrRemoveTag(string tag)
//     {
//         var oppositeTag = tag[..1] == "!" ? tag[1..] : $"!{tag}";
//         if (this.input.IsDown(SButton.LeftShift) || this.input.IsDown(SButton.RightShift))
//         {
//             (tag, oppositeTag) = (oppositeTag, tag);
//         }
//
//         if (this.selected.Contains(oppositeTag))
//         {
//             this.selected.Remove(oppositeTag);
//         }
//
//         if (this.selected.Contains(tag))
//         {
//             this.selected.Remove(tag);
//         }
//         else
//         {
//             this.selected.Add(tag);
//         }
//     }
//
//     private void AddTag(string tag)
//     {
//         var oppositeTag = tag[..1] == "!" ? tag[1..] : $"!{tag}";
//         if (this.input.IsDown(SButton.LeftShift) || this.input.IsDown(SButton.RightShift))
//         {
//             (tag, oppositeTag) = (oppositeTag, tag);
//         }
//
//         if (this.selected.Contains(oppositeTag))
//         {
//             this.selected.Remove(oppositeTag);
//         }
//
//         this.selected.Add(tag);
//     }
//
//     private void Callback(string? tag)
//     {
//         if (tag is not null)
//         {
//             this.AddTag(tag);
//         }
//
//         BetterItemGrabMenu.RemoveOverlay();
//         this.dropDown = null;
//     }
//
//     private void OnItemsRefreshed(object? sender, List<Item> items) => this.refreshItems = true;
//
//     private IEnumerable<Item> SortBySelection(IEnumerable<Item> items) =>
//         this.selection.Any() ? items.OrderBy(item => this.selection.MatchesFilter(item) ? 0 : 1) : items;
// }


