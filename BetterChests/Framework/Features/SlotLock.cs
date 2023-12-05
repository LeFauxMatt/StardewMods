namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.Menus;

/// <summary>Locks items in inventory so they cannot be stashed.</summary>
internal sealed class SlotLock : Feature
{
    private const string Id = "furyx639.BetterChests/SlotLock";

    private static readonly MethodBase InventoryMenuDraw = AccessTools.Method(
        typeof(InventoryMenu),
        nameof(InventoryMenu.draw),
        new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) });

#nullable disable
    private static SlotLock instance;
#nullable enable

    private readonly ModConfig config;
    private readonly Harmony harmony;
    private readonly IModHelper helper;

    private SlotLock(IModHelper helper, ModConfig config)
    {
        this.helper = helper;
        this.config = config;
        this.harmony = new(SlotLock.Id);
    }

    /// <summary>Initializes <see cref="SlotLock" />.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="SlotLock" /> class.</returns>
    public static Feature Init(IModHelper helper, ModConfig config) => SlotLock.instance ??= new(helper, config);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

        // Harmony
        this.harmony.Patch(
            SlotLock.InventoryMenuDraw,
            transpiler: new(typeof(SlotLock), nameof(SlotLock.InventoryMenu_draw_transpiler)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Harmony
        this.harmony.Unpatch(
            SlotLock.InventoryMenuDraw,
            AccessTools.Method(typeof(SlotLock), nameof(SlotLock.InventoryMenu_draw_transpiler)));
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            yield return instruction;

            if (instruction.opcode != OpCodes.Ldloc_0)
            {
                continue;
            }

            yield return new(OpCodes.Ldarg_0);
            yield return new(OpCodes.Ldloc_S, (byte)5);
            yield return CodeInstruction.Call(typeof(SlotLock), nameof(SlotLock.Tint));
        }
    }

    private static Color Tint(Color tint, InventoryMenu menu, int index) =>
        menu.actualInventory.ElementAtOrDefault(index)?.modData.ContainsKey("furyx639.BetterChests/LockedSlot") == true
            ? Utility.StringToColor(SlotLock.instance.config.SlotLockColor) ?? tint
            : tint;

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.config.SlotLockHold
            || e.Button is not SButton.MouseLeft
            || !this.config.ControlScheme.LockSlot.IsDown())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu
            {
                inventory:
                { } inventory,
            } when inventory.isWithinBounds(x, y) => inventory,
            ItemGrabMenu
            {
                ItemsToGrabMenu:
                { } itemsToGrabMenu,
            } when itemsToGrabMenu.isWithinBounds(x, y) => itemsToGrabMenu,
            GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage
            {
                inventory:
                { } inventoryPage,
            } => inventoryPage,
            _ => null,
        };

        var slot = menu?.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (slot is null || !int.TryParse(slot.name, out var index))
        {
            return;
        }

        var item = menu?.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        if (item.modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
        {
            item.modData.Remove("furyx639.BetterChests/LockedSlot");
        }
        else
        {
            item.modData["furyx639.BetterChests/LockedSlot"] = true.ToString();
        }

        this.helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.config.SlotLockHold || !this.config.ControlScheme.LockSlot.JustPressed())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu
            {
                inventory:
                { } inventory,
            } when inventory.isWithinBounds(x, y) => inventory,
            ItemGrabMenu
            {
                ItemsToGrabMenu:
                { } itemsToGrabMenu,
            } when itemsToGrabMenu.isWithinBounds(x, y) => itemsToGrabMenu,
            GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage
            {
                inventory:
                { } inventoryPage,
            } => inventoryPage,
            _ => null,
        };

        var slot = menu?.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (slot is null || !int.TryParse(slot.name, out var index))
        {
            return;
        }

        var item = menu?.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        if (item.modData.ContainsKey("furyx639.BetterChests/LockedSlot"))
        {
            item.modData.Remove("furyx639.BetterChests/LockedSlot");
        }
        else
        {
            item.modData["furyx639.BetterChests/LockedSlot"] = true.ToString();
        }

        this.helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.LockSlot);
    }
}
