namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.StorageHandlers;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Show stats to the side of a chest.
/// </summary>
internal class ChestInfo : IFeature
{
    private static ChestInfo? Instance;

    private readonly ModConfig _config;
    private readonly PerScreen<IList<Point>> _dims = new(() => new List<Point>());
    private readonly IModHelper _helper;

    private readonly PerScreen<IList<KeyValuePair<string, string>>> _info = new(
        () => new List<KeyValuePair<string, string>>());

    private bool _isActivated;

    private ChestInfo(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
    }

    private IList<Point> Dims => this._dims.Value;

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
            info.Add(new("Name", storage.ChestLabel));
        }

        switch (storage)
        {
            case ChestStorage { Chest: { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } }:
                info.Add(new("Type", "JunimoChest"));
                break;
            case ChestStorage:
                info.Add(new("Type", "Chest"));
                break;
            case FridgeStorage:
                info.Add(new("Type", "Fridge"));
                break;
            case JunimoHutStorage:
                info.Add(new("Type", "JunimoHut"));
                break;
            case ObjectStorage:
                info.Add(new("Type", "Object"));
                break;
            case ShippingBinStorage:
                info.Add(new("Type", "ShippingBin"));
                break;
            default:
                info.Add(new("Type", "Other"));
                break;
        }

        info.Add(new("Location", storage.Location.Name));
        if (!storage.Position.Equals(Vector2.Zero))
        {
            info.Add(
                new(
                    "Position",
                    $"({storage.Position.X.ToString(CultureInfo.InvariantCulture)}, {storage.Position.Y.ToString(CultureInfo.InvariantCulture)})"));
        }

        if (storage.Source is Farmer farmer)
        {
            info.Add(new("Inventory", farmer.Name));
        }

        if (storage.Items.Any())
        {
            info.Add(new("Total Items", $"{storage.Items.OfType<Item>().Sum(item => item.Stack):n0}"));
            info.Add(
                new(
                    "Unique Items",
                    $"{storage.Items.OfType<Item>().Select(item => $"{item.GetType().Name}-{item.ParentSheetIndex.ToString(CultureInfo.InvariantCulture)}").Distinct().Count():n0}"));
            info.Add(
                new(
                    "Total Value",
                    $"{storage.Items.OfType<Item>().Sum(item => Utility.getSellToStorePriceOfItem(item)):n0}"));
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

        var x = itemGrabMenu.xPositionOnScreen - Game1.tileSize - 384;
        var y = itemGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth / 2;
        Game1.drawDialogueBox(
            x - IClickableMenu.borderWidth,
            y - IClickableMenu.borderWidth / 2 - IClickableMenu.spaceToClearTopBorder,
            384,
            this.Dims.Sum(dim => dim.Y) + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2,
            false,
            true);

        for (var i = 0; i < this.Info.Count; i++)
        {
            var (key, value) = this.Info[i];
            var dim = this.Dims[i];
            Utility.drawTextWithShadow(b, $"{key}:", Game1.smallFont, new(x, y), Game1.textColor, 1f, 0.1f);
            b.DrawString(Game1.smallFont, value, new(x + dim.X, y), Game1.textColor);
            y += dim.Y;
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

        foreach (var (key, _) in this.Info)
        {
            this.Dims.Add(Game1.smallFont.MeasureString($"{key}: ").ToPoint());
        }
    }
}