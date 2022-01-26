namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using BetterChests.Interfaces;
using BetterChests.Models;
using Common.Extensions;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using FuryCore.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class CustomColorPicker : Feature
{
    private readonly PerScreen<HslColorPicker> _colorPicker = new();
    private readonly PerScreen<ManagedChest> _managedChest = new();
    private readonly PerScreen<ItemGrabMenu> _menu = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomColorPicker"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public CustomColorPicker(IConfigModel config, IModHelper helper, IServiceLocator services)
        : base(config, helper, services)
    {
        CustomColorPicker.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatches(
                    this.Id,
                    new SavedPatch[]
                    {
                        new(
                            AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getCurrentColor)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.DiscreteColorPicker_GetCurrentColor_postfix),
                            PatchType.Postfix),
                        new(
                            AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getColorFromSelection)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.DiscreteColorPicker_GetColorFromSelection_postfix),
                            PatchType.Postfix),
                        new(
                            AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getSelectionFromColor)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.DiscreteColorPicker_GetSelectionFromColor_postfix),
                            PatchType.Postfix),
                        new(
                            AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.ItemGrabMenu_DiscreteColorPicker_Transpiler),
                            PatchType.Transpiler),
                        new(
                            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.ItemGrabMenu_DiscreteColorPicker_Transpiler),
                            PatchType.Transpiler),
                        new(
                            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.ItemGrabMenu_DiscreteColorPicker_Transpiler),
                            PatchType.Transpiler),
                        new(
                            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                            typeof(CustomColorPicker),
                            nameof(CustomColorPicker.ItemGrabMenu_setSourceItem_postfix),
                            PatchType.Postfix),
                    });
            });
    }

    private static CustomColorPicker Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private HslColorPicker ColorPicker
    {
        get => this._colorPicker.Value;
        set => this._colorPicker.Value = value;
    }

    private ManagedChest ManagedChest
    {
        get => this._managedChest.Value;
        set => this._managedChest.Value = value;
    }

    private ItemGrabMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void DiscreteColorPicker_GetCurrentColor_postfix(DiscreteColorPicker __instance, ref Color __result)
    {
        if (__instance is not HslColorPicker colorPicker || !ReferenceEquals(colorPicker, CustomColorPicker.Instance.ColorPicker))
        {
            return;
        }

        __result = colorPicker.GetCurrentColor();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void DiscreteColorPicker_GetColorFromSelection_postfix(DiscreteColorPicker __instance, int selection, ref Color __result)
    {
        __result = HslColorPicker.GetColorFromSelection(selection);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void DiscreteColorPicker_GetSelectionFromColor_postfix(DiscreteColorPicker __instance, Color c, ref int __result)
    {
        __result = HslColorPicker.GetSelectionFromColor(c);
    }

    private static IEnumerable<CodeInstruction> ItemGrabMenu_DiscreteColorPicker_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Newobj)
            {
                if (instruction.operand.Equals(AccessTools.Constructor(typeof(DiscreteColorPicker), new[] { typeof(int), typeof(int), typeof(int), typeof(Item) })))
                {
                    yield return new(OpCodes.Ldarg_0);
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(CustomColorPicker), nameof(CustomColorPicker.GetColorPicker)));
                }
                else
                {
                    yield return instruction;
                }
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
    {
        if (__instance.context is not Chest chest || !ReferenceEquals(chest, CustomColorPicker.Instance.ManagedChest.Chest))
        {
            return;
        }

        __instance.discreteColorPickerCC = null;
    }

    private static DiscreteColorPicker GetColorPicker(int xPosition, int yPosition, int startingColor, Item itemToDrawColored, ItemGrabMenu menu)
    {
        CustomColorPicker.Instance.ColorPicker?.UnregisterEvents(CustomColorPicker.Instance.Helper.Events.Input);

        var item = CustomColorPicker.Instance.Helper.Reflection.GetField<Item>(menu, "sourceItem").GetValue();
        if (item is not Chest chest || !chest.IsPlayerChest())
        {
            CustomColorPicker.Instance.ColorPicker = null;
            return new(xPosition, yPosition, startingColor, itemToDrawColored);
        }

        if (itemToDrawColored is not Chest chestToDraw)
        {
            chestToDraw = new(true, chest.ParentSheetIndex);
        }

        chestToDraw.Name = chest.Name;
        chestToDraw.lidFrameCount.Value = chest.lidFrameCount.Value;
        chestToDraw.playerChoiceColor.Value = chest.playerChoiceColor.Value;
        foreach (var (key, value) in chest.modData.Pairs)
        {
            chestToDraw.modData.Add(key, value);
        }

        CustomColorPicker.Instance.ColorPicker = new(
            CustomColorPicker.Instance.Helper.Content,
            CustomColorPicker.Instance.Config.CustomColorPickerArea == ComponentArea.Left ? menu.xPositionOnScreen - 96 - (IClickableMenu.borderWidth / 2) : menu.xPositionOnScreen + menu.width + 96 + (IClickableMenu.borderWidth / 2),
            menu.yPositionOnScreen - 56 + (IClickableMenu.borderWidth / 2),
            chest.playerChoiceColor.Value,
            chestToDraw);
        CustomColorPicker.Instance.ColorPicker.RegisterEvents(CustomColorPicker.Instance.Helper.Events.Input);

        return CustomColorPicker.Instance.ColorPicker;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Menu = e.ItemGrabMenu?.IsPlayerChestMenu(out _) == true
            ? e.ItemGrabMenu
            : null;

        if (this.Menu is null || !this.ManagedChests.FindChest(e.Chest, out var managedChest))
        {
            return;
        }

        this.Menu.discreteColorPickerCC = null;
        this.ManagedChest = managedChest;
    }
}