namespace StardewMods.StackQuality.Framework;

using StardewValley.Menus;

/// <summary>
///     Common helpers for StackQuality.
/// </summary>
internal sealed class Helpers
{
#nullable disable
    private static Helpers Instance;
#nullable enable

    private readonly IModHelper _helper;

    private Helpers(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Sets the currently held item for the active menu.
    /// </summary>
    public static Item? HeldItem
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage:
                    Game1.player.CursorSlotItem = value;
                    return;
                case JunimoNoteMenu junimoNoteMenu:
                    Helpers.Reflection.GetField<Item?>(junimoNoteMenu, "heldItem").SetValue(value);
                    return;
                case MenuWithInventory menuWithInventory:
                    menuWithInventory.heldItem = value;
                    return;
                case ShopMenu shopMenu:
                    shopMenu.heldItem = value;
                    return;
            }
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the current menu supported StackQuality.
    /// </summary>
    public static bool IsSupported =>
        Game1.activeClickableMenu is JunimoNoteMenu or MenuWithInventory or ShopMenu
        || (Game1.activeClickableMenu as GameMenu)?.GetCurrentPage() is InventoryPage;

    private static IReflectionHelper Reflection => Helpers.Instance._helper.Reflection;

    /// <summary>
    ///     Initializes <see cref="Helpers" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="Helpers" /> class.</returns>
    public static Helpers Init(IModHelper helper)
    {
        return Helpers.Instance ??= new(helper);
    }
}