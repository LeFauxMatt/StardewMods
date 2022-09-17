namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.AtraBase.StringHandlers;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Menus;

/// <summary>
///     Adds tabs to the <see cref="ItemGrabMenu" /> to filter the displayed items.
/// </summary>
internal sealed class ChestMenuTabs : IFeature
{
#nullable disable
    private static ChestMenuTabs Instance;
#nullable enable

    private readonly Lazy<Dictionary<string, ClickableTextureComponent>> _allTabs;
    private readonly ModConfig _config;
    private readonly PerScreen<ItemGrabMenu?> _currentMenu = new();
    private readonly IModHelper _helper;
    private readonly PerScreen<IItemMatcher> _itemMatcher = new(() => new ItemMatcher(true));
    private readonly PerScreen<int> _tabIndex = new(() => -1);
    private readonly PerScreen<List<ClickableTextureComponent>> _tabs = new(() => new());

    private bool _isActivated;

    private ChestMenuTabs(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        this._allTabs = new(
            () =>
            {
                return (
                    from tab in Game1.content.Load<Dictionary<string, string>>("furyx639.BetterChests/Tabs")
                    let span = new SpanSplit(tab.Value, '/')
                    select (
                        tab.Key,
                        Value: new ClickableTextureComponent(
                            span[3],
                            new(0, 0, 16 * Game1.pixelZoom, 13 * Game1.pixelZoom),
                            string.Empty,
                            !string.IsNullOrWhiteSpace(span[0])
                                ? span[0]
                                : helper.Translation.Get($"tabs.{tab.Key}.name").Default(tab.Key),
                            Game1.content.Load<Texture2D>(span[1]),
                            new(16 * int.Parse(span[2]), 4, 16, 12),
                            Game1.pixelZoom))).ToDictionary(tab => tab.Key, tab => tab.Value);
            });
    }

    private static Dictionary<string, ClickableTextureComponent> AllTabs => ChestMenuTabs.Instance._allTabs.Value;

    private List<ClickableTextureComponent> Components => this._tabs.Value;

    private ItemGrabMenu? CurrentMenu
    {
        get => this._currentMenu.Value;
        set => this._currentMenu.Value = value;
    }

