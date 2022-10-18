namespace StardewMods.BetterChests.Framework.Features;

using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewValley.Menus;

/// <summary>
///     Sort items in a chest using a customized criteria.
/// </summary>
internal sealed class OrganizeChest : Feature
{
    private const string Id = "furyx639.BetterChests/OrganizeChest";

    private static readonly MethodBase ItemGrabMenuOrganizeItemsInList = AccessTools.Method(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.organizeItemsInList));

#nullable disable
    private static Feature Instance;
#nullable enable

    private readonly Harmony _harmony;
    private readonly IModHelper _helper;

    private OrganizeChest(IModHelper helper)
    {
        this._helper = helper;
        this._harmony = new(OrganizeChest.Id);
    }

    /// <summary>
    ///     Initializes <see cref="OrganizeChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OrganizeChest" /> class.</returns>
    public static Feature Init(IModHelper helper)
    {
        return OrganizeChest.Instance ??= new OrganizeChest(helper);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this._harmony.Patch(
            OrganizeChest.ItemGrabMenuOrganizeItemsInList,
            new(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

        // Patches
        this._harmony.Unpatch(
            OrganizeChest.ItemGrabMenuOrganizeItemsInList,
            AccessTools.Method(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ItemGrabMenu_organizeItemsInList_prefix(ItemGrabMenu __instance, IList<Item> items)
    {
        if (!ReferenceEquals(__instance.ItemsToGrabMenu.actualInventory, items)
         || BetterItemGrabMenu.Context is not { OrganizeChest: FeatureOption.Enabled })
        {
            return true;
        }

        BetterItemGrabMenu.Context.OrganizeItems();
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        return false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseRight
         || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu
         || BetterItemGrabMenu.Context is not { OrganizeChest: FeatureOption.Enabled })
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.organizeButton?.containsPoint(x, y) != true)
        {
            return;
        }

        BetterItemGrabMenu.Context.OrganizeItems(true);
        this._helper.Input.Suppress(e.Button);
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        Game1.playSound("Ship");
    }
}