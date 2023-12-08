namespace StardewMods.CustomBush.Framework;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Internal;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

/// <summary>Responsible for handling tea logic.</summary>
internal sealed class BushManager
{
    private const string ModDataId = "furyx639.CustomBush/Id";
    private const string ModDataItem = "furyx639.CustomBush/ShakeOff";

    private static readonly ConstructorInfo BushConstructor = AccessTools.Constructor(
        typeof(Bush),
        new[] { typeof(Vector2), typeof(int), typeof(GameLocation), typeof(int) });

#nullable disable
    private static BushManager instance;
#nullable enable
    private readonly AssetHandler assets;
    private readonly MethodInfo checkItemPlantRules;
    private readonly Dictionary<string, BushModel> data;

    private readonly IMonitor monitor;

    /// <summary>Initializes a new instance of the <see cref="BushManager" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="assets">Dependency used for managing assets.</param>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    public BushManager(IMonitor monitor, AssetHandler assets, Harmony harmony)
    {
        BushManager.instance = this;
        this.monitor = monitor;
        this.assets = assets;
        this.data = assets.TeaData;
        this.checkItemPlantRules = typeof(GameLocation).GetMethod(
                "CheckItemPlantRules",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new MethodAccessException("Unable to access CheckItemPlantRules");

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.draw), new[] { typeof(SpriteBatch) }),
            new(typeof(BushManager), nameof(BushManager.Bush_draw_prefix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.inBloom)),
            postfix: new(typeof(BushManager), nameof(BushManager.Bush_inBloom_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.setUpSourceRect)),
            postfix: new(typeof(BushManager), nameof(BushManager.Bush_setUpSourceRect_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Bush), nameof(Bush.shake)),
            transpiler: new(typeof(BushManager), nameof(BushManager.Bush_shake_transpiler)));

        harmony.Patch(
            typeof(GameLocation).GetMethod("CheckItemPlantRules", BindingFlags.Public | BindingFlags.Instance),
            postfix: new(typeof(BushManager), nameof(BushManager.GameLocation_CheckItemPlantRules_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(IndoorPot), nameof(IndoorPot.performObjectDropInAction)),
            postfix: new(typeof(BushManager), nameof(BushManager.IndoorPot_performObjectDropInAction_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.IsTeaSapling)),
            postfix: new(typeof(BushManager), nameof(BushManager.Object_IsTeaSapling_postfix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
            transpiler: new(typeof(BushManager), nameof(BushManager.Object_placementAction_transpiler)));
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static Bush AddModData(Bush bush, SObject obj)
    {
        if (!BushManager.instance.data.ContainsKey(obj.QualifiedItemId))
        {
            return bush;
        }

        bush.modData[BushManager.ModDataId] = obj.QualifiedItemId;
        bush.setUpSourceRect();
        return bush;
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
        if (!__instance.modData.TryGetValue(BushManager.ModDataId, out var id)
            || !BushManager.instance.data.TryGetValue(id, out var bushModel))
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

        var texture = BushManager.instance.assets.GetTexture(bushModel.Texture);
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
        if (__instance.modData.TryGetValue(BushManager.ModDataItem, out var itemId)
            && !string.IsNullOrWhiteSpace(itemId))
        {
            __result = true;
            return;
        }

        if (!__instance.modData.TryGetValue(BushManager.ModDataId, out var id)
            || !BushManager.instance.data.TryGetValue(id, out var bushModel))
        {
            return;
        }

        var season = __instance.Location.GetSeason();
        var dayOfMonth = Game1.dayOfMonth;
        var age = __instance.getAge();

        // Fails basic conditions
        if (age < bushModel.AgeToProduce || dayOfMonth < bushModel.DayToBeginProducing)
        {
            __result = false;
            return;
        }

        // Fails default season conditions
        if (!bushModel.Seasons.Any() && season == Season.Winter && !__instance.IsSheltered())
        {
            __result = false;
            return;
        }

        // Fails custom season conditions
        if (bushModel.Seasons.Any() && !bushModel.Seasons.Contains(season) && !__instance.IsSheltered())
        {
            __result = false;
            return;
        }

        // Try to produce item
        if (!BushManager.instance.TryToProduceRandomItem(__instance, bushModel, out itemId))
        {
            __result = false;
            return;
        }

        __result = true;
        __instance.modData[BushManager.ModDataItem] = itemId;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Bush_setUpSourceRect_postfix(Bush __instance, NetRectangle ___sourceRect)
    {
        if (!__instance.modData.TryGetValue(BushManager.ModDataId, out var id)
            || !BushManager.instance.data.TryGetValue(id, out var bushModel))
        {
            return;
        }

        var season = !__instance.IsSheltered() ? __instance.Location.GetSeason() : Season.Spring;
        var age = __instance.getAge();
        var offset = Math.Min(2, age / 10) * 16;
        var (x, y) = season switch
        {
            Season.Summer => (64 + offset + (__instance.tileSheetOffset.Value * 16), bushModel.TextureSpriteRow * 16),
            Season.Fall => (offset + (__instance.tileSheetOffset.Value * 16), (bushModel.TextureSpriteRow * 16) + 32),
            Season.Winter => (64 + offset + (__instance.tileSheetOffset.Value * 16),
                (bushModel.TextureSpriteRow * 16) + 32),
            _ => ((Math.Min(2, age / 10) * 16) + (__instance.tileSheetOffset.Value * 16),
                bushModel.TextureSpriteRow * 16),
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
                yield return CodeInstruction.Call(typeof(BushManager), nameof(BushManager.GetItemProduced));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void GameLocation_CheckItemPlantRules_postfix(
        GameLocation __instance,
        ref bool __result,
        string itemId,
        bool isGardenPot,
        bool defaultAllowed,
        ref string deniedMessage)
    {
        var metadata = ItemRegistry.GetMetadata(itemId);
        if (metadata is null
            || metadata.TypeIdentifier != "(O)"
            || !BushManager.instance.data.TryGetValue(metadata.QualifiedItemId, out var bushModel))
        {
            return;
        }

#nullable disable
        var parameters = new object[] { bushModel!.PlantableLocationRules, isGardenPot, defaultAllowed, null };
        __result = (bool)BushManager.instance.checkItemPlantRules.Invoke(__instance, parameters)!;
        deniedMessage = (string)parameters[3];
#nullable enable
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static string GetItemProduced(Bush bush)
    {
        if (bush.modData.TryGetValue(BushManager.ModDataItem, out var itemId) && !string.IsNullOrWhiteSpace(itemId))
        {
            bush.modData.Remove(BushManager.ModDataItem);
            return itemId;
        }

        if (bush.modData.TryGetValue(BushManager.ModDataId, out var id)
            && BushManager.instance.data.TryGetValue(id, out var bushModel)
            && BushManager.instance.TryToProduceRandomItem(bush, bushModel, out itemId))
        {
            return itemId;
        }

        return "(O)815";
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void IndoorPot_performObjectDropInAction_postfix(
        IndoorPot __instance,
        Item dropInItem,
        bool probe,
        ref bool __result)
    {
        if (!BushManager.instance.data.ContainsKey(dropInItem.QualifiedItemId) || __instance.hoeDirt.Value.crop != null)
        {
            return;
        }

        if (!probe)
        {
            __instance.bush.Value = new(__instance.TileLocation, 3, __instance.Location);
            __instance.bush.Value.modData[BushManager.ModDataId] = dropInItem.QualifiedItemId;
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

        if (BushManager.instance.data.ContainsKey(__instance.QualifiedItemId))
        {
            __result = true;
        }
    }

    private static IEnumerable<CodeInstruction> Object_placementAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Is(OpCodes.Newobj, BushManager.BushConstructor))
            {
                yield return instruction;
                yield return new(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(BushManager), nameof(BushManager.AddModData));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private bool TryToProduceRandomItem(Bush bush, BushModel bushModel, [NotNullWhen(true)] out string? itemId)
    {
        foreach (var drop in bushModel.ItemsProduced)
        {
            var item = this.TryToProduceItem(bush, drop);
            if (item is null)
            {
                continue;
            }

            itemId = item.QualifiedItemId;
            return true;
        }

        itemId = null;
        return false;
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private Item? TryToProduceItem(Bush bush, DropsModel drop)
    {
        if (!Game1.random.NextBool(drop.Chance))
        {
            return null;
        }

        if (drop.Condition != null
            && !GameStateQuery.CheckConditions(
                drop.Condition,
                bush.Location,
                null,
                null,
                null,
                null,
                bush.Location.SeedsIgnoreSeasonsHere() ? GameStateQuery.SeasonQueryKeys : null))
        {
            return null;
        }

        if (drop.Season.HasValue
            && bush.Location.SeedsIgnoreSeasonsHere()
            && drop.Season != Game1.GetSeasonForLocation(bush.Location))
        {
            return null;
        }

        var item = ItemQueryResolver.TryResolveRandomItem(
            drop,
            new(bush.Location, null, null),
            false,
            null,
            null,
            null,
            delegate(string query, string error)
            {
                this.monitor.LogOnce(
                    $"Custom Bush {bush.modData[BushManager.ModDataId]} failed parsing item query {query} for item {drop.Id}: {error}",
                    LogLevel.Error);
            });

        return item;
    }
}
