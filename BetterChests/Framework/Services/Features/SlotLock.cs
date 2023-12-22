namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;

/// <summary>Locks items in inventory so they cannot be stashed.</summary>
internal sealed class SlotLock : BaseFeature
{
    private static readonly MethodBase InventoryMenuDraw = AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(int) });

#nullable disable
    private static SlotLock instance;
#nullable enable

    private readonly IModEvents events;
    private readonly Harmony harmony;
    private readonly IInputHelper input;

    /// <summary>Initializes a new instance of the <see cref="SlotLock" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="input">Dependency used for checking and changing input state.</param>
    public SlotLock(ILogging logging, ModConfig modConfig, IModEvents events, Harmony harmony, IInputHelper input)
        : base(logging, modConfig)
    {
        SlotLock.instance = this;
        this.events = events;
        this.harmony = harmony;
        this.input = input;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.SlotLock != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.events.Input.ButtonPressed += this.OnButtonPressed;
        this.events.Input.ButtonsChanged += this.OnButtonsChanged;

        // Harmony
        this.harmony.Patch(SlotLock.InventoryMenuDraw, transpiler: new HarmonyMethod(typeof(SlotLock), nameof(SlotLock.InventoryMenu_draw_transpiler)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.events.Input.ButtonPressed -= this.OnButtonPressed;
        this.events.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Harmony
        this.harmony.Unpatch(SlotLock.InventoryMenuDraw, AccessTools.Method(typeof(SlotLock), nameof(SlotLock.InventoryMenu_draw_transpiler)));
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

            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)5);
            yield return CodeInstruction.Call(typeof(SlotLock), nameof(SlotLock.Tint));
        }
    }

    private static Color Tint(Color tint, InventoryMenu menu, int index) =>
        menu.actualInventory.ElementAtOrDefault(index)?.modData.ContainsKey("furyx639.BetterChests/LockedSlot") == true ? Utility.StringToColor(SlotLock.instance.ModConfig.SlotLockColor) ?? tint : tint;

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.ModConfig.SlotLockHold || e.Button is not SButton.MouseLeft || !this.ModConfig.Controls.LockSlot.IsDown())
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

        this.input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.ModConfig.SlotLockHold || !this.ModConfig.Controls.LockSlot.JustPressed())
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

        this.input.SuppressActiveKeybinds(this.ModConfig.Controls.LockSlot);
    }
}
