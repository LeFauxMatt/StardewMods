namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class SlotLock : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SlotLock" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public SlotLock(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
                    AccessTools.Method(
                        typeof(InventoryMenu),
                        nameof(InventoryMenu.draw),
                        new[]
                        {
                            typeof(SpriteBatch), typeof(int), typeof(int), typeof(int),
                        }),
                    typeof(SlotLock),
                    nameof(SlotLock.InventoryMenu_draw_transpiler),
                    PatchType.Transpiler);
            });
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(InventoryMenu)}.{nameof(InventoryMenu.draw)}");
        IPatternPatcher<CodeInstruction> patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

        // ****************************************************************************************
        // Item Tint Patch
        // Replaces all actualInventory with ItemsDisplayed.DisplayedItems(actualInventory)
        // Replaces the tint value for the item slot with SlotLock.Tint to highlight locked slots.
        patcher.AddPatchLoop(
            code =>
            {
                code.RemoveAt(code.Count - 1);
                code.Add(new(OpCodes.Ldarg_0));
                code.Add(new(OpCodes.Ldloc_0));
                code.Add(new(OpCodes.Ldloc_S, (byte)4));
                code.Add(new(OpCodes.Call, AccessTools.Method(typeof(SlotLock), nameof(SlotLock.Tint))));
            },
            new(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1.getSourceRectForStandardTileSheet))),
            new(OpCodes.Newobj),
            new(OpCodes.Ldloc_0));

        // Fill code buffer
        foreach (var inCode in instructions)
        {
            // Return patched code segments
            foreach (var outCode in patcher.From(inCode))
            {
                yield return outCode;
            }
        }

        // Return remaining code
        foreach (var outCode in patcher.FlushBuffer())
        {
            yield return outCode;
        }

        Log.Trace($"{patcher.AppliedPatches.ToString()} / {patcher.TotalPatches.ToString()} patches applied.");
        if (patcher.AppliedPatches < patcher.TotalPatches)
        {
            Log.Warn("Failed to applied all patches!");
        }
    }

    private static Color Tint(InventoryMenu menu, Color tint, int index)
    {
        switch (Game1.activeClickableMenu)
        {
            case ItemGrabMenu { inventory: { } itemGrabMenu } when ReferenceEquals(itemGrabMenu, menu):
            case GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage { inventory: { } inventoryPage } && ReferenceEquals(inventoryPage, menu):
                return menu.actualInventory.ElementAtOrDefault(index)?.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot") == true
                    ? Color.Red
                    : tint;
            default:
                return tint;
        }
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu { inventory: { } itemGrabMenu } => itemGrabMenu,
            GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage { inventory: { } inventoryPage } => inventoryPage,
            _ => null,
        };

        if (menu is null)
        {
            return;
        }

        if (!(this.Config.SlotLockHold && e.Button == SButton.MouseLeft && e.IsDown(this.Config.ControlScheme.LockSlot))
            && !(!this.Config.SlotLockHold && e.Button == this.Config.ControlScheme.LockSlot))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var slot = menu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (slot is null || !int.TryParse(slot.name, out var index))
        {
            return;
        }

        var item = menu.actualInventory.ElementAtOrDefault(index);
        if (item is null)
        {
            return;
        }

        if (item.modData.ContainsKey($"{BetterChests.ModUniqueId}/LockedSlot"))
        {
            item.modData.Remove($"{BetterChests.ModUniqueId}/LockedSlot");
        }
        else
        {
            item.modData[$"{BetterChests.ModUniqueId}/LockedSlot"] = true.ToString();
        }

        this.Helper.Input.Suppress(e.Button);
    }
}