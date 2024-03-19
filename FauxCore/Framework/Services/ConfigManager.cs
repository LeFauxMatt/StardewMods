namespace StardewMods.FauxCore.Framework.Services;

using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Models;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(
        IEventPublisher eventPublisher,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        IManifest manifest,
        IModHelper modHelper)
        : base(eventPublisher, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.manifest = manifest;

        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupModConfigMenu();
        }
    }

    /// <inheritdoc />
    public SimpleLogLevel LogLevel => this.Config.LogLevel;

    private void SetupModConfigMenu()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();

        // Register mod configuration
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

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