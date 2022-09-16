namespace StardewMods.StackQuality;

using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.StackQuality;
using StardewMods.StackQuality.Framework;
using StardewMods.StackQuality.UI;
using StardewValley.Menus;

/// <inheritdoc />
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class StackQuality : Mod
{
#nullable disable
    private static StackQuality Instance;
#nullable enable

    /// <summary>
    ///     Gets a value indicating whether the current menu supported StackQuality.
    /// </summary>
    public static bool IsSupported =>
        Game1.activeClickableMenu is JunimoNoteMenu or MenuWithInventory or ShopMenu
     || (Game1.activeClickableMenu is GameMenu gameMenu && gameMenu.GetCurrentPage() is InventoryPage);

    /// <summary>
    ///     Sets the currently held item for the active menu.
    /// </summary>
    internal static Item? HeldItem
    {
        set
        {
            switch (Game1.activeClickableMenu)
            {
                case GameMenu gameMenu when gameMenu.GetCurrentPage() is InventoryPage:
                    Game1.player.CursorSlotItem = value;
                    return;
                case JunimoNoteMenu junimoNoteMenu:
                    StackQuality.Instance!.Helper.Reflection.GetField<Item?>(junimoNoteMenu, "heldItem")
                                .SetValue(value);
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

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        StackQuality.Instance = this;
        Log.Monitor = this.Monitor;
        Integrations.Init(this.Helper);
        ModPatches.Init(this.Helper, this.ModManifest, (IStackQualityApi)this.GetApi());

        // Events
        this.Helper.Events.Display.RenderedActiveMenu += StackQuality.OnRenderedActiveMenu;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new StackQualityApi();
    }

    [EventPriority(EventPriority.Low)]
    private static void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (!StackQuality.IsSupported
         || Game1.activeClickableMenu.GetChildMenu() is not ItemQualityMenu itemQualityMenu)
        {
            return;
        }

        itemQualityMenu.Draw(e.SpriteBatch);
    }
}