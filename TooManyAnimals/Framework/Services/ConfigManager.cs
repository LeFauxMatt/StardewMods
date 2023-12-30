namespace StardewMods.TooManyAnimals.Framework.Services;

using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.TooManyAnimals.Framework.Interfaces;
using StardewMods.TooManyAnimals.Framework.Models;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IManifest manifest,
        IModHelper modHelper)
        : base(modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.manifest = manifest;

        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupModConfigMenu();
        }
    }

    /// <inheritdoc />
    public int AnimalShopLimit => this.Config.AnimalShopLimit;

    /// <inheritdoc />
    public Controls ControlScheme => this.Config.ControlScheme;

    private void SetupModConfigMenu()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();

        // Register mod configuration
        this.genericModConfigMenuIntegration.Register(this.manifest, this.Reset, () => this.Save(config));

        gmcm.AddSectionTitle(this.manifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Animal Shop Limit
        gmcm.AddNumberOption(
            this.manifest,
            () => config.AnimalShopLimit,
            value => config.AnimalShopLimit = value,
            I18n.Config_AnimalShopLimit_Name,
            I18n.Config_AnimalShopLimit_Tooltip,
            fieldId: nameof(DefaultConfig.AnimalShopLimit));

        gmcm.AddSectionTitle(this.manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Next Page
        gmcm.AddKeybindList(
            this.manifest,
            () => config.ControlScheme.NextPage,
            value => config.ControlScheme.NextPage = value,
            I18n.Config_NextPage_Name,
            I18n.Config_NextPage_Tooltip,
            nameof(Controls.NextPage));

        // Previous Page
        gmcm.AddKeybindList(
            this.manifest,
            () => config.ControlScheme.PreviousPage,
            value => config.ControlScheme.PreviousPage = value,
            I18n.Config_PreviousPage_Name,
            I18n.Config_PreviousPage_Tooltip,
            nameof(Controls.PreviousPage));
    }
}