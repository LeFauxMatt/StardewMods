namespace StardewMods.PortableHoles.Framework;

using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Menus;

/// <summary>
///     Harmony Patches for Portable Holes.
/// </summary>
internal sealed class ModPatches
{
#nullable disable
    private static ModPatches instance;
#nullable enable

    private readonly ModConfig config;

    private ModPatches(IManifest manifest, ModConfig config)
    {
        this.config = config;
        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(CraftingPage), "layoutRecipes"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingPage_layoutRecipes_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingRecipe_createItem_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Item_canStackWith_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
            transpiler: new(typeof(ModPatches), nameof(ModPatches.MineShaft_enterMineShaft_transpiler)));
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
        harmony.Patch(
            AccessTools.Method(typeof(SObject), "loadDisplayName"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_loadDisplayName_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
            new(typeof(ModPatches), nameof(ModPatches.Object_placementAction_prefix)));
    }

    private static ModConfig Config => ModPatches.instance.config;

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IModHelper helper, IManifest manifest, ModConfig config)
    {
        return ModPatches.instance ??= new(manifest, config);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CraftingPage_layoutRecipes_postfix(CraftingPage __instance)
    {
        foreach (var page in __instance.pagesOfCraftingRecipes)
        {
            foreach (var (component, recipe) in page)
            {
                if (!recipe.name.Equals("Portable Hole"))
                {
                    continue;
                }

                component.texture = Game1.content.Load<Texture2D>("furyx639.PortableHoles/Texture");
                component.sourceRect = new(0, 0, 16, 32);
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void CraftingRecipe_createItem_postfix(CraftingRecipe __instance, ref Item __result)
    {
        if (!__instance.name.Equals("Portable Hole")
            || __result is not SObject { bigCraftable.Value: true, ParentSheetIndex: 71 } obj)
        {
            return;
        }

        obj.modData["furyx639.PortableHoles/PortableHole"] = "true";
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (!__result
            || __instance is not SObject { bigCraftable.Value: true, ParentSheetIndex: 71 } obj
            || other is not SObject { bigCraftable.Value: true, ParentSheetIndex: 71 } otherObj)
        {
            return;
        }

        if (obj.modData.ContainsKey("furyx639.PortableHoles/PortableHole")
            ^ otherObj.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            __result = false;
        }
    }

    private static IEnumerable<CodeInstruction> MineShaft_enterMineShaft_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.StoresField(AccessTools.Field(typeof(Farmer), nameof(Farmer.health))))
            {
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.SetFarmerHealth));
            }

            yield return instruction;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
    {
        if (!__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            return true;
        }

        var texture = Game1.content.Load<Texture2D>("furyx639.PortableHoles/Texture");
        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            new Rectangle(0, 0, 16, 32),
            Color.White * alpha,
            0f,
            Vector2.Zero,
            __instance.getScale() * Game1.pixelZoom,
            SpriteEffects.None,
            Math.Max(0f, (((y + 1) * Game1.tileSize) + 2) / 10000f) + (x / 1000000f));
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
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
        if (!__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            return true;
        }

        if (__instance.IsRecipe)
        {
            transparency = 0.5f;
            scaleSize *= 0.75f;
        }

        var texture = Game1.content.Load<Texture2D>("furyx639.PortableHoles/Texture");
        spriteBatch.Draw(
            texture,
            location + new Vector2(32f, 32f),
            new Rectangle(0, 0, 16, 32),
            color * transparency,
            0f,
            new(8f, 16f),
            Game1.pixelZoom * (scaleSize < 0.2 ? scaleSize : scaleSize / 2f),
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
        if (!__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            return true;
        }

        var texture = Game1.content.Load<Texture2D>("furyx639.PortableHoles/Texture");
        var scaleFactor = __instance.getScale();
        scaleFactor *= Game1.pixelZoom;
        var position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
        var destination = new Rectangle(
            (int)(position.X - (scaleFactor.X / 2f)),
            (int)(position.Y - (scaleFactor.Y / 2f)),
            (int)(Game1.tileSize + scaleFactor.X),
            (int)((Game1.tileSize * 2) + (scaleFactor.Y / 2f)));
        spriteBatch.Draw(
            texture,
            destination,
            new Rectangle(0, 0, 16, 32),
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
        if (!__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            return true;
        }

        var texture = Game1.content.Load<Texture2D>("furyx639.PortableHoles/Texture");
        spriteBatch.Draw(
            texture,
            objectPosition,
            new(0, 0, 16, 32),
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
        if (__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            __result = I18n.Item_PortableHole_Description();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_loadDisplayName_postfix(SObject __instance, ref string __result)
    {
        if (__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole"))
        {
            __result = I18n.Item_PortableHole_Name();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static bool Object_placementAction_prefix(SObject __instance)
    {
        return !__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole");
    }

    private static int SetFarmerHealth(int targetHealth)
    {
        return ModPatches.Config.SoftFall ? Game1.player.health : targetHealth;
    }
}