#nullable disable

namespace StardewMods.TooManyAnimals;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Integrations.GenericModConfigMenu;
using CommonHarmony.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.TooManyAnimals.Interfaces;
using StardewMods.TooManyAnimals.Models;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <inheritdoc />
public class TooManyAnimals : Mod
{
    private readonly PerScreen<int> _currentPage = new();
    private readonly PerScreen<PurchaseAnimalsMenu> _menu = new();
    private readonly PerScreen<ClickableTextureComponent> _nextPage = new();
    private readonly PerScreen<ClickableTextureComponent> _previousPage = new();

    private static TooManyAnimals Instance { get; set; }

    private ConfigModel Config { get; set; }

    private int CurrentPage
    {
        get => this._currentPage.Value;
        set
        {
            if (this._currentPage.Value == value)
            {
                return;
            }

            this._currentPage.Value = value;
            Game1.activeClickableMenu = new PurchaseAnimalsMenu(this.Stock);
        }
    }

    private PurchaseAnimalsMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private ClickableTextureComponent NextPage
    {
        get => this._nextPage.Value ??= new(
            new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(365, 495, 12, 11),
            Game1.pixelZoom)
        {
            myID = 69420,
        };
    }

    private ClickableTextureComponent PreviousPage
    {
        get => this._previousPage.Value ??= new(
            new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(352, 495, 12, 11),
            Game1.pixelZoom)
        {
            myID = 69421,
        };
    }

