namespace StardewMods.StackQuality.UI;

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.StackQuality;
using StardewMods.StackQuality.Framework;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ItemQualityMenu : IClickableMenu
{
    private readonly IStackQualityApi api;
    private readonly ClickableComponent[] inventory = new ClickableComponent[4];
    private readonly SObject[] items;
    private readonly SObject @object;
    private readonly int[] stacks;
    private readonly int x;
    private readonly int y;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemQualityMenu" /> class.
    /// </summary>
    /// <param name="api">The StackQuality Api.</param>
    /// <param name="obj">The item to display.</param>
    /// <param name="stacks">The item stacks..</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public ItemQualityMenu(IStackQualityApi api, SObject obj, int[] stacks, int x, int y)
    {
        this.api = api;
        this.@object = obj;
        this.stacks = stacks;
        this.x = x;
        this.y = y;

        if (!this.api.SplitStacks(this.@object, out var itemStacks))
        {
            throw new();
        }

        this.items = itemStacks;

        for (var i = 0; i < 4; ++i)
        {
            this.inventory[i] = new(
                new(
                    this.x + (Game1.tileSize * (i % 2)),
                    this.y + (Game1.tileSize * (i / 2)),
                    Game1.tileSize,
                    Game1.tileSize),
                i.ToString());
        }
    }

    /// <summary>
    ///     Draws the overlay.
    /// </summary>
    /// <param name="b">The SpriteBatch to draw to.</param>
    public void Draw(SpriteBatch b)
    {
        // Draw Background
        Game1.drawDialogueBox(
            this.x - ItemQualityMenu.borderWidth,
            this.y - ItemQualityMenu.spaceToClearTopBorder,
            (Game1.tileSize * 2) + (ItemQualityMenu.borderWidth * 2),
            (Game1.tileSize * 2) + ItemQualityMenu.spaceToClearTopBorder + ItemQualityMenu.borderWidth,
            false,
            true,
            null,
            false,
            false);

        // Draw Slots
        for (var i = 0; i < 4; ++i)
        {
            b.Draw(
                Game1.menuTexture,
                new(this.x + (Game1.tileSize * (i % 2)), this.y + (Game1.tileSize * (i / 2))),
                Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10),
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0.5f);
        }

        // Draw Items
        for (var i = 0; i < 4; ++i)
        {
            this.items[i]
                .drawInMenu(
                    b,
                    new(this.x + (Game1.tileSize * (i % 2)), this.y + (Game1.tileSize * (i / 2))),
                    this.stacks[i] == 0 ? 1f : this.inventory[i].scale,
                    this.stacks[i] == 0 ? 0.25f : 1f,
                    0.865f,
                    StackDrawType.Draw,
                    Color.White,
                    true);
        }

        // Draw Mouse
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void performHoverAction(int hoverX, int hoverY)
    {
        foreach (var cc in this.inventory)
        {
            cc.scale = Math.Max(1f, cc.scale - 0.025f);

            if (cc.containsPoint(hoverX, hoverY))
            {
                cc.scale = Math.Min(cc.scale + 0.05f, 1.1f);
            }
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int clickX, int clickY, bool playSound = true)
    {
        var component = this.inventory.FirstOrDefault(cc => cc.containsPoint(clickX, clickY));
        if (component is null)
        {
            this.exitThisMenuNoSound();
            return;
        }

        var slotNumber = int.Parse(component.name);
        var slot = this.items[slotNumber];
        if (this.stacks[slotNumber] == 0)
        {
            return;
        }

        this.stacks[slotNumber] = 0;
        Helpers.HeldItem = slot;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    protected override void cleanupBeforeExit()
    {
        this.api.UpdateStacks(this.@object, this.stacks);
    }
}