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
    private readonly IStackQualityApi _api;
    private readonly ClickableComponent[] _inventory = new ClickableComponent[4];
    private readonly SObject[] _items;
    private readonly SObject _object;
    private readonly int[] _stacks;
    private readonly int _x;
    private readonly int _y;

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
        this._api = api;
        this._object = obj;
        this._stacks = stacks;
        this._x = x;
        this._y = y;

        if (!this._api.SplitStacks(this._object, out var items))
        {
            throw new();
        }

        this._items = items;

        for (var i = 0; i < 4; ++i)
        {
            this._inventory[i] = new(
                new(
                    this._x + Game1.tileSize * (i % 2),
                    this._y + Game1.tileSize * (i / 2),
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
        for (var i = 0; i < 4; ++i)
        {
            b.Draw(
                Game1.menuTexture,
                new(this._x + Game1.tileSize * (i % 2), this._y + Game1.tileSize * (i / 2)),
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
            this._items[i]
                .drawInMenu(
                    b,
                    new(this._x + Game1.tileSize * (i % 2), this._y + Game1.tileSize * (i / 2)),
                    this._stacks[i] == 0 ? 1f : this._inventory[i].scale,
                    this._stacks[i] == 0 ? 0.25f : 1f,
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
        var slot = this._items[slotNumber];
        if (this._stacks[slotNumber] == 0)
        {
            return;
        }

        this._stacks[slotNumber] = 0;
        Helpers.HeldItem = slot;
        this.exitThisMenuNoSound();
    }

    /// <inheritdoc />
    protected override void cleanupBeforeExit()
    {
        this._api.UpdateStacks(this._object, this._stacks);
    }
}