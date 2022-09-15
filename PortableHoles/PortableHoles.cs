namespace StardewMods.PortableHoles;

using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewValley.Locations;

// TODO: patch CraftingRecipe.getIndexOfMenuView()
// TODO: patch CraftingPage.layoutRecipes()

/// <inheritdoc />
public class PortableHoles : Mod
{
    private static PortableHoles? Instance;

    private ModConfig? _config;

    private ModConfig Config => this._config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        PortableHoles.Instance = this;

        I18n.Init(this.Helper.Translation);
        Log.Monitor = this.Monitor;

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
            postfix: new(typeof(PortableHoles), nameof(PortableHoles.CraftingRecipe_createItem_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(PortableHoles), nameof(PortableHoles.Item_canStackWith_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
            transpiler: new(typeof(PortableHoles), nameof(PortableHoles.MineShaft_enterMineShaft_transpiler)));
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
            new(typeof(PortableHoles), nameof(PortableHoles.Object_draw_prefix)));
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
            new(typeof(PortableHoles), nameof(PortableHoles.Object_drawLocal_prefix)));
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
            new(typeof(PortableHoles), nameof(PortableHoles.Object_drawInMenu_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
            new(typeof(PortableHoles), nameof(PortableHoles.Object_drawWhenHeld_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
            new(typeof(PortableHoles), nameof(PortableHoles.Object_placementAction_prefix)));
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
                yield return CodeInstruction.Call(typeof(PortableHoles), nameof(PortableHoles.SetFarmerHealth));
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

        if (__instance.Stack > 1)
        {
            Utility.drawTinyDigits(
                __instance.Stack,
                spriteBatch,
                location
              + new Vector2(
                    Game1.tileSize
                  - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)
                  + 3f * scaleSize,
                    Game1.tileSize - 18f * scaleSize + 2f),
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
            Math.Max(0f, (f.getStandingY() + 3) / 10000f));
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static bool Object_placementAction_prefix(SObject __instance)
    {
        return !__instance.modData.ContainsKey("furyx639.PortableHoles/PortableHole");
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

    private static int SetFarmerHealth(int targetHealth)
    {
        return PortableHoles.Instance!.Config.SoftFall ? Game1.player.health : targetHealth;
    }

    private static bool TryToPlaceHole()
    {
        if (Game1.currentLocation is not MineShaft { mineLevel: > 120 } mineShaft
         || !mineShaft.shouldCreateLadderOnThisLevel())
        {
            return false;
        }

        var pos = CommonHelpers.GetCursorTile(1);
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
                    data.Add("Portable Hole", $"769 99/Field/71/true/Mining 10/{I18n.Item_PortableHole_Name()}");
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

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (this.Config.UnlockAutomatically && !Game1.player.craftingRecipes.ContainsKey("Portable Hole"))
        {
            Game1.player.craftingRecipes.Add("Portable Hole", 0);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var toolbarIcons = new ToolbarIconsIntegration(this.Helper.ModRegistry);
        if (toolbarIcons.IsLoaded)
        {
            toolbarIcons.API.AddToolbarIcon(
                "PortableHoles.PlaceHole",
                $"{this.ModManifest.UniqueID}/Icons",
                new Rectangle(0, 0, 16, 16),
                I18n.Button_PortableHole_Tooltip());

            toolbarIcons.API.ToolbarIconPressed += PortableHoles.OnToolbarIconPressed;
        }

        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        if (!gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(this.ModManifest, () => this._config = new(), () => this.Helper.WriteConfig(this.Config));

        // Soft Fall
        gmcm.API.AddBoolOption(
            this.ModManifest,
            () => this.Config.SoftFall,
            value => this.Config.SoftFall = value,
            I18n.Config_SoftFall_Name,
            I18n.Config_SoftFall_Tooltip);

        // Unlock Automatically
        gmcm.API.AddBoolOption(
            this.ModManifest,
            () => this.Config.UnlockAutomatically,
            value => this.Config.UnlockAutomatically = value,
            I18n.Config_UnlockAutomatically_Name,
            I18n.Config_UnlockAutomatically_Tooltip);
    }
}