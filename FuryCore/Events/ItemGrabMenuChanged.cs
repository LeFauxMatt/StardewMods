namespace FuryCore.Events;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FuryCore.Enums;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ItemGrabMenuChanged : SortedEventHandler<ItemGrabMenuChangedEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemGrabMenuChanged" /> class.
    /// </summary>
    /// <param name="display"></param>
    /// <param name="services"></param>
    public ItemGrabMenuChanged(IDisplayEvents display, ServiceCollection services)
    {
        ItemGrabMenuChanged.Instance ??= this;

        services.Lazy<HarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{ModEntry.ModUniqueId}.{nameof(ItemGrabMenuChanged)}";
                var ctorParams = new[]
                {
                    typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
                };

                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Constructor(typeof(ItemGrabMenu), ctorParams),
                    typeof(ItemGrabMenuChanged),
                    nameof(ItemGrabMenuChanged.ItemGrabMenu_constructor_postfix),
                    PatchType.Postfix);

                harmonyHelper.ApplyPatches(id);
            });

        display.MenuChanged += this.OnMenuChanged;
    }

    private static ItemGrabMenuChanged Instance { get; set; }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        ItemGrabMenuChanged.Instance.Menu = __instance;

        if (__instance is not { shippingBin: false, context: Chest { playerChest.Value: true } chest })
        {
            ItemGrabMenuChanged.Instance.InvokeAll(new(__instance, null, -1, false));
            return;
        }

        ItemGrabMenuChanged.Instance.InvokeAll(new(__instance, chest, Context.ScreenId, true));
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (ReferenceEquals(e.NewMenu, this.Menu))
        {
            return;
        }

        this.Menu = e.NewMenu;
        if (this.Menu is not ItemGrabMenu { shippingBin: false, context: Chest { playerChest.Value: true, SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin } chest } itemGrabMenu)
        {
            this.InvokeAll(new(this.Menu as ItemGrabMenu, null, -1, false));
            return;
        }

        this.InvokeAll(new(itemGrabMenu, chest, Context.ScreenId, false));
    }
}