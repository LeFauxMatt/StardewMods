namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
internal class ResizeChest : Feature
{
    private readonly PerScreen<IMenuComponent> _downArrow = new();
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly Lazy<IMenuComponents> _menuComponents;
    private readonly Lazy<IMenuItems> _menuItems;
    private readonly PerScreen<IMenuComponent> _upArrow = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeChest"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ResizeChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        ResizeChest.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
                    AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    typeof(ResizeChest),
                    nameof(ResizeChest.Chest_GetActualCapacity_postfix),
                    PatchType.Postfix);
            });
        this._menuComponents = services.Lazy<IMenuComponents>();
        this._menuItems = services.Lazy<IMenuItems>();
    }

    private static ResizeChest Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private IMenuComponents MenuComponents
    {
        get => this._menuComponents.Value;
    }

    private IMenuItems MenuItems
    {
        get => this._menuItems.Value;
    }

    private IMenuComponent UpArrow
    {
        get => this._upArrow.Value ??= new CustomMenuComponent(new(new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new(421, 459, 11, 12), Game1.pixelZoom));
    }

    private IMenuComponent DownArrow
    {
        get => this._downArrow.Value ??= new CustomMenuComponent(new(new(0, 0, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), Game1.mouseCursors, new(421, 472, 11, 12), Game1.pixelZoom));
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
        this.Helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
    {
        if (ResizeChest.Instance.ManagedChests.FindChest(__instance, out var managedChest) && managedChest.ResizeChestCapacity != 0)
        {
            __result = managedChest.ResizeChestCapacity > 0
                ? managedChest.ResizeChestCapacity
                : int.MaxValue;
        }
    }

    [SortedEventPriority(EventPriority.High)]
    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.MenuComponents.Menu is not null)
        {
            // Add Up/Down Arrows
            this.MenuComponents.Components.Add(this.UpArrow);
            this.MenuComponents.Components.Add(this.DownArrow);

            // Initialize Arrow visibility
            this.UpArrow.Component.visible = this.MenuItems.Offset > 0;
            this.DownArrow.Component.visible = this.MenuItems.Offset < this.MenuItems.Rows;

            // Align to ItemsToGrabMenu top/bottom inventory slots
            var topSlot = this.MenuComponents.Menu.GetColumnCount() - 1;
            var bottomSlot = this.MenuComponents.Menu.ItemsToGrabMenu.capacity - 1;
            this.UpArrow.Component.bounds.X = this.MenuComponents.Menu.ItemsToGrabMenu.xPositionOnScreen + this.MenuComponents.Menu.ItemsToGrabMenu.width + 8;
            this.UpArrow.Component.bounds.Y = this.MenuComponents.Menu.ItemsToGrabMenu.inventory[topSlot].bounds.Center.Y - (6 * Game1.pixelZoom);
            this.DownArrow.Component.bounds.X = this.MenuComponents.Menu.ItemsToGrabMenu.xPositionOnScreen + this.MenuComponents.Menu.ItemsToGrabMenu.width + 8;
            this.DownArrow.Component.bounds.Y = this.MenuComponents.Menu.ItemsToGrabMenu.inventory[bottomSlot].bounds.Center.Y - (6 * Game1.pixelZoom);

            // Assign Neighbor IDs
            this.UpArrow.Component.leftNeighborID = this.MenuComponents.Menu.ItemsToGrabMenu.inventory[topSlot].myID;
            this.MenuComponents.Menu.ItemsToGrabMenu.inventory[topSlot].rightNeighborID = this.UpArrow.Id;
            this.DownArrow.Component.leftNeighborID = this.MenuComponents.Menu.ItemsToGrabMenu.inventory[bottomSlot].myID;
            this.MenuComponents.Menu.ItemsToGrabMenu.inventory[bottomSlot].rightNeighborID = this.DownArrow.Id;
            this.UpArrow.Component.downNeighborID = this.DownArrow.Id;
            this.DownArrow.Component.upNeighborID = this.UpArrow.Id;
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (e.Component == this.UpArrow)
        {
            this.MenuItems.Offset--;
        }
        else if (e.Component == this.DownArrow)
        {
            this.MenuItems.Offset++;
        }
        else
        {
            return;
        }

        this.UpArrow.Component.visible = this.MenuItems.Offset > 0;
        this.DownArrow.Component.visible = this.MenuItems.Offset < this.MenuItems.Rows;
    }

    private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
    {
        if (this.MenuItems.Menu is null)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (!this.MenuItems.Menu.ItemsToGrabMenu.isWithinBounds(x, y))
        {
            return;
        }

        switch (e.Delta)
        {
            case > 0:
                this.MenuItems.Offset--;
                break;
            case < 0:
                this.MenuItems.Offset++;
                break;
            default:
                return;
        }

        this.UpArrow.Component.visible = this.MenuItems.Offset > 0;
        this.DownArrow.Component.visible = this.MenuItems.Offset < this.MenuItems.Rows;
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (this.MenuItems.Menu is null)
        {
            return;
        }

        if (this.Config.ControlScheme.ScrollUp.JustPressed())
        {
            this.MenuItems.Offset--;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollUp);
            return;
        }

        if (this.Config.ControlScheme.ScrollDown.JustPressed())
        {
            this.MenuItems.Offset++;
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.ScrollDown);
        }
    }
}