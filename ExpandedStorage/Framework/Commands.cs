namespace StardewMods.ExpandedStorage.Framework;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewValley.Menus;

/// <summary>
///     Command handler for Expanded Storage.
/// </summary>
internal sealed class Commands
{
#nullable disable
    private static Commands Instance;
#nullable enable

    private readonly IModHelper _helper;
    private readonly IDictionary<string, ICustomStorage> _storages;

    private Commands(IModHelper helper, IDictionary<string, ICustomStorage> storages)
    {
        this._helper = helper;
        this._storages = storages;
        this._helper.ConsoleCommands.Add("xs_shop", I18n.Command_XsShop_Description(), this.OpenShopMenu);
    }

    /// <summary>
    ///     Initializes <see cref="Commands" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="storages">All custom chests currently loaded in the game.</param>
    /// <returns>Returns an instance of the <see cref="Commands" /> class.</returns>
    public static Commands Init(IModHelper helper, IDictionary<string, ICustomStorage> storages)
    {
        return Commands.Instance ??= new(helper, storages);
    }

    private void OpenShopMenu(string command, string[] args)
    {
        if (!Context.IsPlayerFree || Game1.activeClickableMenu is not null)
        {
            return;
        }

        var itemPriceAndStock = new Dictionary<ISalable, int[]>();

        // Add chests to stock
        foreach (var (id, _) in this._storages)
        {
            Utility.AddStock(
                itemPriceAndStock,
                new SObject(Vector2.Zero, 232)
                {
                    name = id,
                    modData = { ["furyx639.ExpandedStorage/Storage"] = id },
                    Stack = int.MaxValue,
                },
                0);
        }

        // Add crafting recipes to stock
        var recipes = this._helper.GameContent.Load<Dictionary<string, string>>("Data/CraftingRecipes");
        foreach (var (id, _) in this._storages)
        {
            if (!recipes.ContainsKey(id) || Game1.player.craftingRecipes.ContainsKey(id))
            {
                if (!recipes.ContainsKey($"{id}Recipe") || Game1.player.craftingRecipes.ContainsKey($"{id}Recipe"))
                {
                    continue;
                }
            }

            Utility.AddStock(
                itemPriceAndStock,
                new SObject(Vector2.Zero, 232, true)
                {
                    name = id,
                    modData = { ["furyx639.ExpandedStorage/Storage"] = id },
                    Stack = 1,
                },
                0);
        }

        Game1.activeClickableMenu = new ShopMenu(itemPriceAndStock);
    }
}