namespace StardewMods.TeaTime.Framework;

using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

/// <summary>Responsible for handling tea logic.</summary>
internal sealed class TeaHandler
{
    private const string ModDataId = "furyx639.TeaTime/Id";

#nullable disable
    private static TeaHandler instance;
#nullable enable

    private readonly AssetHandler assets;
    private readonly Dictionary<string, TeaModel> data;

    /// <summary>
    /// Initializes a new instance of the <see cref="TeaHandler"/> class.
    /// </summary>
    /// <param name="assets">Dependency used for managing TeaTime assets.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    public TeaHandler(AssetHandler assets, Harmony harmony)
    {
        TeaHandler.instance = this;
        this.assets = assets;
        this.data = assets.TeaData;

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.draw), new[] { typeof(SpriteBatch) }),
            new(typeof(TeaHandler), nameof(TeaHandler.Bush_draw_prefix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.inBloom)),
            postfix: new(typeof(TeaHandler), nameof(TeaHandler.Bush_inBloom_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.setUpSourceRect)),
            postfix: new(typeof(TeaHandler), nameof(TeaHandler.Bush_setUpSourceRect_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.shake)),
            transpiler: new(typeof(TeaHandler), nameof(TeaHandler.Bush_shake_transpiler)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction)),
            postfix: new(typeof(TeaHandler), nameof(TeaHandler.IndoorPot_performObjectDropInAction_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.IsTeaSapling)),
            postfix: new(typeof(TeaHandler), nameof(TeaHandler.Object_IsTeaSapling_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Bush_draw_prefix(
        Bush __instance,
        SpriteBatch spriteBatch,
        float ___shakeRotation,
        NetRectangle ___sourceRect,
        float ___yDrawOffset)
    {
        if (!__instance.modData.TryGetValue(TeaHandler.ModDataId, out var id)
            || !TeaHandler.instance.data.TryGetValue(id, out var teaModel))
        {
            return true;
        }

        var x = (__instance.Tile.X * 64) + 32;
        var y = (__instance.Tile.Y * 64) + 64 + ___yDrawOffset;
        if (__instance.drawShadow.Value)
        {
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 4)),
                Game1.shadowTexture.Bounds,
                Color.White,
                0,
                new(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4,
                SpriteEffects.None,
                1E-06f);
        }

        var texture = TeaHandler.instance.assets.GetTexture(teaModel.Texture);
        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y)),
            ___sourceRect.Value,
            Color.White,
            ___shakeRotation,
            new(8, 32),
            4,
            __instance.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
            ((__instance.getBoundingBox().Center.Y + 48) / 10000f) - (__instance.Tile.X / 1000000f));

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Bush_inBloom_postfix(Bush __instance, ref bool __result)
    {
        if (!__instance.modData.TryGetValue(TeaHandler.ModDataId, out var id)
            || !TeaHandler.instance.data.TryGetValue(id, out var teaModel))
        {
            return;
        }

        var season = __instance.Location.GetSeason();
        var dayOfMonth = Game1.dayOfMonth;
        var age = __instance.getAge();
        if (age >= teaModel.AgeToProduce && dayOfMonth >= teaModel.DayToBeginProducing)
        {
            if (season == Season.Winter)
            {
                __result = __instance.IsSheltered();
                return;
            }

            __result = true;
            return;
        }

        __result = false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Bush_setUpSourceRect_postfix(Bush __instance, NetRectangle ___sourceRect)
    {
        if (!__instance.modData.TryGetValue(TeaHandler.ModDataId, out var id)
            || !TeaHandler.instance.data.TryGetValue(id, out _))
        {
            return;
        }

        var season = !__instance.IsSheltered() ? __instance.Location.GetSeason() : Season.Spring;
        var age = __instance.getAge();
        var offset = Math.Min(2, age / 10) * 16;
        var (x, y) = season switch
        {
            Season.Summer => (64 + offset + (__instance.tileSheetOffset.Value * 16), 0),
            Season.Fall => (offset + (__instance.tileSheetOffset.Value * 16), 32),
            Season.Winter => (64 + offset + (__instance.tileSheetOffset.Value * 16), 32),
            _ => ((Math.Min(2, age / 10) * 16) + (__instance.tileSheetOffset.Value * 16), 0),
        };
        ___sourceRect.Value = new(x, y, 16, 32);
    }

    private static IEnumerable<CodeInstruction> Bush_shake_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsConstant("(O)815"))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(TeaHandler), nameof(TeaHandler.GetItemProduced));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static string GetItemProduced(Bush bush)
    {
        if (!bush.modData.TryGetValue(TeaHandler.ModDataId, out var id)
            || !TeaHandler.instance.data.TryGetValue(id, out var teaModel))
        {
            return "(O)815";
        }

        return teaModel.ItemProduced;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void IndoorPot_performObjectDropInAction_postfix(IndoorPot __instance, Item dropInItem, bool probe, ref bool __result)
    {
        if (!TeaHandler.instance.data.ContainsKey(dropInItem.QualifiedItemId) || __instance.hoeDirt.Value.crop != null)
        {
            return;
        }

        if (!probe)
        {
            __instance.bush.Value = new(__instance.TileLocation, 3, __instance.Location);
            __instance.bush.Value.modData.Add(TeaHandler.ModDataId, dropInItem.QualifiedItemId);
            if (!__instance.Location.IsOutdoors)
            {
                __instance.bush.Value.inPot.Value = true;
                __instance.bush.Value.loadSprite();
                Game1.playSound("coin");
            }
        }

        __result = true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "RedundantAssignment", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_IsTeaSapling_postfix(SObject __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (TeaHandler.instance.data.ContainsKey(__instance.QualifiedItemId))
        {
            __result = true;
        }
    }
}
