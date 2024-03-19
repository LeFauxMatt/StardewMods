namespace StardewMods.ToolbarIcons.Framework.UI;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewValley.Menus;

/// <summary><see cref="IClickableMenu" /> for configuring Toolbar Icons.</summary>
internal sealed class ToolbarIconsMenu : IClickableMenu
{
    private int index;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsMenu" /> class.</summary>
    /// <param name="icons">The Toolbar Icons to configure.</param>
    /// <param name="components">The <see cref="ClickableTextureComponent" /> of the Toolbar Icons.</param>
    public ToolbarIconsMenu(List<ToolbarIcon> icons, Dictionary<string, ClickableTextureComponent> components)
        : base(
            ((Game1.uiViewport.Width - 800) / 2) - IClickableMenu.borderWidth,
            ((Game1.uiViewport.Height - 600) / 2) - IClickableMenu.borderWidth,
            800 + (IClickableMenu.borderWidth * 2),
            600 + (IClickableMenu.borderWidth * 2),
            true)
    {
        this.Icons = icons;
        var textBounds = components
            .Values.Select(component => Game1.dialogueFont.MeasureString(component.hoverText))
            .ToList();

        this.TextHeight = textBounds.Max(textBound => textBound.Y);
        this.MaxItems = (int)(this.height / this.TextHeight);
        foreach (var icon in this.Icons)
        {
            if (components.TryGetValue(icon.Id, out var component))
            {
                this.Components.Add(
                    new ClickableTextureComponent(
                        new Rectangle(
                            this.xPositionOnScreen + (Game1.tileSize * 2),
                            (int)(this.yPositionOnScreen + (this.Components.Count * this.TextHeight)),
                            32,
                            32),
                        component.texture,
                        component.sourceRect,
                        component.scale)
                    {
                        hoverText = component.hoverText,
                        name = component.name,
                    });
            }
        }

        this.Arrows = Game1.content.Load<Texture2D>("furyx639.ToolbarIcons/Arrows");

        this.OkButton = new ClickableTextureComponent(
            "OK",
            new Rectangle(
                this.xPositionOnScreen + this.width + Game1.tileSize - 8,
                this.yPositionOnScreen + this.height - Game1.tileSize,
                Game1.tileSize,
                Game1.tileSize),
            null,
            null,
            Game1.mouseCursors,
            Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46),
            1f);

        this.UpArrow = new ClickableTextureComponent(
            new Rectangle(this.xPositionOnScreen + this.width + Game1.tileSize, this.yPositionOnScreen + 16, 44, 48),
            Game1.mouseCursors,
            new Rectangle(421, 459, 11, 12),
            Game1.pixelZoom)
        {
            myID = 97865,
            downNeighborID = 106,
            leftNeighborID = 3546,
        };

        this.DownArrow = new ClickableTextureComponent(
            new Rectangle(this.UpArrow.bounds.X, this.yPositionOnScreen + this.height - (Game1.tileSize * 2), 44, 48),
            Game1.mouseCursors,
            new Rectangle(421, 472, 11, 12),
            Game1.pixelZoom)
        {
            myID = 106,
            upNeighborID = 97865,
            leftNeighborID = 3546,
        };

        this.ScrollBar = new ClickableTextureComponent(
            new Rectangle(this.UpArrow.bounds.X + 12, this.UpArrow.bounds.Y + this.UpArrow.bounds.Height + 4, 24, 40),
            Game1.mouseCursors,
            new Rectangle(435, 463, 6, 10),
            Game1.pixelZoom);

