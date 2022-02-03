namespace StardewMods.BetterChestsConfigurator;

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Integrations.BetterChests;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
public class BetterChestsConfigurator : Mod
{
    private BetterChestsIntegration BetterChests { get; set; }

    private GenericModConfigMenuIntegration GMCM { get; set; }

    private ModConfig Config { get; set; }

    private Chest CurrentChest { get; set; }

    private IDictionary<string, string> ChestData { get; set; }

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        this.BetterChests = new(this.Helper.ModRegistry);
        this.GMCM = new(this.Helper.ModRegistry);
        this.Config = this.Helper.ReadConfig<ModConfig>();

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GMCM.API.Register(
            this.ModManifest,
            () => this.Config = new(), 
            () => this.Helper.WriteConfig(this.Config));
        this.GMCM.API.AddParagraph(
            this.ModManifest, 
            () => this.Helper.Translation.Get("config.description.text"));
        this.GMCM.API.SetTitleScreenOnlyForNextOptions(this.ModManifest, true);
        this.GMCM.API.AddKeybindList(
            this.ModManifest,
            () => this.Config.ConfigureChest,
            value => this.Config.ConfigureChest = value,
            () => this.Helper.Translation.Get("config.configure-chest.name"),
            () => this.Helper.Translation.Get("config.configure-chest.tooltip"));
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (this.ChestData is not null && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.GMCM.Unregister(this.ModManifest);
            this.ChestData = null;
            this.CurrentChest = null;
        }
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!this.BetterChests.IsLoaded
            || !this.GMCM.IsLoaded
            || !Context.IsPlayerFree
            || Game1.player.CurrentItem is not Chest chest
            || !this.Config.ConfigureChest.JustPressed())
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(this.Config.ConfigureChest);
        this.CurrentChest = chest;
        this.ChestData = this.CurrentChest.modData.Pairs
                             .Where(modData => modData.Key.StartsWith($"{this.BetterChests.UniqueId}"))
                             .ToDictionary(
                                 modData => modData.Key[(this.BetterChests.UniqueId.Length + 1)..],
                                 modData => modData.Value);
        this.GMCM.Register(this.ModManifest, this.Reset, this.Save);
        this.BetterChests.API.AddChestOptions(this.ModManifest, this.ChestData);
        this.GMCM.API.OpenModMenu(this.ModManifest);
    }

    private void Reset()
    {
        foreach (var (key, _) in this.ChestData)
        {
            this.ChestData[key] = string.Empty;
        }
    }

    private void Save()
    {
        foreach (var (key, value) in this.ChestData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.CurrentChest.modData.Remove($"{this.BetterChests.UniqueId}/{key}");
                continue;
            }

            this.CurrentChest.modData[$"{this.BetterChests.UniqueId}/{key}"] = value;
        }
    }
}