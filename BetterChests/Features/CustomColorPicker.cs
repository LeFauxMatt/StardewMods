namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using Common.Enums;
using Common.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.UI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Adds a chest color picker that support hue, saturation, and lightness.
/// </summary>
internal class CustomColorPicker : IFeature
{
    private const string Id = "BetterChests.CustomColorPicker";

    private readonly PerScreen<HslColorPicker?> _colorPicker = new();
    private readonly PerScreen<IGameObject> _context = new();

    /// <summary>
    ///     Initializes <see cref="CustomColorPicker" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="CustomColorPicker" /> class.</returns>
    public static CustomColorPicker Init(IModHelper helper)
    {
        return CustomColorPicker.Instance ??= new(helper);
    }

    private CustomColorPicker(IModHelper helper)
    {
        this.Helper = helper;
        HarmonyHelper.AddPatches(
            CustomColorPicker.Id,
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
    }

    private static CustomColorPicker? Instance { get; set; }

    private IModHelper Helper { get; }

    private HslColorPicker? ColorPicker
    {
        get => this._colorPicker.Value;
        set => this._colorPicker.Value = value;
    }

    private IGameObject Context
    {
        get => this._context.Value;
        set => this._context.Value = value;
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(CustomColorPicker.Id);
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(CustomColorPicker.Id);
        this.Helper.Events.Display.MenuChanged -= this.OnMenuChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Parameter is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void DiscreteColorPicker_GetColorFromSelection_postfix(int selection, ref Color __result)
    {
        __result = HslColorPicker.GetColorFromSelection(selection);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void DiscreteColorPicker_GetCurrentColor_postfix(DiscreteColorPicker __instance, ref Color __result)
    {
        if (__instance is not HslColorPicker colorPicker || !ReferenceEquals(colorPicker, CustomColorPicker.Instance!.ColorPicker))
        {
            return;
        }

        __result = colorPicker.GetCurrentColor();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Parameter is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void DiscreteColorPicker_GetSelectionFromColor_postfix(Color c, ref int __result)
    {
        __result = HslColorPicker.GetSelectionFromColor(c);
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static DiscreteColorPicker GetColorPicker(int xPosition, int yPosition, int startingColor, Item itemToDrawColored, ItemGrabMenu menu)
    {
        CustomColorPicker.Instance.ColorPicker?.UnregisterEvents(CustomColorPicker.Instance.Helper.Events.Input);

        var item = CustomColorPicker.Instance.Helper.Reflection.GetField<Item>(menu, "sourceItem").GetValue();
        if (item is not Chest chest)
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

        Log.Verbose("Adding CustomColorPicker to ItemGrabMenu");
        CustomColorPicker.Instance.ColorPicker = new(
            CustomColorPicker.Instance.Helper.Content,
            Config.CustomColorPickerArea == ComponentArea.Left ? menu.xPositionOnScreen - 2 * Game1.tileSize - IClickableMenu.borderWidth / 2 : menu.xPositionOnScreen + menu.width + 96 + IClickableMenu.borderWidth / 2,
            menu.yPositionOnScreen - 56 + IClickableMenu.borderWidth / 2,
            chest.playerChoiceColor.Value,
            chestToDraw);
        CustomColorPicker.Instance.ColorPicker.RegisterEvents(CustomColorPicker.Instance.Helper.Events.Input);

        return CustomColorPicker.Instance.ColorPicker;
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
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
    {
        if (__instance.context is null || !ReferenceEquals(__instance.context, CustomColorPicker.Instance.Context))
        {
            return;
        }

        __instance.discreteColorPickerCC = null;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.Menu is not ItemGrabMenu itemGrabMenu || e.Context is null || !this.ManagedObjects.TryGetManagedStorage(e.Context, out var managedChest) || managedChest.CustomColorPicker != FeatureOption.Enabled)
        {
            this.ColorPicker?.UnregisterEvents(this.Helper.Events.Input);
            this.ColorPicker = null;
            return;
        }

        this.Context = e.Context;
        itemGrabMenu.discreteColorPickerCC = null;
    }
}