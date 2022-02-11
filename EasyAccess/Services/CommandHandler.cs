namespace StardewMods.EasyAccess.Services;

using System;
using System.Linq;
using System.Text;
using Common.Helpers;
using StardewModdingAPI;
using StardewMods.EasyAccess.Features;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class CommandHandler : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly Lazy<CollectOutputs> _collectOutputs;
    private readonly Lazy<DispenseInputs> _dispenseInputs;
    private readonly Lazy<ManagedObjects> _managedObjects;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandHandler" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CommandHandler(IConfigData config, IModHelper helper, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this._assetHandler = services.Lazy<AssetHandler>();
        this._collectOutputs = services.Lazy<CollectOutputs>();
        this._dispenseInputs = services.Lazy<DispenseInputs>();
        this._managedObjects = services.Lazy<ManagedObjects>();
        this.Helper.ConsoleCommands.Add(
            "easy_access_info",
            "Prints information to the logs about all producers managed by easy access.",
            this.DumpInfo);
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private CollectOutputs CollectOutputs
    {
        get => this._collectOutputs.Value;
    }

    private IConfigData Config { get; }

    private DispenseInputs DispenseInputs
    {
        get => this._dispenseInputs.Value;
    }

    private IModHelper Helper { get; }

    private ManagedObjects ManagedObjects
    {
        get => this._managedObjects.Value;
    }

    private static void AddProducerData(StringBuilder sb, IProducerData data, string storageName)
    {
        var dictData = SerializedProducerData.GetData(data);
        if (dictData.Values.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        CommandHandler.AppendHeader(sb, storageName);

        foreach (var (key, value) in dictData)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.AppendFormat("{0,25}: {1}\n", key, value);
            }
        }
    }

    private static void AppendControls(StringBuilder sb, IControlScheme controls)
    {
        CommandHandler.AppendHeader(sb, "Controls");

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.CollectItems),
            controls.CollectItems);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.DispenseItems),
            controls.DispenseItems);
    }

    private static void AppendHeader(StringBuilder sb, string text)
    {
        sb.AppendFormat($"\n{{0,{(25 + text.Length / 2).ToString()}}}\n", text);
        sb.AppendFormat($"{{0,{(25 + text.Length / 2).ToString()}}}\n", new string('-', text.Length));
    }

    private void DumpConfig(StringBuilder sb)
    {
        // Main Header
        CommandHandler.AppendHeader(sb, "Mod Config");

        // Control Scheme
        CommandHandler.AppendControls(sb, this.Config.ControlScheme);

        // Default Producer
        CommandHandler.AddProducerData(sb, this.Config.DefaultProducer, "\"Default Producer\" Config");
    }

    private void DumpInfo(string command, string[] args)
    {
        var sb = new StringBuilder();

        // Main Header
        sb.AppendLine("Easy Access Info");

        // Log Config
        this.DumpConfig(sb);

        // Iterate known producers and features
        foreach (var (name, producerData) in this.Assets.ProducerData)
        {
            CommandHandler.AddProducerData(sb, producerData, $"\"{name}\" Config");
        }

        var eligibleProducerOutputs = this.CollectOutputs.EligibleProducers.ToDictionary(managedProducer => managedProducer, _ => string.Empty);
        var eligibleProducerInputs = this.DispenseInputs.EligibleProducers.ToDictionary(managedProducer => managedProducer, _ => string.Empty);

        foreach (var ((location, (x, y)), managedProducer) in this.ManagedObjects.Producers)
        {
            CommandHandler.AddProducerData(sb, managedProducer, $"Producer \"{managedProducer.QualifiedItemId}\" at location {location.NameOrUniqueName} at coordinates ({((int)x).ToString()},{((int)y).ToString()}).");

            if (eligibleProducerOutputs.Keys.Contains(managedProducer))
            {
                eligibleProducerOutputs[managedProducer] = $"Location {location.NameOrUniqueName} at ({((int)x).ToString()},{((int)y).ToString()}).";
            }

            if (eligibleProducerInputs.Keys.Contains(managedProducer))
            {
                eligibleProducerInputs[managedProducer] = $"Location {location.NameOrUniqueName} at ({((int)x).ToString()},{((int)y).ToString()}).";
            }
        }

        CommandHandler.AppendHeader(sb, "Collect Outputs Eligible Producers");
        foreach (var (managedProducer, description) in eligibleProducerOutputs)
        {
            sb.AppendFormat("{0,25}: {1}\n", managedProducer.QualifiedItemId, description);
        }

        CommandHandler.AppendHeader(sb, "Dispense Inputs Eligible Producers");
        foreach (var (managedProducer, description) in eligibleProducerInputs)
        {
            sb.AppendFormat("{0,25}: {1}\n", managedProducer.QualifiedItemId, description);
        }

        Log.Info(sb.ToString());
    }
}