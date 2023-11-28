namespace StardewMods.OrdinaryCapsule;

using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.OrdinaryCapsule.Framework;
using StardewMods.OrdinaryCapsule.Framework.Models;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private ModConfig? config;

    private ModConfig Config => this.config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(this.Helper.Translation);
        ModPatches.Init(this.ModManifest, this.Config);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged += ModEntry.OnObjectListChanged;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new Api(this.Helper);
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
        gmcm.Register(this.ModManifest, () => this.config = new(), () => this.Helper.WriteConfig(this.Config));

        // Break Chance
        gmcm.Api.AddNumberOption(
            this.ModManifest,
            () => this.Config.BreakChance,
            value => this.Config.BreakChance = value,
            I18n.Config_BreakChance_Name,
            I18n.Config_BreakChance_Tooltip,
            0,
            1);

        // Production Time
        gmcm.Api.AddNumberOption(
            this.ModManifest,
            () => this.Config.DefaultProductionTime,
            value => this.Config.DefaultProductionTime = value,
            I18n.Config_DefaultProductionTime_Name,
            I18n.Config_DefaultProductionTime_Tooltip,
            0);

        // Duplicate Everything
        gmcm.Api.AddBoolOption(
            this.ModManifest,
            () => this.Config.DuplicateEverything,
            value => this.Config.DuplicateEverything = value,
            I18n.Config_DuplicateEverything_Name,
            I18n.Config_DuplicateEverything_Name);

        // Unlock Automatically
        gmcm.Api.AddBoolOption(
            this.ModManifest,
            () => this.Config.UnlockAutomatically,
            value => this.Config.UnlockAutomatically = value,
            I18n.Config_UnlockAutomatically_Name,
            I18n.Config_UnlockAutomatically_Tooltip);
    }
}