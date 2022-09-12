namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.StorageHandlers;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Show stats to the side of a chest.
/// </summary>
internal sealed class ChestInfo : IFeature
{
    private static ChestInfo? Instance;

    private readonly ModConfig _config;
    private readonly PerScreen<IList<Tuple<Point, Point>>> _dims = new(() => new List<Tuple<Point, Point>>());
    private readonly IModHelper _helper;

    private readonly PerScreen<IList<KeyValuePair<string, string>>> _info = new(
        () => new List<KeyValuePair<string, string>>());

    private bool _isActivated;

    private ChestInfo(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    private IList<Tuple<Point, Point>> Dims => this._dims.Value;

    private IList<KeyValuePair<string, string>> Info => this._info.Value;

    /// <summary>
    ///     Initializes <see cref="ChestInfo" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestInfo" /> class.</returns>
    public static ChestInfo Init(IModHelper helper, ModConfig config)
    {
        return ChestInfo.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        BetterItemGrabMenu.DrawingMenu += this.OnDrawingMenu;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this._helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        BetterItemGrabMenu.DrawingMenu -= this.OnDrawingMenu;
        this._helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this._helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
    }

    private static IEnumerable<KeyValuePair<string, string>> GetChestInfo(IStorageObject storage)
    {
        var info = new List<KeyValuePair<string, string>>();

        if (!string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            info.Add(new(I18n.ChestInfo_Name(), storage.ChestLabel));
        }

        switch (storage)
        {
            case ChestStorage { Chest: { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } }:
                info.Add(new(I18n.ChestInfo_Type(), Formatting.StorageName("Junimo Chest")));
                break;
            case ChestStorage { Chest: { fridge.Value: true } }:
                info.Add(new(I18n.ChestInfo_Type(), Formatting.StorageName("Mini-Fridge")));
                break;
            case ChestStorage { Chest: { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin } }:
                info.Add(new(I18n.ChestInfo_Type(), Formatting.StorageName("Mini-Shipping Bin")));
                break;
            case ChestStorage:
                info.Add(new(I18n.ChestInfo_Type(), Formatting.StorageName("Chest")));
                break;
            case JunimoHutStorage:
                info.Add(new(I18n.ChestInfo_Type(), Formatting.StorageName("Junimo Hut")));
                break;
            case ShippingBinStorage:
                info.Add(new(I18n.ChestInfo_Type(), Formatting.StorageName("Shipping Bin")));
                break;
            case FridgeStorage:
                info.Add(new(I18n.ChestInfo_Type(), I18n.Storage_Fridge_Name()));
                break;
            case ObjectStorage:
                info.Add(new(I18n.ChestInfo_Type(), "Object"));
                break;
            default:
                info.Add(new(I18n.ChestInfo_Type(), "Other"));
                break;
        }

        info.Add(new(I18n.ChestInfo_Location(), storage.Location.Name));
        if (!storage.Position.Equals(Vector2.Zero))
        {
            info.Add(
                new(
                    I18n.ChestInfo_Position(),
                    $"({storage.Position.X.ToString(CultureInfo.InvariantCulture)}, {storage.Position.Y.ToString(CultureInfo.InvariantCulture)})"));
        }

        if (storage.Source is Farmer farmer)
        {
            info.Add(new(I18n.ChestInfo_Inventory(), farmer.Name));
        }

        if (storage.Items.Any())
        {
            info.Add(
                new(I18n.ChestInfo_TotalItems(), $"{storage.Items.OfType<Item>().Sum(item => (long)item.Stack):n0}"));
            info.Add(
                new(
                    I18n.ChestInfo_UniqueItems(),
                    $"{storage.Items.OfType<Item>().Select(item => $"{item.GetType().Name}-{item.ParentSheetIndex.ToString(CultureInfo.InvariantCulture)}").Distinct().Count():n0}"));
            info.Add(
                new(
                    I18n.ChestInfo_TotalValue(),
                    $"{storage.Items.OfType<SObject>().Sum(obj => (long)obj.salePrice() * obj.Stack / 2):n0}"));
        }

        return info;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this._config.ControlScheme.ToggleInfo.JustPressed())
        {
            return;
        }

        if (Game1.activeClickableMenu is not ItemGrabMenu
         || BetterItemGrabMenu.Context is null
         || this._config.ChestInfo is FeatureOption.Disabled)
        {
            return;
        }

        BetterItemGrabMenu.Context.ChestInfo = BetterItemGrabMenu.Context.ChestInfo is not FeatureOption.Enabled
            ? FeatureOption.Enabled
            : FeatureOption.Disabled;

        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.ToggleInfo);
    }

    private void OnDrawingMenu(object? sender, SpriteBatch b)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu || !this.Info.Any())
        {
            return;
        }

        var x = itemGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth / 2 - 384;
        var y = itemGrabMenu.yPositionOnScreen;
        if (BetterItemGrabMenu.Context?.CustomColorPicker is FeatureOption.Enabled
         && this._config.CustomColorPickerArea is ComponentArea.Left)
        {
            x -= 2 * Game1.tileSize;
        }

        Game1.drawDialogueBox(
            x - IClickableMenu.borderWidth,
            y - IClickableMenu.borderWidth / 2 - IClickableMenu.spaceToClearTopBorder,
            384,
            this.Dims.Sum(dim => dim.Item1.Y) + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2,
            false,
            true);

        for (var i = 0; i < this.Info.Count; ++i)
        {
            var (key, value) = this.Info[i];
            var (dim1, dim2) = this.Dims[i];
            Utility.drawTextWithShadow(b, $"{key}:", Game1.smallFont, new(x, y), Game1.textColor, 1f, 0.1f);
            if (dim1.X + dim2.X <= 384 - IClickableMenu.borderWidth)
            {
                b.DrawString(Game1.smallFont, value, new(x + dim1.X, y), Game1.textColor);
            }
            else
            {
                y += dim1.Y;
                b.DrawString(Game1.smallFont, value, new(x, y), Game1.textColor);
            }

            y += dim1.Y;
        }
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu
         || BetterItemGrabMenu.Context is not { ChestInfo: FeatureOption.Enabled } context)
        {
            this.Info.Clear();
            this.Dims.Clear();
            return;
        }

        this.RefreshChestInfo(context);
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu
         || BetterItemGrabMenu.Context is not { ChestInfo: FeatureOption.Enabled } context)
        {
            this.Info.Clear();
            this.Dims.Clear();
            return;
        }

        this.RefreshChestInfo(context);
    }

    private void RefreshChestInfo(IStorageObject context)
    {
        this.Info.Clear();
        this.Dims.Clear();
        foreach (var kvp in ChestInfo.GetChestInfo(context))
        {
            this.Info.Add(kvp);
        }

        if (!this.Info.Any())
        {
            return;
        }

        foreach (var (key, value) in this.Info)
        {
            this.Dims.Add(
                new(
                    Game1.smallFont.MeasureString($"{key}: ").ToPoint(),
                    Game1.smallFont.MeasureString(value).ToPoint()));
        }
    }
}