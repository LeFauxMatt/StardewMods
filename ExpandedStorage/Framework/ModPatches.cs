namespace StardewMods.ExpandedStorage.Framework;

using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>
///     Harmony Patches for Expanded Storage.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
[SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
[SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
internal sealed class ModPatches
{
#nullable disable
    private static ModPatches Instance;
#nullable enable

    private readonly IGameContentHelper _gameContent;
    private readonly IDictionary<string, IManagedStorage> _storages;

    private ModPatches(
        IGameContentHelper gameContent,
        IManifest manifest,
        IDictionary<string, IManagedStorage> storages)
    {
        this._gameContent = gameContent;
        this._storages = storages;
        var harmony = new Harmony(manifest.UniqueID);

        // Drawing
        harmony.Patch(
            AccessTools.Method(
                typeof(Chest),
                nameof(Chest.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Chest_draw_prefix)));
        harmony.Patch(
            AccessTools.Method(
                typeof(Chest),
                nameof(Chest.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                    typeof(bool),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Chest_drawLocal_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.drawInMenu)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_drawInMenu_prefix)));
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
                    typeof(bool),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawLocal_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.drawInMenu)),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawInMenu_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawWhenHeld_prefix)));

        // Buying


        // Crafting
        harmony.Patch(
            AccessTools.Method(typeof(CraftingPage), "layoutRecipes"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingPage_layoutRecipes_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingRecipe_createItem_postfix)));

        // Handling
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Item_canStackWith_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
            new(typeof(ModPatches), nameof(ModPatches.Object_placementAction_prefix)));
    }

    private static IGameContentHelper GameContent => ModPatches.Instance._gameContent;

    private static IDictionary<string, IManagedStorage> Storages => ModPatches.Instance._storages;

    public static ModPatches Init(
        IGameContentHelper gameContent,
        IManifest manifest,
        IDictionary<string, IManagedStorage> storages)
    {
        return ModPatches.Instance ??= new(gameContent, manifest, storages);
    }

    private static bool Chest_draw_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        var drawX = (float)x;
        var drawY = (float)y;
        if (__instance.localKickStartTile.HasValue)
        {
            drawX = Utility.Lerp(__instance.localKickStartTile.Value.X, drawX, __instance.kickProgress);
            drawY = Utility.Lerp(__instance.localKickStartTile.Value.Y, drawY, __instance.kickProgress);
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX + 0.5f, drawY + 0.5f) * Game1.tileSize),
                Game1.shadowTexture.Bounds,
                Color.Black * 0.5f,
                0f,
                new(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4f,
                SpriteEffects.None,
                0.0001f);
            drawY -= (float)Math.Sin(__instance.kickProgress * Math.PI) * 0.5f;
        }

        storage.Draw(
            __instance,
            name,
            ___currentLidFrame,
            spriteBatch,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY - 1) * Game1.tileSize),
            alpha);
        return false;
    }

    private static bool Chest_drawInMenu_prefix(Chest __instance)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        return true;
    }

    private static bool Chest_drawLocal_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha,
        bool local)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        storage.Draw(
            __instance,
            name,
            ___currentLidFrame,
            spriteBatch,
            local ? new(x, y - 1) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            alpha);
        return false;
    }

    private static void CraftingPage_layoutRecipes_postfix(CraftingPage __instance)
    {
        foreach (var page in __instance.pagesOfCraftingRecipes)
        {
            foreach (var (component, recipe) in page)
            {
                if (!ModPatches.Storages.TryGetValue(recipe.name, out var storage)) { }

                //component.texture = Game1.content.Load<Texture2D>("furyx639.PortableHoles/Texture");
                //component.sourceRect = new(0, 0, 16, 32);
            }
        }
    }

    private static void CraftingRecipe_createItem_postfix(CraftingRecipe __instance, ref Item __result)
    {
        if (__result is not SObject { bigCraftable.Value: true, ParentSheetIndex: 232 }
         || !ModPatches.Storages.TryGetValue(__instance.name, out var storage))
        {
            return;
        }

        foreach (var (key, value) in storage.ModData)
        {
            __result.modData[key] = value;
        }

        __result.modData["furyx639.ExpandedStorage/Storage"] = __instance.name;
    }

    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (!__result
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.ContainsKey(name))
        {
            return;
        }

        if (other is not SObject { bigCraftable.Value: true, ParentSheetIndex: 232 } obj
         || !obj.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var otherName)
         || !name.Equals(otherName, StringComparison.OrdinalIgnoreCase))
        {
            __result = false;
        }
    }

    private static bool Object_draw_prefix(SObject __instance, int x, int y, float alpha)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        return true;
    }

    private static bool Object_drawInMenu_prefix(SObject __instance)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        return true;
    }

    private static bool Object_drawLocal_prefix(SObject __instance, int x, int y, float alpha, bool local)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        return true;
    }

    private static bool Object_drawWhenHeld_prefix(SObject __instance)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        return true;
    }

    private static bool Object_placementAction_prefix(SObject __instance)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        return true;
    }
}