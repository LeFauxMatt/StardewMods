namespace StardewMods.ExpandedStorage.Framework.Services;

using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.ExpandedStorage.Framework.Interfaces;
using StardewMods.ExpandedStorage.Framework.Models;

/// <summary>Handles the config menu.</summary>
internal sealed class ConfigManager : BaseService, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly IManifest manifest;
    private readonly IModHelper modHelper;

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
        this.manifest = manifest;
        this.modHelper = modHelper;
        this.modConfig = this.modHelper.ReadConfig<DefaultConfig>();

        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupModConfigMenu();
        }
    }

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
    }
}