namespace StardewMods.IsItCake.Framework;

using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
///     Harmony Patches for Is It Cake.
/// </summary>
internal sealed class ModPatches
{
#nullable disable
    private static ModPatches instance;
#nullable enable

    private ModPatches(IManifest manifest)
    {
        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingRecipe_createItem_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Item_canStackWith_postfix)));
        harmony.Patch(
            AccessTools.Method(
                typeof(SObject),
                nameof(SObject.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Object_draw_prefix)));
        harmony.Patch(
            AccessTools.Method(
                typeof(SObject),
                nameof(SObject.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                    typeof(float),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawLocal_prefix)));
        harmony.Patch(
            AccessTools.Method(
                typeof(SObject),
                nameof(SObject.drawInMenu),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(Vector2),
                    typeof(float),
                    typeof(float),
                    typeof(float),
                    typeof(StackDrawType),
                    typeof(Color),
                    typeof(bool),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawInMenu_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawWhenHeld_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.getDescription)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_getDescription_postfix)));
    }

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IManifest manifest)
    {
        return ModPatches.instance ??= new(manifest);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CraftingRecipe_createItem_postfix(CraftingRecipe __instance, ref Item __result)
    {
        if (Game1.player.CurrentItem is not SObject { bigCraftable.Value: false } item
            || !(__instance.name.Equals("Chocolate Cake") || __instance.name.Equals("Pink Cake"))
            || __result is not SObject { bigCraftable.Value: false, ParentSheetIndex: 220 or 221 } obj)
        {
            return;
        }

        obj.modData["furyx639.IsItCake/ParentSheetIndex"] = item.ParentSheetIndex.ToString();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (!__result
            || __instance is not SObject { bigCraftable.Value: false, ParentSheetIndex: 220 or 221 } obj
            || other is not SObject { bigCraftable.Value: false, ParentSheetIndex: 220 or 221 } otherObj)
        {
            return;
        }

        if (!obj.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var id))
        {
            id = obj.ParentSheetIndex.ToString();
        }

        if (!otherObj.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var otherId))
        {
            otherId = otherObj.ParentSheetIndex.ToString();
        }

        __result = id == otherId;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
    {
        if (!__instance.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var id)
            || !int.TryParse(id, out var parentSheetIndex))
        {
            return true;
        }

        spriteBatch.Draw(
            Game1.objectSpriteSheet,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16),
            Color.White * alpha,
            0f,
            Vector2.Zero,
            __instance.getScale() * Game1.pixelZoom,
            SpriteEffects.None,
            Math.Max(0f, (((y + 1) * Game1.tileSize) + 2) / 10000f) + (x / 1000000f));
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_drawInMenu_prefix(
        SObject __instance,
        SpriteBatch spriteBatch,
        Vector2 location,
        float scaleSize,
        float transparency,
        float layerDepth,
        Color color)
    {
        if (!__instance.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var id)
            || !int.TryParse(id, out var parentSheetIndex))
        {
            return true;
        }

        spriteBatch.Draw(
            Game1.objectSpriteSheet,
            location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)),
            Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16),
            color * transparency,
            0f,
            new Vector2(8f, 8f) * scaleSize,
            4f * scaleSize,
            SpriteEffects.None,
            layerDepth);

        if (__instance.Stack <= 1)
        {
            return false;
        }

        var position = location
            + new Vector2(
                Game1.tileSize
                - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)
                + (3f * scaleSize),
                Game1.tileSize - (18f * scaleSize) + 2f);
        Utility.drawTinyDigits(
            __instance.Stack,
            spriteBatch,
            position,
            3f * scaleSize,
            1f,
            color);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_drawLocal_prefix(
        SObject __instance,
        SpriteBatch spriteBatch,
        int xNonTile,
        int yNonTile,
        float layerDepth,
        float alpha = 1f)
    {
        if (!__instance.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var id)
            || !int.TryParse(id, out var parentSheetIndex))
        {
            return true;
        }

        var scaleFactor = __instance.getScale();
        scaleFactor *= Game1.pixelZoom;
        var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
        var destination = new Rectangle(
            (int)(position.X - (scaleFactor.X / 2f)),
            (int)(position.Y - (scaleFactor.Y / 2f)),
            (int)(Game1.tileSize + scaleFactor.X),
            (int)((Game1.tileSize * 2) + (scaleFactor.Y / 2f)));
        spriteBatch.Draw(
            Game1.objectSpriteSheet,
            destination,
            Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16),
            Color.White * alpha,
            0f,
            Vector2.Zero,
            SpriteEffects.None,
            layerDepth);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static bool Object_drawWhenHeld_prefix(
        SObject __instance,
        SpriteBatch spriteBatch,
        Vector2 objectPosition,
        Farmer f)
    {
        if (!__instance.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var id)
            || !int.TryParse(id, out var parentSheetIndex))
        {
            return true;
        }

        spriteBatch.Draw(
            Game1.objectSpriteSheet,
            objectPosition,
            GameLocation.getSourceRectForObject(parentSheetIndex),
            Color.White,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            Math.Max(0f, (f.Tile.Y + 3) / 10000f));
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_getDescription_postfix(SObject __instance, ref string __result)
    {
        if (!__instance.modData.TryGetValue("furyx639.IsItCake/ParentSheetIndex", out var id)
            || !int.TryParse(id, out var parentSheetIndex)
            || parentSheetIndex == __instance.ParentSheetIndex)
        {
            return;
        }

        __result = I18n.Object_Cake_Description();
    }
}