namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.UI;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Adds a chest color picker that support hue, saturation, and lightness.
/// </summary>
internal class BetterColorPicker : IFeature
{
    private const string Id = "BetterChests.CustomColorPicker";

    private readonly PerScreen<HslColorPicker?> _colorPicker = new();

    private BetterColorPicker(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
        HarmonyHelper.AddPatches(
            BetterColorPicker.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getCurrentColor)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.DiscreteColorPicker_GetCurrentColor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getColorFromSelection)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.DiscreteColorPicker_GetColorFromSelection_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getSelectionFromColor)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.DiscreteColorPicker_GetSelectionFromColor_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.ItemGrabMenu_DiscreteColorPicker_Transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.ItemGrabMenu_DiscreteColorPicker_Transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.ItemGrabMenu_DiscreteColorPicker_Transpiler),
                    PatchType.Transpiler),
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                    typeof(BetterColorPicker),
                    nameof(BetterColorPicker.ItemGrabMenu_setSourceItem_postfix),
                    PatchType.Postfix),
            });
    }

    private static BetterColorPicker? Instance { get; set; }

    private HslColorPicker? ColorPicker
    {
        get => this._colorPicker.Value;
        set => this._colorPicker.Value = value;
    }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Initializes <see cref="BetterColorPicker" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="BetterColorPicker" /> class.</returns>
    public static BetterColorPicker Init(IModHelper helper, ModConfig config)
    {
        return BetterColorPicker.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        HarmonyHelper.ApplyPatches(BetterColorPicker.Id);
        this.Helper.Events.Display.MenuChanged += BetterColorPicker.OnMenuChanged;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        HarmonyHelper.UnapplyPatches(BetterColorPicker.Id);
        this.Helper.Events.Display.MenuChanged -= BetterColorPicker.OnMenuChanged;
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
        if (__instance is not HslColorPicker colorPicker || !ReferenceEquals(colorPicker, BetterColorPicker.Instance!.ColorPicker))
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
        //BetterColorPicker.Instance!.ColorPicker?.UnregisterEvents(BetterColorPicker.Instance.Helper.Events.Input);

        var item = BetterColorPicker.Instance!.Helper.Reflection.GetField<Item>(menu, "sourceItem").GetValue();
        if (item is not Chest chest)
        {
            BetterColorPicker.Instance.ColorPicker = null;
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
        BetterColorPicker.Instance.ColorPicker = new(
            BetterColorPicker.Instance.Helper,
            BetterColorPicker.Instance.Config.CustomColorPickerArea == ComponentArea.Left ? menu.xPositionOnScreen - 2 * Game1.tileSize - IClickableMenu.borderWidth / 2 : menu.xPositionOnScreen + menu.width + 96 + IClickableMenu.borderWidth / 2,
            menu.yPositionOnScreen - 56 + IClickableMenu.borderWidth / 2,
            chest.playerChoiceColor.Value,
            chestToDraw);
        //BetterColorPicker.Instance.ColorPicker.RegisterEvents(BetterColorPicker.Instance.Helper.Events.Input);

        return BetterColorPicker.Instance.ColorPicker;
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
                    yield return new(OpCodes.Call, AccessTools.Method(typeof(BetterColorPicker), nameof(BetterColorPicker.GetColorPicker)));
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
        if (__instance.context is null || !StorageHelper.TryGetOne(__instance.context, out var storage) || storage.CustomColorPicker != FeatureOption.Disabled)
        {
            return;
        }

        __instance.discreteColorPickerCC = null;
    }

    private static void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        switch (e.NewMenu)
        {
            case ItemSelectionMenu:
                return;
            case ItemGrabMenu { context: { } context } itemGrabMenu when StorageHelper.TryGetOne(context, out var storage) && storage.CustomColorPicker != FeatureOption.Disabled:
                itemGrabMenu.discreteColorPickerCC = null;
                return;
        }
    }
}