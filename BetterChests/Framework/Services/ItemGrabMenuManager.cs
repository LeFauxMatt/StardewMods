namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Extensions;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Manages the item grab menu in the game.</summary>
internal sealed class ItemGrabMenuManager : BaseService
{
#nullable disable
    private static ItemGrabMenuManager instance;
#nullable enable

    private readonly PerScreen<InventoryMenuManager> bottomMenu;
    private readonly ContainerFactory containerFactory;
    private readonly PerScreen<IClickableMenu?> currentMenu = new();
    private readonly IModConfig modConfig;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<InventoryMenuManager> topMenu;

    private EventHandler<ItemGrabMenuChangedEventArgs>? itemGrabMenuChanged;

    /// <summary>Initializes a new instance of the <see cref="ItemGrabMenuManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    public ItemGrabMenuManager(
        ILog log,
        IModConfig modConfig,
        IModEvents modEvents,
        Harmony harmony,
        IInputHelper inputHelper,
        ContainerFactory containerFactory)
        : base(log)
    {
        // Init
        ItemGrabMenuManager.instance = this;
        this.modConfig = modConfig;
        this.inputHelper = inputHelper;
        this.containerFactory = containerFactory;
        this.topMenu = new PerScreen<InventoryMenuManager>(() => new InventoryMenuManager(log));
        this.bottomMenu = new PerScreen<InventoryMenuManager>(() => new InventoryMenuManager(log));

        // Events
        modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
        modEvents.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
        modEvents.GameLoop.UpdateTicked += this.OnUpdateTicked;
        modEvents.GameLoop.UpdateTicking += this.OnUpdateTicking;
        modEvents.Input.ButtonPressed += this.OnButtonPressed;
        modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        modEvents.Input.CursorMoved += this.OnCursorMoved;
        modEvents.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

        // Patches
        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(InventoryMenu),
                nameof(InventoryMenu.draw),
                [typeof(SpriteBatch), typeof(int), typeof(int), typeof(int)]),
            new HarmonyMethod(typeof(ItemGrabMenuManager), nameof(ItemGrabMenuManager.InventoryMenu_draw_prefix)),
            new HarmonyMethod(typeof(ItemGrabMenuManager), nameof(ItemGrabMenuManager.InventoryMenu_draw_postfix)));

        harmony.Patch(
            AccessTools.GetDeclaredConstructors(typeof(ItemGrabMenu)).Single(ctor => ctor.GetParameters().Length > 5),
            transpiler: new HarmonyMethod(
                typeof(ItemGrabMenuManager),
                nameof(ItemGrabMenuManager.ItemGrabMenu_constructor_transpiler)));
    }

    /// <summary>Gets the current item grab menu.</summary>
    public ItemGrabMenu? CurrentMenu =>
        Game1.activeClickableMenu?.Equals(this.currentMenu.Value) == true
            ? this.currentMenu.Value as ItemGrabMenu
            : null;

    /// <summary>Gets the inventory menu manager for the top inventory menu.</summary>
    public IInventoryMenuManager Top => this.topMenu.Value;

    /// <summary>Gets the inventory menu manager for the bottom inventory menu.</summary>
    public IInventoryMenuManager Bottom => this.bottomMenu.Value;

    /// <summary>Event raised when the item grab menu has changed.</summary>
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged
    {
        add => this.itemGrabMenuChanged += value;
        remove => this.itemGrabMenuChanged -= value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_prefix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(ItemGrabMenuManager.instance.topMenu.Value.Menu)
            ? ItemGrabMenuManager.instance.topMenu.Value
            : __instance.Equals(ItemGrabMenuManager.instance.bottomMenu.Value.Menu)
                ? ItemGrabMenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Apply operations
        __instance.actualInventory = __state.ApplyOperation(__state.Container.Items).ToList();
        for (var index = 0; index < __instance.inventory.Count; ++index)
        {
            if (index >= __instance.actualInventory.Count)
            {
                __instance.inventory[index].name = int.MaxValue.ToString(CultureInfo.InvariantCulture);

                continue;
            }

            __instance.inventory[index].name = __state
                .Container.Items.IndexOf(__instance.actualInventory[index])
                .ToString(CultureInfo.InvariantCulture);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_draw_postfix(InventoryMenu __instance, ref InventoryMenuManager? __state)
    {
        __state = __instance.Equals(ItemGrabMenuManager.instance.topMenu.Value.Menu)
            ? ItemGrabMenuManager.instance.topMenu.Value
            : __instance.Equals(ItemGrabMenuManager.instance.bottomMenu.Value.Menu)
                ? ItemGrabMenuManager.instance.bottomMenu.Value
                : null;

        if (__state?.Container is null)
        {
            return;
        }

        // Restore original
        __instance.actualInventory = __state.Container.Items;
    }

    private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var counter = 0;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)))
                && ++counter == 2)
            {
                yield return instruction;
                yield return CodeInstruction.Call(
                    typeof(ItemGrabMenuManager),
                    nameof(ItemGrabMenuManager.GetActualCapacity));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static int GetActualCapacity(int capacity) => capacity > 70 ? 70 : capacity;

    private void OnUpdateTicking(object? sender, UpdateTickingEventArgs e) => this.UpdateMenu();

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) => this.UpdateMenu();

    private void UpdateHighlightMethods()
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        if (this.CurrentMenu.ItemsToGrabMenu.highlightMethod != this.topMenu.Value.HighlightMethod)
        {
            this.topMenu.Value.OriginalHighlightMethod = this.CurrentMenu.ItemsToGrabMenu.highlightMethod;
            this.CurrentMenu.ItemsToGrabMenu.highlightMethod = this.topMenu.Value.HighlightMethod;
        }

        if (this.CurrentMenu.inventory.highlightMethod != this.bottomMenu.Value.HighlightMethod)
        {
            this.bottomMenu.Value.OriginalHighlightMethod = this.CurrentMenu.inventory.highlightMethod;
            this.CurrentMenu.inventory.highlightMethod = this.bottomMenu.Value.HighlightMethod;
        }
    }

    private void UpdateMenu()
    {
        if (Game1.activeClickableMenu == this.currentMenu.Value)
        {
            this.UpdateHighlightMethods();
            return;
        }

        this.currentMenu.Value = Game1.activeClickableMenu;
        if (Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
        {
            this.topMenu.Value.Reset(null, null);
            this.bottomMenu.Value.Reset(null, null);
            this.itemGrabMenuChanged.InvokeAll(this, new ItemGrabMenuChangedEventArgs());
            return;
        }

        // Update top menu
        this.topMenu.Value.Reset(itemGrabMenu, itemGrabMenu.ItemsToGrabMenu);
        if (itemGrabMenu.context is not Chest chest || !this.containerFactory.TryGetOne(chest, out var topContainer))
        {
            topContainer = null;
        }

        this.topMenu.Value.Container = topContainer;

        // Update bottom menu
        this.bottomMenu.Value.Reset(itemGrabMenu, itemGrabMenu.inventory);
        if (!itemGrabMenu.inventory.actualInventory.Equals(Game1.player.Items)
            || !this.containerFactory.TryGetOne(Game1.player, out var bottomContainer))
        {
            bottomContainer = null;
        }

        this.bottomMenu.Value.Container = bottomContainer;

        // Reset filters
        this.UpdateHighlightMethods();
        this.itemGrabMenuChanged.InvokeAll(this, new ItemGrabMenuChangedEventArgs());

        // Disable background fade
        itemGrabMenu.drawBG = false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        switch (e.Button)
        {
            case SButton.MouseLeft when this.topMenu.Value.LeftClick(mouseX, mouseY): break;
            case SButton.MouseLeft when this.bottomMenu.Value.LeftClick(mouseX, mouseY): break;
            default: return;
        }

        this.inputHelper.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        if (this.modConfig.Controls.ScrollUp.JustPressed())
        {
            this.Top.Scrolled--;
            this.inputHelper.SuppressActiveKeybinds(this.modConfig.Controls.ScrollUp);
        }

        if (this.modConfig.Controls.ScrollDown.JustPressed())
        {
            this.Top.Scrolled++;
            this.inputHelper.SuppressActiveKeybinds(this.modConfig.Controls.ScrollDown);
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        this.topMenu.Value.Hover(mouseX, mouseY);
        this.bottomMenu.Value.Hover(mouseX, mouseY);
    }

    private void OnMouseWheelScrolled(object? sender, MouseWheelScrolledEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (this.Top.Menu?.isWithinBounds(mouseX, mouseY) == true)
        {
            var scroll = this.modConfig.Controls.ScrollPage.IsDown() ? this.Top.Rows : 1;
            this.Top.Scrolled += e.Delta > 0 ? -scroll : scroll;
        }

        if (this.Bottom.Menu?.isWithinBounds(mouseX, mouseY) == true)
        {
            var scroll = this.modConfig.Controls.ScrollPage.IsDown() ? this.Bottom.Rows : 1;
            this.Bottom.Scrolled += e.Delta > 0 ? -scroll : scroll;
        }
    }

    [EventPriority((EventPriority)int.MaxValue)]
    private void OnRenderingActiveMenu(object? sender, RenderingActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        // Redraw background
        e.SpriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.5f);
    }

    [EventPriority((EventPriority)int.MinValue)]
    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (this.CurrentMenu is null)
        {
            return;
        }

        // Draw overlay
        this.topMenu.Value.Draw(e.SpriteBatch);
        this.bottomMenu.Value.Draw(e.SpriteBatch);

        // Redraw foreground
        if (this.CurrentMenu.hoverText != null
            && (this.CurrentMenu.hoveredItem == null || this.CurrentMenu.ItemsToGrabMenu == null))
        {
            if (this.CurrentMenu.hoverAmount > 0)
            {
                IClickableMenu.drawToolTip(
                    e.SpriteBatch,
                    this.CurrentMenu.hoverText,
                    string.Empty,
                    null,
                    true,
                    -1,
                    0,
                    null,
                    -1,
                    null,
                    this.CurrentMenu.hoverAmount);
            }
            else
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, this.CurrentMenu.hoverText, Game1.smallFont);
            }
        }

        if (this.CurrentMenu.hoveredItem != null)
        {
            IClickableMenu.drawToolTip(
                e.SpriteBatch,
                this.CurrentMenu.hoveredItem.getDescription(),
                this.CurrentMenu.hoveredItem.DisplayName,
                this.CurrentMenu.hoveredItem,
                this.CurrentMenu.heldItem != null);
        }
        else if (this.CurrentMenu.hoveredItem != null && this.CurrentMenu.ItemsToGrabMenu != null)
        {
            IClickableMenu.drawToolTip(
                e.SpriteBatch,
                this.CurrentMenu.ItemsToGrabMenu.descriptionText,
                this.CurrentMenu.ItemsToGrabMenu.descriptionTitle,
                this.CurrentMenu.hoveredItem,
                this.CurrentMenu.heldItem != null);
        }

        this.CurrentMenu.heldItem?.drawInMenu(
            e.SpriteBatch,
            new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8),
            1f);

        Game1.mouseCursorTransparency = 1f;
        this.CurrentMenu.drawMouse(e.SpriteBatch);
    }
}