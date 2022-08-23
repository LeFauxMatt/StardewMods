namespace StardewMods.OrdinaryCapsule;

using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;

/// <inheritdoc />
public class OrdinaryCapsule : Mod
{
    private static ModConfig? Config;

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(this.Helper.Translation);
        OrdinaryCapsule.Config = this.Helper.ReadConfig<ModConfig>();

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += OrdinaryCapsule.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged += OrdinaryCapsule.OnObjectListChanged;

        // Patches
        var harmony = new Harmony(this.ModManifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(SObject), "getMinutesForCrystalarium"),
            postfix: new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_getMinutesForCrystalarium_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_minutesElapsed_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_getMinutesForCrystalarium_postfix(SObject __instance, ref int __result, int whichGem)
    {
        if (__instance is not { bigCraftable.Value: true, ParentSheetIndex: 97 })
        {
            return;
        }

        var productionTime = Game1.content.Load<Dictionary<string, int>>("furyx639.OrdinaryCapsule/ProductionTime");
        if (productionTime.TryGetValue(whichGem.ToString(), out var minutes))
        {
            __result = minutes;
            return;
        }

        __result = OrdinaryCapsule.Config?.Minutes ?? 1440;
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

    private static void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken")
         && !Game1.player.craftingRecipes.ContainsKey("Ordinary Capsule"))
        {
            Game1.player.craftingRecipes.Add("Ordinary Capsule", 0);
        }
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

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/ProductionTime"))
        {
            e.LoadFrom(() => new Dictionary<string, int>(), AssetLoadPriority.Exclusive);
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
         || Game1.player.CurrentItem is null
         || (Game1.player.CurrentItem.Category is not (-5 or -6 or -14)
          && Game1.player.CurrentItem.ParentSheetIndex != 430)
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

        obj.heldObject.Value = (SObject)Game1.player.CurrentItem.getOne();
        Game1.currentLocation.playSound("select");
        var productionTime =
            this.Helper.GameContent.Load<Dictionary<string, int>>("furyx639.OrdinaryCapsule/ProductionTime");
        obj.MinutesUntilReady =
            productionTime.TryGetValue(Game1.player.CurrentItem.ParentSheetIndex.ToString(), out var minutes)
                ? minutes
                : OrdinaryCapsule.Config?.Minutes ?? 1440;
        Game1.player.reduceActiveItemByOne();
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        if (!gmcm.IsLoaded || OrdinaryCapsule.Config is null)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(
            this.ModManifest,
            () => OrdinaryCapsule.Config = new(),
            () => this.Helper.WriteConfig(OrdinaryCapsule.Config));

        // Production Time
        gmcm.API.AddNumberOption(
            this.ModManifest,
            () => OrdinaryCapsule.Config.Minutes,
            value => OrdinaryCapsule.Config.Minutes = value,
            I18n.Config_ProductionTime_Name,
            I18n.Config_ProductionTime_Tooltip);
    }
}