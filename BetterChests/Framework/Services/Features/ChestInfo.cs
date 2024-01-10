namespace StardewMods.BetterChests.Framework.Services.Features;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Show stats to the side of a chest.</summary>
internal sealed class ChestInfo : BaseFeature<ChestInfo>
{
    private const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private static readonly int LineHeight = (int)Game1.smallFont.MeasureString(ChestInfo.AlphaNumeric).Y;

    private readonly PerScreen<List<Info>> cachedInfo = new(() => []);
    private readonly ContainerFactory containerFactory;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="ChestInfo" /> class.</summary>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public ChestInfo(
        ContainerFactory containerFactory,
        IEventManager eventManager,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.containerFactory = containerFactory;
        this.inputHelper = inputHelper;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.ChestInfo != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<InventoryChangedEventArgs>(this.OnInventoryChanged);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<MenuChangedEventArgs>(this.OnMenuChanged);
        this.Events.Unsubscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<InventoryChangedEventArgs>(this.OnInventoryChanged);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (this.resetCache.Value || !this.cachedInfo.Value.Any() || !this.Config.Controls.ToggleInfo.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.ToggleInfo);
        this.isActive.Value = !this.isActive.Value;
        this.Log.Trace("{0}: Toggled chest info to {1}", this.Id, this.isActive.Value);
    }

    private void OnInventoryChanged(InventoryChangedEventArgs e) => this.resetCache.Value = true;

    private void OnMenuChanged(MenuChangedEventArgs e) => this.resetCache.Value = true;

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
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
            (ChestInfo.LineHeight * this.cachedInfo.Value.Count)
            + IClickableMenu.spaceToClearTopBorder
            + (IClickableMenu.borderWidth * 2),
            false,
            true);

        // Draw info
        foreach (var info in this.cachedInfo.Value)
        {
            // Draw Name
            Utility.drawTextWithShadow(
                e.SpriteBatch,
                info.Name,
                Game1.smallFont,
                new Vector2(x, y),
                Game1.textColor,
                1f,
                0.1f);

            // Draw Value
            if (info.TotalWidth <= 384 - IClickableMenu.borderWidth)
            {
                e.SpriteBatch.DrawString(
                    Game1.smallFont,
                    info.Value,
                    new Vector2(x + info.NameWidth, y),
                    Game1.textColor);

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
        if (!this.containerFactory.TryGetOneFromMenu(out var container)
            || container.Options.ChestInfo != FeatureOption.Enabled)
        {
            return;
        }

        // Add label
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Name(), container.Options.ChestLabel));

        // Add type
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Type(), container.DisplayName));

        if (container.Location is not null)
        {
            // Add location
            this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Location(), container.Location.Name));

            // Add position
            this.cachedInfo.Value.Add(
                new Info(
                    I18n.ChestInfo_Position(),
                    $"{(int)container.TileLocation.X}, {(int)container.TileLocation.Y}"));
        }

        // Add inventory
        if (container is ChildContainer
            {
                Parent: FarmerContainer farmerStorage,
            })
        {
            this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_Inventory(), farmerStorage.Farmer.Name));
        }

        var items = container.Items.Where(item => item is not null).ToList();

        // Total items
        var totalItems = items.Sum(item => item.Stack);
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_TotalItems(), $"{totalItems:n0}"));

        // Unique items
        var uniqueItems = items.Select(item => item.QualifiedItemId).Distinct().Count();
        this.cachedInfo.Value.Add(new Info(I18n.ChestInfo_UniqueItems(), $"{uniqueItems:n0}"));

        // Total value
        var totalValue =
            items.Select(item => (long)item.sellToStorePrice(Game1.player.UniqueMultiplayerID) * item.Stack).Sum();

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