namespace StardewMods.BetterChests.Framework.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewValley.Menus;

/// <summary>
///     Adds tabs to the <see cref="ItemGrabMenu" /> to filter the displayed items.
/// </summary>
internal sealed class ChestMenuTabs : Feature
{
#nullable disable
    private static ChestMenuTabs instance;
#nullable enable

    private readonly Lazy<Dictionary<string, ClickableTextureComponent>> allTabs;
    private readonly ModConfig config;
    private readonly PerScreen<ItemGrabMenu?> currentMenu = new();
    private readonly IModHelper helper;
    private readonly PerScreen<ItemMatcher> itemMatcher = new(() => new(true));
    private readonly PerScreen<int> tabIndex = new(() => -1);
    private readonly PerScreen<List<ClickableTextureComponent>> tabs = new(() => new());

    private ChestMenuTabs(IModHelper helper, ModConfig config)
    {
        this.helper = helper;
        this.config = config;
        this.allTabs = new(
            () =>
            {
                var allTabData = new Dictionary<string, ClickableTextureComponent>();
                var tabData = this.helper.GameContent.Load<Dictionary<string, string>>("furyx639.BetterChests/Tabs");
                foreach (var (name, info) in tabData)
                {
                    var data = info.Split('/');
                    var hoverText = !string.IsNullOrWhiteSpace(data[0])
                        ? data[0]
                        : helper.Translation.Get($"tabs.{name}.name").Default(name);
                    allTabData.Add(
                        name,
                        new(
                            data[3],
                            new(0, 0, 16 * Game1.pixelZoom, 13 * Game1.pixelZoom),
                            string.Empty,
                            hoverText,
                            Game1.content.Load<Texture2D>(data[1]),
                            new(16 * int.Parse(data[2]), 4, 16, 12),
                            Game1.pixelZoom));
                }

                return allTabData;
            });
    }

    private static Dictionary<string, ClickableTextureComponent> AllTabs => ChestMenuTabs.instance.allTabs.Value;

    private List<ClickableTextureComponent> Components => this.tabs.Value;

    private ItemGrabMenu? CurrentMenu
    {
        get => this.currentMenu.Value;
        set => this.currentMenu.Value = value;
    }

    private int Index
    {
        get => this.tabIndex.Value;
        set
        {
            this.tabIndex.Value = value;
            this.ItemMatcher.Clear();
            if (value == -1 || !this.Components.Any())
            {
                Log.Trace("Switching tab to None");
                BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
                return;
            }

            var tab = this.Components[value];
            var tags = tab.name.Split(' ');
            foreach (var tag in tags)
            {
                this.ItemMatcher.Add(tag);
            }

            Log.Trace($"Switching tab to {tab.hoverText}");
            BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        }
    }

    private ItemMatcher ItemMatcher => this.itemMatcher.Value;

    private Dictionary<string, string> Tabs
    {
        get
        {
            var tabData = this.helper.Data.ReadJsonFile<Dictionary<string, string>>("assets/tabs.json");
            if (tabData is not null && tabData.Any())
            {
                return tabData;
            }

            tabData = new()
            {
                {
                    "Clothing", @"/furyx639.BetterChests\Tabs\Texture/0/category_clothing category_boots category_hat"
                },
                {
                    "Cooking",
                    @"/furyx639.BetterChests\\Tabs\\Texture/1/category_syrup category_artisan_goods category_ingredients category_sell_at_pierres_and_marnies category_sell_at_pierres category_meat category_cooking category_milk category_egg"
                },
                {
                    "Crops",
                    @"/furyx639.BetterChests\\Tabs\\Texture/2/category_greens category_flowers category_fruits category_vegetable"
                },
                {
                    "Equipment",
                    @"/furyx639.BetterChests\\Tabs\\Texture/3/category_equipment category_ring category_tool category_weapon"
                },
                {
                    "Fishing",
                    @"/furyx639.BetterChests\\Tabs\\Texture/4/category_bait category_fish category_tackle category_sell_at_fish_shop"
                },
                {
                    "Materials",
                    @"/furyx639.BetterChests\\Tabs\\Texture/5/category_monster_loot category_metal_resources category_building_resources category_minerals category_crafting category_gem"
                },
                {
                    "Misc",
                    @"/furyx639.BetterChests\\Tabs\\Texture/6/category_big_craftable category_furniture category_junk"
                },
                { "Seeds", @"/furyx639.BetterChests\\Tabs\\Texture/7/category_seeds category_fertilizer" },
            };

            this.helper.Data.WriteJsonFile("assets/tabs.json", tabData);

            return tabData;
        }
    }

