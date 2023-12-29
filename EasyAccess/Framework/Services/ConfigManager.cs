namespace StardewMods.EasyAccess.Framework.Services;

using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.EasyAccess.Framework.Interfaces;
using StardewMods.EasyAccess.Framework.Models;

/// <inheritdoc cref="StardewMods.EasyAccess.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : BaseService, IModConfig
{
    private readonly IManifest manifest;
    private readonly IModHelper modHelper;
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;

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

    /// <inheritdoc />
    public int CollectOutputDistance => this.modConfig.CollectOutputDistance;

    /// <inheritdoc />
    public Controls ControlScheme => this.modConfig.ControlScheme;

    /// <inheritdoc />
    public int DispenseInputDistance => this.modConfig.DispenseInputDistance;

    /// <inheritdoc />
    public bool DoDigSpots => this.modConfig.DoDigSpots;

    /// <inheritdoc />
    public bool DoForage => this.modConfig.DoForage;

    /// <inheritdoc />
    public bool DoMachines => this.modConfig.DoMachines;

    /// <inheritdoc />
    public bool DoTerrain => this.modConfig.DoTerrain;

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

        // Collect Items
        gmcm.AddKeybindList(
            this.manifest,
            () => config.ControlScheme.CollectItems,
            value => config.ControlScheme.CollectItems = value,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip,
            nameof(Controls.CollectItems));

        // Dispense Items
        gmcm.AddKeybindList(
            this.manifest,
            () => config.ControlScheme.DispenseItems,
            value => config.ControlScheme.DispenseItems = value,
            I18n.Config_DispenseItems_Name,
            I18n.Config_DispenseItems_Tooltip,
            nameof(Controls.DispenseItems));

        // Collect Output Distance
        gmcm.AddNumberOption(
            this.manifest,
            () => config.CollectOutputDistance,
            value => config.CollectOutputDistance = value,
            I18n.Config_CollectOutputsDistance_Name,
            I18n.Config_CollectOutputsDistance_Tooltip,
            1,
            16,
            1,
            fieldId: nameof(DefaultConfig.CollectOutputDistance));

        // Dispense Input Distance
        gmcm.AddNumberOption(
            this.manifest,
            () => config.DispenseInputDistance,
            value => config.DispenseInputDistance = value,
            I18n.Config_DispenseInputsDistance_Name,
            I18n.Config_DispenseInputsDistance_Tooltip,
            1,
            16,
            1,
            fieldId: nameof(DefaultConfig.DispenseInputDistance));

        // Do Dig Spots
        gmcm.AddBoolOption(
            this.manifest,
            () => config.DoDigSpots,
            value => config.DoDigSpots = value,
            I18n.Config_DoDigSpots_Name,
            I18n.Config_DoDigSpots_Tooltip,
            nameof(DefaultConfig.DoDigSpots));

        // Do Forage
        gmcm.AddBoolOption(
            this.manifest,
            () => config.DoForage,
            value => config.DoForage = value,
            I18n.Config_DoForage_Name,
            I18n.Config_DoForage_Tooltip,
            nameof(DefaultConfig.DoForage));

        // Do Machines
        gmcm.AddBoolOption(
            this.manifest,
            () => config.DoMachines,
            value => config.DoMachines = value,
            I18n.Config_DoMachines_Name,
            I18n.Config_DoMachines_Tooltip,
            nameof(DefaultConfig.DoMachines));

        // Do Terrain
        gmcm.AddBoolOption(
            this.manifest,
            () => config.DoTerrain,
            value => config.DoTerrain = value,
            I18n.Config_DoTerrain_Name,
            I18n.Config_DoTerrain_Tooltip,
            nameof(DefaultConfig.DoTerrain));
    }
}