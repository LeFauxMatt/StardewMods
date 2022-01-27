namespace BetterChests;

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using BetterChests.Features;
using BetterChests.Models;
using BetterChests.Services;
using Common.Extensions;
using FuryCore.Interfaces;
using FuryCore.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;

/// <inheritdoc cref="StardewModdingAPI.Mod" />
public class ModEntry : Mod, IAssetLoader
{
    /// <summary>
    /// Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ConfigModel Config { get; set; }

    private Dictionary<string, ChestData> ChestData { get; set; }

    private ServiceCollection Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModEntry.ModUniqueId = this.ModManifest.UniqueID;
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;
        KeybindListExtensions.InputHelper = this.Helper.Input;

        // Mod Config
        var config = this.Helper.ReadConfig<ConfigData>();
        this.Config = new(config, this.Helper, this.Services);

        // Chest Data
        this.ChestData = this.Helper.Data.ReadJsonFile<Dictionary<string, ChestData>>("assets/chests.json");
        if (this.ChestData is null)
        {
            this.ChestData = new()
            {
                { "Chest", new() },
                { "Stone Chest", new() },
                { "Junimo Chest", new() },
                { "Mini-Fridge", new() },
                { "Mini-Shipping Bin", new() },
                { "Fridge", new() },
                { "Auto-Grabber", new() },
            };
            this.Helper.Data.WriteJsonFile("assets/chests.json", this.ChestData);
        }

        // Services
        this.Services.AddRange(
            new IService[]
            {
                // Mod Services
                new ManagedChests(this.ChestData, this.Config, this.Helper, this.Services),
                new ModConfigMenu(this.Config, this.Helper, this.ModManifest, this.Services),

                // Features
                new CarryChest(this.Config, this.Helper, this.Services),
                new CategorizeChest(this.Config, this.Helper, this.Services),
                new ChestMenuTabs(this.Config, this.Helper, this.Services),
                new CustomColorPicker(this.Config, this.Helper, this.Services),
                new CraftFromChest(this.Config, this.Helper, this.Services),
                new FilterItems(this.Config, this.Helper, this.Services),
                new OpenHeldChest(this.Config, this.Helper, this.Services),
                new ResizeChestMenu(this.Config, this.Helper, this.Services),
                new ResizeChest(this.Config, this.Helper, this.Services),
                new SearchItems(this.Config, this.Helper, this.Services),
                new SlotLock(this.Config, this.Helper, this.Services),
                new StashToChest(this.Config, this.Helper, this.Services),
                new CollectItems(this.Config, this.Helper, this.Services),
            });
        this.Services.ForceEvaluation();

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc/>
    public bool CanLoad<T>(IAssetInfo asset)
    {
        return asset.AssetNameEquals($"{ModEntry.ModUniqueId}/Chests");
    }

    /// <inheritdoc/>
    public T Load<T>(IAssetInfo asset)
    {
        return (T)(object)this.ChestData;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var furyCore = this.Helper.ModRegistry.GetApi<IService>("furyx639.FuryCore");
        this.Services.Add(furyCore);
        this.Services.ForceEvaluation();

        // Activate Features
        foreach (var feature in this.Services.OfType<Feature>())
        {
            feature.Toggle();
        }
    }
}