        this.ScrollBarRunner = new Rectangle(
            this.ScrollBar.bounds.X,
            this.UpArrow.bounds.Y + this.UpArrow.bounds.Height + 4,
            this.ScrollBar.bounds.Width,
            this.height - this.UpArrow.bounds.Height - (Game1.tileSize * 2) - 28);
    }

    private Texture2D Arrows { get; }

    private List<ClickableTextureComponent> Components { get; } = new();

    private ClickableTextureComponent DownArrow { get; }

    private List<ToolbarIcon> Icons { get; }

    private int MaxItems { get; }

    private ClickableTextureComponent OkButton { get; }

    private ClickableTextureComponent ScrollBar { get; }

    private Rectangle ScrollBarRunner { get; }

    private float TextHeight { get; }

    private ClickableTextureComponent UpArrow { get; }

    private string HoverText { get; set; } = string.Empty;

    private int Index
    {
        get => this.index;
        set
        {
            value = Math.Max(Math.Min(value, this.Components.Count - this.MaxItems), 0);
            if (this.index == value)
            {
                return;
            }

            this.index = value;
            this.ScrollBar.bounds.Y = this.ScrollBarRunner.Top
                + (int)((this.ScrollBarRunner.Height - this.ScrollBar.bounds.Height)
                    * ((float)this.index / (this.Components.Count - this.MaxItems)));

            for (var i = 0; i < this.Components.Count; ++i)
            {
                this.Components[i].bounds.Y = (int)(this.yPositionOnScreen + ((i - this.index) * this.TextHeight));
            }
        }
    }

    private bool Scrolling { get; set; }

    /// <inheritdoc />
    public override void draw(SpriteBatch b)
    {
        b.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);

        Game1.drawDialogueBox(
            this.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder,
            this.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder,
            this.width + (IClickableMenu.borderWidth * 2) + (IClickableMenu.spaceToClearSideBorder * 2),
            this.height + IClickableMenu.spaceToClearTopBorder + (IClickableMenu.borderWidth * 2),
            false,
            true);

        this.OkButton.draw(b);
        this.DownArrow.draw(b);
        this.UpArrow.draw(b);
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(403, 383, 6, 6),
            this.ScrollBarRunner.X,
            this.ScrollBarRunner.Y,
            this.ScrollBarRunner.Width,
            this.ScrollBarRunner.Height,
            Color.White,
            4f);

        this.ScrollBar.draw(b);

        for (var i = 0; i < this.Components.Count; ++i)
        {
            if (this.Components[i].bounds.Top < this.yPositionOnScreen)
            {
                continue;
            }

            if (this.Components[i].bounds.Bottom > this.yPositionOnScreen + this.height)
            {
                break;
            }

            var icon = this.Icons.First(
                icon => icon.Id.Equals(this.Components[i].name, StringComparison.OrdinalIgnoreCase));

            // Arrows
            b.Draw(
                this.Arrows,
                new Vector2(this.xPositionOnScreen, this.Components[i].bounds.Center.Y - 16),
                new Rectangle(0, 0, 8, 8),
                Color.White * (i > 0 ? 1f : 0.5f),
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);

            b.Draw(
                this.Arrows,
                new Vector2(
                    this.xPositionOnScreen + (int)(Game1.tileSize / 2f) + 4,
                    this.Components[i].bounds.Center.Y - 16),
                new Rectangle(8, 0, 8, 8),
                Color.White * (i < this.Components.Count - 1 ? 1f : 0.5f),
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                1f);

            // Checkbox
            b.Draw(
                Game1.mouseCursors,
                new Vector2(this.xPositionOnScreen + Game1.tileSize + 16, this.Components[i].bounds.Y),
                icon.Enabled ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked,
                Color.White,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                0.4f);

            // Icon
            this.Components[i].draw(b);

            // Label
            Utility.drawTextWithShadow(
                b,
                this.Components[i].hoverText,
                Game1.dialogueFont,
                new Vector2(this.Components[i].bounds.Right + 8, this.Components[i].bounds.Y - 4),
                Game1.textColor,
                1f,
                0.1f);
        }

        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawHoverText(b, this.HoverText, Game1.smallFont);
        }

        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void leftClickHeld(int x, int y)
    {
        if (!this.Scrolling)
        {
            return;
        }

        var oldY = this.ScrollBar.bounds.Y;
        var percentage = (y - this.ScrollBarRunner.Y)
            / (float)(this.ScrollBarRunner.Height - this.ScrollBar.bounds.Height);

        this.Index = (int)((this.Components.Count - this.MaxItems) * percentage);
        if (oldY != this.ScrollBar.bounds.Y)
        {
            Game1.playSound("shiny4");
        }
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        base.performHoverAction(x, y);

        this.OkButton.scale = this.OkButton.containsPoint(x, y)
            ? Math.Min(1.1f, this.OkButton.scale + 0.05f)
            : Math.Max(1f, this.OkButton.scale - 0.05f);

        this.DownArrow.tryHover(x, y);
        this.UpArrow.tryHover(x, y);
        this.ScrollBar.tryHover(x, y);

        this.HoverText = string.Empty;

        if (x < this.xPositionOnScreen || x > this.xPositionOnScreen + Game1.tileSize + 52)
        {
            return;
        }

        var currentComponent =
            this.Components.FirstOrDefault(component => component.containsPoint(component.bounds.X, y));

        if (currentComponent is null)
        {
            return;
        }

        // Checkbox
        if (x >= this.xPositionOnScreen + Game1.tileSize + 16)
        {
            this.HoverText = I18n.Config_CheckBox_Tooltip();
            return;
        }

        var i = this.Components.IndexOf(currentComponent);

        if (x <= this.xPositionOnScreen + 32 && i > 0)
        {
            this.HoverText = I18n.Config_MoveUp_Tooltip();
        }
        else if (x >= this.xPositionOnScreen + (Game1.tileSize / 2) + 4
            && x <= this.xPositionOnScreen + (Game1.tileSize / 2) + 36
            && i < this.Components.Count - 1)
        {
            this.HoverText = I18n.Config_MoveDown_Tooltip();
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);
        if (this.OkButton.containsPoint(x, y))
        {
            this.exitThisMenu();
            Game1.playSound("bigDeSelect");
        }

        if (this.DownArrow.containsPoint(x, y))
        {
            this.DownArrow.scale = this.DownArrow.baseScale;
            ++this.Index;
            Game1.playSound("shwip");
            return;
        }

        if (this.UpArrow.containsPoint(x, y))
        {
            this.UpArrow.scale = this.UpArrow.baseScale;
            --this.Index;
            Game1.playSound("shwip");
            return;
        }

        if (this.ScrollBar.containsPoint(x, y))
        {
            this.Scrolling = true;
            return;
        }

        if (this.ScrollBarRunner.Contains(x, y))
        {
            this.Scrolling = true;
            this.leftClickHeld(x, y);
            this.releaseLeftClick(x, y);
            return;
        }

        if (x < this.xPositionOnScreen || x > this.xPositionOnScreen + Game1.tileSize + 52)
        {
            return;
        }

        var currentComponent =
            this.Components.FirstOrDefault(component => component.containsPoint(component.bounds.X, y));

        if (currentComponent is null)
        {
            return;
        }

        var currentIcon =
            this.Icons.First(icon => icon.Id.Equals(currentComponent.name, StringComparison.OrdinalIgnoreCase));

        // Checkbox
        if (x >= this.xPositionOnScreen + Game1.tileSize + 16)
        {
            currentIcon.Enabled = !currentIcon.Enabled;
            return;
        }

        var iconIndex = this.Icons.IndexOf(currentIcon);
        var i = this.Components.IndexOf(currentComponent);

        if (x <= this.xPositionOnScreen + 32 && i > 0)
        {
            // Move Up
            var previousIcon = this.Icons.First(
                icon => icon.Id.Equals(this.Components[i - 1].name, StringComparison.OrdinalIgnoreCase));

            var previousIndex = this.Icons.IndexOf(previousIcon);
            this.Icons[previousIndex] = currentIcon;
            this.Icons[iconIndex] = previousIcon;
            this.Components[i] = this.Components[i - 1];
            this.Components[i - 1] = currentComponent;
        }
        else if (x >= this.xPositionOnScreen + (Game1.tileSize / 2) + 4
            && x <= this.xPositionOnScreen + (Game1.tileSize / 2) + 36
            && i < this.Components.Count - 1)
        {
            // Move Down
            var nextIcon = this.Icons.First(
                icon => icon.Id.Equals(this.Components[i + 1].name, StringComparison.OrdinalIgnoreCase));

            var nextIndex = this.Icons.IndexOf(nextIcon);
            this.Icons[nextIndex] = currentIcon;
            this.Icons[iconIndex] = nextIcon;
            this.Components[i] = this.Components[i + 1];
            this.Components[i + 1] = currentComponent;
        }

        for (i = 0; i < this.Components.Count; ++i)
        {
            this.Components[i].bounds.Y = (int)(this.yPositionOnScreen + ((i - this.index) * this.TextHeight));
        }
    }

    /// <inheritdoc />
    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        var i = this.Index;
        switch (direction)
        {
            case > 0:
                if (i != --this.Index)
                {
                    Game1.playSound("shwip");
                }

                return;

            case < 0:
                if (i != ++this.Index)
                {
                    Game1.playSound("shwip");
                }

                break;
        }
    }

    /// <inheritdoc />
    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);
        this.Scrolling = false;
    }
}