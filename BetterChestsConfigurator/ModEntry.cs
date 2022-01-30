namespace BetterChestsConfigurator;

using Common.Extensions;
using Common.Helpers;
using Mod.BetterChests.Models;
using Common.Integrations.BetterChests;
using Common.Integrations.GenericModConfigMenu;
using Mod.BetterChests;
using Mod.BetterChests.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

/// <inheritdoc />
public class ModEntry : Mod
{
    private BetterChestsIntegration BetterChestsMod { get; set; }

    private GenericModConfigMenuIntegration GMCM { get; set; }

    private ModConfig Config { get; set; }

    private ConfigurableChest ChestData { get; set; }

    private bool MenuOpened { get; set; }

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        I18n.Init(helper.Translation);
        this.BetterChestsMod = new(this.Helper.ModRegistry);
        this.GMCM = new(this.Helper.ModRegistry);
        this.Config = this.Helper.ReadConfig<ModConfig>();

        // Console Commands
        this.Helper.ConsoleCommands.Add(
            "chest_config",
            I18n.Command_ChestConfig_Description(),
            this.ConfigureChest);

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (this.MenuOpened && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.GMCM.Unregister(this.ModManifest);
            this.MenuOpened = false;
        }
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!this.BetterChestsMod.IsLoaded
            || !this.GMCM.IsLoaded
            || !Context.IsPlayerFree
            || Game1.player.CurrentItem is not Chest chest
            || !this.Config.ConfigureChest.JustPressed())
        {
            return;
        }

        this.ConfigureChest(chest);
        this.Helper.Input.SuppressActiveKeybinds(this.Config.ConfigureChest);
    }

    private void ConfigureChest(string command, string[] args)
    {
        if (!this.BetterChestsMod.IsLoaded
            || !this.GMCM.IsLoaded
            || !Context.IsPlayerFree
            || Game1.player.CurrentItem is not Chest chest)
        {
            return;
        }

        this.ConfigureChest(chest);
    }

    private void ConfigureChest(Chest chest)
    {
        this.ChestData = new(chest, this.BetterChestsMod.UniqueId);
        this.GMCM.Register(this.ModManifest, this.Reset, this.Save);
        BetterChests.Integration.ChestConfig(this.ModManifest, this.ChestData);
        this.GMCM.API.OpenModMenu(this.ModManifest);
        this.MenuOpened = true;
    }

    private void Reset()
    {
        ((IChestData)new ChestData()).CopyTo(this.ChestData);
    }

    private void Save()
    {
        this.ChestData.Save();
    }
}