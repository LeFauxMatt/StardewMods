namespace StardewMods.TooManyAnimals.Framework.Services;

using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley.Menus;

/// <summary>Handles interactions with the Animals Menu.</summary>
internal sealed class AnimalsMenuHandler
{
#nullable disable
    private static AnimalsMenuHandler instance;
#nullable enable

    private readonly ModConfig modConfig;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<List<SObject>?> completeStock = new();
    private readonly PerScreen<int> currentPage = new();

    private readonly PerScreen<ClickableTextureComponent> nextPage = new(
        () => new ClickableTextureComponent(
            new Rectangle(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(365, 495, 12, 11),
            Game1.pixelZoom)
        {
            myID = 69420,
        });

    private readonly PerScreen<ClickableTextureComponent> previousPage = new(
        () => new ClickableTextureComponent(
            new Rectangle(0, 0, 12 * Game1.pixelZoom, 11 * Game1.pixelZoom),
            Game1.mouseCursors,
            new Rectangle(352, 495, 12, 11),
            Game1.pixelZoom)
        {
            myID = 69421,
        });

    /// <summary>Initializes a new instance of the <see cref="AnimalsMenuHandler" /> class.</summary>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public AnimalsMenuHandler(ModConfig modConfig, Harmony harmony, IInputHelper inputHelper, IModEvents modEvents)
    {
        // Init
        AnimalsMenuHandler.instance = this;
        this.modConfig = modConfig;
        this.inputHelper = inputHelper;

        // Patches
        harmony.Patch(
            AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new[] { typeof(List<SObject>), typeof(GameLocation) }),
            new HarmonyMethod(
                typeof(AnimalsMenuHandler),
                nameof(AnimalsMenuHandler.PurchaseAnimalsMenu_constructor_prefix)));

        // Events
        modEvents.Display.MenuChanged += this.OnMenuChanged;
        modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        modEvents.Input.ButtonPressed += this.OnButtonPressed;
    }

    private static void PurchaseAnimalsMenu_constructor_prefix(ref List<SObject> stock)
    {
        // Get actual stock
        AnimalsMenuHandler.instance.completeStock.Value ??= stock;

        // Filter stock
        stock = [];
        foreach (var obj in AnimalsMenuHandler.instance.completeStock.Value)
        {
            if (!Game1.farmAnimalData.TryGetValue(obj.Name, out var animalData) || !animalData.CustomFields.Any())
            {
                stock.Add(obj);
            }

            // Do custom stuff here
        }

        // Paginate stock
        stock = stock
            .Skip(AnimalsMenuHandler.instance.currentPage.Value * AnimalsMenuHandler.instance.modConfig.AnimalShopLimit)
            .Take(AnimalsMenuHandler.instance.modConfig.AnimalShopLimit)
            .ToList();
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.inputHelper.IsSuppressed(e.Button) || !this.TryGetStock(out var stock))
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
            && (this.currentPage.Value + 1) * this.modConfig.AnimalShopLimit < stock.Count)
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

        if (this.modConfig.ControlScheme.NextPage.JustPressed()
            && (this.currentPage.Value + 1) * this.modConfig.AnimalShopLimit < stock.Count)
        {
            this.SetPage(this.currentPage.Value + 1);
            return;
        }

        if (this.modConfig.ControlScheme.PreviousPage.JustPressed() && this.currentPage.Value > 0)
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
        this.nextPage.Value.bounds.X = menu.xPositionOnScreen + menu.width - this.nextPage.Value.bounds.Width;
        this.nextPage.Value.bounds.Y = menu.yPositionOnScreen + menu.height;
        this.nextPage.Value.leftNeighborID = this.previousPage.Value.myID;
        this.previousPage.Value.bounds.X = menu.xPositionOnScreen;
        this.previousPage.Value.bounds.Y = menu.yPositionOnScreen + menu.height;
        this.previousPage.Value.rightNeighborID = this.nextPage.Value.myID;

        for (var index = 0; index < menu.animalsToPurchase.Count; ++index)
        {
            var i = index + (this.currentPage.Value * this.modConfig.AnimalShopLimit);
            if (menu.animalsToPurchase[index].texture == Game1.mouseCursors)
            {
                menu.animalsToPurchase[index].sourceRect.X = i % 3 * 16 * 2;
                menu.animalsToPurchase[index].sourceRect.Y = 448 + (i / 3 * 16);
            }

            if (menu.animalsToPurchase[index].texture != Game1.mouseCursors2)
            {
                continue;
            }

            menu.animalsToPurchase[index].sourceRect.X = 128 + (i % 3 * 16 * 2);
            menu.animalsToPurchase[index].sourceRect.Y = i / 3 * 16;
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

        if ((this.currentPage.Value + 1) * this.modConfig.AnimalShopLimit < stock.Count)
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
            || this.completeStock.Value.Count > this.modConfig.AnimalShopLimit)
        {
            stock = null;
            return false;
        }

        stock = this.completeStock.Value;
        return true;
    }
}