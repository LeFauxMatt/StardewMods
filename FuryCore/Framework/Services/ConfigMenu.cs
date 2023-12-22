namespace StardewMods.FuryCore.Framework.Services;

using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigMenu
{
    private readonly ModConfig config;
    private readonly IModHelper helper;

    /// <summary>Initializes a new instance of the <see cref="ConfigMenu" /> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="helper">Dependency for events, input, and content.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ConfigMenu(ModConfig config, IModHelper helper, IManifest manifest)
    {
        this.config = config;
        this.helper = helper;

        var gmcm = new GenericModConfigMenuIntegration(this.helper.ModRegistry);
        if (!gmcm.IsLoaded)
        {
            return;
        }

        gmcm.Api.Register(manifest, this.Reset, this.Save);

        // general options
        gmcm.Api.AddSectionTitle(manifest, I18n.Config_Section_General_Title, I18n.Config_Section_General_Description);

        gmcm.Api.AddTextOption(
            manifest,
            () => this.config.LogLevel.ToStringFast(),
            value => this.config.LogLevel = SimpleLogLevelExtensions.TryParse(value, out var logLevel) ? logLevel : SimpleLogLevel.Less,
            I18n.Config_LogLevel_Title,
            I18n.Config_LogLevel_Description,
            SimpleLogLevelExtensions.GetNames(),
            LocalizedTextManager.TryFormat);
    }

    private void Reset() => this.config.LogLevel = SimpleLogLevel.Less;

    private void Save() => this.helper.WriteConfig(this.config);
}
