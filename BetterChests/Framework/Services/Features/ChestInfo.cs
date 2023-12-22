﻿namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Models.Storages;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Show stats to the side of a chest.</summary>
internal sealed class ChestInfo : BaseFeature
{
    private const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static readonly int LineHeight = (int)Game1.smallFont.MeasureString(ChestInfo.AlphaNumeric).Y;
    private readonly PerScreen<List<Info>> cachedInfo = new(() => []);

    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;

    private readonly PerScreen<bool> isActive = new();
    private readonly IModEvents modEvents;
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="ChestInfo" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public ChestInfo(ILogging logging, ModConfig modConfig, ContainerFactory containerFactory, IInputHelper inputHelper, IModEvents modEvents)
        : base(logging, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.ChestInfo != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Display.MenuChanged += this.OnMenuChanged;
        this.modEvents.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        this.modEvents.Player.InventoryChanged += this.OnInventoryChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Display.MenuChanged += this.OnMenuChanged;
        this.modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.modEvents.Player.InventoryChanged -= this.OnInventoryChanged;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.resetCache.Value || !this.cachedInfo.Value.Any() || !this.ModConfig.Controls.ToggleInfo.JustPressed())
        {
            return;
        }

        this.isActive.Value = !this.isActive.Value;
        this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.ToggleInfo);
    }

    private void OnInventoryChanged(object? sender, InventoryChangedEventArgs e) => this.resetCache.Value = true;

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e) => this.resetCache.Value = true;

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        // Check if info needs to be refreshed
        if (this.resetCache.Value)
        {
            this.RefreshInfo();
            this.resetCache.Value = false;
        }

        // Check if active and is info
        if (!this.isActive.Value || !this.cachedInfo.Value.Any())
        {
            return;
        }

        var x = Game1.activeClickableMenu.xPositionOnScreen - (IClickableMenu.borderWidth / 2) - 384;
        var y = Game1.activeClickableMenu.yPositionOnScreen;

        // Draw background
        Game1.drawDialogueBox(
            x - IClickableMenu.borderWidth,
            y - (IClickableMenu.borderWidth / 2) - IClickableMenu.spaceToClearTopBorder,
            384,
            (ChestInfo.LineHeight * this.cachedInfo.Value.Count) + IClickableMenu.spaceToClearTopBorder + (IClickableMenu.borderWidth * 2),
            false,
            true);

        // Draw info
        foreach (var info in this.cachedInfo.Value)
        {
            // Draw Name
            Utility.drawTextWithShadow(e.SpriteBatch, info.Name, Game1.smallFont, new Vector2(x, y), Game1.textColor, 1f, 0.1f);

            // Draw Value
            if (info.TotalWidth <= 384 - IClickableMenu.borderWidth)
            {
                e.SpriteBatch.DrawString(Game1.smallFont, info.Value, new Vector2(x + info.NameWidth, y), Game1.textColor);

                y += ChestInfo.LineHeight;
                continue;
            }

            y += ChestInfo.LineHeight;
            e.SpriteBatch.DrawString(Game1.smallFont, info.Value, new Vector2(x, y), Game1.textColor);

            y += ChestInfo.LineHeight;
        }
    }

    private void RefreshInfo()
    {
        this.cachedInfo.Value.Clear();
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            }
            || !this.containerFactory.TryGetOne(chest, out var container)
            || container.Options.ChestInfo != FeatureOption.Enabled)
        {
            return;
        }

        // Add label or name
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Name(), container.Options.ChestLabel));

        // Add type
        var type = container.StorageType switch
        {
            BuildingStorage
            {
                Data:
                { } buildingData,
            } => TokenParser.ParseText(buildingData.Name),
            LocationStorage => I18n.Storage_Fridge_Name(),
            BigCraftableStorage
            {
                Data:
                { } objectData,
            } => objectData.DisplayName,
            _ => I18n.Storage_Other_Name(),
        };

        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Type(), type));

        // Add Location
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Location(), container.Location.Name));

        // Add Position
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Position(), $"{(int)container.TileLocation.X}, {(int)container.TileLocation.Y}"));

        // Add Inventory
        if (container is ChildContainer
            {
                Parent: FarmerContainer farmerStorage,
            })
        {
            this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Inventory(), farmerStorage.Farmer.Name));
        }

        var items = container.Items.Where(item => item is not null).ToList();

        // Total Items
        var totalItems = items.Sum(item => item.Stack);
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_TotalItems(), $"{totalItems:n0}"));

        // Unique Items
        var uniqueItems = items.Select(item => item.QualifiedItemId).Distinct().Count();
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_UniqueItems(), $"{uniqueItems:n0}"));

        // Total Value
        var totalValue = items.Select(item => (long)item.sellToStorePrice(Game1.player.UniqueMultiplayerID) * item.Stack).Sum();

        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_TotalValue(), $"{totalValue:n0}"));
    }

    private readonly struct Info(string name, string value)
    {
        public string Name { get; } = name;

        public string Value { get; } = value;

        public int NameWidth { get; } = (int)Game1.smallFont.MeasureString($"{name} ").X;

        public int TotalWidth { get; } = (int)Game1.smallFont.MeasureString($"{name} {value}").X;
    }
}