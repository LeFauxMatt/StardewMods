namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Collections.Immutable;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Adds tabs to the <see cref="ItemGrabMenu" /> to filter the displayed items.</summary>
internal sealed class InventoryTabs : BaseFeature, IItemFilter
{
    private readonly PerScreen<List<InventoryTab>> cachedTabs = new();
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<int> currentIndex = new(() => -1);
    private readonly IModEvents events;
    private readonly IInputHelper input;
    private readonly InventoryTabFactory inventoryTabFactory;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly PerScreen<int> newIndex = new(() => -1);
    private readonly PerScreen<bool> resetCache = new(() => true);

    /// <summary>Initializes a new instance of the <see cref="InventoryTabs" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="inventoryTabFactory">Dependency used for managing inventory tabs.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    public InventoryTabs(ILogging logging, ModConfig modConfig, IModEvents events, IInputHelper input, ContainerFactory containerFactory, InventoryTabFactory inventoryTabFactory, ItemGrabMenuManager itemGrabMenuManager)
        : base(logging, modConfig)
    {
        this.events = events;
        this.input = input;
        this.containerFactory = containerFactory;
        this.inventoryTabFactory = inventoryTabFactory;
        this.itemGrabMenuManager = itemGrabMenuManager;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.Default.InventoryTabs != FeatureOption.Disabled;

    /// <inheritdoc />
    public bool MatchesFilter(Item item) =>
        this.resetCache.Value || !this.cachedTabs.Value.Any() || this.newIndex.Value < 0 || this.newIndex.Value >= this.cachedTabs.Value.Count || this.cachedTabs.Value[this.newIndex.Value].MatchesItem(item);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Display.RenderedActiveMenu -= this.OnRenderedActiveMenu;
        this.events.Input.ButtonPressed += this.OnButtonPressed;
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.events.Input.ButtonPressed -= this.OnButtonPressed;
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    private IEnumerable<Item> FilterByTab(IEnumerable<Item> items)
    {
        if (this.ModConfig.Default.HideUnselectedItems == FeatureOption.Enabled)
        {
            return items.Where(this.MatchesFilter);
        }

        return this.currentIndex.Value == -1 ? items : items.OrderByDescending(this.MatchesFilter);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.resetCache.Value || !this.cachedTabs.Value.Any() || e.Button is not (SButton.MouseLeft or SButton.MouseRight or SButton.ControllerA))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var tab = this.cachedTabs.Value.FirstOrDefault(tab => tab.Component.containsPoint(x, y));
        if (tab is null)
        {
            return;
        }

        this.newIndex.Value = this.cachedTabs.Value.IndexOf(tab);
        this.input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.resetCache.Value || !this.cachedTabs.Value.Any())
        {
            return;
        }

        if (this.ModConfig.Controls.PreviousTab.JustPressed())
        {
            this.newIndex.Value--;
            this.input.SuppressActiveKeybinds(this.ModConfig.Controls.PreviousTab);
        }

        if (this.ModConfig.Controls.NextTab.JustPressed())
        {
            this.newIndex.Value++;
            this.input.SuppressActiveKeybinds(this.ModConfig.Controls.NextTab);
        }
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (this.resetCache.Value || !this.cachedTabs.Value.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.cachedTabs.Value.Any(tab => tab.Component.containsPoint(x, y)))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.newIndex.Value--;
                return;
            case < 0:
                this.newIndex.Value++;
                return;
            default:
                return;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        // Check if tabs needs to be refreshed
        if (this.resetCache.Value)
        {
            this.RefreshTabs();
            this.resetCache.Value = false;
        }

        // Check if there are any tabs
        if (!this.cachedTabs.Value.Any())
        {
            return;
        }

        // Wrap index
        if (this.newIndex.Value < -1)
        {
            this.newIndex.Value = this.cachedTabs.Value.Count - 1;
        }
        else if (this.newIndex.Value >= this.cachedTabs.Value.Count)
        {
            this.newIndex.Value = -1;
        }

        // Check if index changed
        if (this.newIndex.Value != this.currentIndex.Value)
        {
            // Unshift previous tab
            this.cachedTabs.Value[this.currentIndex.Value].Component.bounds.Y -= 8;

            // Shift active tab
            this.cachedTabs.Value[this.newIndex.Value].Component.bounds.Y += 8;

            this.currentIndex.Value = this.newIndex.Value;
        }

        var (x, y) = Game1.getMousePosition(true);
        for (var i = 0; i < this.cachedTabs.Value.Count; ++i)
        {
            var tab = this.cachedTabs.Value[i];
            var color = this.currentIndex.Value == i ? Color.White : Color.Gray;

            // Tab background
            e.SpriteBatch.Draw(
                tab.Component.texture,
                new Vector2(tab.Component.bounds.X, tab.Component.bounds.Y),
                new Rectangle(128, tab.Component.sourceRect.Y, 16, tab.Component.sourceRect.Height),
                color,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                0.86f);

            // Tab icon
            tab.Component.draw(e.SpriteBatch, color, 0.86f + (tab.Component.bounds.Y / 20000f));

            // Hover text
            if (tab.Component.containsPoint(x, y))
            {
                (Game1.activeClickableMenu as ItemGrabMenu)!.hoverText = tab.Component.hoverText;
            }
        }
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.Context?.Options.InventoryTabs != FeatureOption.Enabled)
        {
            return;
        }

        this.itemGrabMenuManager.TopMenu.AddHighlightMethod(this.MatchesFilter);
        this.itemGrabMenuManager.TopMenu.AddOperation(this.FilterByTab);
        this.resetCache.Value = true;
    }

    private void RefreshTabs()
    {
        this.cachedTabs.Value.Clear();
        if (Game1.activeClickableMenu is not ItemGrabMenu
            {
                context: Chest chest,
            } itemGrabMenu
            || !this.containerFactory.TryGetOne(chest, out var container)
            || container.Options.InventoryTabs != FeatureOption.Enabled
            || !container.Options.InventoryTabList.Any())
        {
            return;
        }

        // Load tabs
        foreach (var name in container.Options.InventoryTabList)
        {
            if (this.inventoryTabFactory.TryGetOne(name, out var tab))
            {
                this.cachedTabs.Value.Add(tab);
            }
        }

        // Assign positions and ids
        var bottomRow = itemGrabMenu.ItemsToGrabMenu.inventory.TakeLast(12).ToArray();
        var topRow = itemGrabMenu.inventory.inventory.Take(12).ToArray();
        var components = this.cachedTabs.Value.Select(tab => tab.Component).ToImmutableArray();
        for (var i = 0; i < components.Length; ++i)
        {
            components[i].myID = 69_420 + i;
            itemGrabMenu.allClickableComponents.Add(components[i]);
            if (i > 0)
            {
                components[i - 1].rightNeighborID = 69_420 + i;
                components[i].leftNeighborID = 69_419 + i;
            }

            if (i < topRow.Length)
            {
                topRow[i].upNeighborID = 69_420 + i;
                components[i].downNeighborID = topRow[i].myID;
            }

            if (i < bottomRow.Length)
            {
                bottomRow[i].downNeighborID = 69_420 + i;
                components[i].upNeighborID = bottomRow[i].myID;
            }

            components[i].bounds.X = i > 0 ? components[i - 1].bounds.Right : itemGrabMenu.ItemsToGrabMenu.inventory[0].bounds.Left;

            components[i].bounds.Y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen + (Game1.tileSize * itemGrabMenu.ItemsToGrabMenu.rows) + IClickableMenu.borderWidth;
        }
    }
}
