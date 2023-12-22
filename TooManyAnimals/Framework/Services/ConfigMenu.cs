namespace StardewMods.TooManyAnimals.Framework.Services;

using StardewMods.Common.Services.Integrations.GenericModConfigMenu;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigMenu
{
    private readonly ModConfig config;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IModHelper helper;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigMenu" /> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="helper">Dependency for events, input, and content.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ConfigMenu(ModConfig config, IModHelper helper, IManifest manifest)
    {
        this.config = config;
        this.helper = helper;
        this.manifest = manifest;

        this.gmcm = new GenericModConfigMenuIntegration(this.helper.ModRegistry);
        this.Setup();
    }

    private void Reset()
    {
        this.config.AnimalShopLimit = 30;
        this.config.ControlScheme = new Controls();
    }

    private void Save() => this.helper.WriteConfig(this.config);

    private void Setup()
    {
        if (!this.gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        this.gmcm.Api.Register(this.manifest, this.Reset, this.Save);

        this.gmcm.Api.AddSectionTitle(this.manifest, I18n.Section_General_Name, I18n.Section_General_Description);

        // Animal Shop Limit
        this.gmcm.Api.AddNumberOption(
            this.manifest,
            () => this.config.AnimalShopLimit,
            value => this.config.AnimalShopLimit = value,
            I18n.Config_AnimalShopLimit_Name,
            I18n.Config_AnimalShopLimit_Tooltip,
            fieldId: nameof(ModConfig.AnimalShopLimit));

        this.gmcm.Api.AddSectionTitle(this.manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Next Page
        this.gmcm.Api.AddKeybindList(
            this.manifest,
            () => this.config.ControlScheme.NextPage,
            value => this.config.ControlScheme.NextPage = value,
            I18n.Config_NextPage_Name,
            I18n.Config_NextPage_Tooltip,
            nameof(Controls.NextPage));

        // Previous Page
        this.gmcm.Api.AddKeybindList(
            this.manifest,
            () => this.config.ControlScheme.PreviousPage,
            value => this.config.ControlScheme.PreviousPage = value,
            I18n.Config_PreviousPage_Name,
            I18n.Config_PreviousPage_Tooltip,
            nameof(Controls.PreviousPage));
    }
}
