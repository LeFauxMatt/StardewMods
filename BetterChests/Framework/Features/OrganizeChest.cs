namespace StardewMods.BetterChests.Framework.Features;

using System.Reflection;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Enums;
using StardewValley.Menus;

/// <summary>Sort items in a chest using a customized criteria.</summary>
internal sealed class OrganizeChest : Feature
{
    private const string Id = "furyx639.BetterChests/OrganizeChest";

    private static readonly MethodBase ItemGrabMenuOrganizeItemsInList = AccessTools.Method(
        typeof(ItemGrabMenu),
        nameof(ItemGrabMenu.organizeItemsInList));

#nullable disable
    private static Feature instance;
#nullable enable

    private readonly Harmony harmony;
    private readonly IModHelper helper;

    private OrganizeChest(IModHelper helper)
    {
        this.helper = helper;
        this.harmony = new(OrganizeChest.Id);
    }

    /// <summary>Initializes <see cref="OrganizeChest" />.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OrganizeChest" /> class.</returns>
    public static Feature Init(IModHelper helper) => OrganizeChest.instance ??= new OrganizeChest(helper);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        this.harmony.Patch(
            OrganizeChest.ItemGrabMenuOrganizeItemsInList,
            new(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

        // Patches
        this.harmony.Unpatch(
            OrganizeChest.ItemGrabMenuOrganizeItemsInList,
            AccessTools.Method(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ItemGrabMenu_organizeItemsInList_prefix(ItemGrabMenu? __instance, IList<Item> items)
    {
        if (BetterItemGrabMenu.Context?.OrganizeChest != FeatureOption.Enabled)
        {
            return true;
        }

        __instance ??= Game1.activeClickableMenu as ItemGrabMenu;

        if (!Equals(__instance?.ItemsToGrabMenu.actualInventory, items))
        {
            return true;
        }

        var groupBy = BetterItemGrabMenu.Context.OrganizeChestGroupBy;
        var sortBy = BetterItemGrabMenu.Context.OrganizeChestSortBy;

        if (groupBy == GroupBy.Default && sortBy == SortBy.Default)
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
            || BetterItemGrabMenu.Context is not
            {
                OrganizeChest: FeatureOption.Enabled,
            })
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.organizeButton?.containsPoint(x, y) != true)
        {
            return;
        }

        BetterItemGrabMenu.Context.OrganizeItems(true);
        this.helper.Input.Suppress(e.Button);
        BetterItemGrabMenu.RefreshItemsToGrabMenu = true;
        Game1.playSound("Ship");
    }
}
