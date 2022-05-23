#nullable disable

namespace StardewMods.TooManyAnimals.Services;

using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.TooManyAnimals.Interfaces;

/// <inheritdoc />
internal class ModConfigMenu : IModService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModConfigMenu" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    public ModConfigMenu(IConfigModel config, IModHelper helper, IManifest manifest)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private IConfigModel Config { get; }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private void GenerateConfig()
    {
        // Register mod configuration
        this.GMCM.Register(this.Manifest, this.Config.Reset, this.Config.Save);

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Animal Shop Limit
        this.GMCM.API.AddNumberOption(
            this.Manifest,
            () => this.Config.AnimalShopLimit,
            value => this.Config.AnimalShopLimit = value,
            I18n.Config_AnimalShopLimit_Name,
            I18n.Config_AnimalShopLimit_Tooltip,
            fieldId: nameof(IConfigData.AnimalShopLimit));

        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Next Page
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.ControlScheme.NextPage,
            value => this.Config.ControlScheme.NextPage = value,
            I18n.Config_NextPage_Name,
            I18n.Config_NextPage_Tooltip,
            nameof(IControlScheme.NextPage));

        // Previous Page
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => this.Config.ControlScheme.PreviousPage,
            value => this.Config.ControlScheme.PreviousPage = value,
            I18n.Config_PreviousPage_Name,
            I18n.Config_PreviousPage_Tooltip,
            nameof(IControlScheme.PreviousPage));
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GenerateConfig();
    }
}