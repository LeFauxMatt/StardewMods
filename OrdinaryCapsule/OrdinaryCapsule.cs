namespace StardewMods.OrdinaryCapsule;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.ItemRepository;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.OrdinaryCapsule.Models;

/// <inheritdoc />
public class OrdinaryCapsule : Mod
{
    private static readonly Dictionary<int, int> CachedTimes = new();

    private static readonly Lazy<List<Item>> ItemsLazy = new(
        () => new(new ItemRepository().GetAll().Select(item => item.Item)));

#nullable disable
    private static OrdinaryCapsule Instance;
#nullable enable

    private ModConfig? _config;

    private static IEnumerable<Item> AllItems => OrdinaryCapsule.ItemsLazy.Value;

    private ModConfig Config => this._config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        OrdinaryCapsule.Instance = this;
        I18n.Init(this.Helper.Translation);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged += OrdinaryCapsule.OnObjectListChanged;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
            transpiler: new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_checkForAction_transpiler)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), "getMinutesForCrystalarium"),
            postfix: new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_getMinutesForCrystalarium_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_minutesElapsed_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            postfix: new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_minutesElapsed_postfix)));
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new OrdinaryCapsuleApi(this.Helper);
    }

    private static int GetMinutes(Item item)
    {
        var capsuleItems = Game1.content.Load<List<CapsuleItem>>("furyx639.OrdinaryCapsule/CapsuleItems");
        var minutes = capsuleItems.Where(capsuleItem => capsuleItem.ContextTags.Any(item.GetContextTags().Contains))
                                  .Select(capsuleItem => capsuleItem.ProductionTime)
                                  .FirstOrDefault();
        OrdinaryCapsule.CachedTimes[item.ParentSheetIndex] =
            minutes > 0 ? minutes : OrdinaryCapsule.Instance.Config.DefaultProductionTime;
        return OrdinaryCapsule.CachedTimes[item.ParentSheetIndex];
    }

    private static int GetMinutes(int parentSheetIndex)
    {
        if (OrdinaryCapsule.CachedTimes.TryGetValue(parentSheetIndex, out var minutes))
        {
            return minutes;
        }

        var item = OrdinaryCapsule.AllItems.FirstOrDefault(item => item.ParentSheetIndex == parentSheetIndex);
        if (item is not null)
        {
            return OrdinaryCapsule.GetMinutes(item);
        }

        OrdinaryCapsule.CachedTimes[parentSheetIndex] = OrdinaryCapsule.Instance.Config.DefaultProductionTime;
        return OrdinaryCapsule.CachedTimes[parentSheetIndex];
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
                yield return CodeInstruction.Call(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.PlaySound));
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

        var minutes = OrdinaryCapsule.GetMinutes(whichGem);
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
        if (OrdinaryCapsule.Instance.Config.BreakChance <= 0
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

        if (Game1.random.NextDouble() > OrdinaryCapsule.Instance.Config.BreakChance)
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

    private static void OnObjectListChanged(object? sender, ObjectListChangedEventArgs e)
    {
        foreach (var (_, obj) in e.Added)
        {
            if (obj is not { bigCraftable.Value: true, ParentSheetIndex: 97 })
            {
                continue;
            }

            obj.Name = "Crystalarium";
        }
    }

    private static string PlaySound(string sound, SObject obj, SObject? heldObj)
    {
        if (heldObj is null || obj is not { bigCraftable.Value: true, ParentSheetIndex: 97 })
        {
            return sound;
        }

        var capsuleItems = Game1.content.Load<List<CapsuleItem>>("furyx639.OrdinaryCapsule/CapsuleItems");
        return capsuleItems.FirstOrDefault(
                               capsuleItem => capsuleItem.ContextTags.Any(heldObj.GetContextTags().Contains))
                           ?.Sound
            ?? sound;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/CapsuleItems"))
        {
            e.LoadFromModFile<List<CapsuleItem>>("assets/items.json", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data.Add(
                        "Ordinary Capsule",
                        $"335 99 337 2 439 1 787 1/Home/97/true/null/{I18n.Item_OrdinaryCapsule_Name()}");
                });
            return;
        }

        if (e.Name.IsEquivalentTo("Data/BigCraftablesInformation"))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<int, string>().Data;
                    data.Add(
                        97,
                        $"Ordinary Capsule/0/-300/Crafting -9/{I18n.Item_OrdinaryCapsule_Description()}/true/true/0//{I18n.Item_OrdinaryCapsule_Name()}");
                });
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
         || Game1.player.CurrentItem is not SObject { bigCraftable.Value: false }
         || !e.Button.IsUseToolButton())
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1);
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
         || obj is not { bigCraftable.Value: true, Name: "Crystalarium", ParentSheetIndex: 97 }
         || obj.heldObject.Value is not null
         || obj.MinutesUntilReady > 0)
        {
            return;
        }

        var capsuleItems = Game1.content.Load<List<CapsuleItem>>("furyx639.OrdinaryCapsule/CapsuleItems");
        var capsuleItem = capsuleItems.FirstOrDefault(
            capsuleItem => capsuleItem.ContextTags.Any(Game1.player.CurrentItem.GetContextTags().Contains));
        if (capsuleItem is null && !this.Config.DuplicateEverything)
        {
            return;
        }

        obj.heldObject.Value = (SObject)Game1.player.CurrentItem.getOne();
        Game1.currentLocation.playSound(capsuleItem?.Sound ?? "select");
        obj.MinutesUntilReady = capsuleItem?.ProductionTime > 0
            ? capsuleItem.ProductionTime
            : this.Config.DefaultProductionTime;
        Game1.player.reduceActiveItemByOne();
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (!Game1.player.craftingRecipes.ContainsKey("Ordinary Capsule")
         && (this.Config.UnlockAutomatically || Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken")))
        {
            Game1.player.craftingRecipes.Add("Ordinary Capsule", 0);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        if (!gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(this.ModManifest, () => this._config = new(), () => this.Helper.WriteConfig(this.Config));

        // Break Chance
        gmcm.API.AddNumberOption(
            this.ModManifest,
            () => this.Config.BreakChance,
            value => this.Config.BreakChance = value,
            I18n.Config_BreakChance_Name,
            I18n.Config_BreakChance_Tooltip,
            0,
            1);

        // Production Time
        gmcm.API.AddNumberOption(
            this.ModManifest,
            () => this.Config.DefaultProductionTime,
            value => this.Config.DefaultProductionTime = value,
            I18n.Config_DefaultProductionTime_Name,
            I18n.Config_DefaultProductionTime_Tooltip,
            0);

        // Duplicate Everything
        gmcm.API.AddBoolOption(
            this.ModManifest,
            () => this.Config.DuplicateEverything,
            value => this.Config.DuplicateEverything = value,
            I18n.Config_DuplicateEverything_Name,
            I18n.Config_DuplicateEverything_Name);

        // Unlock Automatically
        gmcm.API.AddBoolOption(
            this.ModManifest,
            () => this.Config.UnlockAutomatically,
            value => this.Config.UnlockAutomatically = value,
            I18n.Config_UnlockAutomatically_Name,
            I18n.Config_UnlockAutomatically_Tooltip);
    }
}