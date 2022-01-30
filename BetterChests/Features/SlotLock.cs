namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Common.Helpers;
using Common.Helpers.PatternPatcher;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Interfaces;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class SlotLock : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly PerScreen<bool[]> _lockedSlots = new(() => new bool[Game1.player.Items.Count]);

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotLock"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public SlotLock(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        SlotLock.Instance = this;
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

    /// <summary>
    /// Gets an array indicating which item slots are locked by the player.
    /// </summary>
    public IList<bool> LockedSlots
    {
        get
        {
            var lockedSlots = Game1.player.modData.TryGetValue($"{BetterChests.ModUniqueId}/LockedSlots", out var lockedSlotsData) && !string.IsNullOrWhiteSpace(lockedSlotsData)
                ? lockedSlotsData.ToCharArray()
                : new char[Game1.player.Items.Count];
            if (lockedSlots.Length < Game1.player.Items.Count)
            {
                Array.Resize(ref lockedSlots, Game1.player.Items.Count);
            }

            this._lockedSlots.Value = lockedSlots.Select(slot => slot == '1').ToArray();
            return this._lockedSlots.Value;
        }

        private set
        {
            Game1.player.modData[$"{BetterChests.ModUniqueId}/LockedSlots"] = new(value.Select(slot => slot ? '1' : '0').ToArray());
        }
    }

    private static SlotLock Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc/>
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc/>
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace($"Applying patches to {nameof(InventoryMenu)}.{nameof(InventoryMenu.draw)}");
        var patcher = new PatternPatcher<CodeInstruction>((c1, c2) => c1.opcode.Equals(c2.opcode) && (c1.operand is null || c1.OperandIs(c2.operand)));

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
        return Game1.activeClickableMenu switch
        {
            ItemGrabMenu { inventory: { } itemGrabMenu } when ReferenceEquals(itemGrabMenu, menu) => SlotLock.Instance.GetTint(tint, index),
            GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage { inventory: { } inventoryPage } && ReferenceEquals(inventoryPage, menu) => SlotLock.Instance.GetTint(tint, index),
            _ => tint,
        };
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        var menu = Game1.activeClickableMenu switch
        {
            ItemGrabMenu { inventory: { } itemGrabMenu } => itemGrabMenu,
            GameMenu gameMenu when gameMenu.pages[gameMenu.currentTab] is InventoryPage { inventory: { } inventoryPage } => inventoryPage,
            _ => null,
        };

        if (menu is null || !this.Config.ControlScheme.LockSlot.JustPressed())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var slot = menu.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (slot is null)
        {
            return;
        }

        var index = Convert.ToInt32(slot.name);
        var lockedSlots = this.LockedSlots;
        lockedSlots[index] = !lockedSlots[index];
        this.LockedSlots = lockedSlots;
        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.LockSlot);
    }

    private Color GetTint(Color tint, int index)
    {
        return this.LockedSlots.ElementAtOrDefault(index) ? Color.Red : tint;
    }
}