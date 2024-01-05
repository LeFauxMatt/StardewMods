namespace StardewMods.BetterChests.Framework.UI;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewValley.Menus;

/// <summary>Represents a complex menu option for categorizing a container of items.</summary>
internal sealed class CategorizeOption : BaseComplexOption
{
    private const int MinimumHeight = 256;
    private const int Rows = 4;
    private const int Columns = 12;
    private const int Space = 64;

    private static readonly Lazy<List<Item>> AllItems = new(
        () =>
        {
            return ItemRegistry
                .ItemTypes.SelectMany(
                    itemType => itemType
                        .GetAllIds()
                        .Select(localId => ItemRegistry.Create(itemType.Identifier + localId)))
                .ToList();
        });

    private readonly List<ClickableComponent> allComponents = [];
    private readonly IGameContentHelper gameContentHelper;
    private readonly IModEvents modEvents;
    private readonly IInputHelper inputHelper;
    private readonly Dictionary<string, InventoryTabData> inventoryTabData;
    private readonly ItemMatcher itemMatcherForFiltering;
    private readonly ItemMatcher itemMatcherForSorting;
    private readonly ClickableTextureComponent upArrow;
    private readonly ClickableTextureComponent downArrow;
    private readonly ITranslationHelper translationHelper;

    private string? hoverText;
    private int indexItems;
    private int indexCategories;
    private int indexQuality;
    private bool initialized;
    private List<Item> items = [];
    private Vector2 lastPos;
    private int offset;
    private HashSet<string> tags = [];

    /// <summary>Initializes a new instance of the <see cref="CategorizeOption" /> class.</summary>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="getInventoryTabData">Function which returns inventory tab data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="translationHelper">Dependency used for accessing translations.</param>
    public CategorizeOption(
        IGameContentHelper gameContentHelper,
        Func<Dictionary<string, InventoryTabData>> getInventoryTabData,
        IModEvents modEvents,
        IInputHelper inputHelper,
        ItemMatcherFactory itemMatcherFactory,
        ITranslationHelper translationHelper)
    {
        this.gameContentHelper = gameContentHelper;
        this.modEvents = modEvents;
        this.inputHelper = inputHelper;
        this.inventoryTabData = getInventoryTabData();
        this.itemMatcherForFiltering = itemMatcherFactory.GetDefault();
        this.itemMatcherForSorting = itemMatcherFactory.GetDefault();
        this.translationHelper = translationHelper;
        this.upArrow = new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom) { myID = 5318009 };

        this.downArrow = new ClickableTextureComponent(
            new Rectangle(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom) { myID = 5318009 };
    }

    /// <inheritdoc />
    public override int Height { get; protected set; } = CategorizeOption.MinimumHeight;

    /// <summary>Initializes the context tags used for categorization.</summary>
    /// <param name="initTags">The list of context tags used for categorization.</param>
    public void Init(HashSet<string> initTags) => this.tags = initTags;

    /// <inheritdoc />
    public override void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        if (!this.initialized)
        {
            this.Init(pos);
        }
        else if (this.lastPos != pos)
        {
            this.Update(pos);
        }

