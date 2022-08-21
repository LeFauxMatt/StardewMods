namespace StardewMods.PortableHoles;

using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Locations;

/// <inheritdoc />
public class PortableHoles : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(this.Helper.Translation);
        Log.Monitor = this.Monitor;

        // Patches
        HarmonyHelper.AddPatches(
            this.ModManifest.UniqueID,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
                    typeof(PortableHoles),
                    nameof(PortableHoles.CraftingRecipe_createItem_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                    typeof(PortableHoles),
                    nameof(PortableHoles.Item_canStackWith_postfix),
                    PatchType.Postfix),
                new(
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
                    typeof(PortableHoles),
                    nameof(PortableHoles.Object_draw_prefix),
                    PatchType.Prefix),
                new(
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
                    typeof(PortableHoles),
                    nameof(PortableHoles.Object_drawLocal_prefix),
                    PatchType.Prefix),
                new(
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
                    typeof(PortableHoles),
                    nameof(PortableHoles.Object_drawInMenu_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
                    typeof(PortableHoles),
                    nameof(PortableHoles.Object_drawWhenHeld_prefix),
                    PatchType.Prefix),
            });
        HarmonyHelper.ApplyPatches(this.ModManifest.UniqueID);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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
            Game1.pixelZoom,
            SpriteEffects.None,
            Math.Max(0f, ((y + 1) * Game1.tileSize + 2) / 10000f) + x / 1000000f);
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
        StackDrawType drawStackNumber,
        Color color,
        bool drawShadow)
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

        if (__instance.Stack > 1)
        {
            Utility.drawTinyDigits(
                __instance.Stack,
                spriteBatch,
                location
              + new Vector2(
                    64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) + 3f * scaleSize,
                    64f - 18f * scaleSize + 2f),
                3f * scaleSize,
                1f,
                color);
        }

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
            (int)(position.X - scaleFactor.X / 2f),
            (int)(position.Y - scaleFactor.Y / 2f),
            (int)(Game1.tileSize + scaleFactor.X),
            (int)(Game1.tileSize * 2 + scaleFactor.Y / 2f));
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
            Math.Max(0f, (f.getStandingY() + 3) / 10000f));
        return false;
    }

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        switch (id)
        {
            case "PortableHoles.PlaceHole":
                PortableHoles.TryToPlaceHole();
                return;
        }
    }

    private static bool TryToPlaceHole()
    {
        if (Game1.currentLocation is not MineShaft mineShaft || !mineShaft.shouldCreateLadderOnThisLevel())
        {
            return false;
        }

        var pos = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y)
                / Game1.tileSize;
        if (!Game1.wasMouseVisibleThisFrame
         || Game1.mouseCursorTransparency == 0f
         || !Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player))
        {
            pos = Game1.player.GetGrabTile();
        }

        mineShaft.createLadderDown((int)pos.X, (int)pos.Y, true);
        return true;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/texture.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data.Add("Portable Hole", "769 99/Field/71/true/Mining 10/Staircase");
                });
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!e.Button.IsUseToolButton()
         || Game1.player.CurrentItem is not SObject { bigCraftable.Value: true, ParentSheetIndex: 71 } obj
         || !obj.modData.ContainsKey($"{this.ModManifest.UniqueID}/PortableHole"))
        {
            return;
        }

        if (!PortableHoles.TryToPlaceHole())
        {
            return;
        }

        Game1.player.reduceActiveItemByOne();
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var toolbarIcons = new ToolbarIconsIntegration(this.Helper.ModRegistry);
        if (!toolbarIcons.IsLoaded)
        {
            return;
        }

        toolbarIcons.API.AddToolbarIcon(
            "PortableHoles.PlaceHole",
            $"{this.ModManifest.UniqueID}/Icons",
            new Rectangle(0, 0, 16, 16),
            I18n.Button_PortableHole_Tooltip());

        toolbarIcons.API.ToolbarIconPressed += PortableHoles.OnToolbarIconPressed;
    }
}