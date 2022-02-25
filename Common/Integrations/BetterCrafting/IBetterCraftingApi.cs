namespace Common.Integrations.BetterCrafting;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

public interface IBetterCraftingApi
{
    /// <summary>
    ///     Return the Better Crafting menu's type. In case you want to do
    ///     spooky stuff to it, I guess.
    /// </summary>
    /// <returns>The BetterCraftingMenu type.</returns>
    Type GetMenuType();

    /// <summary>
    ///     Invalidate the recipe cache. You should call this if your recipe
    ///     provider ever adds new recipes after registering it.
    /// </summary>
    void InvalidateRecipeCache();

    /// <summary>
    ///     Try to open the Better Crafting menu. This may fail if there is another
    ///     menu open that cannot be replaced.
    ///     If opening the menu from an object in the world, such as a workbench,
    ///     its location and tile position can be provided for automatic detection
    ///     of nearby chests.
    /// </summary>
    /// <param name="cooking">If true, open the cooking menu. If false, open the crafting menu.</param>
    /// <param name="containers">An optional list of chests to draw extra crafting materials from.</param>
    /// <param name="location">The map the associated object is in, or null if there is no object.</param>
    /// <param name="position">The tile position the associated object is at, or null if there is no object.</param>
    /// <param name="silent_open">If true, do not make a sound upon opening the menu.</param>
    /// <param name="listed_recipes">
    ///     An optional list of recipes by name. If provided, only these recipes will be listed in the
    ///     crafting menu.
    /// </param>
    /// <returns>Whether or not the menu was opened successfully.</returns>
    bool OpenCraftingMenu(
        bool cooking,
        IList<Chest> containers = null,
        GameLocation location = null,
        Vector2? position = null,
        bool silent_open = false,
        IList<string> listed_recipes = null);
}