    private int Index
    {
        get => this._tabIndex.Value;
        set
        {
            this._tabIndex.Value = value;
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

    private IItemMatcher ItemMatcher => this._itemMatcher.Value;

    private Dictionary<string, string> Tabs
    {
        get
        {
            var tabs = this._helper.Data.ReadJsonFile<Dictionary<string, string>>("assets/tabs.json");
            if (tabs is not null && tabs.Any())
            {
                return tabs;
            }

            tabs = new()
            {
                {
                    "Clothing", "/furyx639.BetterChests\\Tabs\\Texture/0/category_clothing category_boots category_hat"
                },
                {
                    "Cooking",
                    "/furyx639.BetterChests\\Tabs\\Texture/1/category_syrup category_artisan_goods category_ingredients category_sell_at_pierres_and_marnies category_sell_at_pierres category_meat category_cooking category_milk category_egg"
                },
                {
                    "Crops",
                    "/furyx639.BetterChests\\Tabs\\Texture/2/category_greens category_flowers category_fruits category_vegetable"
                },
                {
                    "Equipment",
                    "/furyx639.BetterChests\\Tabs\\Texture/3/category_equipment category_ring category_tool category_weapon"
                },
                {
                    "Fishing",
                    "/furyx639.BetterChests\\Tabs\\Texture/4/category_bait category_fish category_tackle category_sell_at_fish_shop"
                },
                {
                    "Materials",
                    "/furyx639.BetterChests\\Tabs\\Texture/5/category_monster_loot category_metal_resources category_building_resources category_minerals category_crafting category_gem"
                },
                {
                    "Misc",
                    "/furyx639.BetterChests\\Tabs\\Texture/6/category_big_craftable category_furniture category_junk"
                },
                { "Seeds", "/furyx639.BetterChests\\Tabs\\Texture/7/category_seeds category_fertilizer" },
            };

            this._helper.Data.WriteJsonFile("assets/tabs.json", tabs);

            return tabs;
        }
    }

    /// <summary>
    ///     Initializes <see cref="ChestMenuTabs" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestMenuTabs" /> class.</returns>
    public static IFeature Init(IModHelper helper, ModConfig config)
    {
        return ChestMenuTabs.Instance ??= new(helper, config);
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
        this._helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this._helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this._helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
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
        this._helper.Events.Content.AssetRequested -= this.OnAssetRequested;
        this._helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this._helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    private IEnumerable<Item> FilterByTab(IEnumerable<Item> items)
    {
        if (this._config.HideItems is FeatureOption.Enabled)
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

        this._helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.CurrentMenu is null || !this.Components.Any())
        {
            return;
        }

        if (this._config.ControlScheme.PreviousTab.JustPressed())
        {
            this.Index = this.Index == -1 ? this.Components.Count - 1 : this.Index - 1;
            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.PreviousTab);
        }

        if (this._config.ControlScheme.NextTab.JustPressed())
        {
            this.Index = this.Index == this.Components.Count - 1 ? -1 : this.Index + 1;
            this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.NextTab);
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
                tab.draw(b, Color.White, 0.86f + tab.bounds.Y / 20000f);

                // draw texture
                var bounds = Game1.smallFont.MeasureString(tab.hoverText).ToPoint();
                IClickableMenu.drawTextureBox(
                    b,
                    Game1.menuTexture,
                    new(0, 256, 60, 60),
                    this.CurrentMenu.xPositionOnScreen + this.CurrentMenu.width - bounds.X - Game1.tileSize - 8,
                    tab.bounds.Y - 12,
                    bounds.X + 32,
                    bounds.Y + Game1.tileSize / 3,
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
            tab.draw(b, Color.Gray, 0.86f + tab.bounds.Y / 20000f);
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
            { } clickableMenu when clickableMenu.GetChildMenu() is ItemGrabMenu itemGrabMenu => itemGrabMenu,
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

        var tabs = BetterItemGrabMenu.Context.ChestMenuTabSet.Any()
            ? ChestMenuTabs.AllTabs.Where(tab => BetterItemGrabMenu.Context.ChestMenuTabSet.Contains(tab.Key))
            : ChestMenuTabs.AllTabs;

        this.Components.AddRange(
            tabs.Select(
                    (tab, index) =>
                    {
                        tab.Value.myID = 69420 + index;

                        if (string.IsNullOrWhiteSpace(tab.Value.hoverText))
                        {
                            tab.Value.hoverText = this._helper.Translation.Get($"tab.{tab.Key}.Name").Default(tab.Key);
                        }

                        return tab.Value;
                    })
                .OrderBy(tab => tab.hoverText));

        ClickableTextureComponent? prevTab = null;
        var bottomRow = this.CurrentMenu.ItemsToGrabMenu.inventory.TakeLast(12).ToArray();
        var topRow = this.CurrentMenu.inventory.inventory.Take(12).ToArray();
        for (var index = 0; index < this.Components.Count; ++index)
        {
            var tab = this.Components.ElementAt(index);
            this.CurrentMenu.allClickableComponents.Add(tab);
            if (prevTab is not null)
            {
                prevTab.rightNeighborID = tab.myID;
                tab.leftNeighborID = prevTab.myID;
            }

            if (index < topRow.Length)
            {
                topRow.ElementAt(index).upNeighborID = tab.myID;
                tab.downNeighborID = topRow.ElementAt(index).myID;
            }

            if (index < bottomRow.Length)
            {
                bottomRow.ElementAt(index).downNeighborID = tab.myID;
                tab.upNeighborID = bottomRow.ElementAt(index).myID;
            }

            tab.bounds.X = prevTab?.bounds.Right ?? this.CurrentMenu.ItemsToGrabMenu.inventory[0].bounds.Left;
            tab.bounds.Y = this.CurrentMenu.ItemsToGrabMenu.yPositionOnScreen
                         + Game1.tileSize * this.CurrentMenu.ItemsToGrabMenu.rows
                         + IClickableMenu.borderWidth;
            prevTab = tab;
        }

        BetterItemGrabMenu.ItemsToGrabMenu?.AddTransformer(this.FilterByTab);
        BetterItemGrabMenu.ItemsToGrabMenu?.AddHighlighter(this.ItemMatcher);
    }
}