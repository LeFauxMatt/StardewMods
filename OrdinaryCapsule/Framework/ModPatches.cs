namespace StardewMods.OrdinaryCapsule.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using StardewMods.Common.Helpers.ItemRepository;
using StardewMods.OrdinaryCapsule.Framework.Models;

/// <summary>
///     Harmony Patches for Ordinary Capsule.
/// </summary>
internal sealed class ModPatches
{
    private static readonly Dictionary<int, int> CachedTimes = new();

    private static readonly Lazy<List<Item>> ItemsLazy = new(
        () => new(new ItemRepository().GetAll().Select(item => item.Item)));

#nullable disable
    private static ModPatches Instance;
#nullable enable

    private readonly ModConfig _config;

    private ModPatches(IManifest manifest, ModConfig config)
    {
        this._config = config;

        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
            transpiler: new(typeof(ModPatches), nameof(ModPatches.Object_checkForAction_transpiler)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), "getMinutesForCrystalarium"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_getMinutesForCrystalarium_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            new(typeof(ModPatches), nameof(ModPatches.Object_minutesElapsed_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_minutesElapsed_postfix)));
    }

    private static IEnumerable<Item> AllItems => ModPatches.ItemsLazy.Value;

    private static ModConfig Config => ModPatches.Instance._config;

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IManifest manifest, ModConfig config)
    {
        return ModPatches.Instance ??= new(manifest, config);
    }

    private static int GetMinutes(int parentSheetIndex)
    {
        if (ModPatches.CachedTimes.TryGetValue(parentSheetIndex, out var minutes))
        {
            return minutes;
        }

        var item = ModPatches.AllItems.FirstOrDefault(item => item.ParentSheetIndex == parentSheetIndex);
        if (item is not null)
        {
            return ModPatches.GetMinutes(item);
        }

        ModPatches.CachedTimes[parentSheetIndex] = ModPatches.Config.DefaultProductionTime;
        return ModPatches.CachedTimes[parentSheetIndex];
    }

    private static int GetMinutes(Item item)
    {
        var capsuleItems = Game1.content.Load<List<CapsuleItem>>("furyx639.OrdinaryCapsule/CapsuleItems");
        var minutes = capsuleItems.Where(capsuleItem => capsuleItem.ContextTags.Any(item.GetContextTags().Contains))
                                  .Select(capsuleItem => capsuleItem.ProductionTime)
                                  .FirstOrDefault();
        ModPatches.CachedTimes[item.ParentSheetIndex] = minutes > 0 ? minutes : ModPatches.Config.DefaultProductionTime;
        return ModPatches.CachedTimes[item.ParentSheetIndex];
    }

    private static IEnumerable<CodeInstruction> Object_checkForAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(AccessTools.Method(typeof(Game1), nameof(Game1.playSound))))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return new(OpCodes.Ldloc_1);
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.PlaySound));
                yield return instruction;
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_getMinutesForCrystalarium_postfix(SObject __instance, ref int __result, int whichGem)
    {
        if (__instance is not { bigCraftable.Value: true, ParentSheetIndex: 97 })
        {
            return;
        }

        var minutes = ModPatches.GetMinutes(whichGem);
        if (minutes == 0)
        {
            return;
        }

        __result = minutes;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_minutesElapsed_postfix(SObject __instance, GameLocation environment)
    {
        if (ModPatches.Config.BreakChance <= 0
         || __instance is not
            {
                bigCraftable.Value: true,
                Name: "Crystalarium",
                ParentSheetIndex: 97,
                heldObject.Value: not null,
                MinutesUntilReady: 0,
            })
        {
            return;
        }

        if (Game1.random.NextDouble() > ModPatches.Config.BreakChance)
        {
            return;
        }

        __instance.ParentSheetIndex = 98;
        Game1.createItemDebris(__instance.heldObject.Value.getOne(), __instance.TileLocation * Game1.tileSize, 2);
        __instance.heldObject.Value = null;
        environment.localSound("breakingGlass");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_minutesElapsed_prefix(SObject __instance)
    {
        if (__instance is not { bigCraftable.Value: true, Name: "Crystalarium", ParentSheetIndex: 97 })
        {
            return true;
        }

        return __instance.heldObject.Value is not null;
    }

    private static string PlaySound(string sound, SObject obj, SObject? heldObj)
    {
        if (heldObj is null || obj is not { bigCraftable.Value: true, ParentSheetIndex: 97 })
        {
            return sound;
        }

        var capsuleItems = Game1.content.Load<CapsuleItems>("furyx639.OrdinaryCapsule/CapsuleItems");
        return capsuleItems.FirstOrDefault(
                               capsuleItem => capsuleItem.ContextTags.Any(heldObj.GetContextTags().Contains))
                           ?.Sound
            ?? sound;
    }
}