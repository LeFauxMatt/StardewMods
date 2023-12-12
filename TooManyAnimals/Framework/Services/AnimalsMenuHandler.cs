namespace StardewMods.TooManyAnimals.Framework.Services;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;

/// <summary>Handles interactions with the Animals Menu.</summary>
internal sealed class AnimalsMenuHandler
{
#nullable disable
    private static AnimalsMenuHandler instance;
#nullable enable

    private readonly PerScreen<List<SObject>?> completeStock = new();
    private readonly ModConfig config;
    private readonly PerScreen<int> currentPage = new();
    private readonly IInputHelper input;

    private readonly PerScreen<ClickableTextureComponent> nextPage = new(
        () => new(
            new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(365, 495, 12, 11),
            Game1.pixelZoom)
        {
            myID = 69420,
        });

    private readonly PerScreen<ClickableTextureComponent> previousPage = new(
        () => new(
            new(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
            Game1.mouseCursors,
            new(352, 495, 12, 11),
            Game1.pixelZoom)
        {
            myID = 69421,
        });

    /// <summary>Initializes a new instance of the <see cref="AnimalsMenuHandler" /> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public AnimalsMenuHandler(ModConfig config, IModEvents events, Harmony harmony, IInputHelper input)
    {
        // Init
        AnimalsMenuHandler.instance = this;
        this.config = config;
        this.input = input;

        // Patches
        harmony.Patch(
            AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new[] { typeof(List<SObject>), typeof(GameLocation) }),
            new(typeof(AnimalsMenuHandler), nameof(AnimalsMenuHandler.PurchaseAnimalsMenu_constructor_prefix)));

        // Events
        events.Display.MenuChanged += this.OnMenuChanged;
        events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        events.Input.ButtonsChanged += this.OnButtonsChanged;
        events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private static void PurchaseAnimalsMenu_constructor_prefix(ref List<SObject> stock)
    {
        // Get actual stock
        AnimalsMenuHandler.instance.completeStock.Value ??= stock;

        // Limit stock
        stock = AnimalsMenuHandler.instance.completeStock.Value
            .Skip(AnimalsMenuHandler.instance.currentPage.Value * AnimalsMenuHandler.instance.config.AnimalShopLimit)
            .Take(AnimalsMenuHandler.instance.config.AnimalShopLimit)
            .ToList();
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.input.IsSuppressed(e.Button) || !this.TryGetStock(out var stock))
        {
            return;
        }

        if (e.Button is not (SButton.MouseLeft or SButton.MouseRight)
            && !(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (this.nextPage.Value.containsPoint(x, y)
            && (this.currentPage.Value + 1) * this.config.AnimalShopLimit < stock.Count)
        {
            this.SetPage(this.currentPage.Value + 1);
            return;
        }

        if (this.previousPage.Value.containsPoint(x, y) && this.currentPage.Value > 0)
        {
            this.SetPage(this.currentPage.Value - 1);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!this.TryGetStock(out var stock))
        {
            return;
        }

        if (this.config.ControlScheme.NextPage.JustPressed()
            && (this.currentPage.Value + 1) * this.config.AnimalShopLimit < stock.Count)
        {
            this.SetPage(this.currentPage.Value + 1);
            return;
        }

        if (this.config.ControlScheme.PreviousPage.JustPressed() && this.currentPage.Value > 0)
        {
            this.SetPage(this.currentPage.Value - 1);
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        // Reset Stock/CurrentPage
        if (e.NewMenu is not PurchaseAnimalsMenu menu)
        {
            this.completeStock.Value = null;
            this.currentPage.Value = 0;
            return;
        }

        // Reposition Next/Previous Page Buttons
        this.nextPage.Value.bounds.X = (menu.xPositionOnScreen + menu.width) - this.nextPage.Value.bounds.Width;
        this.nextPage.Value.bounds.Y = menu.yPositionOnScreen + menu.height;
        this.nextPage.Value.leftNeighborID = this.previousPage.Value.myID;
        this.previousPage.Value.bounds.X = menu.xPositionOnScreen;
        this.previousPage.Value.bounds.Y = menu.yPositionOnScreen + menu.height;
        this.previousPage.Value.rightNeighborID = this.nextPage.Value.myID;

        for (var index = 0; index < menu.animalsToPurchase.Count; ++index)
        {
            var i = index + (this.currentPage.Value * this.config.AnimalShopLimit);
            if (menu.animalsToPurchase[index].texture == Game1.mouseCursors)
            {
                menu.animalsToPurchase[index].sourceRect.X = (i % 3) * 16 * 2;
                menu.animalsToPurchase[index].sourceRect.Y = 448 + ((i / 3) * 16);
            }

            if (menu.animalsToPurchase[index].texture != Game1.mouseCursors2)
            {
                continue;
            }

            menu.animalsToPurchase[index].sourceRect.X = 128 + ((i % 3) * 16 * 2);
            menu.animalsToPurchase[index].sourceRect.Y = (i / 3) * 16;
        }

        // Assign neighborId for controller
        var maxY = menu.animalsToPurchase.Max(component => component.bounds.Y);
        var bottomComponents = menu.animalsToPurchase.Where(component => component.bounds.Y == maxY).ToList();
        this.previousPage.Value.upNeighborID = bottomComponents
            .OrderBy(component => Math.Abs(component.bounds.Center.X - this.previousPage.Value.bounds.X))
            .First()
            .myID;

        this.nextPage.Value.upNeighborID = bottomComponents
            .OrderBy(component => Math.Abs(component.bounds.Center.X - this.nextPage.Value.bounds.X))
            .First()
            .myID;

        foreach (var component in bottomComponents)
        {
            component.downNeighborID = component.bounds.Center.X <= menu.xPositionOnScreen + (menu.width / 2)
                ? this.previousPage.Value.myID
                : this.nextPage.Value.myID;
        }
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!this.TryGetStock(out var stock))
        {
            return;
        }

        if ((this.currentPage.Value + 1) * this.config.AnimalShopLimit < stock.Count)
        {
            this.nextPage.Value.draw(e.SpriteBatch);
        }

        if (this.currentPage.Value > 0)
        {
            this.previousPage.Value.draw(e.SpriteBatch);
        }
    }

    private void SetPage(int page)
    {
        if (this.currentPage.Value == page)
        {
            return;
        }

        this.currentPage.Value = page;
        Game1.activeClickableMenu = new PurchaseAnimalsMenu(this.completeStock.Value);
    }

    private bool TryGetStock([NotNullWhen(true)] out List<SObject>? stock)
    {
        if (Game1.activeClickableMenu is not PurchaseAnimalsMenu
            || this.completeStock.Value is null
            || this.completeStock.Value.Count > this.config.AnimalShopLimit)
        {
            stock = null;
            return false;
        }

        stock = this.completeStock.Value;
        return true;
    }
}