    /// <summary>
    ///     Initializes <see cref="ChestMenuTabs" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestMenuTabs" /> class.</returns>
    public static Feature Init(IModHelper helper, ModConfig config)
    {
        return ChestMenuTabs.instance ??= new(helper, config);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        BetterItemGrabMenu.DrawingMenu += this.OnDrawingMenu;
        this.helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        BetterItemGrabMenu.DrawingMenu -= this.OnDrawingMenu;
        this.helper.Events.Content.AssetRequested -= this.OnAssetRequested;
        this.helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    private IEnumerable<Item> FilterByTab(IEnumerable<Item> items)
    {
        if (this.config.HideItems is FeatureOption.Enabled)
        {
            return this.ItemMatcher.Any() ? items.Where(this.ItemMatcher.Matches) : items;
        }

        return this.ItemMatcher.Any() ? items.OrderBy(item => this.ItemMatcher.Matches(item) ? 0 : 1) : items;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.BetterChests/Tabs"))
        {
            e.LoadFrom(() => this.Tabs, AssetLoadPriority.Exclusive);
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null || !this.Components.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var tab = this.Components.FirstOrDefault(tab => tab.containsPoint(x, y));
        var index = tab is not null ? this.Components.IndexOf(tab) : -1;
        switch (e.Button)
        {
            case SButton.MouseLeft or SButton.MouseRight or SButton.ControllerA when index != -1:
                this.Index = this.Index == index ? -1 : index;
                break;
            default:
                return;
        }

        this.helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.CurrentMenu is null || !this.Components.Any())
        {
            return;
        }

        if (this.config.ControlScheme.PreviousTab.JustPressed())
        {
            this.Index = this.Index == -1 ? this.Components.Count - 1 : this.Index - 1;
            this.helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.PreviousTab);
        }

        if (this.config.ControlScheme.NextTab.JustPressed())
        {
            this.Index = this.Index == this.Components.Count - 1 ? -1 : this.Index + 1;
            this.helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.NextTab);
        }
    }

    [EventPriority(EventPriority.High)]
    private void OnDrawingMenu(object? sender, SpriteBatch b)
    {
        if (this.CurrentMenu is null || !this.Components.Any())
        {
            return;
        }

        ClickableTextureComponent? tab;
        for (var index = 0; index < this.Components.Count; ++index)
        {
            tab = this.Components[index];
            tab.sourceRect.Y = 4;
            tab.sourceRect.Height = 12;
            if (index == this.Index)
            {
                tab.sourceRect.Y = 3;
                tab.sourceRect.Height = 13;
                b.Draw(
                    tab.texture,
                    new(tab.bounds.X, tab.bounds.Y),
                    new(128, tab.sourceRect.Y, 16, tab.sourceRect.Height),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Game1.pixelZoom,
                    SpriteEffects.None,
                    0.86f);
                tab.draw(b, Color.White, 0.86f + (tab.bounds.Y / 20000f));

                // draw texture
                var bounds = Game1.smallFont.MeasureString(tab.hoverText).ToPoint();
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.menuTexture,
                    new(0, 256, 60, 60),
                    this.CurrentMenu.xPositionOnScreen + this.CurrentMenu.width - bounds.X - Game1.tileSize - 8,
                    tab.bounds.Y - 12,
                    bounds.X + 32,
                    bounds.Y + (Game1.tileSize / 3),
                    Color.White,
                    drawShadow: false);

                Utility.drawTextWithShadow(
                    b,
                    tab.hoverText,
                    Game1.smallFont,
                    new(
                        this.CurrentMenu.xPositionOnScreen + this.CurrentMenu.width - bounds.X - Game1.tileSize + 8,
                        tab.bounds.Y),
                    Game1.textColor);
                continue;
            }

            b.Draw(
                tab.texture,
                new(tab.bounds.X, tab.bounds.Y),
                new(128, tab.sourceRect.Y, 16, tab.sourceRect.Height),
                Color.Gray,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                0.86f);
            tab.draw(b, Color.Gray, 0.86f + (tab.bounds.Y / 20000f));
        }

        var (x, y) = Game1.getMousePosition(true);
        tab = this.Components.FirstOrDefault(t => t.containsPoint(x, y));
        if (tab is not null && !string.IsNullOrWhiteSpace(tab.hoverText))
        {
            this.CurrentMenu.hoverText = tab.hoverText;
        }
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (this.CurrentMenu is null || !this.Components.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.Components.Any(tab => tab.containsPoint(x, y)))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.Index = this.Index == -1 ? this.Components.Count - 1 : this.Index - 1;
                break;
            case < 0:
                this.Index = this.Index == this.Components.Count - 1 ? -1 : this.Index + 1;
                break;
            default:
                return;
        }
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            { } parentMenu when parentMenu.GetChildMenu() is ItemGrabMenu itemGrabMenu => itemGrabMenu,
            ItemGrabMenu itemGrabMenu => itemGrabMenu,
            _ => null,
        };

        if (menu is not null && ReferenceEquals(menu, this.CurrentMenu))
        {
            return;
        }

        this.CurrentMenu = menu;
        this.Components.Clear();
        if (this.CurrentMenu is null or { shippingBin: true }
            || BetterItemGrabMenu.Context?.ChestMenuTabs is not FeatureOption.Enabled)
        {
            return;
        }

        foreach (var (name, tab) in ChestMenuTabs.AllTabs)
        {
            if (BetterItemGrabMenu.Context.ChestMenuTabSet.Any()
                && !BetterItemGrabMenu.Context.ChestMenuTabSet.Contains(name))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(tab.hoverText))
            {
                tab.hoverText = this.helper.Translation.Get($"tab.{name}.Name").Default(name);
            }

            this.Components.Add(tab);
        }

        this.Components.Sort(
            (c1, c2) => string.Compare(c1.hoverText, c2.hoverText, StringComparison.OrdinalIgnoreCase));

        var bottomRow = this.CurrentMenu.ItemsToGrabMenu.inventory.TakeLast(12).ToArray();
        var topRow = this.CurrentMenu.inventory.inventory.Take(12).ToArray();
        for (var i = 0; i < this.Components.Count; ++i)
        {
            this.Components[i].myID = 69_420 + i;
            this.CurrentMenu.allClickableComponents.Add(this.Components[i]);
            if (i > 0)
            {
                this.Components[i - 1].rightNeighborID = 69_420 + i;
                this.Components[i].leftNeighborID = 69_419 + i;
            }

            if (i < topRow.Length)
            {
                topRow[i].upNeighborID = 69_420 + i;
                this.Components[i].downNeighborID = topRow.ElementAt(i).myID;
            }

            if (i < bottomRow.Length)
            {
                bottomRow[i].downNeighborID = 69_420 + i;
                this.Components[i].upNeighborID = bottomRow.ElementAt(i).myID;
            }

            this.Components[i].bounds.X = i > 0
                ? this.Components[i - 1].bounds.Right
                : this.CurrentMenu.ItemsToGrabMenu.inventory[0].bounds.Left;
            this.Components[i].bounds.Y = this.CurrentMenu.ItemsToGrabMenu.yPositionOnScreen
                + (Game1.tileSize * this.CurrentMenu.ItemsToGrabMenu.rows)
                + IClickableMenu.borderWidth;
        }

        BetterItemGrabMenu.ItemsToGrabMenu?.AddTransformer(this.FilterByTab);
        BetterItemGrabMenu.ItemsToGrabMenu?.AddHighlighter(this.ItemMatcher);
    }
}