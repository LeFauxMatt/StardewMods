namespace StardewMods.EasyAccess.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.EasyAccess.Enums;
using StardewMods.EasyAccess.Features;
using StardewMods.EasyAccess.Helpers;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Models.Config;
using StardewMods.EasyAccess.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class ModConfigMenu : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModConfigMenu" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ModConfigMenu(IConfigModel config, IModHelper helper, IManifest manifest, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this._assetHandler = services.Lazy<AssetHandler>();
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IConfigModel Config { get; }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    /// <summary>
    ///     Add producer feature options to GMCM based on a dictionary of string keys/values representing
    ///     <see cref="IProducerData" />.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="data">The chest data to base the config on.</param>
    public void ProducerConfig(IManifest manifest, IDictionary<string, string> data)
    {
        var producerData = new SerializedProducerData(data);
        this.ProducerConfig(manifest, producerData, false);
    }

    private void ControlsConfig(IControlScheme controls)
    {
        this.GMCM.API.AddSectionTitle(this.Manifest, I18n.Section_Controls_Name, I18n.Section_Controls_Description);

        // Collect Items
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.CollectItems,
            value => controls.CollectItems = value,
            I18n.Config_CollectItems_Name,
            I18n.Config_CollectItems_Tooltip,
            nameof(IControlScheme.CollectItems));

        // Dispense Items
        this.GMCM.API.AddKeybindList(
            this.Manifest,
            () => controls.DispenseItems,
            value => controls.DispenseItems = value,
            I18n.Config_DispenseItems_Name,
            I18n.Config_DispenseItems_Tooltip,
            nameof(IControlScheme.DispenseItems));
    }

    private void GenerateConfig()
    {
        // Register mod configuration
        this.GMCM.Register(
            this.Manifest,
            () =>
            {
                this.Config.Reset();
                foreach (var (_, data) in this.Assets.ProducerData)
                {
                    ((IProducerData)new ProducerData()).CopyTo(data);
                }
            },
            () =>
            {
                this.Config.Save();
                this.Assets.SaveProducerData();
            });

        // Controls
        this.ControlsConfig(this.Config.ControlScheme);

        // Features
        this.ProducerConfig(this.Manifest, this.Config.DefaultProducer, true);

        this.GMCM.API.AddPageLink(this.Manifest, "Producers", I18n.Section_Producers_Name);
        this.GMCM.API.AddParagraph(this.Manifest, I18n.Section_Producers_Description);

        // Producers
        this.GMCM.API.AddPage(this.Manifest, "Producers");

        foreach (var (name, _) in this.Assets.ProducerData.OrderBy(producerData => producerData.Key))
        {
            this.GMCM.API.AddPageLink(this.Manifest, name, () => name);
        }

        foreach (var (name, data) in this.Assets.ProducerData)
        {
            this.GMCM.API.AddPage(this.Manifest, name);
            this.ProducerConfig(this.Manifest, data, false);
        }
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GenerateConfig();
    }

    /// <summary>
    ///     Adds GMCM options for producer data.
    /// </summary>
    /// <param name="manifest">The mod's manifest.</param>
    /// <param name="producerData">The producer data to configure.</param>
    /// <param name="defaultConfig">Set to true if configuring the default producer config options.</param>
    private void ProducerConfig(IManifest manifest, IProducerData producerData, bool defaultConfig)
    {
        var rangeValues = (defaultConfig
                              ? new[] { FeatureOptionRange.Disabled, FeatureOptionRange.Location, FeatureOptionRange.World }
                              : new[] { FeatureOptionRange.Disabled, FeatureOptionRange.Default, FeatureOptionRange.Location, FeatureOptionRange.World })
                          .Select(FormatHelper.GetRangeString)
                          .ToArray();
        var defaultRange = defaultConfig ? FeatureOptionRange.Location : FeatureOptionRange.Default;

        this.GMCM.API.AddSectionTitle(manifest, I18n.Section_Features_Name, I18n.Section_Features_Description);

        // Collect Outputs
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetRangeString(producerData.CollectOutputs),
            value => producerData.CollectOutputs = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_CollectOutputs_Name,
            I18n.Config_CollectOutputs_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(CollectOutputs));

        // Collect Output Distance
        this.GMCM.API.AddNumberOption(
            manifest,
            () => producerData.CollectOutputDistance switch
            {
                -1 => 16,
                _ => producerData.CollectOutputDistance,
            },
            value => producerData.CollectOutputDistance = value switch
            {
                16 => -1,
                _ => value,
            },
            I18n.Config_CollectOutputsDistance_Name,
            I18n.Config_CollectOutputsDistance_Tooltip,
            defaultConfig ? 1 : 0,
            16,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IProducerData.CollectOutputDistance));

        // Dispense Inputs
        this.GMCM.API.AddTextOption(
            manifest,
            () => FormatHelper.GetRangeString(producerData.DispenseInputs),
            value => producerData.DispenseInputs = Enum.TryParse(value, out FeatureOptionRange range) ? range : defaultRange,
            I18n.Config_DispenseInputs_Name,
            I18n.Config_DispenseInputs_Tooltip,
            rangeValues,
            FormatHelper.FormatRange,
            nameof(CollectOutputs));

        // Dispense Input Distance
        this.GMCM.API.AddNumberOption(
            manifest,
            () => producerData.DispenseInputDistance switch
            {
                -1 => 16,
                _ => producerData.DispenseInputDistance,
            },
            value => producerData.DispenseInputDistance = value switch
            {
                16 => -1,
                _ => value,
            },
            I18n.Config_DispenseInputsDistance_Name,
            I18n.Config_DispenseInputsDistance_Tooltip,
            defaultConfig ? 1 : 0,
            16,
            1,
            FormatHelper.FormatRangeDistance,
            nameof(IProducerData.DispenseInputDistance));

        // Dispense Input Priority
        this.GMCM.API.AddNumberOption(
            manifest,
            () => producerData.DispenseInputPriority,
            value => producerData.DispenseInputPriority = value,
            I18n.Config_DispenseInputsPriority_Name,
            I18n.Config_DispenseInputsPriority_Tooltip,
            fieldId: nameof(IProducerData.DispenseInputPriority));
    }
}