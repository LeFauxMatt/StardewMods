namespace StardewMods.BetterChests;

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Services;

/// <inheritdoc cref="StardewModdingAPI.Mod" />
public class BetterChests : Mod, IAssetLoader
{
    /// <summary>
    /// Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ConfigModel Config { get; set; }

    private Dictionary<string, ChestData> ChestData { get; set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        BetterChests.ModUniqueId = this.ModManifest.UniqueID;
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;

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
        this.Services.Add(
            new ManagedChests(this.Config, this.Helper),
            new ModConfigMenu(this.ChestData, this.Config, this.Helper, this.ModManifest),
            new CarryChest(this.Config, this.Helper, this.Services),
            new CategorizeChest(this.Config, this.Helper, this.Services),
            new ChestMenuTabs(this.Config, this.Helper, this.Services),
            new CollectItems(this.Config, this.Helper, this.Services),
            new CraftFromChest(this.Config, this.Helper, this.Services),
            new CustomColorPicker(this.Config, this.Helper, this.Services),
            new FilterItems(this.Config, this.Helper, this.Services),
            new OpenHeldChest(this.Config, this.Helper, this.Services),
            new ResizeChest(this.Config, this.Helper, this.Services),
            new ResizeChestMenu(this.Config, this.Helper, this.Services),
            new SearchItems(this.Config, this.Helper, this.Services),
            new SlotLock(this.Config, this.Helper, this.Services),
            new StashToChest(this.Config, this.Helper, this.Services),
            new UnloadChest(this.Config, this.Helper, this.Services));

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc/>
    public override object GetApi()
    {
        return new BetterChestsApi(this.ChestData, this.Helper, this.Services);
    }

    /// <inheritdoc/>
    public bool CanLoad<T>(IAssetInfo asset)
    {
        return this.ChestData.Keys.Any(key => asset.AssetNameEquals($"{BetterChests.ModUniqueId}/Chests/{key}"));
    }

    /// <inheritdoc/>
    public T Load<T>(IAssetInfo asset)
    {
        var key = PathUtilities.GetSegments(asset.AssetName)[2];
        if (!this.ChestData.TryGetValue(key, out var chestData))
        {
            return default;
        }

        return (T)(object)SerializedChestData.GetData(chestData);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var furyCore = this.Helper.ModRegistry.GetApi<IModServices>("furyx639.FuryCore");
        this.Services.Add((IModService)furyCore);

        // Activate Features
        foreach (var feature in this.Services.FindServices<Feature>())
        {
            feature.Toggle();
        }
    }
}