        for (var index = 0; index < this.indexItems; ++index)
        {
            var component = this.allComponents[index];
            spriteBatch.Draw(
                Game1.menuTexture,
                new Vector2(component.bounds.X, component.bounds.Y),
                Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10),
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0.5f);
        }

        var (mouseX, mouseY) = this.inputHelper.GetCursorPosition().GetScaledScreenPixels().ToPoint();

        for (var index = 0; index < this.allComponents.Count; ++index)
        {
            var component = this.allComponents[index];
            var item = this.items.ElementAtOrDefault(index);
            var color = Color.White;
            var transparency = 0.25f;

            // Draw items
            if (index < this.indexItems)
            {
                var isFiltered = item is not null && this.itemMatcherForFiltering.MatchesFilter(item);
                var isSorted = item is not null && this.itemMatcherForSorting.MatchesFilter(item);
                if (isFiltered)
                {
                    transparency = 1f;
                }
                else if (isSorted)
                {
                    transparency = 0.5f;
                }

                item?.drawInMenu(
                    spriteBatch,
                    new Vector2(component.bounds.X, component.bounds.Y),
                    component.containsPoint(mouseX, mouseY) ? 1f : 0.95f,
                    transparency,
                    0.86f + (component.bounds.Y / 20000f),
                    StackDrawType.Hide,
                    color,
                    true);

                if (this.tags.Contains(component.label))
                {
                    var y = component.bounds.Y + component.bounds.Height - 58;
                    spriteBatch.Draw(
                        Game1.mouseCursors,
                        new Vector2(component.bounds.X + component.bounds.Width - 58, y),
                        new Rectangle(107, 442, 7, 8),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        2,
                        SpriteEffects.None,
                        0.86f + (component.bounds.Y / 20000f));

                    continue;
                }

                if (this.tags.Contains("!" + component.label))
                {
                    var y = component.bounds.Y + component.bounds.Height - 58;
                    spriteBatch.Draw(
                        Game1.mouseCursors,
                        new Vector2(component.bounds.X + component.bounds.Width - 58, y),
                        new Rectangle(322, 498, 12, 12),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        2,
                        SpriteEffects.None,
                        0.86f + (component.bounds.Y / 20000f));
                }

                continue;
            }

            // Draw categories
            if (index < this.indexCategories)
            {
                if (component is not ClickableTextureComponent categoryComponent
                    || !this.inventoryTabData.TryGetValue(categoryComponent.name, out var data))
                {
                    continue;
                }

                var tag = string.Join(' ', data.Rules);
                if (this.tags.Contains(tag))
                {
                    transparency = 1f;
                }

                categoryComponent.tryHover(mouseX, mouseY);
                categoryComponent.draw(spriteBatch, color * transparency, 0.86f + (component.bounds.Y / 20000f));

                continue;
            }

            // Draw quality
            if (index < this.indexQuality)
            {
                var quality = index - this.indexCategories + 1;
                quality = quality == 3 ? 4 : quality;

                if (this.tags.Contains(component.label))
                {
                    transparency = 1f;
                }
                else if (this.tags.Contains("!" + component.label))
                {
                    color = Color.Red;
                    transparency = 1f;
                }

                CategorizeOption.DrawQuality(
                    spriteBatch,
                    new Vector2(component.bounds.X, component.bounds.Y),
                    quality,
                    color,
                    transparency,
                    component.containsPoint(mouseX, mouseY) ? 1f : 0.95f);

                continue;
            }

            if (component is not ClickableTextureComponent arrowComponent)
            {
                continue;
            }

            arrowComponent.tryHover(mouseX, mouseY);
            arrowComponent.draw(spriteBatch, Color.White, 0.86f + (component.bounds.Y / 20000f));
        }

        IClickableMenu.drawHoverText(spriteBatch, this.hoverText, Game1.smallFont);
    }

    /// <inheritdoc />
    public override void BeforeMenuOpened()
    {
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.modEvents.Input.CursorMoved += this.OnCursorMoved;
    }

    /// <inheritdoc />
    public override void BeforeMenuClosed()
    {
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.modEvents.Input.CursorMoved -= this.OnCursorMoved;
    }

    private static void DrawQuality(
        SpriteBatch spriteBatch,
        Vector2 pos,
        int quality,
        Color color,
        float transparency,
        float scale)
    {
        var qualityRect =
            quality < 4 ? new Rectangle(338 + ((quality - 1) * 8), 400, 8, 8) : new Rectangle(346, 392, 8, 8);

        var yOffset = quality < 4 || transparency < 1f
            ? 0f
            : ((float)Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f;

        spriteBatch.Draw(
            Game1.mouseCursors,
            pos + new Vector2(12f, 52f + yOffset),
            qualityRect,
            color * transparency,
            0f,
            new Vector2(4f, 4f),
            Game1.pixelZoom * (1f + yOffset) * scale,
            SpriteEffects.None,
            1f);
    }

    private void RefreshItems()
    {
        this.itemMatcherForFiltering.SearchText = string.Join(
            ' ',
            this.tags.Select(
                tag => tag.StartsWith("quality_", StringComparison.OrdinalIgnoreCase) ? "quality_none" : tag));

        this.itemMatcherForSorting.SearchText = string.Join(
            ' ',
            this.tags.Where(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase)));

        this.items = CategorizeOption
            .AllItems.Value.OrderByDescending(this.itemMatcherForSorting.MatchesFilter)
            .ThenByDescending(this.itemMatcherForFiltering.MatchesFilter)
            .Skip(this.offset * CategorizeOption.Columns)
            .Take(this.indexItems + 1)
            .ToList();

        this.upArrow.visible = this.offset > 0;
        this.downArrow.visible = this.items.Count > this.indexItems;

        for (var index = 0; index < CategorizeOption.Rows * CategorizeOption.Columns; ++index)
        {
            var item = this.items[index];
            var tag = item
                .GetContextTags()
                .Where(tag => tag.StartsWith("id_", StringComparison.OrdinalIgnoreCase))
                .MinBy(tag => tag.Contains('('));

            this.allComponents[index].label = tag;
        }
    }

    private void Init(Vector2 pos)
    {
        this.indexItems = CategorizeOption.Rows * CategorizeOption.Columns;
        this.indexCategories = this.indexItems + this.inventoryTabData.Count;
        this.indexQuality = this.indexCategories + 3;
        int index;

        // Add item components
        for (index = 0; index < this.indexItems; ++index)
        {
            this.allComponents.Add(
                new ClickableComponent(
                    new Rectangle(
                        (int)pos.X + (index % CategorizeOption.Columns * CategorizeOption.Space) - 380,
                        (int)pos.Y + (index / CategorizeOption.Columns * CategorizeOption.Space),
                        CategorizeOption.Space,
                        CategorizeOption.Space),
                    index.ToString(CultureInfo.InvariantCulture)));
        }

        // Add category components
        foreach (var (key, data) in this.inventoryTabData)
        {
            this.allComponents.Add(
                new ClickableTextureComponent(
                    key,
                    new Rectangle(
                        (int)pos.X + (index % CategorizeOption.Columns * CategorizeOption.Space) - 380,
                        (int)pos.Y + (index / CategorizeOption.Columns * CategorizeOption.Space),
                        CategorizeOption.Space,
                        CategorizeOption.Space),
                    string.Empty,
                    this.translationHelper.Get($"tab.{data.Name}.Name").Default(data.Name),
                    this.gameContentHelper.Load<Texture2D>(data.Path),
                    new Rectangle(16 * data.Index, 0, 16, 16),
                    Game1.pixelZoom));

            ++index;
        }

        // Add quality components
        this.allComponents.Add(
            new ClickableComponent(
                new Rectangle((int)pos.X - 430, (int)pos.Y, 64, 64),
                "quality_silver",
                "quality_silver"));

        this.allComponents.Add(
            new ClickableComponent(
                new Rectangle((int)pos.X - 430, (int)pos.Y + 64, 64, 64),
                "quality_gold",
                "quality_gold"));

        this.allComponents.Add(
            new ClickableComponent(
                new Rectangle((int)pos.X - 430, (int)pos.Y + 128, 64, 64),
                "quality_iridium",
                "quality_iridium"));

        // Add arrow components
        this.upArrow.bounds.X = (int)pos.X + 400;
        this.upArrow.bounds.Y = (int)pos.Y + 4;
        this.allComponents.Add(this.upArrow);

        this.downArrow.bounds.Y = (int)pos.Y + 208;
        this.downArrow.bounds.X = (int)pos.X + 400;
        this.allComponents.Add(this.downArrow);

        this.initialized = true;
        this.lastPos = pos;
        this.RefreshItems();
    }

    private void Update(Vector2 pos)
    {
        for (var index = 0; index < this.allComponents.Count; ++index)
        {
            var component = this.allComponents[index];

            // Update item components
            if (index < this.indexItems)
            {
                component.bounds = new Rectangle(
                    (int)pos.X + (index % CategorizeOption.Columns * CategorizeOption.Space) - 380,
                    (int)pos.Y + (index / CategorizeOption.Columns * CategorizeOption.Space),
                    CategorizeOption.Space,
                    CategorizeOption.Space);

                continue;
            }

            // Update category components
            if (index < this.indexCategories)
            {
                component.bounds = new Rectangle(
                    (int)pos.X + (index % CategorizeOption.Columns * CategorizeOption.Space) - 380,
                    (int)pos.Y + (index / CategorizeOption.Columns * CategorizeOption.Space) + CategorizeOption.Space,
                    CategorizeOption.Space,
                    CategorizeOption.Space);

                continue;
            }

            // Update quality components
            if (index < this.indexQuality)
            {
                component.bounds.X = (int)pos.X - 430;
                component.bounds.Y = (int)pos.Y + ((index - this.indexCategories) * CategorizeOption.Space);
                continue;
            }

            // Update arrow components
            component.bounds.X = (int)pos.X + 400;
            component.bounds.Y = (int)pos.Y + 4 + ((index - this.indexQuality) * 396);
        }

        this.lastPos = pos;
        this.RefreshItems();
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not (SButton.MouseLeft or SButton.MouseRight))
        {
            return;
        }

        var (mouseX, mouseY) = e.Cursor.GetScaledScreenPixels().ToPoint();
        var component =
            this.allComponents.FirstOrDefault(
                component => component.visible && component.containsPoint(mouseX, mouseY));

        if (component is null)
        {
            return;
        }

        if (component == this.upArrow)
        {
            this.offset--;
            this.RefreshItems();
            return;
        }

        if (component == this.downArrow)
        {
            this.offset++;
            this.RefreshItems();
            return;
        }

        var tag = component.label;
        var tagToAdd = e.Button == SButton.MouseLeft ? tag : "!" + tag;

        if (component is ClickableTextureComponent textureComponent)
        {
            if (!this.inventoryTabData.TryGetValue(textureComponent.hoverText, out var data))
            {
                return;
            }

            tag = string.Join(' ', data.Rules);
            tagToAdd = tag;
        }

        if (this.tags.Contains(tag))
        {
            this.tags.Remove(tag);
            this.RefreshItems();
            return;
        }

        if (this.tags.Contains("!" + tag))
        {
            this.tags.Remove("!" + tag);
            this.RefreshItems();
            return;
        }

        this.tags.Add(tagToAdd);
        this.RefreshItems();
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        var (mouseX, mouseY) = e.NewPosition.GetScaledScreenPixels().ToPoint();
        var component =
            this.allComponents.FirstOrDefault(
                component => component.visible && component.containsPoint(mouseX, mouseY));

        if (component is null)
        {
            this.hoverText = null;
            return;
        }

        if (component is ClickableTextureComponent textureComponent)
        {
            this.hoverText = textureComponent.hoverText;
            return;
        }

        if (int.TryParse(component.name, out var index))
        {
            this.hoverText = this.items[index].DisplayName;
            return;
        }

        this.hoverText = component.label;
    }
}