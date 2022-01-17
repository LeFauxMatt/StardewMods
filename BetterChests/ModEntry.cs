namespace BetterChests;

using System.Linq;
using Common.Helpers;
using Models;
using Features;
using FuryCore.Interfaces;
using FuryCore.Services;
using Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;

/// <inheritdoc />
public class ModEntry : Mod
{
    private ModConfig Config { get; set; }

    private ServiceCollection Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Log.Init(this.Monitor);

        this.Config = this.Helper.ReadConfig<ModConfig>();

        // Services
        this.Services.AddRange(
            new IService[]
            {
                // Common Services
                new HarmonyHelper(this.ModManifest),

                // Mod Services
                new ManagedChests(this.Config, this.Helper),
                new ModConfigMenu(this.Config, this.Helper, this.ModManifest),

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
                new StashToChest(this.Config, this.Helper, this.Services),
                new CollectItems(this.Config, this.Helper, this.Services),
            });
        this.Services.ForceEvaluation();

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new BetterChestsApi(this.Config, this.Services);
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var furyCore = this.Helper.ModRegistry.GetApi<IService>("furyx639.FuryCore");
        this.Services.Add(furyCore);
        this.Services.ForceEvaluation();

        // Activate Features
        foreach (var feature in this.Services.OfType<Feature>())
        {
            feature.Activate();
        }
    }
}