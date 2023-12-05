namespace StardewMods.BetterChests.Framework.Features;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Show stats to the side of a chest.</summary>
internal sealed class ChestInfo : BaseFeature
{
    private readonly ModConfig config;
    private readonly PerScreen<IList<Tuple<Point, Point>>> dims = new(() => new List<Tuple<Point, Point>>());
    private readonly IModEvents events;

    private readonly PerScreen<IList<KeyValuePair<string, string>>> info =
        new(() => new List<KeyValuePair<string, string>>());

    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="ChestInfo" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public ChestInfo(IMonitor monitor, ModConfig config, IModEvents events, IInputHelper input)
        : base(monitor, nameof(ChestInfo), () => config.ChestInfo is not FeatureOption.Disabled)
    {
        this.config = config;
        this.events = events;
        this.input = input;
    }

    private IList<Tuple<Point, Point>> Dims => this.dims.Value;

    private IList<KeyValuePair<string, string>> Info => this.info.Value;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterItemGrabMenu.DrawingMenu += this.OnDrawingMenu;
        this.events.Display.MenuChanged += this.OnMenuChanged;
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterItemGrabMenu.DrawingMenu -= this.OnDrawingMenu;
        this.events.Display.MenuChanged += this.OnMenuChanged;
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.events.Player.InventoryChanged -= this.OnInventoryChanged;
    }

    private static IEnumerable<KeyValuePair<string, string>> GetChestInfo(StorageNode storage)
    {
        var info = new List<KeyValuePair<string, string>>();

        if (!string.IsNullOrWhiteSpace(storage.ChestLabel))
        {
            info.Add(new(I18n.ChestInfo_Name(), storage.ChestLabel));
        }

        if (storage is not
            {
                Data: Storage storageObject,
            })
        {
            return info;
        }

        // Type
        switch (storageObject)
        {
            case ChestStorage
            {
                Chest.SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            }:
                info.Add(new(I18n.ChestInfo_Type(), FormatService.StorageName("Junimo Chest")));
                break;
            case ChestStorage
            {
                Chest.fridge.Value: true,
            }:
                info.Add(new(I18n.ChestInfo_Type(), FormatService.StorageName("Mini-Fridge")));
                break;
            case ChestStorage
            {
                Chest.SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin,
            }:
                info.Add(new(I18n.ChestInfo_Type(), FormatService.StorageName("Mini-Shipping Bin")));
                break;
            case ChestStorage:
                info.Add(new(I18n.ChestInfo_Type(), FormatService.StorageName("Chest")));
                break;
            case JunimoHutStorage:
                info.Add(new(I18n.ChestInfo_Type(), FormatService.StorageName("Junimo Hut")));
                break;
            case ShippingBinStorage:
                info.Add(new(I18n.ChestInfo_Type(), FormatService.StorageName("Shipping Bin")));
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

        // Location
        info.Add(new(I18n.ChestInfo_Location(), storageObject.Location.Name));

        // Position
        if (!storageObject.Position.Equals(Vector2.Zero))
        {
            info.Add(
                new(
                    I18n.ChestInfo_Position(),
                    $"({storageObject.Position.X.ToString(CultureInfo.InvariantCulture)}, {storageObject.Position.Y.ToString(CultureInfo.InvariantCulture)})"));
        }

        // Farmer inventory
        if (storageObject.Source is Farmer farmer)
        {
            info.Add(new(I18n.ChestInfo_Inventory(), farmer.Name));
        }

        // Item Stats
        if (!storageObject.Inventory.HasAny())
        {
            return info;
        }

        info.Add(
            new(
                I18n.ChestInfo_TotalItems(),
                $"{storageObject.Inventory.Where(item => item is not null).Sum(item => (long)item.Stack):n0}"));

        info.Add(
            new(
                I18n.ChestInfo_UniqueItems(),
                $"{storageObject.Inventory.Where(item => item is not null).Select(item => item.ItemId).Distinct().Count():n0}"));

        info.Add(
            new(
                I18n.ChestInfo_TotalValue(),
                $"{storageObject.Inventory.Sum(item => (long)item.sellToStorePrice(Game1.player.UniqueMultiplayerID) * item.Stack):n0}"));

        return info;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.config.ControlScheme.ToggleInfo.JustPressed())
        {
            return;
        }

        if (Game1.activeClickableMenu is not ItemGrabMenu
            || BetterItemGrabMenu.Context is null
            || this.config.ChestInfo is FeatureOption.Disabled)
        {
            return;
        }

        BetterItemGrabMenu.Context.ChestInfo = BetterItemGrabMenu.Context.ChestInfo is not FeatureOption.Enabled
            ? FeatureOption.Enabled
            : FeatureOption.Disabled;

        this.input.SuppressActiveKeybinds(this.config.ControlScheme.ToggleInfo);
    }

    private void OnDrawingMenu(object? sender, SpriteBatch b)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu || !this.Info.Any())
        {
            return;
        }

        var x = itemGrabMenu.xPositionOnScreen - (IClickableMenu.borderWidth / 2) - 384;
        var y = itemGrabMenu.yPositionOnScreen;
        if (BetterItemGrabMenu.Context?.CustomColorPicker is FeatureOption.Enabled
            && this.config.CustomColorPickerArea is ComponentArea.Left)
        {
            x -= 2 * Game1.tileSize;
        }

        Game1.drawDialogueBox(
            x - IClickableMenu.borderWidth,
            y - (IClickableMenu.borderWidth / 2) - IClickableMenu.spaceToClearTopBorder,
            384,
            this.Dims.Sum(dim => dim.Item1.Y) + IClickableMenu.spaceToClearTopBorder + (IClickableMenu.borderWidth * 2),
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
            || BetterItemGrabMenu.Context is not
            {
                ChestInfo: FeatureOption.Enabled,
            } context)
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
            || BetterItemGrabMenu.Context is not
            {
                ChestInfo: FeatureOption.Enabled,
            } context)
        {
            this.Info.Clear();
            this.Dims.Clear();
            return;
        }

        this.RefreshChestInfo(context);
    }

    private void RefreshChestInfo(StorageNode context)
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
