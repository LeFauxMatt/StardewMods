namespace StardewMods.TooManyAnimals.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.TooManyAnimals.Interfaces;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class AnimalMenuHandler : IModService
{
    private readonly PerScreen<int> _currentPage = new();
    private readonly PerScreen<PurchaseAnimalsMenu> _menu = new();
    private readonly PerScreen<IClickableComponent> _nextPage = new();
    private readonly PerScreen<IClickableComponent> _previousPage = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AnimalMenuHandler" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public AnimalMenuHandler(IConfigData config, IModHelper helper, IModServices services)
    {
        AnimalMenuHandler.Instance = this;
        this.Config = config;
        this.Helper = helper;

        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{TooManyAnimals.ModUniqueId}.{nameof(AnimalMenuHandler)}";
                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new[] { typeof(List<SObject>) }),
                    typeof(AnimalMenuHandler),
                    nameof(AnimalMenuHandler.PurchaseAnimalsMenu_constructor_prefix));
                harmonyHelper.ApplyPatches(id);
            });
        services.Lazy<ICustomEvents>(
            customEvents =>
            {
                customEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
                customEvents.MenuComponentsLoading += this.OnMenuComponentsLoading;
                customEvents.MenuComponentPressed += this.OnMenuComponentPressed;
            });

        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    private static AnimalMenuHandler Instance { get; set; }

    private IConfigData Config { get; }

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

    private IModHelper Helper { get; }

    private PurchaseAnimalsMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private IClickableComponent NextPage
    {
        get => this._nextPage.Value ??= new CustomClickableComponent(
            new(
                new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
                Game1.mouseCursors,
                new(365, 495, 12, 11),
                Game1.pixelZoom));
    }

    private IClickableComponent PreviousPage
    {
        get => this._previousPage.Value ??= new CustomClickableComponent(
            new(
                new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
                Game1.mouseCursors,
                new(352, 495, 12, 11),
                Game1.pixelZoom));
    }

    private List<SObject> Stock { get; set; }

    private static void PurchaseAnimalsMenu_constructor_prefix(ref List<SObject> stock)
    {
        // Get actual stock
        AnimalMenuHandler.Instance.Stock ??= stock;

        // Limit stock
        stock = AnimalMenuHandler.Instance.Stock
                                 .Skip(AnimalMenuHandler.Instance.CurrentPage * AnimalMenuHandler.Instance.Config.AnimalShopLimit)
                                 .Take(AnimalMenuHandler.Instance.Config.AnimalShopLimit)
                                 .ToList();
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

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        // Reset Stock/CurrentPage
        if (e.Menu is not PurchaseAnimalsMenu menu)
        {
            this.Menu = null;
            this.Stock = null;
            this._currentPage.Value = 0;
            return;
        }

        // Reposition Next/Previous Page Buttons
        this.Menu = menu;
        this.NextPage.X = this.Menu.xPositionOnScreen + this.Menu.width - this.NextPage.Component.bounds.Width;
        this.NextPage.Y = this.Menu.yPositionOnScreen + this.Menu.height;
        this.NextPage.IsVisible = (this.CurrentPage + 1) * this.Config.AnimalShopLimit < this.Stock.Count;
        this.NextPage.Component.leftNeighborID = this.PreviousPage.Id;
        this.PreviousPage.X = this.Menu.xPositionOnScreen;
        this.PreviousPage.Y = this.Menu.yPositionOnScreen + this.Menu.height;
        this.PreviousPage.IsVisible = this.CurrentPage > 0;
        this.PreviousPage.Component.rightNeighborID = this.NextPage.Id;

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
    }

    private void OnMenuComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (ReferenceEquals(e.Component, this.PreviousPage) && this.CurrentPage > 0)
        {
            this.CurrentPage--;
        }
        else if (ReferenceEquals(e.Component, this.NextPage) && (this.CurrentPage + 1) * this.Config.AnimalShopLimit < this.Stock.Count)
        {
            this.CurrentPage++;
        }
    }

    private void OnMenuComponentsLoading(object sender, MenuComponentsLoadingEventArgs e)
    {
        if (e.Menu is not PurchaseAnimalsMenu menu)
        {
            return;
        }

        e.AddComponent(this.NextPage);
        e.AddComponent(this.PreviousPage);

        // Assign neighborId for controller
        var maxY = menu.animalsToPurchase.Max(component => component.bounds.Y);
        var bottomComponents = menu.animalsToPurchase.Where(component => component.bounds.Y == maxY).ToList();
        this.PreviousPage.Component.upNeighborID = bottomComponents.OrderBy(component => Math.Abs(component.bounds.Center.X - this.PreviousPage.X)).First().myID;
        this.NextPage.Component.upNeighborID = bottomComponents.OrderBy(component => Math.Abs(component.bounds.Center.X - this.NextPage.X)).First().myID;
        foreach (var component in bottomComponents)
        {
            component.downNeighborID = component.bounds.Center.X <= menu.xPositionOnScreen + menu.width / 2 ? this.PreviousPage.Id : this.NextPage.Id;
        }
    }
}