    private List<SObject> Stock { get; set; }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        TooManyAnimals.Instance = this;
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);

        // Mod Config
        IConfigData config = null;
        try
        {
            config = this.Helper.ReadConfig<ConfigData>();
        }
        catch (Exception)
        {
            // ignored
        }

        this.Config = new(config ?? new ConfigData(), this.Helper);

        var harmony = new HarmonyHelper();
        harmony.AddPatch(
            this.ModManifest.UniqueID,
            AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new[] { typeof(List<SObject>) }),
            typeof(TooManyAnimals),
            nameof(TooManyAnimals.PurchaseAnimalsMenu_constructor_prefix));
        harmony.ApplyPatches(this.ModManifest.UniqueID);

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private static void PurchaseAnimalsMenu_constructor_prefix(ref List<SObject> stock)
    {
        // Get actual stock
        TooManyAnimals.Instance.Stock ??= stock;

        // Limit stock
        stock = TooManyAnimals.Instance.Stock
                              .Skip(TooManyAnimals.Instance.CurrentPage * TooManyAnimals.Instance.Config.AnimalShopLimit)
                              .Take(TooManyAnimals.Instance.Config.AnimalShopLimit)
                              .ToList();
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        if (e.Button is not SButton.MouseLeft or SButton.MouseRight && !(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.NextPage.containsPoint(x, y) && (this.CurrentPage + 1) * this.Config.AnimalShopLimit < this.Stock.Count)
        {
            this.CurrentPage++;
        }

        if (this.PreviousPage.containsPoint(x, y) && this.CurrentPage > 0)
        {
            this.CurrentPage--;
        }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (Game1.activeClickableMenu is not PurchaseAnimalsMenu || this.Stock is null || this.Stock.Count <= this.Config.AnimalShopLimit)
        {
            return;
        }

        if (this.Config.ControlScheme.NextPage.JustPressed() && (this.CurrentPage + 1) * this.Config.AnimalShopLimit < this.Stock.Count)
        {
            this.CurrentPage++;
            return;
        }

        if (this.Config.ControlScheme.PreviousPage.JustPressed() && this.CurrentPage > 0)
        {
            this.CurrentPage--;
        }
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        if (!gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(this.ModManifest, this.Config.Reset, this.Config.Save);

        gmcm.API.AddSectionTitle(this.ModManifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Animal Shop Limit
        gmcm.API.AddNumberOption(
            this.ModManifest,
            () => this.Config.AnimalShopLimit,
            value => this.Config.AnimalShopLimit = value,
            I18n.Config_AnimalShopLimit_Name,
            I18n.Config_AnimalShopLimit_Tooltip,
            fieldId: nameof(IConfigData.AnimalShopLimit));

        gmcm.API.AddSectionTitle(this.ModManifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Next Page
        gmcm.API.AddKeybindList(
            this.ModManifest,
            () => this.Config.ControlScheme.NextPage,
            value => this.Config.ControlScheme.NextPage = value,
            I18n.Config_NextPage_Name,
            I18n.Config_NextPage_Tooltip,
            nameof(IControlScheme.NextPage));

        // Previous Page
        gmcm.API.AddKeybindList(
            this.ModManifest,
            () => this.Config.ControlScheme.PreviousPage,
            value => this.Config.ControlScheme.PreviousPage = value,
            I18n.Config_PreviousPage_Name,
            I18n.Config_PreviousPage_Tooltip,
            nameof(IControlScheme.PreviousPage));
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        // Reset Stock/CurrentPage
        if (e.NewMenu is not PurchaseAnimalsMenu menu)
        {
            this.Menu = null;
            this.Stock = null;
            this._currentPage.Value = 0;
            return;
        }

        // Reposition Next/Previous Page Buttons
        this.Menu = menu;
        this.NextPage.bounds.X = this.Menu.xPositionOnScreen + this.Menu.width - this.NextPage.bounds.Width;
        this.NextPage.bounds.Y = this.Menu.yPositionOnScreen + this.Menu.height;
        this.NextPage.leftNeighborID = this.PreviousPage.myID;
        this.PreviousPage.bounds.X = this.Menu.xPositionOnScreen;
        this.PreviousPage.bounds.Y = this.Menu.yPositionOnScreen + this.Menu.height;
        this.PreviousPage.rightNeighborID = this.NextPage.myID;

        for (var index = 0; index < this.Menu.animalsToPurchase.Count; index++)
        {
            var i = index + this.CurrentPage * this.Config.AnimalShopLimit;
            if (ReferenceEquals(this.Menu.animalsToPurchase[index].texture, Game1.mouseCursors))
            {
                this.Menu.animalsToPurchase[index].sourceRect.X = i % 3 * 16 * 2;
                this.Menu.animalsToPurchase[index].sourceRect.Y = 448 + i / 3 * 16;
            }

            if (ReferenceEquals(this.Menu.animalsToPurchase[index].texture, Game1.mouseCursors2))
            {
                this.Menu.animalsToPurchase[index].sourceRect.X = 128 + i % 3 * 16 * 2;
                this.Menu.animalsToPurchase[index].sourceRect.Y = i / 3 * 16;
            }
        }

        // Assign neighborId for controller
        var maxY = menu.animalsToPurchase.Max(component => component.bounds.Y);
        var bottomComponents = menu.animalsToPurchase.Where(component => component.bounds.Y == maxY).ToList();
        this.PreviousPage.upNeighborID = bottomComponents.OrderBy(component => Math.Abs(component.bounds.Center.X - this.PreviousPage.bounds.X)).First().myID;
        this.NextPage.upNeighborID = bottomComponents.OrderBy(component => Math.Abs(component.bounds.Center.X - this.NextPage.bounds.X)).First().myID;
        foreach (var component in bottomComponents)
        {
            component.downNeighborID = component.bounds.Center.X <= menu.xPositionOnScreen + menu.width / 2 ? this.PreviousPage.myID : this.NextPage.myID;
        }
    }

    private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
    {
        if (this.Menu is null)
        {
            return;
        }

        if ((this.CurrentPage + 1) * this.Config.AnimalShopLimit < this.Stock.Count)
        {
            this.NextPage.draw(e.SpriteBatch);
        }

        if (this.CurrentPage > 0)
        {
            this.PreviousPage.draw(e.SpriteBatch);
        }
    }
}