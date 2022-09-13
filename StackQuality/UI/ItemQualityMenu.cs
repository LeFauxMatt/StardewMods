namespace StardewMods.StackQuality.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.StackQuality.Helpers;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ItemQualityMenu : IClickableMenu
{
    private readonly List<ClickableComponent> _inventory = new();
    private readonly SObject _object;
    private readonly int _x;
    private readonly int _y;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemQualityMenu" /> class.
    /// </summary>
    /// <param name="obj">The item to display.</param>
    /// <param name="x">The x-coordinate.</param>
    /// <param name="y">The y-coordinate.</param>
    public ItemQualityMenu(SObject obj, int x, int y)
    {
        this._object = obj;
        this._x = x;
        this._y = y;

        var stacks = this._object.GetStacks();
        for (var index = 0; index < 4; ++index)
        {
            var item = (SObject)this._object.getOne();
            item.modData.Remove("furyx639.StackQuality/qualities");
            item.Quality = index == 3 ? 4 : index;
            item.Stack = stacks[index];
            this.Items.Add(item);
            this._inventory.Add(
                new(
                    new(
                        this._x + Game1.tileSize * (index % 2),
                        this._y + Game1.tileSize * (index / 2),
                        Game1.tileSize,
                        Game1.tileSize),
                    index.ToString()));
        }
    }

    private List<SObject> Items { get; } = new();

    /// <summary>
    ///     Draws the overlay.
    /// </summary>
    /// <param name="b">The SpriteBatch to draw to.</param>
    public void Draw(SpriteBatch b)
    {
        // Draw Background
        Game1.drawDialogueBox(
            this._x - ItemQualityMenu.borderWidth,
            this._y - ItemQualityMenu.spaceToClearTopBorder,
            Game1.tileSize * 2 + ItemQualityMenu.borderWidth * 2,
            Game1.tileSize * 2 + ItemQualityMenu.spaceToClearTopBorder + ItemQualityMenu.borderWidth,
            false,
            true,
            null,
            false,
            false);

        // Draw Slots
        for (var index = 0; index < 4; ++index)
        {
            b.Draw(
                Game1.menuTexture,
                new(this._x + Game1.tileSize * (index % 2), this._y + Game1.tileSize * (index / 2)),
                Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10),
                Color.White,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0.5f);
        }

        // Draw Items
        for (var index = 0; index < 4; ++index)
        {
            this.Items[index]
                .drawInMenu(
                    b,
                    new(this._x + Game1.tileSize * (index % 2), this._y + Game1.tileSize * (index / 2)),
                    this.Items[index].Stack == 0 ? 1f : this._inventory[index].scale,
                    this.Items[index].Stack == 0 ? 0.25f : 1f,
                    0.865f,
                    StackDrawType.Draw,
                    Color.White,
                    true);

            // Draw Quality
        }

        // Draw Mouse
        Game1.mouseCursorTransparency = 1f;
        this.drawMouse(b);
    }

    /// <inheritdoc />
    public override void performHoverAction(int x, int y)
    {
        foreach (var cc in this._inventory)
        {
            cc.scale = Math.Max(1f, cc.scale - 0.025f);

            if (cc.containsPoint(x, y))
            {
                cc.scale = Math.Min(cc.scale + 0.05f, 1.1f);
            }
        }
    }

    /// <inheritdoc />
    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        var component = this._inventory.FirstOrDefault(cc => cc.containsPoint(x, y));
        if (component is null)
        {
            this.exitThisMenuNoSound();
            return;
        }

        var slotNumber = int.Parse(component.name);
        var slot = this.Items[slotNumber];
        if (slot.Stack == 0)
        {
            return;
        }

        var obj = (SObject)slot.getOne();
        var stacks = new int[4];
        stacks[obj.Quality == 4 ? 3 : obj.Quality] = slot.Stack;
        slot.Stack = 0;
        obj.UpdateQuality(stacks);
        StackQuality.HeldItem = obj;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    protected override void cleanupBeforeExit()
    {
        var stacks = this.Items.Select(item => item.Stack).ToArray();
        this._object.UpdateQuality(stacks);
    }
}