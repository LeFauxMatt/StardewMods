namespace StardewMods.BetterChests.Features;

using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.Common.Enums;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Sort items in a chest using a customized criteria.
/// </summary>
internal class OrganizeChest : IFeature
{
    private const string Id = "furyx639.BetterChests/OrganizeChest";

    private static OrganizeChest? Instance;

    private readonly IModHelper _helper;

    private bool _isActivated;

    private OrganizeChest(IModHelper helper)
    {
        this._helper = helper;
        HarmonyHelper.AddPatches(
            OrganizeChest.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
                    typeof(OrganizeChest),
                    nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix),
                    PatchType.Prefix),
            });
    }

    /// <summary>
    ///     Initializes <see cref="OrganizeChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OrganizeChest" /> class.</returns>
    public static OrganizeChest Init(IModHelper helper)
    {
        return OrganizeChest.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (this._isActivated)
        {
            return;
        }

        this._isActivated = true;
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (!this._isActivated)
        {
            return;
        }

        this._isActivated = false;
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private static bool ItemGrabMenu_organizeItemsInList_prefix(IList<Item> items)
    {
        if (Game1.activeClickableMenu is not ItemGrabMenu { context: Item context } itemGrabMenu
         || !ReferenceEquals(itemGrabMenu.ItemsToGrabMenu.actualInventory, items)
         || !StorageHelper.TryGetOne(context, out var storage)
         || storage.OrganizeChest == FeatureOption.Disabled)
        {
            return true;
        }

        storage.OrganizeItems();
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        return false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseRight
         || Game1.activeClickableMenu is not ItemGrabMenu { context: Item context } itemGrabMenu
         || !StorageHelper.TryGetOne(context, out var storage)
         || storage.OrganizeChest == FeatureOption.Disabled)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.organizeButton?.containsPoint(x, y) != true)
        {
            return;
        }

        storage.OrganizeItems(true);
        this._helper.Input.Suppress(e.Button);
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        Game1.playSound("Ship");
    }
}