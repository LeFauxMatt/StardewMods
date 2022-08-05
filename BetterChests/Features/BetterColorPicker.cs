namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.UI;
using StardewMods.Common.Enums;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Adds a chest color picker that support hue, saturation, and lightness.
/// </summary>
internal class BetterColorPicker : IFeature
{
    private readonly PerScreen<HslColorPicker> _perScreenColorPicker = new(() => new());

    private BetterColorPicker(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static BetterColorPicker? Instance { get; set; }

    private HslColorPicker ColorPicker => this._perScreenColorPicker.Value;

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    /// <summary>
    ///     Initializes <see cref="BetterColorPicker" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterColorPicker" /> class.</returns>
    public static BetterColorPicker Init(IModHelper helper, ModConfig config)
    {
        return BetterColorPicker.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this.IsActivated)
        {
            return;
        }

        this.IsActivated = true;
        this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this.IsActivated)
        {
            return;
        }

        this.IsActivated = false;
        this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
         || Game1.activeClickableMenu is not ItemGrabMenu
            {
                chestColorPicker: not null,
                colorPickerToggleButton: var toggleButton,
            })
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!toggleButton.containsPoint(x, y))
        {
            return;
        }

        Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
        Game1.playSound("drumkit6");
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                chestColorPicker: not null,
            })
        {
            return;
        }

        this.ColorPicker.Draw(e.SpriteBatch);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                chestColorPicker: not null,
                context: Chest chest,
            } itemGrabMenu)
        {
            return;
        }

        if (itemGrabMenu.discreteColorPickerCC is not null)
        {
            itemGrabMenu.chestColorPicker.visible = false;
            itemGrabMenu.discreteColorPickerCC = null;
            var x = this.Config.CustomColorPickerArea switch
            {
                ComponentArea.Left => itemGrabMenu.xPositionOnScreen
                                    - 2 * Game1.tileSize
                                    - IClickableMenu.borderWidth / 2,
                ComponentArea.Right => itemGrabMenu.xPositionOnScreen
                                     + itemGrabMenu.width
                                     + 96
                                     + IClickableMenu.borderWidth / 2,
            };
            var y = itemGrabMenu.yPositionOnScreen - 56 + IClickableMenu.borderWidth / 2;
            this.ColorPicker.Init(x, y, chest.playerChoiceColor.Value);
        }

        this.ColorPicker.Update(this.Helper.Input);
        chest.playerChoiceColor.Value = this.ColorPicker.Color;
    }
}