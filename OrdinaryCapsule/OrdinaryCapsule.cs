namespace StardewMods.OrdinaryCapsule;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Helpers.ItemRepository;
using StardewMods.Common.Integrations.GenericModConfigMenu;

/// <inheritdoc />
public class OrdinaryCapsule : Mod
{
    private static readonly Dictionary<int, int> CachedTimes = new();

    private static readonly Lazy<List<Item>> ItemsLazy = new(
        () => new(from item in new ItemRepository().GetAll() select item.Item));

    private ModConfig? _config;

    private static IEnumerable<Item> AllItems => OrdinaryCapsule.ItemsLazy.Value;

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this.Helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = config ?? new ModConfig();
            Log.Trace(this._config.ToString());
            return this._config;
        }
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
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
            AccessTools.Method(typeof(SObject), "getMinutesForCrystalarium"),
            postfix: new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_getMinutesForCrystalarium_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.minutesElapsed)),
            new(typeof(OrdinaryCapsule), nameof(OrdinaryCapsule.Object_minutesElapsed_prefix)));
    }

    private static int GetMinutes(Item item)
    {
        var productionTimes = Game1.content.Load<Dictionary<string, int>>("furyx639.OrdinaryCapsule/ProductionTime");
        var minutes = productionTimes.Where(kvp => item.GetContextTags().Contains(kvp.Key))
                                     .Select(kvp => kvp.Value)
                                     .FirstOrDefault();
        OrdinaryCapsule.CachedTimes[item.ParentSheetIndex] = minutes;
        return minutes;
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

        OrdinaryCapsule.CachedTimes[parentSheetIndex] = 0;
        return 0;
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
        if (minutes > 0)
        {
            __result = minutes;
        }
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

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/ProductionTime"))
        {
            e.LoadFromModFile<Dictionary<string, int>>("assets/items.json", AssetLoadPriority.Exclusive);
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

        var minutes = OrdinaryCapsule.GetMinutes(Game1.player.CurrentItem);
        if (minutes == 0)
        {
            return;
        }

        obj.heldObject.Value = (SObject)Game1.player.CurrentItem.getOne();
        Game1.currentLocation.playSound("select");
        obj.MinutesUntilReady = minutes;
        Game1.player.reduceActiveItemByOne();
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (this.Config.UnlockAutomatically
         || (Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken")
          && !Game1.player.craftingRecipes.ContainsKey("Ordinary Capsule")))
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

        // Unlock Automatically
        gmcm.API.AddBoolOption(
            this.ModManifest,
            () => this.Config.UnlockAutomatically,
            value => this.Config.UnlockAutomatically = value,
            I18n.Config_UnlockAutomatically_Name,
            I18n.Config_UnlockAutomatically_Tooltip);
    }
}