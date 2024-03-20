namespace StardewMods.CrystallineJunimoChests;

using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.CrystallineJunimoChests.Models;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Harmony Patches for Enhanced Junimo Chests.</summary>
internal sealed class ModPatches
{
    private static IModContentHelper modContentHelper = null!;
    private static string modId = null!;
    private static Texture2D texture = null!;

    /// <summary>Initializes a new instance of the <see cref="ModPatches" /> class.</summary>
    /// <param name="modContentHelper">Dependency used for accessing mod content.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="texture">The texture to use for colored Junimo chests.</param>
    public ModPatches(IModContentHelper modContentHelper, IManifest manifest, Texture2D texture)
    {
        ModPatches.modContentHelper = modContentHelper;
        ModPatches.modId = manifest.UniqueID;
        ModPatches.texture = texture;

        // Patches
        var harmony = new Harmony(ModPatches.modId);

        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Chest),
                nameof(Chest.draw),
                [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
            new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.Draw)));

        harmony.Patch(
            AccessTools.DeclaredMethod(
                typeof(Chest),
                nameof(Chest.draw),
                [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)]),
            new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.DrawLocal)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.GetActualCapacity)),
            postfix: new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.GetActualCapacity)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.getLastLidFrame)),
            postfix: new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.GetLastLidFrame)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
            postfix: new HarmonyMethod(typeof(ModPatches), nameof(ModPatches.PlacementAction)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Draw(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return true;
        }

        var drawX = (float)x;
        var drawY = (float)y;
        if (__instance.localKickStartTile.HasValue)
        {
            drawX = Utility.Lerp(__instance.localKickStartTile.Value.X, drawX, __instance.kickProgress);
            drawY = Utility.Lerp(__instance.localKickStartTile.Value.Y, drawY, __instance.kickProgress);
        }

        var baseSortOrder = Math.Max(0f, (((drawY + 1f) * 64f) - 24f) / 10000f) + (drawX * 1E-05f);
        if (__instance.localKickStartTile.HasValue)
        {
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2((drawX + 0.5f) * 64f, (drawY + 0.5f) * 64f)),
                Game1.shadowTexture.Bounds,
                Color.Black * 0.5f,
                0f,
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4f,
                SpriteEffects.None,
                0.0001f);

            drawY -= (float)Math.Sin(__instance.kickProgress * Math.PI) * 0.5f;
        }

        var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY - 1f) * Game1.tileSize);
        var startingLidFrame = __instance.startingLidFrame.Value;
        var lastLidFrame = __instance.getLastLidFrame();

        var frame = new Rectangle(
            Math.Min(lastLidFrame - startingLidFrame + 1, Math.Max(0, ___currentLidFrame - startingLidFrame)) * 16,
            0,
            16,
            32);

        if (__instance.playerChoiceColor.Value.Equals(Color.Black))
        {
            spriteBatch.Draw(
                ModPatches.texture,
                pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
                frame,
                Color.White * alpha,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                baseSortOrder);

            return false;
        }

        spriteBatch.Draw(
            ModPatches.texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 32 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder);

        spriteBatch.Draw(
            ModPatches.texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 64 },
            __instance.playerChoiceColor.Value * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 1E-05f);

        var selection = DiscreteColorPicker.getSelectionFromColor(__instance.playerChoiceColor.Value);
        if (selection == -1)
        {
            return false;
        }

        spriteBatch.Draw(
            ModPatches.texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = (selection * 32) + 96 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 2E-05f);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool DrawLocal(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha, bool local)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return true;
        }

        var pos = local
            ? new Vector2(x, y - 64)
            : Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize);

        var baseSortOrder = local ? 0.89f : ((y * 64) + 4) / 10000f;
        var frame = new Rectangle(0, 0, 16, 32);

        if (__instance.playerChoiceColor.Value.Equals(Color.Black))
        {
            spriteBatch.Draw(
                ModPatches.texture,
                pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
                frame,
                Color.White * alpha,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                baseSortOrder);

            return false;
        }

        spriteBatch.Draw(
            ModPatches.texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 32 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder);

        spriteBatch.Draw(
            ModPatches.texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = 64 },
            __instance.playerChoiceColor.Value * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 1E-05f);

        var selection = DiscreteColorPicker.getSelectionFromColor(__instance.playerChoiceColor.Value);
        if (selection == -1)
        {
            return false;
        }

        spriteBatch.Draw(
            ModPatches.texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame with { Y = (32 * selection) + 96 },
            Color.White * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 2E-05f);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void GetActualCapacity(Chest __instance, ref int __result)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return;
        }

        __result = 9;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void GetLastLidFrame(Chest __instance, ref int __result)
    {
        if (!__instance.playerChest.Value || __instance.QualifiedItemId != "(BC)256")
        {
            return;
        }

        __result = __instance.startingLidFrame.Value + 4;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyBefore("furyx639.BetterChests")]
    private static void PlacementAction(SObject __instance, ref bool __result, GameLocation location, int x, int y)
    {
        if (!__result)
        {
            return;
        }

        var tile = new Vector2((int)(x / (float)Game1.tileSize), (int)(y / (float)Game1.tileSize));
        if (!location.Objects.TryGetValue(tile, out var obj)
            || obj is not Chest
            {
                SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            } chest)
        {
            return;
        }

        // Change special chest type to none so that it can be colorable
        chest.SpecialChestType = Chest.SpecialChestTypes.None;
        chest.GlobalInventoryId = "JunimoChests";

        // Disable some BetterChests features
        chest.modData["furyx639.BetterChests/HslColorPicker"] = "Disabled";
        chest.modData["furyx639.BetterChests/InventoryTabs"] = "Disabled";
        chest.modData["furyx639.BetterChests/ResizeChest"] = "Disabled";

        var data = ModPatches.modContentHelper.Load<DataModel>("assets/data.json");
        if (__instance.modData.TryGetValue($"{ModPatches.modId}/Color", out var colorString)
            && int.TryParse(colorString, out var selection)
            && selection >= 0
            && selection < data.Colors.Length)
        {
            // Copy color from item to chest
            chest.GlobalInventoryId = $"{ModPatches.modId}-{data.Colors[selection].Name}";
            chest.playerChoiceColor.Value = data.Colors[selection].Color;
        }
    }
}