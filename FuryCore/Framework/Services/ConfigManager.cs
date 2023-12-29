namespace StardewMods.FuryCore.Framework.Services;

using StardewMods.Common.Enums;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.FuryCore.Framework.Interfaces;
using StardewMods.FuryCore.Framework.Models;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigManager : BaseService, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IModHelper modHelper;
    private readonly IManifest manifest;

    private IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        ILog log,
        IManifest manifest,
        IModHelper modHelper)
        : base(log, manifest)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.modHelper = modHelper;
        this.manifest = manifest;
        this.modConfig = this.modHelper.ReadConfig<DefaultConfig>();

        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupModConfigMenu();
        }
    }

    /// <inheritdoc />
    public SimpleLogLevel LogLevel => this.modConfig.LogLevel;

    private void Reset() => this.modConfig = new DefaultConfig();

    private void Save(DefaultConfig config)
    {
        this.modHelper.WriteConfig(config);
        this.modConfig = config;
    }

    private void SetupModConfigMenu()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.modHelper.ReadConfig<DefaultConfig>();

        // Register mod configuration
        this.genericModConfigMenuIntegration.Register(this.manifest, this.Reset, () => this.Save(config));

        // general options
        gmcm.AddSectionTitle(this.manifest, I18n.Config_Section_General_Title, I18n.Config_Section_General_Description);

        gmcm.AddTextOption(
            this.manifest,
            () => config.LogLevel.ToStringFast(),
            value => config.LogLevel = SimpleLogLevelExtensions.TryParse(value, out var logLevel)
                ? logLevel
                : SimpleLogLevel.Less,
            I18n.Config_LogLevel_Title,
            I18n.Config_LogLevel_Description,
            SimpleLogLevelExtensions.GetNames(),
            LocalizedTextManager.TryFormat);
    }
}