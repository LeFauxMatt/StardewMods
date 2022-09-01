namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Menus;

/// <summary>
///     Locks items in inventory so they cannot be stashed.
/// </summary>
internal class SlotLock : IFeature
{
    private const string Id = "furyx639.BetterChests/SlotLock";

    private static SlotLock? Instance;

    private readonly ModConfig _config;
    private readonly IModHelper _helper;

    private bool _isActivated;

    private SlotLock(IModHelper helper, ModConfig config)
    {
        this._helper = helper;
        this._config = config;
        HarmonyHelper.AddPatches(
            SlotLock.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(
                        typeof(InventoryMenu),
                        nameof(InventoryMenu.draw),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(int),
                            typeof(int),
                            typeof(int),
                        }),
                    typeof(SlotLock),
                    nameof(SlotLock.InventoryMenu_draw_transpiler),
                    PatchType.Transpiler),
            });
    }

    /// <summary>
    ///     Initializes <see cref="SlotLock" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="SlotLock" /> class.</returns>
    public static SlotLock Init(IModHelper helper, ModConfig config)
    {
        return SlotLock.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        HarmonyHelper.ApplyPatches(SlotLock.Id);
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        HarmonyHelper.UnapplyPatches(SlotLock.Id);
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldloc_0)
            {
                yield return instruction;
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Ldloc_S, (byte)4);
                yield return CodeInstruction.Call(typeof(SlotLock), nameof(SlotLock.Tint));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static Color Tint(Color tint, InventoryMenu menu, int index)
    {
        return menu.actualInventory.ElementAtOrDefault(index)?.modData.ContainsKey("furyx639.BetterChests/LockedSlot")
            == true
            ? SlotLock.Instance!._config.SlotLockColor.ToColor()
            : tint;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this._config.SlotLockHold
         || e.Button is not SButton.MouseLeft
         || !this._config.ControlScheme.LockSlot.IsDown())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu { inventory: { } inventory } when inventory.isWithinBounds(x, y) => inventory,
            ItemGrabMenu { ItemsToGrabMenu: { } itemsToGrabMenu } when itemsToGrabMenu.isWithinBounds(x, y) =>
                itemsToGrabMenu,
            GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage { inventory: { } inventoryPage }
                => inventoryPage,
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

        this._helper.Input.Suppress(e.Button);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this._config.SlotLockHold || !this._config.ControlScheme.LockSlot.JustPressed())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu { inventory: { } inventory } when inventory.isWithinBounds(x, y) => inventory,
            ItemGrabMenu { ItemsToGrabMenu: { } itemsToGrabMenu } when itemsToGrabMenu.isWithinBounds(x, y) =>
                itemsToGrabMenu,
            GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage { inventory: { } inventoryPage }
                => inventoryPage,
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

        this._helper.Input.SuppressActiveKeybinds(this._config.ControlScheme.LockSlot);
    }
}