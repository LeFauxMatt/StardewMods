namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Models;
using StardewMods.Common.Enums;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Adds tabs to the <see cref="ItemGrabMenu" /> to filter the displayed items.
/// </summary>
internal class ChestMenuTabs : IFeature
{
    private static Dictionary<string, ClickableTextureComponent>? CachedTabs;

    private readonly PerScreen<ItemMatcher?> _itemMatcher = new();
    private readonly PerScreen<int> _tabIndex = new(() => -1);
    private readonly PerScreen<List<ClickableTextureComponent>?> _tabs = new();

    private ChestMenuTabs(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static ChestMenuTabs? Instance { get; set; }

    private Dictionary<string, ClickableTextureComponent> AllTabs
    {
        get
        {
            if (ChestMenuTabs.CachedTabs is not null)
            {
                return ChestMenuTabs.CachedTabs;
            }

            var tabs = this.Helper.GameContent.Load<Dictionary<string, string>>("furyx639.BetterChests/Tabs");
            if (!tabs.Any())
            {
                tabs = new()
                {
                    {
                        "Clothing",
                        "/furyx639.BetterChests\\Tabs\\Texture/0/category_clothing category_boots category_hat"
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
                    {
                        "Seeds",
                        "/furyx639.BetterChests\\Tabs\\Texture/7/category_seeds category_fertilizer"
                    },
                };
                this.Helper.Data.WriteJsonFile("assets/tabs.json", tabs);
            }

            return ChestMenuTabs.CachedTabs ??= (
                    from tab in
                        from tab in tabs
                        select (tab.Key, Value: tab.Value.Split('/'))
                    select (tab.Key, Value: new ClickableTextureComponent(
                        tab.Value[3],
                        new(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                        string.Empty,
                        tab.Value[0],
                        this.Helper.GameContent.Load<Texture2D>(tab.Value[1]),
                        new(16 * int.Parse(tab.Value[2]), 0, 16, 16),
                        Game1.pixelZoom)))
                .ToDictionary(tab => tab.Key, tab => tab.Value);
        }
    }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private int Index
    {
        get => this._tabIndex.Value;
        set
        {
            this._tabIndex.Value = value;
            this.ItemMatcher.Clear();
            if (value == -1 || this.Tabs is null || !this.Tabs.Any())
            {
                BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
                return;
            }

            var tab = this.Tabs[value];
            var tags = tab.name.Split(' ');
            foreach (var tag in tags)
            {
                this.ItemMatcher.Add(tag);
            }

            BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        }
    }

    private bool IsActivated { get; set; }

    private ItemMatcher ItemMatcher
    {
        get => this._itemMatcher.Value ??= new(true);
    }

    private List<ClickableTextureComponent>? Tabs
    {
        get => this._tabs.Value;
        set => this._tabs.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="ChestMenuTabs" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestMenuTabs" /> class.</returns>
    public static ChestMenuTabs Init(IModHelper helper, ModConfig config)
    {
        return ChestMenuTabs.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
            this.Helper.Events.Display.RenderingActiveMenu -= this.OnRenderingActiveMenu;
            this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
        }
    }

    private IEnumerable<Item> FilterByTab(IEnumerable<Item> items)
    {
        return items.Where(this.ItemMatcher.Matches);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu || this.Tabs is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var tab = this.Tabs.FirstOrDefault(tab => tab.containsPoint(x, y));
        var index = tab is not null ? this.Tabs.IndexOf(tab) : -1;
        switch (e.Button)
        {
            case SButton.MouseLeft when index != -1:
                this.Index = this.Index == index ? -1 : index;
                break;
            case SButton.MouseRight when index != -1:
                this.Index = this.Index == index ? -1 : index;
                break;
            default:
                return;
        }

        this.Helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu || this.Tabs is null)
        {
            return;
        }

        if (this.Config.ControlScheme.PreviousTab.JustPressed())
        {
            this.Index = this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.PreviousTab);
        }

        if (this.Config.ControlScheme.NextTab.JustPressed())
        {
            this.Index = this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.NextTab);
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.NewMenu is not ItemGrabMenu { context: { } context }
            || !StorageHelper.TryGetOne(context, out var storage)
            || storage.ChestMenuTabs == FeatureOption.Disabled)
        {
            this.Tabs = null;
            return;
        }

        var tabs = storage.ChestMenuTabSet.Any()
            ? this.AllTabs.Where(tab => storage.ChestMenuTabSet.Contains(tab.Key))
            : this.AllTabs;

        this.Tabs = tabs
                    .Select(tab =>
                    {
                        if (string.IsNullOrWhiteSpace(tab.Value.hoverText))
                        {
                            tab.Value.hoverText = this.Helper.Translation.Get($"tab.{tab.Key}.Name").Default(tab.Key);
                        }

                        return tab.Value;
                    })
                    .OrderBy(tab => tab.hoverText)
                    .ToList();

        ClickableTextureComponent? prevTab = null;
        foreach (var tab in this.Tabs)
        {
            if (prevTab is not null)
            {
                prevTab.rightNeighborID = tab.myID;
                tab.leftNeighborID = prevTab.myID;
            }

            prevTab = tab;
        }

        BetterItemGrabMenu.ItemsToGrabMenu?.AddTransformer(this.FilterByTab);
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu || this.Tabs is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.Tabs.Any(tab => tab.containsPoint(x, y)))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.Index = this.Index == -1 ? this.Tabs.Count - 1 : this.Index - 1;
                break;
            case < 0:
                this.Index = this.Index == this.Tabs.Count - 1 ? -1 : this.Index + 1;
                break;
            default:
                return;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu || this.Tabs is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var tab = this.Tabs.FirstOrDefault(tab => tab.containsPoint(x, y));
        if (tab is not null && !string.IsNullOrWhiteSpace(tab.hoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, tab.hoverText, Game1.smallFont);
        }

        itemGrabMenu.drawMouse(e.SpriteBatch);
    }

    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu || this.Tabs is null)
        {
            return;
        }

        e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

        ClickableTextureComponent? prevTab = null;
        for (var index = 0; index < this.Tabs.Count; index++)
        {
            var tab = this.Tabs[index];
            tab.bounds.X = prevTab?.bounds.Right ?? itemGrabMenu.ItemsToGrabMenu.inventory[0].bounds.Left;
            tab.bounds.Y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen + Game1.tileSize * itemGrabMenu.ItemsToGrabMenu.rows + IClickableMenu.borderWidth - 16;
            prevTab = tab;
            if (index == this.Index)
            {
                tab.bounds.Y += Game1.pixelZoom;
                tab.draw(e.SpriteBatch, Color.White, 0.86f + tab.bounds.Y / 20000f);
                continue;
            }

            tab.draw(e.SpriteBatch, Color.Gray, 0.86f + tab.bounds.Y / 20000f);
        }
    }
}