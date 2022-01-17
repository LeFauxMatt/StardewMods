namespace FuryCore.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Common.Helpers;
using CommonHarmony;
using FuryCore.Enums;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ItemsDisplayed : SortedEventHandler<ItemsDisplayedEventArgs>
{
    private readonly PerScreen<ItemsDisplayedEventArgs> _args = new();
    private readonly PerScreen<Chest> _chest = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsDisplayed"/> class.
    /// </summary>
    /// <param name="modEvents"></param>
    /// <param name="services"></param>
    public ItemsDisplayed(IModEvents modEvents, ServiceCollection services)
    {
        ItemsDisplayed.Instance = this;

        services.Lazy<HarmonyHelper>(
            harmonyHelper =>
            {
                var methodParams = new[]
                {
                    typeof(SpriteBatch), typeof(int), typeof(int), typeof(int),
                };

                harmonyHelper.AddPatch(
                    nameof(ItemsDisplayed),
                    AccessTools.Method(typeof(InventoryMenu), nameof(InventoryMenu.draw), methodParams),
                    typeof(ItemsDisplayed),
                    nameof(ItemsDisplayed.InventoryMenu_draw_transpiler),
                    PatchType.Transpiler);

                harmonyHelper.ApplyPatches(nameof(ItemsDisplayed));
            });

        services.Lazy<CustomEvents>(events =>
        {
            events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        });

        modEvents.World.ChestInventoryChanged += this.OnChestInventoryChanged;
        modEvents.Player.InventoryChanged += this.OnInventoryChanged;
    }

    private static ItemsDisplayed Instance { get; set; }

    private ItemsDisplayedEventArgs Args
    {
        get => this._args.Value;
        set => this._args.Value = value;
    }

    private Chest Chest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    private static IEnumerable<CodeInstruction> InventoryMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        Log.Trace("Replace actualInventory with displayed inventory.");
        var displayedItemsPatch = new PatternPatch();
        displayedItemsPatch
            .Find(
                new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(InventoryMenu), nameof(InventoryMenu.actualInventory))),
                })
            .Patch(
                delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ItemsDisplayed), nameof(ItemsDisplayed.DisplayedItems))));
                })
            .Repeat(-1);

        var patternPatches = new PatternPatches(instructions, displayedItemsPatch);
        foreach (var patternPatch in patternPatches)
        {
            yield return patternPatch;
        }

        if (!patternPatches.Done)
        {
            Log.Warn($"Failed to apply all patches in {typeof(ItemsDisplayed)}::{nameof(ItemsDisplayed.InventoryMenu_draw_transpiler)}");
        }
    }

    private static IList<Item> DisplayedItems(IList<Item> actualInventory, InventoryMenu inventoryMenu)
    {
        return ReferenceEquals(inventoryMenu, ItemsDisplayed.Instance.Menu?.ItemsToGrabMenu)
            ? ItemsDisplayed.Instance.Args?.Items.Take(inventoryMenu.capacity).ToList() ?? actualInventory
            : actualInventory;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu;
        this.Chest = e.Chest;

        if (this.Menu is null or { inventory.highlightMethod.Target: ItemsHighlighted }
            || this.Chest is null or not
            {
                playerChest.Value: true,
                SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin,
            })
        {
            this.Args = null;
            return;
        }

        this.Args = new(this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID), this.Menu);
        this.InvokeAll(this.Args);
    }

    private void OnChestInventoryChanged(object sender, ChestInventoryChangedEventArgs e)
    {
        if (ReferenceEquals(e.Chest, this.Chest))
        {
            this.Args?.ForceRefresh();
        }
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        if (e.IsLocalPlayer)
        {
            this.Args?.ForceRefresh();
        }
    }
}